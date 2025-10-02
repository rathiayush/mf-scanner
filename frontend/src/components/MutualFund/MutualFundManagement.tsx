import React, { useState, useEffect } from 'react';
import {
  Card,
  Table,
  Button,
  Space,
  Typography,
  Modal,
  Form,
  Input,
  InputNumber,
  Switch,
  Select,
  message,
  Popconfirm,
  Tag,
  Row,
  Col,
  Statistic,
  Checkbox,
  Alert
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  SearchOutlined,
  FundOutlined,
  WarningOutlined
} from '@ant-design/icons';
import { mutualFundService } from '../../services/api';
import {
  MutualFundDto,
  CreateMutualFundDto,
  UpdateMutualFundDto,
  MutualFundSearchDto,
  PagedResult,
  DeleteMutualFundsRequestDto,
  DeleteMutualFundsResponseDto
} from '../../types';
import dayjs from 'dayjs';

const { Title, Text } = Typography;
const { Option } = Select;

interface MutualFundFormData {
  fundName: string;
  fundCode?: string;
  amc?: string;
  category?: string;
  subCategory?: string;
  launchDate?: string;
  nav?: number;
  aum?: number;
  expenseRatio?: number;
  status?: string;
  isActive?: boolean;
}

const MutualFundManagement: React.FC = () => {
  const [funds, setFunds] = useState<PagedResult<MutualFundDto>>({
    data: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false
  });
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingFund, setEditingFund] = useState<MutualFundDto | null>(null);
  const [searchParams, setSearchParams] = useState<MutualFundSearchDto>({
    page: 1,
    pageSize: 20
  });
  const [form] = Form.useForm<MutualFundFormData>();
  
  // Hard delete states
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
  const [hardDeleteModalVisible, setHardDeleteModalVisible] = useState(false);
  const [confirmDeletion, setConfirmDeletion] = useState(false);
  const [deleteReason, setDeleteReason] = useState('');
  const [hardDeleteLoading, setHardDeleteLoading] = useState(false);

  useEffect(() => {
    loadFunds();
  }, [searchParams]);

  const loadFunds = async () => {
    setLoading(true);
    try {
      const result = await mutualFundService.searchMutualFunds(searchParams);
      setFunds(result);
    } catch (error: any) {
      message.error(error.message || 'Failed to load mutual funds');
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (values: any) => {
    setSearchParams({
      ...searchParams,
      ...values,
      page: 1
    });
  };

  const handleTableChange = (page: number, pageSize: number) => {
    setSearchParams({
      ...searchParams,
      page,
      pageSize
    });
  };

  const showModal = (fund?: MutualFundDto) => {
    setEditingFund(fund || null);
    setModalVisible(true);
    
    if (fund) {
      form.setFieldsValue({
        fundName: fund.fundName,
        fundCode: fund.fundCode,
        amc: fund.amc,
        category: fund.category,
        subCategory: fund.subCategory,
        launchDate: fund.launchDate,
        nav: fund.nav,
        aum: fund.aum,
        expenseRatio: fund.expenseRatio,
        status: fund.status,
        isActive: fund.isActive
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ isActive: true, status: 'Active' });
    }
  };

  const handleModalOk = async () => {
    try {
      const values = await form.validateFields();
      
      if (editingFund) {
        // Update existing fund
        const updateData: UpdateMutualFundDto = {
          id: editingFund.id,
          fundName: values.fundName,
          fundCode: values.fundCode,
          amc: values.amc,
          category: values.category,
          subCategory: values.subCategory,
          launchDate: values.launchDate,
          nav: values.nav,
          aum: values.aum,
          expenseRatio: values.expenseRatio,
          status: values.status || 'Active',
          isActive: values.isActive !== false
        };
        
        await mutualFundService.updateMutualFund(editingFund.id, updateData);
        message.success('Mutual fund updated successfully');
      } else {
        // Create new fund
        const createData: CreateMutualFundDto = {
          fundName: values.fundName,
          fundCode: values.fundCode,
          amc: values.amc,
          category: values.category,
          subCategory: values.subCategory,
          launchDate: values.launchDate,
          nav: values.nav,
          aum: values.aum,
          expenseRatio: values.expenseRatio
        };
        
        await mutualFundService.createMutualFund(createData);
        message.success('Mutual fund created successfully');
      }
      
      setModalVisible(false);
      setEditingFund(null);
      form.resetFields();
      loadFunds();
    } catch (error: any) {
      message.error(error.message || 'Failed to save mutual fund');
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await mutualFundService.deleteMutualFund(id);
      message.success('Mutual fund deleted successfully');
      loadFunds();
    } catch (error: any) {
      message.error(error.message || 'Failed to delete mutual fund');
    }
  };

  // Hard delete functionality
  const handleHardDelete = async () => {
    if (!confirmDeletion) {
      message.error('Please confirm the deletion to proceed');
      return;
    }

    setHardDeleteLoading(true);
    try {
      const request: DeleteMutualFundsRequestDto = {
        mutualFundIds: selectedRowKeys.map(key => Number(key)),
        confirmDeletion: true,
        reason: deleteReason
      };

      const response: DeleteMutualFundsResponseDto = await mutualFundService.hardDeleteMutualFunds(request);
      
      if (response.success) {
        message.success(`Successfully deleted ${response.deletedFunds.length} mutual funds`);
        setSelectedRowKeys([]);
        setHardDeleteModalVisible(false);
        setConfirmDeletion(false);
        setDeleteReason('');
        loadFunds();
      } else {
        message.error(response.message || 'Failed to delete mutual funds');
      }
    } catch (error: any) {
      message.error(error.message || 'Failed to delete mutual funds');
    } finally {
      setHardDeleteLoading(false);
    }
  };

  const showHardDeleteModal = () => {
    if (selectedRowKeys.length === 0) {
      message.warning('Please select at least one mutual fund to delete');
      return;
    }
    setHardDeleteModalVisible(true);
  };

  const handleHardDeleteModalCancel = () => {
    setHardDeleteModalVisible(false);
    setConfirmDeletion(false);
    setDeleteReason('');
  };

  const rowSelection = {
    selectedRowKeys,
    onChange: (newSelectedRowKeys: React.Key[]) => {
      setSelectedRowKeys(newSelectedRowKeys);
    },
    getCheckboxProps: (record: MutualFundDto) => ({
      disabled: false,
      name: record.fundName,
    }),
  };

  const columns = [
    {
      title: 'Fund Name',
      dataIndex: 'fundName',
      key: 'fundName',
      width: 200,
      render: (text: string, record: MutualFundDto) => (
        <Space direction="vertical" size="small">
          <Text strong>{text}</Text>
          <Text type="secondary" style={{ fontSize: '12px' }}>{record.fundCode}</Text>
        </Space>
      ),
    },
    {
      title: 'AMC',
      dataIndex: 'amc',
      key: 'amc',
      width: 150,
    },
    {
      title: 'Category',
      dataIndex: 'category',
      key: 'category',
      width: 120,
    },
    {
      title: 'NAV',
      dataIndex: 'nav',
      key: 'nav',
      width: 100,
      render: (nav: number) => nav ? `₹${nav.toFixed(2)}` : '-',
    },
    {
      title: 'AUM (Cr)',
      dataIndex: 'aum',
      key: 'aum',
      width: 100,
      render: (aum: number) => aum ? `₹${aum.toFixed(0)}` : '-',
    },
    {
      title: 'Expense Ratio',
      dataIndex: 'expenseRatio',
      key: 'expenseRatio',
      width: 100,
      render: (ratio: number) => ratio ? `${ratio.toFixed(2)}%` : '-',
    },
    {
      title: 'Portfolios',
      dataIndex: 'portfolioCount',
      key: 'portfolioCount',
      width: 80,
      render: (count: number) => <Tag color="blue">{count}</Tag>,
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 80,
      render: (isActive: boolean) => (
        <Tag color={isActive ? 'green' : 'red'}>
          {isActive ? 'Active' : 'Inactive'}
        </Tag>
      ),
    },
    {
      title: 'Last Updated',
      dataIndex: 'lastUpdated',
      key: 'lastUpdated',
      width: 120,
      render: (date: string) => date ? dayjs(date).format('DD/MM/YYYY') : '-',
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (record: MutualFundDto) => (
        <Space>
          <Button
            type="text"
            icon={<EditOutlined />}
            onClick={() => showModal(record)}
          />
          <Popconfirm
            title="Are you sure you want to delete this mutual fund?"
            onConfirm={() => handleDelete(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Button
              type="text"
              danger
              icon={<DeleteOutlined />}
            />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <Title level={2}>Mutual Fund Management</Title>
      <Text type="secondary">
        Manage mutual fund information and portfolio data.
      </Text>

      <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
        <Col span={24}>
          <Card title="Summary Statistics">
            <Row gutter={[16, 16]}>
              <Col span={6}>
                <Statistic
                  title="Total Funds"
                  value={funds.totalCount}
                  prefix={<FundOutlined />}
                />
              </Col>
              <Col span={6}>
                <Statistic
                  title="Active Funds"
                  value={funds.data.filter(f => f.isActive).length}
                  valueStyle={{ color: '#3f8600' }}
                />
              </Col>
              <Col span={6}>
                <Statistic
                  title="Total Portfolios"
                  value={funds.data.reduce((sum, f) => sum + f.portfolioCount, 0)}
                />
              </Col>
              <Col span={6}>
                <Statistic
                  title="Current Page"
                  value={`${funds.page} / ${funds.totalPages}`}
                />
              </Col>
            </Row>
          </Card>
        </Col>

        <Col span={24}>
          <Card 
            title="Search & Filter"
            extra={
              <Space>
                <Button
                  danger
                  icon={<DeleteOutlined />}
                  onClick={showHardDeleteModal}
                  disabled={selectedRowKeys.length === 0}
                >
                  Hard Delete ({selectedRowKeys.length})
                </Button>
                <Button
                  type="primary"
                  icon={<PlusOutlined />}
                  onClick={() => showModal()}
                >
                  Add Mutual Fund
                </Button>
              </Space>
            }
          >
            <Form
              layout="inline"
              onFinish={handleSearch}
              style={{ marginBottom: 16 }}
            >
              <Form.Item name="searchTerm">
                <Input
                  placeholder="Search by name, code, or AMC"
                  prefix={<SearchOutlined />}
                  style={{ width: 250 }}
                />
              </Form.Item>
              <Form.Item name="category">
                <Select
                  placeholder="Category"
                  style={{ width: 150 }}
                  allowClear
                >
                  <Option value="Equity">Equity</Option>
                  <Option value="Debt">Debt</Option>
                  <Option value="Hybrid">Hybrid</Option>
                  <Option value="Solution Oriented">Solution Oriented</Option>
                </Select>
              </Form.Item>
              <Form.Item name="amc">
                <Input
                  placeholder="AMC"
                  style={{ width: 150 }}
                />
              </Form.Item>
              <Form.Item name="isActive">
                <Select
                  placeholder="Status"
                  style={{ width: 120 }}
                  allowClear
                >
                  <Option value={true}>Active</Option>
                  <Option value={false}>Inactive</Option>
                </Select>
              </Form.Item>
              <Form.Item>
                <Button type="primary" htmlType="submit">
                  Search
                </Button>
              </Form.Item>
            </Form>
          </Card>
        </Col>

        <Col span={24}>
          <Card title="Mutual Funds">
            <Table
              columns={columns}
              dataSource={funds.data}
              rowKey="id"
              rowSelection={rowSelection}
              loading={loading}
              pagination={{
                current: funds.page,
                pageSize: funds.pageSize,
                total: funds.totalCount,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} items`,
                onChange: handleTableChange,
              }}
              scroll={{ x: 1200 }}
            />
          </Card>
        </Col>
      </Row>

      {/* Add/Edit Modal */}
      <Modal
        title={editingFund ? 'Edit Mutual Fund' : 'Add Mutual Fund'}
        open={modalVisible}
        onOk={handleModalOk}
        onCancel={() => {
          setModalVisible(false);
          setEditingFund(null);
          form.resetFields();
        }}
        width={800}
        confirmLoading={loading}
      >
        <Form form={form} layout="vertical">
          <Form.Item
            name="fundName"
            label="Fund Name"
            rules={[{ required: true, message: 'Fund name is required' }]}
          >
            <Input placeholder="Enter fund name" />
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="fundCode" label="Fund Code">
                <Input placeholder="Enter fund code" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="amc" label="AMC">
                <Input placeholder="Enter AMC name" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="category" label="Category">
                <Select placeholder="Select category">
                  <Option value="Equity">Equity</Option>
                  <Option value="Debt">Debt</Option>
                  <Option value="Hybrid">Hybrid</Option>
                  <Option value="Solution Oriented">Solution Oriented</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="subCategory" label="Sub Category">
                <Input placeholder="Enter sub category" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="nav" label="NAV">
                <InputNumber
                  placeholder="Enter NAV"
                  style={{ width: '100%' }}
                  min={0}
                  precision={2}
                />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="aum" label="AUM (Cr)">
                <InputNumber
                  placeholder="Enter AUM"
                  style={{ width: '100%' }}
                  min={0}
                  precision={2}
                />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="expenseRatio" label="Expense Ratio (%)">
                <InputNumber
                  placeholder="Enter expense ratio"
                  style={{ width: '100%' }}
                  min={0}
                  max={5}
                  precision={2}
                />
              </Form.Item>
            </Col>
          </Row>

          {editingFund && (
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item name="status" label="Status">
                  <Select>
                    <Option value="Active">Active</Option>
                    <Option value="Closed">Closed</Option>
                    <Option value="Suspended">Suspended</Option>
                  </Select>
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="isActive" label="Is Active" valuePropName="checked">
                  <Switch />
                </Form.Item>
              </Col>
            </Row>
          )}
        </Form>
      </Modal>

      {/* Hard Delete Confirmation Modal */}
      <Modal
        title="Hard Delete Mutual Funds"
        open={hardDeleteModalVisible}
        onOk={handleHardDelete}
        onCancel={handleHardDeleteModalCancel}
        width={600}
        confirmLoading={hardDeleteLoading}
        okButtonProps={{
          danger: true,
          disabled: !confirmDeletion,
        }}
        okText="Delete Permanently"
      >
        <Space direction="vertical" size="large" style={{ width: '100%' }}>
          <Alert
            message="Warning: Permanent Deletion"
            description="This action will permanently delete the selected mutual funds and all associated data including portfolio files, snapshots, holdings, and holding changes. This action cannot be undone."
            type="warning"
            showIcon
            icon={<WarningOutlined />}
          />
          
          <div>
            <Text strong>You are about to delete {selectedRowKeys.length} mutual fund(s):</Text>
            <div style={{ marginTop: 8, maxHeight: 120, overflowY: 'auto', border: '1px solid #d9d9d9', borderRadius: 4, padding: 8 }}>
              {funds.data
                .filter(fund => selectedRowKeys.includes(fund.id))
                .map(fund => (
                  <div key={fund.id} style={{ marginBottom: 4 }}>
                    <Text>{fund.fundName} ({fund.fundCode})</Text>
                  </div>
                ))}
            </div>
          </div>
          
          <Form layout="vertical">
            <Form.Item label="Reason for deletion (optional):">
              <Input.TextArea
                value={deleteReason}
                onChange={(e) => setDeleteReason(e.target.value)}
                placeholder="Please provide a reason for this deletion..."
                rows={3}
              />
            </Form.Item>
            
            <Form.Item>
              <Checkbox
                checked={confirmDeletion}
                onChange={(e) => setConfirmDeletion(e.target.checked)}
              >
                <Text strong>I understand that this action is permanent and cannot be undone.</Text>
              </Checkbox>
            </Form.Item>
          </Form>
        </Space>
      </Modal>
    </div>
  );
};

export default MutualFundManagement;
