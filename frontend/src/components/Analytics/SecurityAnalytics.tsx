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
  Divider,
  Input,
} from 'antd';
import {
  BarChartOutlined,
  FundOutlined,
  SwapOutlined,
  EyeOutlined,
  StockOutlined,
} from '@ant-design/icons';
import { analyticsService } from '../../services/api';
import SecurityChangeDetailsModal from './modals/SecurityChangeDetailsModal';
import {
  SecurityAnalyticsRequestDto,
  SecurityAnalyticsResponseDto,
  SecurityMonthlyChangeDto,
  MonthlyChangeTypeDto,
  SecurityDropdownDto,
} from '../../types';
import dayjs, { Dayjs } from 'dayjs';

const { Title, Text } = Typography;
const { RangePicker } = DatePicker;
const { Option } = Select;
const { Search } = Input;

const SecurityAnalytics: React.FC = () => {
  const [availableSecurities, setAvailableSecurities] = useState<SecurityDropdownDto[]>([]);
  const [selectedSecurity, setSelectedSecurity] = useState<SecurityDropdownDto | null>(null);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [isinSearchTerm, setIsinSearchTerm] = useState<string>('');
  const [searchLoading, setSearchLoading] = useState(false);
  const [dateRange, setDateRange] = useState<[Dayjs, Dayjs] | null>(null);
  const [analyticsData, setAnalyticsData] = useState<SecurityAnalyticsResponseDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [showFundDetailsModal, setShowFundDetailsModal] = useState(false);
  const [selectedMonthData, setSelectedMonthData] = useState<SecurityMonthlyChangeDto | null>(null);

  // Sorting state
  const [sortBy, setSortBy] = useState<string>('monthYear');
  const [sortDirection, setSortDirection] = useState<string>('desc');

  useEffect(() => {
    const debounceTimer = setTimeout(() => {
      loadAvailableSecurities(searchTerm, isinSearchTerm);
    }, 300); // 300ms debounce

    return () => clearTimeout(debounceTimer);
  }, [searchTerm, isinSearchTerm]);

  const loadAvailableSecurities = async (securityName?: string, isin?: string) => {
    setSearchLoading(true);
    try {
      // Use different APIs based on search type
      let response;
      if (isin) {
        // ISIN search uses dedicated endpoint
        response = await analyticsService.getSecuritiesByIsin(isin);
      } else if (securityName) {
        // Security name search uses general endpoint
        response = await analyticsService.getAvailableSecurities(securityName);
      } else {
        // No search term - get all securities
        response = await analyticsService.getAvailableSecurities();
      }

      // Remove duplicates based on ISIN to prevent React key warnings
      const uniqueSecurities = response.securities
        .filter((security, index, self) =>
          index === self.findIndex(s => s.isin === security.isin)
        );

      setAvailableSecurities(uniqueSecurities);
    } catch (error: any) {
      console.error('Failed to load available securities:', error);
      message.error('Failed to load available securities');
      // Fallback to empty array to prevent crashes
      setAvailableSecurities([]);
    } finally {
      setSearchLoading(false);
    }
  };

  const handleAnalyze = async () => {
    if (!selectedSecurity) {
      message.warning('Please select a security to analyze');
      return;
    }

    if (!dateRange) {
      message.warning('Please select a date range');
      return;
    }

    setLoading(true);
    try {
      const request: SecurityAnalyticsRequestDto = {
        isin: selectedSecurity.isin,
        securityName: selectedSecurity.securityName,
        fromDate: dateRange[0].format('YYYY-MM-DD'),
        toDate: dateRange[1].format('YYYY-MM-DD'),
        page: currentPage,
        pageSize: pageSize,
        sortBy,
        sortDirection
      };

      const response = await analyticsService.getSecurityAnalytics(request);
      setAnalyticsData(response);
    } catch (error: any) {
      console.error('Failed to analyze security data:', error);
      message.error(error.message || 'Failed to analyze security data');
      setAnalyticsData(null);
    } finally {
      setLoading(false);
    }
  };

  const handleTableChange = (page: number, size?: number) => {
    setCurrentPage(page);
    if (size) setPageSize(size);
    // Reload with new pagination would happen here in real implementation
  };

  const filteredSecurities = availableSecurities;


  const monthlyChangesColumns = [
    {
      title: 'Month',
      dataIndex: 'monthYear',
      key: 'monthYear',
      width: 120,
      render: (date: string) => dayjs(date).format('MMM YYYY'),
      sorter: true,
    },
    {
      title: 'Change Type',
      dataIndex: 'changeTypes',
      key: 'changeTypes',
      width: 200,
      render: (changeTypes: MonthlyChangeTypeDto[]) => (
        <Space direction="vertical" size="small">
          {changeTypes.map((ct, index) => {
            const colors = {
              Added: 'green',
              Removed: 'red',
              Increased: 'blue',
              Decreased: 'orange',
              Mixed: 'purple',
              DataNotPresent: 'default'
            };
            return (
              <Tag key={index} color={colors[ct.changeType as keyof typeof colors]}>
                {ct.changeType} ({ct.mutualFundsCount})
              </Tag>
            );
          })}
        </Space>
      ),
    },
    {
      title: 'Mutual Funds',
      dataIndex: 'totalMutualFundsCount',
      key: 'totalMutualFundsCount',
      width: 120,
      render: (count: number) => (
        <Tag color="blue" style={{ fontSize: '12px', fontWeight: 'bold' }}>
          {count} funds
        </Tag>
      ),
    },
    {
      title: 'Weight Change',
      dataIndex: 'totalAbsoluteWeightChange',
      key: 'totalAbsoluteWeightChange',
      width: 140,
      render: (change: number) => (
        <Text type={change > 0 ? 'success' : change < 0 ? 'danger' : undefined}>
          {change > 0 ? '+' : ''}{change?.toFixed(2) ?? '0.00'}%
        </Text>
      ),
      sorter: true,
    },
    {
      title: 'Quantity Change',
      dataIndex: 'totalQuantityChange',
      key: 'totalQuantityChange',
      width: 140,
      render: (change: number) => (
        <Text type={change > 0 ? 'success' : change < 0 ? 'danger' : undefined}>
          {change > 0 ? '+' : ''}{change?.toLocaleString('en-IN') ?? '0'}
        </Text>
      ),
      sorter: true,
    },
    {
      title: 'Value Change (Cr)',
      dataIndex: 'totalValueChange',
      key: 'totalValueChange',
      width: 140,
      render: (change: number) => (
        <Text type={change > 0 ? 'success' : change < 0 ? 'danger' : undefined}>
          {change > 0 ? '+' : ''}{(change / 10000000)?.toLocaleString('en-IN', { maximumFractionDigits: 2 }) ?? '0.00'}
        </Text>
      ),
      sorter: true,
    },
    {
      title: 'Total Value (Cr)',
      dataIndex: 'totalCurrentValue',
      key: 'totalCurrentValue',
      width: 140,
      render: (value: number) => (
        <Text strong>
          {(value / 10000000)?.toLocaleString('en-IN', { maximumFractionDigits: 2 }) ?? '0.00'}
        </Text>
      ),
      sorter: true,
    },
    {
      title: 'Impact Ratio %',
      dataIndex: 'valueImpactRatio',
      key: 'valueImpactRatio',
      width: 120,
      render: (ratio: number) => (
        <Text type={ratio > 0 ? 'success' : ratio < 0 ? 'danger' : undefined}>
          {ratio > 0 ? '+' : ''}{ratio?.toFixed(2) ?? '0.00'}%
        </Text>
      ),
      sorter: true,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 100,
      render: (record: SecurityMonthlyChangeDto) => (
        <Button
          type="link"
          size="small"
          icon={<EyeOutlined />}
          onClick={() => {
            setSelectedMonthData(record);
            setShowFundDetailsModal(true);
          }}
          style={{ padding: 0, fontSize: '12px' }}
        >
          Details
        </Button>
      ),
    },
  ];

  const isMobile = window.innerWidth < 768;

  // Responsive styles for date picker dropdown across all screen sizes
  React.useEffect(() => {
    const style = document.createElement('style');
    style.setAttribute('data-security-analytics-range-picker', 'true');
    style.innerHTML = `
      /* Mobile styles (360px - 767px) */
      @media (max-width: 767px) {
        .security-analytics-range-picker.ant-picker-dropdown {
          position: fixed !important;
          top: 50% !important;
          left: 50% !important;
          transform: translate(-50%, -50%) !important;
          width: calc(100vw - 32px) !important;
          max-width: 350px !important;
          max-height: 80vh !important;
          z-index: 2000 !important;
          box-shadow: 0 6px 16px 0 rgba(0, 0, 0, 0.08), 0 3px 6px -4px rgba(0, 0, 0, 0.12), 0 9px 28px 8px rgba(0, 0, 0, 0.05) !important;
        }
        .security-analytics-range-picker .ant-picker-panel-container {
          width: 100% !important;
          overflow-y: auto !important;
        }
        .security-analytics-range-picker .ant-picker-panels {
          flex-direction: column !important;
        }
        .security-analytics-range-picker .ant-picker-panel {
          width: 100% !important;
          margin-bottom: 8px !important;
        }
        .security-analytics-range-picker .ant-picker-date-panel {
          width: 100% !important;
        }
        .security-analytics-range-picker .ant-picker-body {
          padding: 12px !important;
        }
        .security-analytics-range-picker .ant-picker-cell {
          padding: 4px !important;
        }
        .security-analytics-range-picker .ant-picker-cell-inner {
          min-width: 36px !important;
          height: 36px !important;
          line-height: 36px !important;
          font-size: 14px !important;
        }
        .security-analytics-range-picker .ant-picker-header {
          padding: 0 12px !important;
        }
        .security-analytics-range-picker .ant-picker-header-view {
          font-size: 14px !important;
        }
      }
      
      /* Tablet styles (768px - 1180px) */
      @media (min-width: 768px) and (max-width: 1180px) {
        .security-analytics-range-picker.ant-picker-dropdown {
          z-index: 1500 !important;
          max-width: 600px !important;
        }
        .security-analytics-range-picker .ant-picker-panels {
          display: flex !important;
          flex-direction: row !important;
        }
        .security-analytics-range-picker .ant-picker-panel {
          width: 280px !important;
        }
        .security-analytics-range-picker .ant-picker-cell-inner {
          min-width: 32px !important;
          height: 32px !important;
          line-height: 32px !important;
        }
      }
      
      /* Desktop styles (1181px+) */
      @media (min-width: 1181px) {
        .security-analytics-range-picker.ant-picker-dropdown {
          z-index: 1000 !important;
        }
      }
    `;
    document.head.appendChild(style);
    return () => { document.head.querySelector('[data-security-analytics-range-picker]')?.remove(); };
  }, []);

  return (
    <div id="security-analytics-container" style={{ padding: isMobile ? '8px' : '0', overflowX: 'hidden' }}>
      <Title level={isMobile ? 3 : 2}>Security Analytics</Title>
      <Text type="secondary" style={{ fontSize: isMobile ? '12px' : '14px' }}>
        Analyze how a specific security has been traded by mutual fund portfolios over time.
      </Text>

      <Row gutter={[16, 16]} style={{ marginTop: isMobile ? 16 : 24 }}>
        <Col span={24}>
          <Card title="Analysis Parameters">
            <Row gutter={[16, 16]}>
              <Col xs={24} sm={24} md={12} lg={12} xl={12}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text strong>Search Security</Text>
                  <Select
                    placeholder="Enter ISIN (e.g., INE040A01034)"
                    value={isinSearchTerm}
                    onChange={(value) => {
                      setIsinSearchTerm(value);
                      // Auto-select security if ISIN matches exactly one result
                      const matchingSecurity = availableSecurities.find(s => s.isin === value);
                      if (matchingSecurity) {
                        setSelectedSecurity(matchingSecurity);
                      }
                    }}
                    onSearch={(value) => setIsinSearchTerm(value)}
                    showSearch
                    filterOption={false}
                    style={{ width: '100%' }}
                    allowClear
                    loading={searchLoading}
                    notFoundContent={searchLoading ? 'Searching...' : 'No ISIN found'}
                  >
                    {availableSecurities
                      .filter(security => security.isin.toLowerCase().includes(isinSearchTerm.toLowerCase()))
                      .map(security => (
                        <Option key={security.isin} value={security.isin}>
                          <Space>
                            <Text>{security.securityName}</Text>
                            <Text type="secondary" style={{ fontSize: '12px' }}>
                              ({security.isin})
                            </Text>
                          </Space>
                        </Option>
                      ))}
                  </Select>
                  <Select
                    style={{ width: '100%' }}
                    placeholder="Select a security to analyze"
                    showSearch
                    filterOption={false}
                    loading={searchLoading}
                    value={selectedSecurity?.isin}
                    onChange={(value) => {
                      const security = availableSecurities.find(s => s.isin === value);
                      setSelectedSecurity(security || null);
                    }}
                    onSearch={(value) => {
                      setSearchTerm(value);
                    }}
                    notFoundContent={searchLoading ? 'Searching...' : 'No securities found'}
                  >
                    {availableSecurities.map(security => (
                      <Option key={`${security.isin}-${security.securityName}`} value={security.isin}>
                        <Space>
                          <Text>{security.securityName}</Text>
                          <Text type="secondary" style={{ fontSize: '12px' }}>
                            ({security.isin})
                          </Text>
                        </Space>
                      </Option>
                    ))}
                  </Select>
                </Space>
              </Col>

              <Col xs={24} sm={24} md={12} lg={12} xl={12}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text strong>Date Range</Text>
                  <RangePicker
                    size={isMobile ? 'large' : 'middle'}
                    style={{ 
                      width: '100%',
                      height: isMobile ? '44px' : 'auto'
                    }}
                    value={dateRange}
                    onChange={(dates) => {
                      if (dates && dates.length === 2) {
                        setDateRange([dates[0]!, dates[1]!]);
                      } else {
                        setDateRange(null);
                      }
                    }}
                    placeholder={['From Date', 'To Date']}
                    format="DD/MM/YYYY"
                    inputReadOnly={isMobile}
                    dropdownClassName="security-analytics-range-picker"
                    getPopupContainer={() => document.body}
                    placement="bottomLeft"
                  />
                </Space>
              </Col>
            </Row>

            <div style={{ marginTop: 16, textAlign: 'right' }}>
              <Button
                type="primary"
                icon={<BarChartOutlined />}
                loading={loading}
                onClick={handleAnalyze}
                disabled={!selectedSecurity || !dateRange}
              >
                Analyze Security Timeline
              </Button>
            </div>
          </Card>
        </Col>

        {analyticsData && (
          <Col span={24}>
            <Card title="Security Summary">
              <Row gutter={[16, 16]}>
                <Col xs={12} sm={12} md={6} lg={6} xl={6}>
                  <Statistic
                    title="Security"
                    value={analyticsData.securityName}
                    prefix={<StockOutlined />}
                  />
                </Col>
                <Col xs={12} sm={12} md={6} lg={6} xl={6}>
                  <Statistic
                    title="ISIN"
                    value={analyticsData.isin}
                  />
                </Col>
                <Col xs={12} sm={12} md={6} lg={6} xl={6}>
                  <Statistic
                    title="Analysis Period"
                    value={`${dayjs(analyticsData.fromDate).format('MMM YYYY')} - ${dayjs(analyticsData.toDate).format('MMM YYYY')}`}
                  />
                </Col>
                <Col xs={12} sm={12} md={6} lg={6} xl={6}>
                  <Statistic
                    title="Total Mutual Funds"
                    value={analyticsData.totalMutualFunds}
                    prefix={<FundOutlined />}
                  />
                </Col>
              </Row>

              <Divider />

              <Row gutter={[16, 16]}>
                <Col xs={12} sm={12} md={6} lg={6} xl={6}>
                  <Statistic
                    title="Months Analyzed"
                    value={analyticsData.summary.totalMonthsAnalyzed}
                  />
                </Col>
                <Col xs={12} sm={12} md={6} lg={6} xl={6}>
                  <Statistic
                    title="Months with Changes"
                    value={analyticsData.summary.monthsWithChanges}
                  />
                </Col>
                <Col xs={12} sm={12} md={6} lg={6} xl={6}>
                  <Statistic
                    title="Funds Involved"
                    value={analyticsData.summary.totalMutualFundsInvolved}
                  />
                </Col>
                <Col xs={12} sm={12} md={6} lg={6} xl={6}>
                  <Statistic
                    title="Total Weight Change"
                    value={analyticsData.summary.totalAbsoluteWeightChange}
                    precision={2}
                    suffix="%"
                    prefix={<SwapOutlined />}
                  />
                </Col>
              </Row>
            </Card>
          </Col>
        )}

        {analyticsData && analyticsData.monthlyChanges.length > 0 && (
          <Col span={24}>
            <Card>
              <div style={{ marginBottom: 16 }}>
                <Space wrap>
                  <Text strong>Sort by:</Text>
                  <Select
                    value={sortBy}
                    onChange={setSortBy}
                    style={{ width: 150 }}
                  >
                    <Option value="monthYear">Month</Option>
                    <Option value="changeType">Change Type</Option>
                    <Option value="mutualFundsCount">Mutual Funds Count</Option>
                    <Option value="totalWeightChange">Weight Change</Option>
                  </Select>

                  <Select
                    value={sortDirection}
                    onChange={setSortDirection}
                    style={{ width: 100 }}
                  >
                    <Option value="asc">Asc</Option>
                    <Option value="desc">Desc</Option>
                  </Select>

                  <Text type="secondary">
                    Showing {analyticsData.monthlyChanges.length} months of data
                  </Text>
                </Space>
              </div>

              <Table
                dataSource={analyticsData.monthlyChanges}
                columns={monthlyChangesColumns}
                rowKey="monthYear"
                loading={loading}
                pagination={{
                  current: currentPage,
                  pageSize: pageSize,
                  total: analyticsData.monthlyChanges.length,
                  showSizeChanger: true,
                  pageSizeOptions: ['5', '10', '20', '50'],
                  showQuickJumper: !isMobile,
                  showTotal: (total, range) =>
                    isMobile ? `${range[0]}-${range[1]}/${total}` : `${range[0]}-${range[1]} of ${total} months`,
                  onChange: handleTableChange,
                  onShowSizeChange: handleTableChange,
                  simple: isMobile,
                  size: isMobile ? 'small' : 'default'
                }}
                scroll={{ x: isMobile ? 600 : 800 }}
                size={isMobile ? 'small' : 'middle'}
              />
            </Card>
          </Col>
        )}

        {analyticsData && analyticsData.monthlyChanges.length === 0 && (
          <Col span={24}>
            <Alert
              message="No Changes Found"
              description="No portfolio changes were found for the selected security in the specified time period."
              type="info"
              showIcon
            />
          </Col>
        )}
      </Row>

      <SecurityChangeDetailsModal 
        visible={showFundDetailsModal} 
        data={selectedMonthData} 
        onClose={() => {
          setShowFundDetailsModal(false);
          setSelectedMonthData(null);
        }}
      />
    </div>
  );
};

export default SecurityAnalytics;
