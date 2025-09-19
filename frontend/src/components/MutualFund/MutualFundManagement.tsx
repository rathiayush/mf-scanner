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
  DatePicker,
  InputNumber,
  Switch,
  Select,
  message,
  Popconfirm,
  Tag,
  Row,
  Col,
  Statistic
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  SearchOutlined,
  FundOutlined
} from '@ant-design/icons';
import { mutualFundService } from '../../services/api';
import {
  MutualFundDto,
  CreateMutualFundDto,
  UpdateMutualFundDto,
  MutualFundSearchDto,
  PagedResult
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
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => showModal()}
              >
                Add Mutual Fund
              </Button>
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
          <Card>
            <Table
              dataSource={funds.data}
              columns={columns}
              rowKey="id"
              loading={loading}
              pagination={{
                current: funds.page,
                pageSize: funds.pageSize,
                total: funds.totalCount,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total, range) =>
                  `${range[0]}-${range[1]} of ${total} funds`,
                onChange: handleTableChange,
                onShowSizeChange: handleTableChange,
              }}
              scroll={{ x: 1200 }}
            />
          </Card>
        </Col>
      </Row>

      <Modal
        title={editingFund ? 'Edit Mutual Fund' : 'Add Mutual Fund'}
        open={modalVisible}
        onOk={handleModalOk}
        onCancel={() => {
          setModalVisible(false);
          setEditingFund(null);
          form.resetFields();
        }}
        width={600}
        destroyOnClose
      >
        <Form
          form={form}
          layout="vertical"
          initialValues={{ isActive: true, status: 'Active' }}
        >
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
    </div>
  );
};

export default MutualFundManagement;
