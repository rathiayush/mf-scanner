import React, { useState, useEffect } from 'react';
import { Card, Select, DatePicker, Button, Table, Row, Col, Typography, Space, message, Statistic } from 'antd';
import { EyeOutlined, FundOutlined, CalendarOutlined } from '@ant-design/icons';
import dayjs, { Dayjs } from 'dayjs';
import { apiService } from '../services/api';

const { Title, Text } = Typography;
const { Option } = Select;

interface MutualFund {
  id: number;
  fundName: string;
  fundCode: string;
}

interface PortfolioHolding {
  isin: string;
  securityName: string;
  ticker: string;
  quantity: number;
  marketValue: number;
  weight: number;
  sector: string;
  industry: string;
}

interface PortfolioHoldingsResponse {
  data: PortfolioHolding[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

const PortfolioHoldingsPage: React.FC = () => {
  const [mutualFunds, setMutualFunds] = useState<MutualFund[]>([]);
  const [selectedFundId, setSelectedFundId] = useState<number | null>(null);
  const [selectedDate, setSelectedDate] = useState<Dayjs | null>(null);
  const [holdings, setHoldings] = useState<PortfolioHoldingsResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [sortBy, setSortBy] = useState<string>('Weight');
  const [sortDirection, setSortDirection] = useState<string>('desc');

  // Load mutual funds on component mount
  useEffect(() => {
    loadMutualFunds();
  }, []);

  const loadMutualFunds = async () => {
    try {
      const response = await apiService.get('/api/MutualFund');
      setMutualFunds(response.data);
    } catch (error) {
      console.error('Error loading mutual funds:', error);
      message.error('Failed to load mutual funds');
    }
  };

  const loadPortfolioHoldings = async (
    fundId: number,
    targetDate: Dayjs,
    page: number = 1,
    size: number = 20,
    sort: string = 'Weight',
    direction: string = 'desc'
  ) => {
    setLoading(true);
    try {
      const params = new URLSearchParams({
        mutualFundId: fundId.toString(),
        targetDate: targetDate.format('YYYY-MM-DD'),
        page: page.toString(),
        pageSize: size.toString(),
        sortBy: sort,
        sortDirection: direction
      });

      const response = await apiService.get(`/api/Analytics/portfolio-holdings?${params}`);
      setHoldings(response.data);
    } catch (error) {
      console.error('Error loading portfolio holdings:', error);
      message.error('Failed to load portfolio holdings');
      setHoldings(null);
    } finally {
      setLoading(false);
    }
  };

  const handleViewHoldings = () => {
    if (!selectedFundId || !selectedDate) {
      message.warning('Please select both a mutual fund and a date');
      return;
    }

    setCurrentPage(1);
    loadPortfolioHoldings(selectedFundId, selectedDate, 1, pageSize, sortBy, sortDirection);
  };

  const handleTableChange = (page: number, size?: number) => {
    if (!selectedFundId || !selectedDate) return;

    const newPageSize = size || pageSize;
    setCurrentPage(page);
    if (size !== pageSize) {
      setPageSize(newPageSize);
    }
    loadPortfolioHoldings(selectedFundId, selectedDate, page, newPageSize, sortBy, sortDirection);
  };

  const handleSort = (field: string) => {
    if (!selectedFundId || !selectedDate) return;

    const newDirection = sortBy === field && sortDirection === 'desc' ? 'asc' : 'desc';
    setSortBy(field);
    setSortDirection(newDirection);
    setCurrentPage(1);
    loadPortfolioHoldings(selectedFundId, selectedDate, 1, pageSize, field, newDirection);
  };

  const getSortOrder = (field: string) => {
    if (sortBy === field) {
      return sortDirection === 'asc' ? 'ascend' : 'descend';
    }
    return null;
  };

  const columns = [
    {
      title: 'Security Name',
      dataIndex: 'securityName',
      key: 'securityName',
      sorter: true,
      sortOrder: getSortOrder('SecurityName'),
      onHeaderCell: () => ({
        onClick: () => handleSort('SecurityName'),
      }),
      render: (text: string, record: PortfolioHolding) => (
        <div>
          <div style={{ fontWeight: 500 }}>{text}</div>
          <Text type="secondary" style={{ fontSize: '12px' }}>
            {record.ticker} • {record.isin}
          </Text>
        </div>
      ),
    },
    {
      title: 'Sector',
      dataIndex: 'sector',
      key: 'sector',
      sorter: true,
      sortOrder: getSortOrder('Sector'),
      onHeaderCell: () => ({
        onClick: () => handleSort('Sector'),
      }),
    },
    {
      title: 'Industry',
      dataIndex: 'industry',
      key: 'industry',
      sorter: true,
      sortOrder: getSortOrder('Industry'),
      onHeaderCell: () => ({
        onClick: () => handleSort('Industry'),
      }),
    },
    {
      title: 'Quantity',
      dataIndex: 'quantity',
      key: 'quantity',
      align: 'right' as const,
      sorter: true,
      sortOrder: getSortOrder('Quantity'),
      onHeaderCell: () => ({
        onClick: () => handleSort('Quantity'),
      }),
      render: (value: number) => value.toLocaleString(),
    },
    {
      title: 'Market Value (₹)',
      dataIndex: 'marketValue',
      key: 'marketValue',
      align: 'right' as const,
      sorter: true,
      sortOrder: getSortOrder('MarketValue'),
      onHeaderCell: () => ({
        onClick: () => handleSort('MarketValue'),
      }),
      render: (value: number) => `₹${value.toLocaleString()}`,
    },
    {
      title: 'Weight (%)',
      dataIndex: 'weight',
      key: 'weight',
      align: 'right' as const,
      sorter: true,
      sortOrder: getSortOrder('Weight'),
      onHeaderCell: () => ({
        onClick: () => handleSort('Weight'),
      }),
      render: (value: number) => `${value.toFixed(2)}%`,
    },
  ];

  const selectedFund = mutualFunds.find(f => f.id === selectedFundId);
  const totalMarketValue = holdings?.data.reduce((sum, holding) => sum + holding.marketValue, 0) || 0;

  return (
    <div id="portfolio-holdings-page">
      <Title id="portfolio-holdings-title" level={2}>Portfolio Holdings Viewer</Title>
      <Text id="portfolio-holdings-description" type="secondary">
        View detailed portfolio holdings for a specific mutual fund on a selected date.
      </Text>

      <Row id="portfolio-holdings-main-row" gutter={[16, 16]} style={{ marginTop: 24 }}>
        <Col id="selection-parameters-col" span={24}>
          <Card id="selection-parameters-card" title="Selection Parameters">
            <Row id="selection-parameters-row" gutter={[16, 16]}>
              <Col id="fund-selection-col" span={12}>
                <Space id="fund-selection-space" direction="vertical" style={{ width: '100%' }}>
                  <Text id="fund-selection-label" strong>Select Mutual Fund</Text>
                  <Select
                    id="fund-selection-dropdown"
                    style={{ width: '100%' }}
                    placeholder="Choose a mutual fund"
                    value={selectedFundId}
                    onChange={setSelectedFundId}
                    showSearch
                    optionFilterProp="children"
                    filterOption={(input, option) =>
                      (option?.label as string)?.toLowerCase().includes(input.toLowerCase())
                    }
                  >
                    {mutualFunds.map(fund => (
                      <Option key={fund.id} value={fund.id}>
                        <Space>
                          <Text>{fund.fundName}</Text>
                          <Text type="secondary">({fund.fundCode})</Text>
                        </Space>
                      </Option>
                    ))}
                  </Select>
                </Space>
              </Col>
              
              <Col id="date-selection-col" span={12}>
                <Space id="date-selection-space" direction="vertical" style={{ width: '100%' }}>
                  <Text id="date-selection-label" strong>Select Date</Text>
                  <DatePicker
                    id="date-selection-picker"
                    style={{ width: '100%' }}
                    value={selectedDate}
                    onChange={setSelectedDate}
                    placeholder="Choose a date"
                    format="YYYY-MM-DD"
                  />
                </Space>
              </Col>
            </Row>

            <div id="view-holdings-button-container" style={{ marginTop: 16, textAlign: 'right' }}>
              <Button
                id="view-holdings-button"
                type="primary"
                icon={<EyeOutlined />}
                loading={loading}
                onClick={handleViewHoldings}
                disabled={!selectedFundId || !selectedDate}
              >
                View Holdings
              </Button>
            </div>
          </Card>
        </Col>

        {holdings && (
          <>
            <Col id="holdings-summary-col" span={24}>
              <Card id="holdings-summary-card" title="Holdings Summary">
                <Row id="holdings-summary-row" gutter={[16, 16]}>
                  <Col id="fund-name-stat-col" span={6}>
                    <Statistic
                      id="fund-name-statistic"
                      title="Fund Name"
                      value={selectedFund?.fundName || 'Unknown'}
                      prefix={<FundOutlined />}
                    />
                  </Col>
                  <Col id="date-stat-col" span={6}>
                    <Statistic
                      id="date-statistic"
                      title="Date"
                      value={selectedDate?.format('DD MMM YYYY') || ''}
                      prefix={<CalendarOutlined />}
                    />
                  </Col>
                  <Col id="total-holdings-stat-col" span={6}>
                    <Statistic
                      id="total-holdings-statistic"
                      title="Total Holdings"
                      value={holdings.totalCount}
                    />
                  </Col>
                  <Col id="total-value-stat-col" span={6}>
                    <Statistic
                      id="total-value-statistic"
                      title="Total Market Value"
                      value={`₹${totalMarketValue.toLocaleString()}`}
                    />
                  </Col>
                </Row>
              </Card>
            </Col>

            <Col id="portfolio-holdings-table-col" span={24}>
              <Card id="portfolio-holdings-table-card" title="Portfolio Holdings">
                <Table
                  id="portfolio-holdings-table"
                  columns={columns}
                  dataSource={holdings.data}
                  loading={loading}
                  pagination={{
                    current: currentPage,
                    pageSize: pageSize,
                    total: holdings.totalCount,
                    showSizeChanger: true,
                    showQuickJumper: true,
                    showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} holdings`,
                    onChange: handleTableChange,
                    onShowSizeChange: handleTableChange,
                  }}
                  rowKey="isin"
                  scroll={{ x: 1200 }}
                />
              </Card>
            </Col>
          </>
        )}

        {!selectedFundId && !selectedDate && (
          <Col id="empty-state-col" span={24}>
            <Card id="empty-state-card">
              <div id="empty-state-content" style={{ textAlign: 'center', padding: '40px 0' }}>
                <FundOutlined id="empty-state-icon" style={{ fontSize: '48px', color: '#d9d9d9', marginBottom: 16 }} />
                <Title id="empty-state-title" level={4} type="secondary">Select Fund and Date</Title>
                <Text id="empty-state-description" type="secondary">
                  Choose a mutual fund and date to view its portfolio holdings
                </Text>
              </div>
            </Card>
          </Col>
        )}
      </Row>
    </div>
  );
};

export default PortfolioHoldingsPage;
