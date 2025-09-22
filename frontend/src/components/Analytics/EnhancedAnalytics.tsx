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
  Tabs,
  Modal,
  Progress,
  Divider
} from 'antd';
import {
  BarChartOutlined,
  FundOutlined,
  SwapOutlined,
  TrophyOutlined,
  EyeOutlined
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
  const [isAllFundsSelected, setIsAllFundsSelected] = useState(false);
  const [dateRange, setDateRange] = useState<[Dayjs, Dayjs] | null>(null);
  const [availableDates, setAvailableDates] = useState<AvailableDatesResponseDto | null>(null);
  const [analyticsData, setAnalyticsData] = useState<AnalyticsResponseDto | null>(null);
  const [holdingChanges, setHoldingChanges] = useState<PagedResult<HoldingChangeDto> | null>(null);
  const [selectedStatusFilter, setSelectedStatusFilter] = useState<string>('all');
  const [topShares, setTopShares] = useState<TopShareDto[]>([]);
  const [portfolioSnapshots, setPortfolioSnapshots] = useState<any[]>([]);
  const [currentHoldings, setCurrentHoldings] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [showFundsModal, setShowFundsModal] = useState(false);
  const [selectedModalData, setSelectedModalData] = useState<any>(null);
  const [showHoldingFundsModal, setShowHoldingFundsModal] = useState(false);
  const [selectedHoldingModalData, setSelectedHoldingModalData] = useState<any>(null);

  useEffect(() => {
    loadMutualFunds();
    loadAvailableDates();
  }, []);

  useEffect(() => {
    if (selectedFunds.length > 0 || isAllFundsSelected) {
      loadAvailableDates(isAllFundsSelected ? undefined : selectedFunds);
    }
  }, [selectedFunds, isAllFundsSelected]);

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
    // Reset status filter to 'all' when analyzing
    setSelectedStatusFilter('all');
    
    if (selectedFunds.length === 0 && !isAllFundsSelected) {
      message.warning('Please select at least one mutual fund or select "All Funds"');
      return;
    }

    if (!dateRange) {
      message.warning('Please select a date range');
      return;
    }

    // Validate that selected dates have snapshot data for all selected funds using backend validation
    try {
      const fundsToValidate = isAllFundsSelected ? [] : selectedFunds;
      console.log('Validating dates for funds:', fundsToValidate, 'isAllFunds:', isAllFundsSelected);
      console.log('Date range:', dateRange);
      
      const validation = await analyticsService.validateDatesForFunds(
        fundsToValidate,
        dateRange[0].format('YYYY-MM-DD'),
        dateRange[1].format('YYYY-MM-DD')
      );
      
      console.log('Validation response:', validation);

      if (!validation.isValid) {
        message.warning(
          isAllFundsSelected 
            ? 'No snapshot data available for the selected date range. Please select dates where portfolio data exists.'
            : 'No snapshot data available for selected funds on one or both selected dates. Please select dates where all funds have portfolio data.'
        );
        return;
      }
    } catch (error: any) {
      message.error('Failed to validate date availability. Please try again.');
      return;
    }

    setLoading(true);
    try {
      const request = {
        mutualFundIds: isAllFundsSelected ? [] : selectedFunds,
        fromDate: dateRange[0].format('YYYY-MM-DD'),
        toDate: dateRange[1].format('YYYY-MM-DD'),
        page: currentPage,
        pageSize: pageSize
      };

      console.log('Analytics Request:', request);
      console.log('Selected Funds:', selectedFunds, 'isAllFunds:', isAllFundsSelected);

      // Fetch portfolio snapshots and holdings for the selected date range
      const fundsForAnalysis = isAllFundsSelected ? mutualFunds.map(f => f.id) : selectedFunds;
      const [analytics, changes, shares, fromHoldings, toHoldings] = await Promise.all([
        analyticsService.getAnalytics(request),
        analyticsService.getHoldingChanges(request),
        analyticsService.getTopShares(
          isAllFundsSelected ? '' : selectedFunds.join(','), 
          request.fromDate, 
          request.toDate
        ),
        // Fetch current holdings for from date
        Promise.all(fundsForAnalysis.map(fundId => 
          analyticsService.getCurrentHoldings({
            mutualFundId: fundId,
            targetDate: request.fromDate,
            dateRangeStart: dateRange[0].subtract(30, 'day').format('YYYY-MM-DD'),
            dateRangeEnd: dateRange[0].add(30, 'day').format('YYYY-MM-DD')
          })
        )),
        // Fetch current holdings for to date
        Promise.all(fundsForAnalysis.map(fundId => 
          analyticsService.getCurrentHoldings({
            mutualFundId: fundId,
            targetDate: request.toDate,
            dateRangeStart: dateRange[1].subtract(30, 'day').format('YYYY-MM-DD'),
            dateRangeEnd: dateRange[1].add(30, 'day').format('YYYY-MM-DD')
          })
        ))
      ]);

      setAnalyticsData(analytics);
      setHoldingChanges(changes);
      setTopShares(shares);
      
      // Combine holdings data from both dates
      const allHoldings = [...fromHoldings, ...toHoldings].filter(h => h.success);
      setCurrentHoldings(allHoldings);
      
      // Create portfolio snapshots summary
      const snapshots = allHoldings.map(holding => ({
        fundId: holding.mutualFundId,
        fundName: holding.mutualFundName,
        date: holding.date,
        totalValue: holding.totalValue,
        holdingsCount: holding.holdingsCount,
        holdings: holding.holdings
      }));
      setPortfolioSnapshots(snapshots);
    } catch (error: any) {
      message.error(error.message || 'Failed to analyze data');
    } finally {
      setLoading(false);
    }
  };

  const handleTableChange = (page: number, size?: number) => {
    setCurrentPage(page);
    if (size) setPageSize(size);
    loadHoldingChanges(page, size || pageSize);
  };

  const handleStatusFilterChange = (value: string) => {
    setSelectedStatusFilter(value);
    // Reload holding changes with the new filter, passing the new value directly
    loadHoldingChanges(currentPage, pageSize, value);
  };

  const loadHoldingChanges = async (page: number, size: number, filterValue?: string) => {
    if (!dateRange || (selectedFunds.length === 0 && !isAllFundsSelected)) return;

    setLoading(true);

    try {
      // Use the passed filterValue or fall back to current state
      const currentFilter = filterValue !== undefined ? filterValue : selectedStatusFilter;
      
      const request = {
        mutualFundIds: isAllFundsSelected ? [] : selectedFunds,
        fromDate: dateRange[0].format('YYYY-MM-DD'),
        toDate: dateRange[1].format('YYYY-MM-DD'),
        page,
        pageSize: size,
        changeTypeFilter: currentFilter === 'all' ? undefined : currentFilter
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
      title: 'Total Funds',
      dataIndex: 'mutualFundNames',
      key: 'totalFunds',
      width: 100,
      align: 'center' as const,
      render: (names: string[], record: HoldingChangeDto) => (
        <Space direction="vertical" size="small" style={{ alignItems: 'center' }}>
          <Tag color="blue" style={{ fontSize: '12px', fontWeight: 'bold' }}>
            {names.length}
          </Tag>
          {names.length > 2 && (
            <Button 
              type="link" 
              size="small" 
              icon={<EyeOutlined />}
              onClick={() => {
                setSelectedHoldingModalData({
                  security: record.securityName,
                  isin: record.isin,
                  sector: record.sector,
                  changeType: record.changeType,
                  funds: names.map((name) => ({
                    name,
                    previousWeight: record.previousWeight,
                    currentWeight: record.currentWeight,
                    weightChange: record.weightChange
                  }))
                });
                setShowHoldingFundsModal(true);
              }}
              style={{ padding: 0, fontSize: '10px' }}
            >
              Details
            </Button>
          )}
        </Space>
      ),
    },
    {
      title: 'Mutual Funds',
      dataIndex: 'mutualFundNames',
      key: 'mutualFundNames',
      render: (names: string[]) => (
        <Space direction="vertical" size="small">
          {names.slice(0, 2).map(name => (
            <Tag key={name} style={{ fontSize: '11px' }}>{name}</Tag>
          ))}
          {names.length > 2 && (
            <Text type="secondary" style={{ fontSize: '11px' }}>+{names.length - 2} more</Text>
          )}
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
            <Button 
              type="link" 
              size="small" 
              icon={<EyeOutlined />}
              onClick={() => {
                setSelectedModalData(holdings);
                setShowFundsModal(true);
              }}
              style={{ padding: 0, fontSize: '12px' }}
            >
              View all {holdings.length} funds
            </Button>
          )}
        </Space>
      ),
    },
  ];

  const disabledDate = (current: Dayjs) => {
    // If no funds selected and not all funds selected, use global date range
    if (selectedFunds.length === 0 && !isAllFundsSelected) {
      if (!availableDates || !availableDates.minDate || !availableDates.maxDate) {
        return false; // Enable all dates
      }
      
      try {
        const minDate = dayjs(availableDates.minDate);
        const maxDate = dayjs(availableDates.maxDate);
        
        if (!minDate.isValid() || !maxDate.isValid()) {
          return false; // Enable all dates
        }
        
        return current.isBefore(minDate.startOf('month')) || current.isAfter(maxDate.endOf('month'));
      } catch (error) {
        return false; // Enable all dates
      }
    }
    
    // If funds are selected or all funds selected, validate against fund-specific availability
    if (availableDates?.fundDateAvailabilities && availableDates.fundDateAvailabilities.length > 0) {
      // Get the selected funds' availability data
      const selectedFundAvailabilities = isAllFundsSelected 
        ? availableDates.fundDateAvailabilities
        : availableDates.fundDateAvailabilities.filter(
            fund => selectedFunds.includes(fund.mutualFundId)
          );
      
      if (selectedFundAvailabilities.length === 0) {
        return false; // No availability data, enable all dates
      }
      
      try {
        // Find the intersection of available dates for all selected funds
        const availableDateStrings = selectedFundAvailabilities.reduce((intersection, fund) => {
          const fundDates = fund.availableDates.map(dateStr => dayjs(dateStr).format('YYYY-MM-DD'));
          return intersection.length === 0 
            ? fundDates 
            : intersection.filter(date => fundDates.includes(date));
        }, [] as string[]);
        
        if (availableDateStrings.length === 0) {
          return true; // No common available dates, disable all dates
        }
        
        // Check if current date is in the available dates
        const currentDateStr = current.format('YYYY-MM-DD');
        return !availableDateStrings.includes(currentDateStr);
        
      } catch (error) {
        // Fallback to global range if there's an error
        if (!availableDates.minDate || !availableDates.maxDate) {
          return false;
        }
        
        const minDate = dayjs(availableDates.minDate);
        const maxDate = dayjs(availableDates.maxDate);
        
        if (!minDate.isValid() || !maxDate.isValid()) {
          return false;
        }
        
        return current.isBefore(minDate.startOf('month')) || current.isAfter(maxDate.endOf('month'));
      }
    }
    
    // Fallback to global date range if no fund-specific data
    if (!availableDates || !availableDates.minDate || !availableDates.maxDate) {
      return false; // Enable all dates
    }
    
    try {
      const minDate = dayjs(availableDates.minDate);
      const maxDate = dayjs(availableDates.maxDate);
      
      if (!minDate.isValid() || !maxDate.isValid()) {
        return false; // Enable all dates
      }
      
      return current.isBefore(minDate.startOf('month')) || current.isAfter(maxDate.endOf('month'));
    } catch (error) {
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
                    placeholder="Select mutual funds to analyze or choose 'All Funds'"
                    showSearch
                    filterOption={(input, option) =>
                      (option?.children as string)?.toLowerCase().includes(input.toLowerCase())
                    }
                    value={isAllFundsSelected ? ['all'] : selectedFunds}
                    onChange={(value) => {
                      if (value.includes('all')) {
                        setIsAllFundsSelected(true);
                        setSelectedFunds([]);
                      } else {
                        setIsAllFundsSelected(false);
                        setSelectedFunds(value.filter(v => v !== 'all'));
                      }
                    }}
                  >
                    <Option key="all" value="all">
                      <Space>
                        <Text strong style={{ color: '#1890ff' }}>All Funds</Text>
                        <Text type="secondary">(Analyze all available funds)</Text>
                      </Space>
                    </Option>
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
                disabled={(selectedFunds.length === 0 && !isAllFundsSelected) || !dateRange}
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
                    <div>
                      <div style={{ marginBottom: 16 }}>
                        <Space>
                          <Text strong>Filter by Status:</Text>
                          <Select
                            value={selectedStatusFilter}
                            onChange={handleStatusFilterChange}
                            style={{ width: 150 }}
                          >
                            <Option value="all">All Statuses</Option>
                            <Option value="Added">Added</Option>
                            <Option value="Removed">Removed</Option>
                            <Option value="Increased">Increased</Option>
                            <Option value="Decreased">Decreased</Option>
                            <Option value="Mixed">Mixed</Option>
                          </Select>
                          <Text type="secondary">
                            Showing {holdingChanges?.data.length || 0} of {holdingChanges?.totalCount || 0} holdings
                          </Text>
                        </Space>
                      </div>
                      <Table
                        dataSource={holdingChanges?.data || []}
                      columns={holdingChangesColumns}
                      rowKey="isin"
                      loading={loading}
                      pagination={{
                        current: holdingChanges?.page || 1,
                        pageSize: holdingChanges?.pageSize || 20,
                        total: holdingChanges?.totalCount || 0,
                        showSizeChanger: true,
                        showQuickJumper: true,
                        showTotal: (total, range) =>
                          `${range[0]}-${range[1]} of ${total} holdings`,
                        onChange: handleTableChange,
                        onShowSizeChange: handleTableChange,
                      }}
                      scroll={{ x: 1200 }}
                    />
                    </div>
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

                <TabPane 
                  tab={
                    <span>
                      <FundOutlined />
                      Portfolio Snapshots ({portfolioSnapshots.length})
                    </span>
                  } 
                  key="snapshots"
                >
                  {portfolioSnapshots.length > 0 ? (
                    <Row gutter={[16, 16]}>
                      {portfolioSnapshots.map((snapshot, index) => (
                        <Col span={12} key={index}>
                          <Card 
                            title={`${snapshot.fundName} - ${snapshot.date}`}
                            size="small"
                          >
                            <Row gutter={[8, 8]}>
                              <Col span={12}>
                                <Statistic
                                  title="Total Value"
                                  value={snapshot.totalValue}
                                  prefix="₹"
                                  precision={2}
                                />
                              </Col>
                              <Col span={12}>
                                <Statistic
                                  title="Holdings Count"
                                  value={snapshot.holdingsCount}
                                />
                              </Col>
                            </Row>
                            <div style={{ marginTop: 16 }}>
                              <Text strong>Top 5 Holdings:</Text>
                              {snapshot.holdings?.slice(0, 5).map((holding: any, idx: number) => (
                                <div key={idx} style={{ marginTop: 4 }}>
                                  <Text style={{ fontSize: '12px' }}>
                                    {holding.companyName}: {holding.weight?.toFixed(2)}%
                                  </Text>
                                </div>
                              ))}
                            </div>
                          </Card>
                        </Col>
                      ))}
                    </Row>
                  ) : (
                    <Alert
                      message="No Portfolio Snapshots"
                      description="Run analysis to see portfolio snapshots for selected funds and dates."
                      type="info"
                      showIcon
                    />
                  )}
                </TabPane>

                <TabPane 
                  tab={
                    <span>
                      <BarChartOutlined />
                      Current Holdings ({currentHoldings.reduce((acc, h) => acc + (h.holdingsCount || 0), 0)})
                    </span>
                  } 
                  key="holdings"
                >
                  {currentHoldings.length > 0 ? (
                    <Row gutter={[16, 16]}>
                      {currentHoldings.map((holding, index) => (
                        <Col span={24} key={index}>
                          <Card 
                            title={`${holding.mutualFundName} - ${holding.date}`}
                            size="small"
                          >
                            <Table
                              dataSource={holding.holdings}
                              columns={[
                                {
                                  title: 'Company Name',
                                  dataIndex: 'companyName',
                                  key: 'companyName',
                                },
                                {
                                  title: 'Weight',
                                  dataIndex: 'weight',
                                  key: 'weight',
                                  render: (weight: number) => `${weight?.toFixed(2)}%`,
                                },
                                {
                                  title: 'Value',
                                  dataIndex: 'value',
                                  key: 'value',
                                  render: (value: number) => `₹${value?.toLocaleString()}`,
                                },
                                {
                                  title: 'Sector',
                                  dataIndex: 'sector',
                                  key: 'sector',
                                },
                              ]}
                              rowKey="id"
                              pagination={{ pageSize: 10 }}
                              size="small"
                            />
                          </Card>
                        </Col>
                      ))}
                    </Row>
                  ) : (
                    <Alert
                      message="No Holdings Data"
                      description="Run analysis to see detailed holdings for selected funds and dates."
                      type="info"
                      showIcon
                    />
                  )}
                </TabPane>
              </Tabs>
            </Card>
          </Col>
        )}

        {selectedFunds.length === 0 && !isAllFundsSelected && (
          <Col span={24}>
            <Alert
              message="Select Mutual Funds"
              description="Please select one or more mutual funds or choose 'All Funds' to begin analysis."
              type="info"
              showIcon
            />
          </Col>
        )}
      </Row>

      {/* Mutual Funds Weight Modal */}
      <Modal
        title={
          <Space>
            <FundOutlined />
            <Text strong>Mutual Fund Holdings Details</Text>
          </Space>
        }
        open={showFundsModal}
        onCancel={() => {
          setShowFundsModal(false);
          setSelectedModalData(null);
        }}
        footer={[
          <Button key="close" onClick={() => {
            setShowFundsModal(false);
            setSelectedModalData(null);
          }}>
            Close
          </Button>
        ]}
        width={800}
        style={{ top: 20 }}
      >
        {selectedModalData && (
          <div style={{ maxHeight: '60vh', overflowY: 'auto' }}>
            <Alert
              message="Fund Weight Distribution"
              description={`Showing holdings across ${selectedModalData.length} mutual funds`}
              type="info"
              showIcon
              style={{ marginBottom: 16 }}
            />
            
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              {selectedModalData
                .sort((a: any, b: any) => b.weight - a.weight)
                .map((holding: any, index: number) => (
                  <Card 
                    key={holding.mutualFundId} 
                    size="small" 
                    style={{ 
                      border: '1px solid #f0f0f0',
                      borderRadius: 8,
                      backgroundColor: index % 2 === 0 ? '#fafafa' : '#ffffff'
                    }}
                  >
                    <Row align="middle" gutter={[16, 8]}>
                      <Col span={2}>
                        <Tag color="blue" style={{ margin: 0 }}>
                          #{index + 1}
                        </Tag>
                      </Col>
                      <Col span={14}>
                        <Space direction="vertical" size={2}>
                          <Text strong style={{ fontSize: '14px' }}>
                            {holding.mutualFundName}
                          </Text>
                          <Text type="secondary" style={{ fontSize: '12px' }}>
                            Fund ID: {holding.mutualFundId}
                          </Text>
                        </Space>
                      </Col>
                      <Col span={8}>
                        <Space direction="vertical" size={2} style={{ width: '100%' }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <Text strong style={{ fontSize: '16px', color: '#1890ff' }}>
                              {holding.weight.toFixed(2)}%
                            </Text>
                          </div>
                          <Progress 
                            percent={holding.weight} 
                            showInfo={false} 
                            strokeColor={{
                              '0%': '#108ee9',
                              '100%': '#87d068',
                            }}
                            size="small"
                          />
                        </Space>
                      </Col>
                    </Row>
                  </Card>
                ))}
            </Space>
            
            <Divider />
            
            <Row gutter={[16, 16]}>
              <Col span={8}>
                <Statistic
                  title="Total Funds"
                  value={selectedModalData.length}
                  prefix={<FundOutlined />}
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Average Weight"
                  value={(selectedModalData.reduce((sum: number, h: any) => sum + h.weight, 0) / selectedModalData.length).toFixed(2)}
                  suffix="%"
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Total Weight"
                  value={selectedModalData.reduce((sum: number, h: any) => sum + h.weight, 0).toFixed(2)}
                  suffix="%"
                />
              </Col>
            </Row>
          </div>
        )}
      </Modal>

      {/* Holding Changes Funds Modal */}
      <Modal
        title={
          <Space>
            <SwapOutlined />
            <Text strong>Holding Change Fund Details</Text>
          </Space>
        }
        open={showHoldingFundsModal}
        onCancel={() => {
          setShowHoldingFundsModal(false);
          setSelectedHoldingModalData(null);
        }}
        footer={[
          <Button key="close" onClick={() => {
            setShowHoldingFundsModal(false);
            setSelectedHoldingModalData(null);
          }}>
            Close
          </Button>
        ]}
        width={900}
        style={{ top: 20 }}
      >
        {selectedHoldingModalData && (
          <div style={{ maxHeight: '60vh', overflowY: 'auto' }}>
            <Alert
              message={`Holding Change: ${selectedHoldingModalData.security}`}
              description={
                <Space direction="vertical" size={2}>
                  <Text>ISIN: {selectedHoldingModalData.isin}</Text>
                  <Text>Sector: {selectedHoldingModalData.sector}</Text>
                  <Tag color={
                    selectedHoldingModalData.changeType === 'Added' ? 'green' :
                    selectedHoldingModalData.changeType === 'Removed' ? 'red' :
                    selectedHoldingModalData.changeType === 'Increased' ? 'blue' :
                    selectedHoldingModalData.changeType === 'Decreased' ? 'orange' :
                    'purple'
                  }>
                    {selectedHoldingModalData.changeType}
                  </Tag>
                </Space>
              }
              type="info"
              showIcon
              style={{ marginBottom: 16 }}
            />
            
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              {selectedHoldingModalData.funds.map((fund: any, index: number) => (
                <Card 
                  key={index} 
                  size="small" 
                  style={{ 
                    border: '1px solid #f0f0f0',
                    borderRadius: 8,
                    backgroundColor: index % 2 === 0 ? '#fafafa' : '#ffffff'
                  }}
                >
                  <Row align="middle" gutter={[16, 8]}>
                    <Col span={2}>
                      <Tag color="blue" style={{ margin: 0 }}>
                        #{index + 1}
                      </Tag>
                    </Col>
                    <Col span={10}>
                      <Space direction="vertical" size={2}>
                        <Text strong style={{ fontSize: '14px' }}>
                          {fund.name}
                        </Text>
                      </Space>
                    </Col>
                    <Col span={4}>
                      <Space direction="vertical" size={2} align="center">
                        <Text type="secondary" style={{ fontSize: '12px' }}>Previous</Text>
                        <Text style={{ fontSize: '14px' }}>
                          {fund.previousWeight ? `${fund.previousWeight.toFixed(2)}%` : '-'}
                        </Text>
                      </Space>
                    </Col>
                    <Col span={4}>
                      <Space direction="vertical" size={2} align="center">
                        <Text type="secondary" style={{ fontSize: '12px' }}>Current</Text>
                        <Text strong style={{ fontSize: '14px', color: '#1890ff' }}>
                          {fund.currentWeight.toFixed(2)}%
                        </Text>
                      </Space>
                    </Col>
                    <Col span={4}>
                      <Space direction="vertical" size={2} align="center">
                        <Text type="secondary" style={{ fontSize: '12px' }}>Change</Text>
                        <Text 
                          strong 
                          style={{ 
                            fontSize: '14px', 
                            color: fund.weightChange > 0 ? '#52c41a' : fund.weightChange < 0 ? '#ff4d4f' : '#666666'
                          }}
                        >
                          {fund.weightChange > 0 ? '+' : ''}{fund.weightChange.toFixed(2)}%
                        </Text>
                      </Space>
                    </Col>
                  </Row>
                </Card>
              ))}
            </Space>
            
            <Divider />
            
            <Row gutter={[16, 16]}>
              <Col span={8}>
                <Statistic
                  title="Total Funds"
                  value={selectedHoldingModalData.funds.length}
                  prefix={<FundOutlined />}
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Average Current Weight"
                  value={(selectedHoldingModalData.funds.reduce((sum: number, f: any) => sum + f.currentWeight, 0) / selectedHoldingModalData.funds.length).toFixed(2)}
                  suffix="%"
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Total Weight Change"
                  value={selectedHoldingModalData.funds.reduce((sum: number, f: any) => sum + f.weightChange, 0).toFixed(2)}
                  suffix="%"
                  valueStyle={{ 
                    color: selectedHoldingModalData.funds.reduce((sum: number, f: any) => sum + f.weightChange, 0) > 0 ? '#52c41a' : '#ff4d4f'
                  }}
                />
              </Col>
            </Row>
          </div>
        )}
      </Modal>
    </div>
  );
};

export default EnhancedAnalytics;
