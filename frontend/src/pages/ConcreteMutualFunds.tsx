import React, { useState, useEffect } from 'react';
import { 
  Table, 
  Button, 
  Input, 
  Select, 
  Space, 
  Modal, 
  Form, 
  message, 
  Popconfirm, 
  Card, 
  Row, 
  Col,
  Tag,
  Tooltip,
  Switch
} from 'antd';
import { 
  PlusOutlined, 
  EditOutlined, 
  DeleteOutlined, 
  SearchOutlined,
  ReloadOutlined,
  UploadOutlined
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { concreteMutualFundService } from '../services/concreteMutualFundService';

const { Option } = Select;
const { TextArea } = Input;

interface ConcreteMutualFund {
  id: number;
  fundName: string;
  amcName?: string;
  category?: string;
  subCategory?: string;
  isActive: boolean;
  matchPriority: number;
  notes?: string;
  createdDate: string;
  lastUpdated?: string;
}

interface ConcreteMutualFundForm {
  fundName: string;
  amcName?: string;
  category?: string;
  subCategory?: string;
  isActive: boolean;
  matchPriority: number;
  notes?: string;
}

interface SearchFilters {
  searchTerm?: string;
  amcName?: string;
  category?: string;
  isActive?: boolean;
  page: number;
  pageSize: number;
  sortBy: string;
  sortDescending: boolean;
}

const ConcreteMutualFunds: React.FC = () => {
  const [funds, setFunds] = useState<ConcreteMutualFund[]>([]);
  const [loading, setLoading] = useState(false);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(50);
  const [isModalVisible, setIsModalVisible] = useState(false);
  const [editingFund, setEditingFund] = useState<ConcreteMutualFund | null>(null);
  const [amcNames, setAmcNames] = useState<string[]>([]);
  const [categories, setCategories] = useState<string[]>([]);
  const [searchFilters, setSearchFilters] = useState<SearchFilters>({
    page: 1,
    pageSize: 50,
    sortBy: 'fundName',
    sortDescending: false
  });

  const [form] = Form.useForm();

  useEffect(() => {
    fetchFunds();
    fetchFilterOptions();
  }, [searchFilters]);

  const fetchFunds = async () => {
    setLoading(true);
    try {
      const response = await concreteMutualFundService.search(searchFilters);
      setFunds(response.items);
      setTotalCount(response.totalCount);
    } catch (error) {
      message.error('Failed to fetch mutual funds');
      console.error('Error fetching funds:', error);
    } finally {
      setLoading(false);
    }
  };

  const fetchFilterOptions = async () => {
    try {
      const [amcResponse, categoryResponse] = await Promise.all([
        concreteMutualFundService.getDistinctAMCNames(),
        concreteMutualFundService.getDistinctCategories()
      ]);
      setAmcNames(amcResponse);
      setCategories(categoryResponse);
    } catch (error) {
      console.error('Error fetching filter options:', error);
    }
  };

  const handleSearch = (filters: Partial<SearchFilters>) => {
    setSearchFilters(prev => ({
      ...prev,
      ...filters,
      page: 1
    }));
    setCurrentPage(1);
  };

  const handleTableChange = (pagination: any, filters: any, sorter: any) => {
    const newFilters: Partial<SearchFilters> = {
      page: pagination.current,
      pageSize: pagination.pageSize
    };

    if (sorter.field) {
      newFilters.sortBy = sorter.field;
      newFilters.sortDescending = sorter.order === 'descend';
    }

    setSearchFilters(prev => ({ ...prev, ...newFilters }));
    setCurrentPage(pagination.current);
    setPageSize(pagination.pageSize);
  };

  const showModal = (fund?: ConcreteMutualFund) => {
    setEditingFund(fund || null);
    if (fund) {
      form.setFieldsValue({
        fundName: fund.fundName,
        amcName: fund.amcName,
        category: fund.category,
        subCategory: fund.subCategory,
        isActive: fund.isActive,
        matchPriority: fund.matchPriority,
        notes: fund.notes
      });
    } else {
      form.resetFields();
      form.setFieldsValue({
        isActive: true,
        matchPriority: 100
      });
    }
    setIsModalVisible(true);
  };

  const handleOk = async () => {
    try {
      const values = await form.validateFields();
      const formData: ConcreteMutualFundForm = {
        fundName: values.fundName.trim(),
        amcName: values.amcName?.trim(),
        category: values.category?.trim(),
        subCategory: values.subCategory?.trim(),
        isActive: values.isActive,
        matchPriority: values.matchPriority,
        notes: values.notes?.trim()
      };

      if (editingFund) {
        await concreteMutualFundService.update(editingFund.id, formData);
        message.success('Mutual fund updated successfully');
      } else {
        await concreteMutualFundService.create(formData);
        message.success('Mutual fund created successfully');
      }

      setIsModalVisible(false);
      form.resetFields();
      setEditingFund(null);
      fetchFunds();
      fetchFilterOptions();
    } catch (error: any) {
      message.error(error.response?.data || 'Operation failed');
      console.error('Error saving fund:', error);
    }
  };

  const handleCancel = () => {
    setIsModalVisible(false);
    form.resetFields();
    setEditingFund(null);
  };

  const handleDelete = async (id: number, fundName: string) => {
    try {
      await concreteMutualFundService.delete(id);
      message.success(`Deleted ${fundName} successfully`);
      fetchFunds();
    } catch (error: any) {
      message.error(error.response?.data || 'Delete failed');
      console.error('Error deleting fund:', error);
    }
  };

  const handleBulkSeed = async () => {
    try {
      setLoading(true);
      // This would call a seeding endpoint if implemented
      message.info('Bulk seeding functionality would be implemented here');
    } catch (error) {
      message.error('Bulk seeding failed');
    } finally {
      setLoading(false);
    }
  };

  const columns: ColumnsType<ConcreteMutualFund> = [
    {
      title: 'Fund Name',
      dataIndex: 'fundName',
      key: 'fundName',
      sorter: true,
      width: '30%',
      ellipsis: {
        showTitle: false,
      },
      render: (text) => (
        <Tooltip title={text}>
          <span>{text}</span>
        </Tooltip>
      ),
    },
    {
      title: 'AMC',
      dataIndex: 'amcName',
      key: 'amcName',
      sorter: true,
      width: '15%',
      render: (text) => text || '-',
    },
    {
      title: 'Category',
      dataIndex: 'category',
      key: 'category',
      sorter: true,
      width: '12%',
      render: (text) => text || '-',
    },
    {
      title: 'Sub Category',
      dataIndex: 'subCategory',
      key: 'subCategory',
      width: '12%',
      render: (text) => text || '-',
    },
    {
      title: 'Priority',
      dataIndex: 'matchPriority',
      key: 'matchPriority',
      sorter: true,
      width: '8%',
      align: 'center',
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      width: '8%',
      align: 'center',
      render: (isActive) => (
        <Tag color={isActive ? 'green' : 'red'}>
          {isActive ? 'Active' : 'Inactive'}
        </Tag>
      ),
    },
    {
      title: 'Created',
      dataIndex: 'createdDate',
      key: 'createdDate',
      sorter: true,
      width: '10%',
      render: (date) => new Date(date).toLocaleDateString(),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: '10%',
      align: 'center',
      render: (_, record) => (
        <Space>
          <Button
            type="text"
            icon={<EditOutlined />}
            onClick={() => showModal(record)}
            size="small"
          />
          <Popconfirm
            title="Are you sure you want to delete this mutual fund?"
            description={`This will permanently delete "${record.fundName}"`}
            onConfirm={() => handleDelete(record.id, record.fundName)}
            okText="Yes"
            cancelText="No"
          >
            <Button
              type="text"
              danger
              icon={<DeleteOutlined />}
              size="small"
            />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: '24px' }}>
      <Card>
        <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
          <Col>
            <h2>Concrete Mutual Funds Management</h2>
          </Col>
          <Col>
            <Space>
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => showModal()}
              >
                Add Fund
              </Button>
              <Button
                icon={<UploadOutlined />}
                onClick={handleBulkSeed}
                loading={loading}
              >
                Bulk Seed
              </Button>
              <Button
                icon={<ReloadOutlined />}
                onClick={fetchFunds}
                loading={loading}
              >
                Refresh
              </Button>
            </Space>
          </Col>
        </Row>

        {/* Search Filters */}
        <Row gutter={16} style={{ marginBottom: 16 }}>
          <Col span={8}>
            <Input
              placeholder="Search fund names..."
              prefix={<SearchOutlined />}
              value={searchFilters.searchTerm}
              onChange={(e) => handleSearch({ searchTerm: e.target.value })}
              allowClear
            />
          </Col>
          <Col span={4}>
            <Select
              placeholder="AMC"
              value={searchFilters.amcName}
              onChange={(value) => handleSearch({ amcName: value })}
              allowClear
              style={{ width: '100%' }}
            >
              {amcNames.map(amc => (
                <Option key={amc} value={amc}>{amc}</Option>
              ))}
            </Select>
          </Col>
          <Col span={4}>
            <Select
              placeholder="Category"
              value={searchFilters.category}
              onChange={(value) => handleSearch({ category: value })}
              allowClear
              style={{ width: '100%' }}
            >
              {categories.map(category => (
                <Option key={category} value={category}>{category}</Option>
              ))}
            </Select>
          </Col>
          <Col span={4}>
            <Select
              placeholder="Status"
              value={searchFilters.isActive}
              onChange={(value) => handleSearch({ isActive: value })}
              allowClear
              style={{ width: '100%' }}
            >
              <Option value={true}>Active</Option>
              <Option value={false}>Inactive</Option>
            </Select>
          </Col>
        </Row>

        <Table
          columns={columns}
          dataSource={funds}
          rowKey="id"
          loading={loading}
          pagination={{
            current: currentPage,
            pageSize: pageSize,
            total: totalCount,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) =>
              `${range[0]}-${range[1]} of ${total} items`,
          }}
          onChange={handleTableChange}
          scroll={{ x: 1200 }}
        />
      </Card>

      <Modal
        title={editingFund ? 'Edit Mutual Fund' : 'Add New Mutual Fund'}
        open={isModalVisible}
        onOk={handleOk}
        onCancel={handleCancel}
        width={600}
        destroyOnClose
      >
        <Form
          form={form}
          layout="vertical"
          initialValues={{
            isActive: true,
            matchPriority: 100
          }}
        >
          <Form.Item
            name="fundName"
            label="Fund Name"
            rules={[
              { required: true, message: 'Please enter fund name' },
              { max: 500, message: 'Fund name cannot exceed 500 characters' }
            ]}
          >
            <Input placeholder="Enter fund name" />
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="amcName"
                label="AMC Name"
                rules={[
                  { max: 100, message: 'AMC name cannot exceed 100 characters' }
                ]}
              >
                <Input placeholder="Enter AMC name" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="matchPriority"
                label="Match Priority"
                rules={[
                  { required: true, message: 'Please enter match priority' },
                  { type: 'number', min: 1, max: 1000, message: 'Priority must be between 1 and 1000' }
                ]}
              >
                <Input type="number" placeholder="100" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="category"
                label="Category"
                rules={[
                  { max: 100, message: 'Category cannot exceed 100 characters' }
                ]}
              >
                <Input placeholder="e.g., Equity, Hybrid, Debt" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="subCategory"
                label="Sub Category"
                rules={[
                  { max: 100, message: 'Sub category cannot exceed 100 characters' }
                ]}
              >
                <Input placeholder="e.g., Large Cap, Mid Cap" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="isActive"
            label="Status"
            valuePropName="checked"
          >
            <Switch checkedChildren="Active" unCheckedChildren="Inactive" />
          </Form.Item>

          <Form.Item
            name="notes"
            label="Notes"
            rules={[
              { max: 1000, message: 'Notes cannot exceed 1000 characters' }
            ]}
          >
            <TextArea 
              rows={3} 
              placeholder="Optional notes about this mutual fund"
              showCount
              maxLength={1000}
            />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default ConcreteMutualFunds;
