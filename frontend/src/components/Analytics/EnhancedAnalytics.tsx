import React, { useState, useEffect } from 'react';
import {
  Card,
  Select,
  DatePicker,
  Button,
  Table,
  Row,
  Col,
  Typography,
  Space,
  Tag,
  Statistic,
  Alert,
  message,
  Tabs
} from 'antd';
import {
  BarChartOutlined,
  FundOutlined,
  SwapOutlined,
  TrophyOutlined
} from '@ant-design/icons';
import { mutualFundService, analyticsService } from '../../services/api';
import {
  MutualFundDto,
  AnalyticsResponseDto,
  HoldingChangeDto,
  TopShareDto,
  AvailableDatesResponseDto,
  PagedResult
} from '../../types';
import dayjs, { Dayjs } from 'dayjs';

const { Title, Text } = Typography;
const { RangePicker } = DatePicker;
const { Option } = Select;
const { TabPane } = Tabs;

const EnhancedAnalytics: React.FC = () => {
  const [mutualFunds, setMutualFunds] = useState<MutualFundDto[]>([]);
  const [selectedFunds, setSelectedFunds] = useState<number[]>([]);
  const [dateRange, setDateRange] = useState<[Dayjs, Dayjs] | null>(null);
  const [availableDates, setAvailableDates] = useState<AvailableDatesResponseDto | null>(null);
  const [analyticsData, setAnalyticsData] = useState<AnalyticsResponseDto | null>(null);
  const [holdingChanges, setHoldingChanges] = useState<PagedResult<HoldingChangeDto> | null>(null);
  const [topShares, setTopShares] = useState<TopShareDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  useEffect(() => {
    loadMutualFunds();
    loadAvailableDates();
  }, []);

  useEffect(() => {
    if (selectedFunds.length > 0) {
      loadAvailableDates(selectedFunds);
    }
  }, [selectedFunds]);

  const loadMutualFunds = async () => {
    try {
      const funds = await mutualFundService.getMutualFundsForDropdown();
      setMutualFunds(funds);
    } catch (error) {
      message.error('Failed to load mutual funds');
    }
  };

  const loadAvailableDates = async (fundIds?: number[]) => {
    try {
      const dates = await analyticsService.getAvailableDates(fundIds);
      setAvailableDates(dates);
    } catch (error) {
      message.error('Failed to load available dates');
    }
  };

  const handleAnalyze = async () => {
    if (selectedFunds.length === 0) {
      message.warning('Please select at least one mutual fund');
      return;
    }

    if (!dateRange) {
      message.warning('Please select a date range');
      return;
    }

    setLoading(true);
    try {
      const request = {
        mutualFundIds: selectedFunds,
        fromDate: dateRange[0].format('YYYY-MM-DD'),
        toDate: dateRange[1].format('YYYY-MM-DD'),
        page: currentPage,
        pageSize: pageSize
      };

      const [analytics, changes, shares] = await Promise.all([
        analyticsService.getAnalytics(request),
        analyticsService.getHoldingChanges(request),
        analyticsService.getTopShares(selectedFunds, request.fromDate, request.toDate)
      ]);

      setAnalyticsData(analytics);
      setHoldingChanges(changes);
      setTopShares(shares);
    } catch (error: any) {
      message.error(error.message || 'Failed to analyze data');
    } finally {
      setLoading(false);
    }
  };

  const handleTableChange = async (page: number, size: number) => {
    if (!dateRange || selectedFunds.length === 0) return;

    setCurrentPage(page);
    setPageSize(size);
    setLoading(true);

    try {
      const request = {
        mutualFundIds: selectedFunds,
        fromDate: dateRange[0].format('YYYY-MM-DD'),
        toDate: dateRange[1].format('YYYY-MM-DD'),
        page,
        pageSize: size
      };

      const changes = await analyticsService.getHoldingChanges(request);
      setHoldingChanges(changes);
    } catch (error: any) {
      message.error(error.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  const holdingChangesColumns = [
    {
      title: 'Security Name',
      dataIndex: 'securityName',
      key: 'securityName',
      width: 200,
      render: (text: string, record: HoldingChangeDto) => (
        <Space direction="vertical" size="small">
          <Text strong>{text}</Text>
          <Text type="secondary" style={{ fontSize: '12px' }}>{record.isin}</Text>
        </Space>
      ),
    },
    {
      title: 'Sector',
      dataIndex: 'sector',
      key: 'sector',
      width: 120,
    },
    {
      title: 'Change Type',
      dataIndex: 'changeType',
      key: 'changeType',
      width: 100,
      render: (type: string) => {
        const colors = {
          Added: 'green',
          Removed: 'red',
          Increased: 'blue',
          Decreased: 'orange',
          Mixed: 'purple'
        };
        return <Tag color={colors[type as keyof typeof colors]}>{type}</Tag>;
      },
    },
    {
      title: 'Previous Weight',
      dataIndex: 'previousWeight',
      key: 'previousWeight',
      width: 120,
      render: (weight: number) => weight ? `${weight.toFixed(2)}%` : '-',
    },
    {
      title: 'Current Weight',
      dataIndex: 'currentWeight',
      key: 'currentWeight',
      width: 120,
      render: (weight: number) => `${weight.toFixed(2)}%`,
    },
    {
      title: 'Weight Change',
      dataIndex: 'weightChange',
      key: 'weightChange',
      width: 120,
      render: (change: number) => (
        <Text type={change > 0 ? 'success' : change < 0 ? 'danger' : undefined}>
          {change > 0 ? '+' : ''}{change.toFixed(2)}%
        </Text>
      ),
    },
    {
      title: 'Mutual Funds',
      dataIndex: 'mutualFundNames',
      key: 'mutualFundNames',
      render: (names: string[]) => (
        <Space direction="vertical" size="small">
          {names.map(name => (
            <Tag key={name}>{name}</Tag>
          ))}
        </Space>
      ),
    },
  ];

  const topSharesColumns = [
    {
      title: 'Security Name',
      dataIndex: 'securityName',
      key: 'securityName',
      width: 200,
      render: (text: string, record: TopShareDto) => (
        <Space direction="vertical" size="small">
          <Text strong>{text}</Text>
          <Text type="secondary" style={{ fontSize: '12px' }}>{record.isin}</Text>
        </Space>
      ),
    },
    {
      title: 'Sector',
      dataIndex: 'sector',
      key: 'sector',
      width: 120,
    },
    {
      title: 'Total Weight',
      dataIndex: 'totalWeight',
      key: 'totalWeight',
      width: 120,
      render: (weight: number) => `${weight.toFixed(2)}%`,
      sorter: (a: TopShareDto, b: TopShareDto) => b.totalWeight - a.totalWeight,
    },
    {
      title: 'Holding Funds',
      dataIndex: 'holdingFundsCount',
      key: 'holdingFundsCount',
      width: 100,
      render: (count: number) => <Tag color="blue">{count} funds</Tag>,
    },
    {
      title: 'Fund Holdings',
      dataIndex: 'fundHoldings',
      key: 'fundHoldings',
      render: (holdings: any[]) => (
        <Space direction="vertical" size="small">
          {holdings.slice(0, 3).map(holding => (
            <div key={holding.mutualFundId}>
              <Text style={{ fontSize: '12px' }}>
                {holding.mutualFundName}: {holding.weight.toFixed(2)}%
              </Text>
            </div>
          ))}
          {holdings.length > 3 && (
            <Text type="secondary" style={{ fontSize: '12px' }}>
              +{holdings.length - 3} more
            </Text>
          )}
        </Space>
      ),
    },
  ];

  const disabledDate = (current: Dayjs) => {
    // If no available dates or invalid data, enable all dates
    if (!availableDates || !availableDates.minDate || !availableDates.maxDate) {
      return false; // Enable all dates
    }
    
    try {
      // Parse min and max dates from the API response
      const minDate = dayjs(availableDates.minDate);
      const maxDate = dayjs(availableDates.maxDate);
      
      // If dates are invalid, enable all dates
      if (!minDate.isValid() || !maxDate.isValid()) {
        return false; // Enable all dates
      }
      
      // Enable all dates within the available range
      return current.isBefore(minDate.startOf('month')) || current.isAfter(maxDate.endOf('month'));
    } catch (error) {
      // If there's any error in parsing, enable all dates
      return false; // Enable all dates
    }
  };

  return (
    <div>
      <Title level={2}>Portfolio Analytics</Title>
      <Text type="secondary">
        Analyze portfolio changes and holdings across multiple mutual funds over time.
      </Text>

      <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
        <Col span={24}>
          <Card title="Analysis Parameters">
            <Row gutter={[16, 16]}>
              <Col span={12}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text strong>Select Mutual Funds</Text>
                  <Select
                    mode="multiple"
                    style={{ width: '100%' }}
                    placeholder="Select mutual funds to analyze"
                    showSearch
                    filterOption={(input, option) =>
                      (option?.children as string)?.toLowerCase().includes(input.toLowerCase())
                    }
                    value={selectedFunds}
                    onChange={setSelectedFunds}
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
              
              <Col span={12}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text strong>Date Range</Text>
                  <RangePicker
                    style={{ width: '100%' }}
                    value={dateRange}
                    onChange={(dates) => {
          if (dates && dates.length === 2) {
            setDateRange([dates[0]!, dates[1]!]);
          } else {
            setDateRange(null);
          }
        }}
                    disabledDate={disabledDate}
                    placeholder={['From Date', 'To Date']}
                  />
                  {availableDates && (
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Available: {availableDates.minDate} to {availableDates.maxDate}
                    </Text>
                  )}
                </Space>
              </Col>
            </Row>

            <div style={{ marginTop: 16, textAlign: 'right' }}>
              <Button
                type="primary"
                icon={<BarChartOutlined />}
                loading={loading}
                onClick={handleAnalyze}
                disabled={selectedFunds.length === 0 || !dateRange}
              >
                Analyze Portfolio
              </Button>
            </div>
          </Card>
        </Col>

        {analyticsData && (
          <Col span={24}>
            <Card title="Analytics Summary">
              <Row gutter={[16, 16]}>
                <Col span={6}>
                  <Statistic
                    title="Total Holdings Analyzed"
                    value={analyticsData.summary.totalHoldingsAnalyzed}
                    prefix={<FundOutlined />}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="Added Holdings"
                    value={analyticsData.summary.addedHoldings}
                    valueStyle={{ color: '#3f8600' }}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="Removed Holdings"
                    value={analyticsData.summary.removedHoldings}
                    valueStyle={{ color: '#cf1322' }}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="Total Weight Change"
                    value={analyticsData.summary.totalWeightChange}
                    precision={2}
                    suffix="%"
                    prefix={<SwapOutlined />}
                  />
                </Col>
              </Row>
            </Card>
          </Col>
        )}

        {(holdingChanges || topShares.length > 0) && (
          <Col span={24}>
            <Card>
              <Tabs defaultActiveKey="changes">
                <TabPane 
                  tab={
                    <span>
                      <SwapOutlined />
                      Holding Changes ({holdingChanges?.totalCount || 0})
                    </span>
                  } 
                  key="changes"
                >
                  {holdingChanges && (
                    <Table
                      dataSource={holdingChanges.data}
                      columns={holdingChangesColumns}
                      rowKey="isin"
                      loading={loading}
                      pagination={{
                        current: holdingChanges.page,
                        pageSize: holdingChanges.pageSize,
                        total: holdingChanges.totalCount,
                        showSizeChanger: true,
                        showQuickJumper: true,
                        showTotal: (total, range) =>
                          `${range[0]}-${range[1]} of ${total} holdings`,
                        onChange: handleTableChange,
                        onShowSizeChange: handleTableChange,
                      }}
                      scroll={{ x: 1000 }}
                    />
                  )}
                </TabPane>

                <TabPane 
                  tab={
                    <span>
                      <TrophyOutlined />
                      Top 15 Shares ({topShares.length})
                    </span>
                  } 
                  key="topShares"
                >
                  <Table
                    dataSource={topShares}
                    columns={topSharesColumns}
                    rowKey="isin"
                    pagination={false}
                    scroll={{ x: 1000 }}
                  />
                </TabPane>
              </Tabs>
            </Card>
          </Col>
        )}

        {selectedFunds.length === 0 && (
          <Col span={24}>
            <Alert
              message="Select Mutual Funds"
              description="Please select one or more mutual funds to begin analysis."
              type="info"
              showIcon
            />
          </Col>
        )}
      </Row>
    </div>
  );
};

export default EnhancedAnalytics;
