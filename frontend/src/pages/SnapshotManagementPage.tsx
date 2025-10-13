import React, { useState, useEffect } from 'react';
import { 
  Card, 
  Select, 
  DatePicker, 
  Table, 
  Button, 
  Space, 
  Typography, 
  message, 
  Modal, 
  Input, 
  Row, 
  Col, 
  Statistic,
  Popconfirm,
  Tooltip,
  Tag
} from 'antd';
import { 
  SearchOutlined, 
  DeleteOutlined, 
  EyeOutlined, 
  ReloadOutlined,
  ExclamationCircleOutlined 
} from '@ant-design/icons';
import type { ColumnsType, TablePaginationConfig } from 'antd/es/table';
import dayjs from 'dayjs';
import { 
  MutualFundDto, 
  SnapshotDto, 
  SnapshotListResponseDto,
  SnapshotHoldingsResponseDto,
  CurrentHoldingDto,
  DeleteSnapshotResponseDto
} from '../types';
import { mutualFundService } from '../services/api';
import { snapshotManagementService } from '../services/snapshotManagementService';

const { Title, Text } = Typography;
const { Search } = Input;

interface TableParams {
  pagination?: TablePaginationConfig;
  sortField?: string;
  sortOrder?: string;
  filters?: Record<string, any>;
}

const SnapshotManagementPage: React.FC = () => {
  // State for mutual fund selection
  const [mutualFunds, setMutualFunds] = useState<MutualFundDto[]>([]);
  const [selectedMutualFund, setSelectedMutualFund] = useState<number | null>(null);
  const [selectedMutualFundName, setSelectedMutualFundName] = useState<string>('');
  
  // State for snapshots
  const [snapshots, setSnapshots] = useState<SnapshotDto[]>([]);
  const [selectedSnapshot, setSelectedSnapshot] = useState<SnapshotDto | null>(null);
  
  // State for holdings
  const [holdings, setHoldings] = useState<CurrentHoldingDto[]>([]);
  const [holdingsLoading, setHoldingsLoading] = useState(false);
  const [tableParams, setTableParams] = useState<TableParams>({
    pagination: {
      current: 1,
      pageSize: 20,
      total: 0,
    },
  });
  
  // State for search and filters
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [sectorFilter, setSectorFilter] = useState<string>('');
  const [industryFilter, setIndustryFilter] = useState<string>('');
  
  // Loading states
  const [loadingMutualFunds, setLoadingMutualFunds] = useState(false);
  const [loadingSnapshots, setLoadingSnapshots] = useState(false);
  
  // Modal state
  const [deleteModalVisible, setDeleteModalVisible] = useState(false);
  const [deletingSnapshot, setDeletingSnapshot] = useState(false);

  // Load mutual funds on component mount
  useEffect(() => {
    loadMutualFunds();
  }, []);

  // Load snapshots when mutual fund is selected
  useEffect(() => {
    if (selectedMutualFund) {
      loadSnapshots();
    } else {
      setSnapshots([]);
      setSelectedSnapshot(null);
      setHoldings([]);
    }
  }, [selectedMutualFund]);

  // Load holdings when snapshot is selected
  useEffect(() => {
    if (selectedSnapshot && selectedMutualFund) {
      loadHoldings();
    } else {
      setHoldings([]);
    }
  }, [selectedSnapshot, selectedMutualFund, tableParams.pagination?.current, tableParams.pagination?.pageSize, tableParams.sortField, tableParams.sortOrder, searchTerm, sectorFilter, industryFilter]);

  const loadMutualFunds = async () => {
    setLoadingMutualFunds(true);
    try {
      // Use dropdown API which returns MutualFundDto[] directly
      const funds = await mutualFundService.getMutualFundsForDropdown();
      setMutualFunds(funds);
    } catch (error) {
      console.error('Error loading mutual funds:', error);
      message.error('Error loading mutual funds');
    } finally {
      setLoadingMutualFunds(false);
    }
  };

  const loadSnapshots = async () => {
    if (!selectedMutualFund) return;
    
    setLoadingSnapshots(true);
    try {
      const response = await snapshotManagementService.getSnapshots(selectedMutualFund);
      if (response.success) {
        setSnapshots(response.snapshots);
        setSelectedMutualFundName(response.mutualFundName);
      } else {
        message.error(response.message || 'Failed to load snapshots');
        setSnapshots([]);
      }
    } catch (error) {
      console.error('Error loading snapshots:', error);
      message.error('Error loading snapshots');
      setSnapshots([]);
    } finally {
      setLoadingSnapshots(false);
    }
  };

  const loadHoldings = async () => {
    if (!selectedSnapshot || !selectedMutualFund) return;

    setHoldingsLoading(true);
    try {
      const response = await snapshotManagementService.getSnapshotHoldings({
        mutualFundId: selectedMutualFund,
        snapshotDate: selectedSnapshot.date,
        page: tableParams.pagination?.current || 1,
        pageSize: tableParams.pagination?.pageSize || 20,
        sortBy: tableParams.sortField,
        sortDirection: tableParams.sortOrder === 'ascend' ? 'asc' : 'desc',
        searchTerm: searchTerm || undefined,
        sectorFilter: sectorFilter || undefined,
        industryFilter: industryFilter || undefined,
      });

      if (response.success) {
        setHoldings(response.holdings.data);
        setTableParams({
          ...tableParams,
          pagination: {
            ...tableParams.pagination,
            total: response.holdings.totalCount,
            current: response.holdings.page,
            pageSize: response.holdings.pageSize,
          },
        });
      } else {
        message.error(response.message || 'Failed to load holdings');
        setHoldings([]);
      }
    } catch (error) {
      console.error('Error loading holdings:', error);
      message.error('Error loading holdings');
      setHoldings([]);
    } finally {
      setHoldingsLoading(false);
    }
  };

  const handleDeleteSnapshot = async () => {
    if (!selectedSnapshot || !selectedMutualFund) return;

    setDeletingSnapshot(true);
    try {
      const response = await snapshotManagementService.deleteSnapshot({
        mutualFundId: selectedMutualFund,
        snapshotDate: selectedSnapshot.date,
      });

      if (response.success) {
        message.success(`Snapshot deleted successfully. ${response.deletedRecordsCount} records removed.`);
        setDeleteModalVisible(false);
        setSelectedSnapshot(null);
        setHoldings([]);
        // Reload snapshots to refresh the list
        await loadSnapshots();
      } else {
        message.error(response.message || 'Failed to delete snapshot');
      }
    } catch (error) {
      console.error('Error deleting snapshot:', error);
      message.error('Error deleting snapshot');
    } finally {
      setDeletingSnapshot(false);
    }
  };

  const handleTableChange = (
    pagination: TablePaginationConfig,
    filters: any,
    sorter: any,
  ) => {
    setTableParams({
      pagination,
      filters,
      sortField: sorter.field,
      sortOrder: sorter.order,
    });
  };

  const handleSearch = (value: string) => {
    setSearchTerm(value);
    setTableParams({
      ...tableParams,
      pagination: {
        ...tableParams.pagination,
        current: 1, // Reset to first page when searching
      },
    });
  };

  const clearFilters = () => {
    setSearchTerm('');
    setSectorFilter('');
    setIndustryFilter('');
    setTableParams({
      ...tableParams,
      pagination: {
        ...tableParams.pagination,
        current: 1,
      },
    });
  };

  const holdingsColumns: ColumnsType<CurrentHoldingDto> = [
    {
      title: '#',
      key: 'serial',
      width: 48,
      align: 'center',
      render: (_: any, __: CurrentHoldingDto, index: number) => {
        const current = tableParams.pagination?.current || 1;
        const pageSize = tableParams.pagination?.pageSize || 20;
        return ((current - 1) * pageSize) + index + 1;
      },
    },
    {
      title: 'Company Name',
      dataIndex: 'companyName',
      key: 'companyName',
      sorter: true,
      width: 220,
      ellipsis: true,
      render: (text: string) => (
        <Tooltip title={text}>
          <Text>{text}</Text>
        </Tooltip>
      ),
    },
    {
      title: 'Sector',
      dataIndex: 'sector',
      key: 'sector',
      sorter: true,
      width: 150,
      render: (text: string) => text ? <Tag color="blue">{text}</Tag> : '-',
    },
    {
      title: 'Industry',
      dataIndex: 'industry',
      key: 'industry',
      sorter: true,
      width: 150,
      render: (text: string) => text ? <Tag color="green">{text}</Tag> : '-',
    },
    {
      title: 'Quantity',
      dataIndex: 'quantity',
      key: 'quantity',
      sorter: true,
      width: 120,
      align: 'right',
      render: (value: number) => value.toLocaleString(),
    },
    {
      title: 'Market Value',
      dataIndex: 'value',
      key: 'value',
      sorter: true,
      width: 150,
      align: 'right',
      render: (value: number) => `₹${value.toLocaleString()}`,
    },
    {
      title: 'Weight (%)',
      dataIndex: 'weight',
      key: 'weight',
      sorter: true,
      width: 120,
      align: 'right',
      render: (value: number) => `${value.toFixed(2)}%`,
    },
  ];

  return (
    <div style={{ padding: '24px' }}>
      <Title level={2}>Snapshot Management</Title>
      <Text type="secondary">
        Select a mutual fund and snapshot date to view portfolio holdings. You can also delete snapshots and all related data.
      </Text>

      <Row gutter={[16, 16]} style={{ marginTop: '24px' }}>
        {/* Mutual Fund Selection */}
        <Col xs={24} md={12}>
          <Card title="Select Mutual Fund" size="small">
            <Select
              style={{ width: '100%' }}
              placeholder="Search and select mutual fund"
              showSearch
              loading={loadingMutualFunds}
              value={selectedMutualFund}
              onChange={(value, option: any) => {
                setSelectedMutualFund(value);
                setSelectedSnapshot(null);
                setHoldings([]);
              }}
              filterOption={(input, option) =>
                (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
              }
              options={mutualFunds.map(fund => ({
                value: fund.id,
                label: `${fund.fundName} (${fund.fundCode})`,
              }))}
            />
          </Card>
        </Col>

        {/* Snapshot Selection */}
        <Col xs={24} md={12}>
          <Card title="Select Snapshot Date" size="small">
            <Select
              style={{ width: '100%' }}
              placeholder="Select snapshot date"
              loading={loadingSnapshots}
              value={selectedSnapshot?.id}
              onChange={(value) => {
                const snapshot = snapshots.find(s => s.id === value);
                setSelectedSnapshot(snapshot || null);
              }}
              disabled={!selectedMutualFund}
              options={snapshots.map(snapshot => ({
                value: snapshot.id,
                label: `${dayjs(snapshot.date).format('MMM DD, YYYY')} - ₹${snapshot.totalValue.toLocaleString()} (${snapshot.holdingsCount} holdings)`,
              }))}
            />
          </Card>
        </Col>
      </Row>

      {/* Snapshot Summary */}
      {selectedSnapshot && (
        <Card 
          title={`Portfolio Holdings - ${selectedMutualFundName}`}
          style={{ marginTop: '16px' }}
          extra={
            <Space>
              <Button
                icon={<ReloadOutlined />}
                onClick={loadHoldings}
                loading={holdingsLoading}
              >
                Refresh
              </Button>
              <Popconfirm
                title="Delete Snapshot"
                description="Are you sure you want to delete this snapshot? This will permanently remove all portfolio holdings, files, and related holding changes."
                onConfirm={() => setDeleteModalVisible(true)}
                okText="Yes"
                cancelText="No"
                icon={<ExclamationCircleOutlined style={{ color: 'red' }} />}
              >
                <Button
                  danger
                  icon={<DeleteOutlined />}
                  loading={deletingSnapshot}
                >
                  Delete Snapshot
                </Button>
              </Popconfirm>
            </Space>
          }
        >
          <Row gutter={16} style={{ marginBottom: '16px' }}>
            <Col span={6}>
              <Statistic
                title="Snapshot Date"
                value={dayjs(selectedSnapshot.date).format('MMM DD, YYYY')}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Total Value"
                value={selectedSnapshot.totalValue}
                prefix="₹"
                precision={0}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Total Holdings"
                value={selectedSnapshot.holdingsCount}
              />
            </Col>
            <Col span={6}>
              <Statistic
                title="Created"
                value={dayjs(selectedSnapshot.createdAt).format('MMM DD, YYYY')}
              />
            </Col>
          </Row>

          {/* Search and Filters */}
          <Row gutter={[16, 16]} style={{ marginBottom: '16px' }}>
            <Col xs={24} sm={8}>
              <Search
                placeholder="Search by symbol, company, sector, or industry"
                allowClear
                enterButton={<SearchOutlined />}
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                onSearch={handleSearch}
              />
            </Col>
            <Col xs={12} sm={4}>
              <Input
                placeholder="Filter by sector"
                allowClear
                value={sectorFilter}
                onChange={(e) => setSectorFilter(e.target.value)}
              />
            </Col>
            <Col xs={12} sm={4}>
              <Input
                placeholder="Filter by industry"
                allowClear
                value={industryFilter}
                onChange={(e) => setIndustryFilter(e.target.value)}
              />
            </Col>
            <Col xs={24} sm={8}>
              <Space>
                <Button onClick={clearFilters}>Clear Filters</Button>
                <Text type="secondary">
                  Showing {holdings.length} of {tableParams.pagination?.total || 0} holdings
                </Text>
              </Space>
            </Col>
          </Row>

          {/* Holdings Table */}
          <Table
            columns={holdingsColumns}
            dataSource={holdings}
            rowKey="id"
            pagination={tableParams.pagination}
            loading={holdingsLoading}
            onChange={handleTableChange}
            scroll={{ x: 1000 }}
            size="small"
          />
        </Card>
      )}

      {/* Delete Confirmation Modal */}
      <Modal
        title="Confirm Snapshot Deletion"
        open={deleteModalVisible}
        onOk={handleDeleteSnapshot}
        onCancel={() => setDeleteModalVisible(false)}
        confirmLoading={deletingSnapshot}
        okText="Delete"
        okType="danger"
        cancelText="Cancel"
      >
        <div>
          <p><strong>Warning:</strong> This action cannot be undone!</p>
          <p>You are about to permanently delete the following:</p>
          <ul>
            <li>Portfolio snapshot for {dayjs(selectedSnapshot?.date).format('MMM DD, YYYY')}</li>
            <li>All portfolio holdings ({selectedSnapshot?.holdingsCount} records)</li>
            <li>Associated portfolio files</li>
            <li>All related holding changes</li>
          </ul>
          <p>Are you sure you want to proceed?</p>
        </div>
      </Modal>
    </div>
  );
};

export default SnapshotManagementPage;
