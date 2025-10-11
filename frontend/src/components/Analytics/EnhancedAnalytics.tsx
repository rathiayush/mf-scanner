import React, { useEffect, useMemo } from 'react';
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
import { TopSharesRequestDto } from '../../types';
import dayjs, { Dayjs } from 'dayjs';

// Hooks
import { useAnalyticsState } from './hooks/useAnalyticsState';
import { useSortingAndPagination } from './hooks/useSortingAndPagination';
import { useModalState } from './hooks/useModalState';

// Components
import HoldingChangesControls from './components/HoldingChangesControls';
import TopSharesControls from './components/TopSharesControls';
import PortfolioSnapshotsTab from './components/PortfolioSnapshotsTab';

// Modals
import FundHoldingsModal from './modals/FundHoldingsModal';
import HoldingChangeFundsModal from './modals/HoldingChangeFundsModal';

// Columns
import { getHoldingChangesColumns } from './columns/holdingChangesColumns';
import { getTopSharesColumns } from './columns/topSharesColumns';

// Utils
import { disabledDateLogic } from './utils/dateValidation';

const { Title, Text } = Typography;
const { RangePicker } = DatePicker;
const { Option } = Select;
const { TabPane } = Tabs;

// Add styles for blinking animation
const styles = `
  @keyframes blink {
    0% { opacity: 1; }
    50% { opacity: 0.3; }
    100% { opacity: 1; }
  }
`;

const styleElement = document.createElement('style');
styleElement.textContent = styles;
if (!document.head.querySelector('style[data-analytics-styles]')) {
  styleElement.setAttribute('data-analytics-styles', 'true');
  document.head.appendChild(styleElement);
}

const EnhancedAnalytics: React.FC = () => {
  // State hooks
  const analyticsState = useAnalyticsState();
  const sortingPagination = useSortingAndPagination();
  const modalState = useModalState();

  useEffect(() => {
    loadMutualFunds();
    loadAvailableDates();
  }, []);

  useEffect(() => {
    if (analyticsState.selectedFunds.length > 0 || analyticsState.isAllFundsSelected) {
      loadAvailableDates(analyticsState.isAllFundsSelected ? undefined : analyticsState.selectedFunds);
    }
  }, [analyticsState.selectedFunds, analyticsState.isAllFundsSelected]);

  const loadMutualFunds = async () => {
    try {
      const funds = await mutualFundService.getMutualFundsForDropdown();
      analyticsState.setMutualFunds(funds);
    } catch (error) {
      message.error('Failed to load mutual funds');
    }
  };

  // Debounce security name search to avoid refreshing on every keystroke
  useEffect(() => {
    const handler = setTimeout(() => {
      // Only trigger when we have a valid analysis context
      if (!analyticsState.dateRange || (analyticsState.selectedFunds.length === 0 && !analyticsState.isAllFundsSelected)) return;
      // Always reset to first page on search change
      sortingPagination.setCurrentPage(1);
      loadHoldingChanges(1, sortingPagination.pageSize);
    }, 300);

    return () => clearTimeout(handler);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [sortingPagination.securityNameSearch]);

  const loadAvailableDates = async (fundIds?: number[]) => {
    try {
      const dates = await analyticsService.getAvailableDates(fundIds);
      analyticsState.setAvailableDates(dates);
    } catch (error) {
      message.error('Failed to load available dates');
    }
  };

  const handleAnalyze = async () => {
    sortingPagination.setSelectedStatusFilters([]);
    
    if (analyticsState.selectedFunds.length === 0 && !analyticsState.isAllFundsSelected) {
      message.warning('Please select at least one mutual fund or select "All Funds"');
      return;
    }

    if (!analyticsState.dateRange) {
      message.warning('Please select a date range');
      return;
    }

    try {
      const fundsToValidate = analyticsState.isAllFundsSelected ? [] : analyticsState.selectedFunds;
      
      const validation = await analyticsService.validateDatesForFunds(
        fundsToValidate,
        analyticsState.dateRange[0].format('YYYY-MM-DD'),
        analyticsState.dateRange[1].format('YYYY-MM-DD')
      );

      if (!validation.isValid) {
        message.warning(
          analyticsState.isAllFundsSelected 
            ? 'No snapshot data available for the selected date range. Please select dates where portfolio data exists.'
            : 'No snapshot data available for selected funds on one or both selected dates. Please select dates where all funds have portfolio data.'
        );
        return;
      }
    } catch (error: any) {
      message.error('Failed to validate date availability. Please try again.');
      return;
    }

    analyticsState.setLoading(true);
    try {
      const request = {
        mutualFundIds: analyticsState.isAllFundsSelected ? [] : analyticsState.selectedFunds,
        fromDate: analyticsState.dateRange[0].format('YYYY-MM-DD'),
        toDate: analyticsState.dateRange[1].format('YYYY-MM-DD'),
        page: sortingPagination.currentPage,
        pageSize: sortingPagination.pageSize,
        sortBy: sortingPagination.holdingChangesSortBy,
        sortDirection: sortingPagination.holdingChangesSortDirection,
        changeTypeFilters: sortingPagination.selectedStatusFilters.length > 0 ? sortingPagination.selectedStatusFilters : undefined,
        changeTypeMatchMode: sortingPagination.changeTypeMatchMode,
        sectorFilters: sortingPagination.selectedSectors.length > 0 ? sortingPagination.selectedSectors : undefined,
        securityNameSearch: sortingPagination.securityNameSearch ? sortingPagination.securityNameSearch : undefined,
        weightChangeMin: sortingPagination.weightChangeMin,
        weightChangeMax: sortingPagination.weightChangeMax,
        minFundsCount: sortingPagination.minFundsCount
      };

      const fundsForAnalysis = analyticsState.isAllFundsSelected 
        ? analyticsState.mutualFunds.map(f => f.id) 
        : analyticsState.selectedFunds;
      
      const topSharesRequest: TopSharesRequestDto = {
        mutualFundIds: analyticsState.isAllFundsSelected ? [] : analyticsState.selectedFunds,
        fromDate: request.fromDate,
        toDate: request.toDate,
        page: sortingPagination.topSharesPage,
        pageSize: sortingPagination.topSharesPageSize,
        sortBy: sortingPagination.topSharesSortBy,
        sortDirection: sortingPagination.topSharesSortDirection
      };

      // Old implementation with current holdings API calls
      // const [analytics, changes, shares, fromHoldings, toHoldings] = await Promise.all([
      //   analyticsService.getAnalytics(request),
      //   analyticsService.getHoldingChanges(request),
      //   analyticsService.getTopShares(topSharesRequest),
      //   Promise.all(fundsForAnalysis.map(fundId => 
      //     analyticsService.getCurrentHoldings({
      //       mutualFundId: fundId,
      //       targetDate: request.fromDate,
      //       dateRangeStart: analyticsState.dateRange[0].subtract(30, 'day').format('YYYY-MM-DD'),
      //       dateRangeEnd: analyticsState.dateRange[0].add(30, 'day').format('YYYY-MM-DD'),
      //       page: 1,
      //       pageSize: 100
      //     })
      //   )),
      //   Promise.all(fundsForAnalysis.map(fundId => 
      //     analyticsService.getCurrentHoldings({
      //       mutualFundId: fundId,
      //       targetDate: request.toDate,
      //       dateRangeStart: analyticsState.dateRange[1].subtract(30, 'day').format('YYYY-MM-DD'),
      //       dateRangeEnd: analyticsState.dateRange[1].add(30, 'day').format('YYYY-MM-DD'),
      //       page: 1,
      //       pageSize: 100
      //     })
      //   ))
      // ]);

      // New implementation without current holdings API calls
      const [analytics, changes, shares] = await Promise.all([
        analyticsService.getAnalytics(request),
        analyticsService.getHoldingChanges(request),
        analyticsService.getTopShares(topSharesRequest)
      ]);

      analyticsState.setAnalyticsData(analytics);
      analyticsState.setHoldingChanges(changes);
      analyticsState.setTopShares(shares);
      
      // Initialize empty snapshots array since we're not fetching current holdings
      analyticsState.setCurrentHoldings([]);
      analyticsState.setPortfolioSnapshots([]);
      
      // Old implementation of setting up snapshots from holdings
      // const allHoldings = [...fromHoldings, ...toHoldings].filter(h => h.success);
      // analyticsState.setCurrentHoldings(allHoldings);
      // 
      // const snapshots = allHoldings.map(holding => ({
      //   fundId: holding.mutualFundId,
      //   fundName: holding.mutualFundName,
      //   date: holding.date,
      //   totalValue: holding.totalValue,
      //   holdingsCount: holding.holdingsCount,
      //   holdings: holding.holdings
      // }));
      // analyticsState.setPortfolioSnapshots(snapshots);
    } catch (error: any) {
      message.error(error.message || 'Failed to analyze data');
    } finally {
      analyticsState.setLoading(false);
    }
  };

  const handleTableChange = (page: number, size?: number) => {
    sortingPagination.setCurrentPage(page);
    if (size) sortingPagination.setPageSize(size);
    loadHoldingChanges(page, size || sortingPagination.pageSize);
  };

  const handleStatusFilterChange = (values: string[]) => {
    sortingPagination.setSelectedStatusFilters(values);
    sortingPagination.setCurrentPage(1);
    loadHoldingChanges(1, sortingPagination.pageSize, values);
  };

  const loadTopShares = async (page?: number, pageSize?: number, sortBy?: string, sortDirection?: string) => {
    if (!analyticsState.dateRange || (analyticsState.selectedFunds.length === 0 && !analyticsState.isAllFundsSelected)) return;

    analyticsState.setLoading(true);
    try {
      const topSharesRequest: TopSharesRequestDto = {
        mutualFundIds: analyticsState.isAllFundsSelected ? [] : analyticsState.selectedFunds,
        fromDate: analyticsState.dateRange[0].format('YYYY-MM-DD'),
        toDate: analyticsState.dateRange[1].format('YYYY-MM-DD'),
        page: page || sortingPagination.topSharesPage,
        pageSize: pageSize || sortingPagination.topSharesPageSize,
        sortBy: sortBy || sortingPagination.topSharesSortBy,
        sortDirection: sortDirection || sortingPagination.topSharesSortDirection
      };

      const shares = await analyticsService.getTopShares(topSharesRequest);
      analyticsState.setTopShares(shares);
    } catch (error) {
      message.error('Failed to load top shares');
    } finally {
      analyticsState.setLoading(false);
    }
  };

  const loadHoldingChanges = async (
    page: number,
    size: number,
    filterValues?: string[],
    sortBy?: string,
    sortDirection?: string,
    matchModeOverride?: 'any' | 'only'
  ) => {
    if (!analyticsState.dateRange || (analyticsState.selectedFunds.length === 0 && !analyticsState.isAllFundsSelected)) return;

    analyticsState.setLoading(true);

    try {
      const currentFilters = filterValues !== undefined ? filterValues : sortingPagination.selectedStatusFilters;
      const request = {
        mutualFundIds: analyticsState.isAllFundsSelected ? [] : analyticsState.selectedFunds,
        fromDate: analyticsState.dateRange[0].format('YYYY-MM-DD'),
        toDate: analyticsState.dateRange[1].format('YYYY-MM-DD'),
        page,
        pageSize: size,
        changeTypeFilters: currentFilters && currentFilters.length > 0 ? currentFilters : undefined,
        changeTypeMatchMode: matchModeOverride ?? sortingPagination.changeTypeMatchMode,
        sectorFilters: sortingPagination.selectedSectors.length > 0 ? sortingPagination.selectedSectors : undefined,
        securityNameSearch: sortingPagination.securityNameSearch ? sortingPagination.securityNameSearch : undefined,
        weightChangeMin: sortingPagination.weightChangeMin,
        weightChangeMax: sortingPagination.weightChangeMax,
        minFundsCount: sortingPagination.minFundsCount,
        sortBy: sortBy || sortingPagination.holdingChangesSortBy,
        sortDirection: sortDirection || sortingPagination.holdingChangesSortDirection
      };

      const changes = await analyticsService.getHoldingChanges(request);
      analyticsState.setHoldingChanges(changes);
    } catch (error: any) {
      message.error(error.message || 'Failed to load data');
    } finally {
      analyticsState.setLoading(false);
    }
  };

  const disabledDate = (current: Dayjs) => {
    return disabledDateLogic(
      current,
      analyticsState.selectedFunds,
      analyticsState.isAllFundsSelected,
      analyticsState.availableDates
    );
  };

  const holdingChangesColumns = getHoldingChangesColumns({
    requestSort: sortingPagination.requestSort,
    getSortOrder: sortingPagination.getSortOrder,
    onViewFundDetails: modalState.openHoldingFundsModal
  });

  const topSharesColumns = getTopSharesColumns({
    onViewFundHoldings: modalState.openFundsModal
  });

  // Build available filter values from current page data for dropdown options
  const availableSectors = useMemo(() => {
    const set = new Set<string>();
    (analyticsState.holdingChanges?.data || []).forEach(h => {
      if (h.sector) set.add(h.sector);
    });
    return Array.from(set).sort();
  }, [analyticsState.holdingChanges]);

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
                    autoClearSearchValue={false}
                    defaultActiveFirstOption={false}
                    optionFilterProp="title"
                    value={analyticsState.isAllFundsSelected ? ['all'] : analyticsState.selectedFunds}
                    onChange={(value) => {
                      if (value.includes('all')) {
                        analyticsState.setIsAllFundsSelected(true);
                        analyticsState.setSelectedFunds([]);
                      } else {
                        analyticsState.setIsAllFundsSelected(false);
                        analyticsState.setSelectedFunds(value.filter(v => v !== 'all'));
                      }
                    }}
                  >
                    <Option key="all" value="all" title="All Funds">
                      <Space>
                        <Text strong style={{ color: '#1890ff' }}>All Funds</Text>
                        <Text type="secondary">(Analyze all available funds)</Text>
                      </Space>
                    </Option>
                    {analyticsState.mutualFunds.map(fund => (
                      <Option key={fund.id} value={fund.id} title={`${fund.fundName} (${fund.fundCode})`}>
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
                    value={analyticsState.dateRange}
                    onChange={(dates) => {
                      if (dates && dates.length === 2) {
                        analyticsState.setDateRange([dates[0]!, dates[1]!]);
                      } else {
                        analyticsState.setDateRange(null);
                      }
                    }}
                    disabledDate={disabledDate}
                    placeholder={['From Date', 'To Date']}
                  />
                  {analyticsState.availableDates && (
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      Available: {analyticsState.availableDates.minDate} to {analyticsState.availableDates.maxDate}
                    </Text>
                  )}
                </Space>
              </Col>
            </Row>

            <div style={{ marginTop: 16, textAlign: 'right' }}>
              <Button
                type="primary"
                icon={<BarChartOutlined />}
                loading={analyticsState.loading}
                onClick={handleAnalyze}
                disabled={(analyticsState.selectedFunds.length === 0 && !analyticsState.isAllFundsSelected) || !analyticsState.dateRange}
              >
                Analyze Portfolio
              </Button>
            </div>
          </Card>
        </Col>

        {analyticsState.analyticsData && (
          <Col span={24}>
            <Card title="Analytics Summary">
              <Row gutter={[16, 16]}>
                <Col span={6}>
                  <Statistic
                    title="Total Holdings Analyzed"
                    value={analyticsState.analyticsData.summary.totalHoldingsAnalyzed}
                    prefix={<FundOutlined />}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="Added Holdings"
                    value={analyticsState.analyticsData.summary.addedHoldings}
                    valueStyle={{ color: '#3f8600' }}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="Removed Holdings"
                    value={analyticsState.analyticsData.summary.removedHoldings}
                    valueStyle={{ color: '#cf1322' }}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="Total Weight Change"
                    value={analyticsState.analyticsData.summary.totalWeightChange}
                    precision={2}
                    suffix="%"
                    prefix={<SwapOutlined />}
                  />
                </Col>
              </Row>
            </Card>
          </Col>
        )}

        {(analyticsState.holdingChanges || (analyticsState.topShares && analyticsState.topShares.data.length > 0)) && (
          <Col span={24}>
            <Card>
              <Tabs defaultActiveKey="changes">
                <TabPane 
                  tab={
                    <span>
                      <SwapOutlined />
                      Holding Changes ({analyticsState.holdingChanges?.totalCount || 0})
                    </span>
                  } 
                  key="changes"
                >
                  {analyticsState.holdingChanges && (
                    <div>
                      <div style={{ marginBottom: 16 }}>
                        <HoldingChangesControls
                          selectedStatusFilters={sortingPagination.selectedStatusFilters}
                          onStatusFilterChange={handleStatusFilterChange}
                          matchMode={sortingPagination.changeTypeMatchMode}
                          onMatchModeChange={(value) => {
                            console.log('[HoldingChanges] onMatchModeChange', value);
                            sortingPagination.setChangeTypeMatchMode(value);
                            sortingPagination.setCurrentPage(1);
                            loadHoldingChanges(
                              1,
                              sortingPagination.pageSize,
                              sortingPagination.selectedStatusFilters,
                              sortingPagination.holdingChangesSortBy,
                              sortingPagination.holdingChangesSortDirection,
                              value
                            );
                          }}
                          sortBy={sortingPagination.holdingChangesSortBy}
                          onSortByChange={(value) => {
                            sortingPagination.setHoldingChangesSortBy(value);
                            sortingPagination.setCurrentPage(1);
                            loadHoldingChanges(1, sortingPagination.pageSize, sortingPagination.selectedStatusFilters, value, sortingPagination.holdingChangesSortDirection);
                          }}
                          sortDirection={sortingPagination.holdingChangesSortDirection}
                          onSortDirectionChange={(value) => {
                            sortingPagination.setHoldingChangesSortDirection(value);
                            sortingPagination.setCurrentPage(1);
                            loadHoldingChanges(1, sortingPagination.pageSize, sortingPagination.selectedStatusFilters, sortingPagination.holdingChangesSortBy, value);
                          }}
                          totalCount={analyticsState.holdingChanges?.totalCount || 0}
                          currentCount={analyticsState.holdingChanges?.data.length || 0}
                          // filters
                          availableSectors={availableSectors}
                          selectedSectors={sortingPagination.selectedSectors}
                          onSectorsChange={(v) => sortingPagination.setSelectedSectors(v)}
                          securityNameSearch={sortingPagination.securityNameSearch}
                          onSecurityNameSearch={(v: string) => {
                            // Update state only; debounced effect below will trigger API call
                            sortingPagination.setSecurityNameSearch(v);
                            sortingPagination.setCurrentPage(1);
                          }}
                          weightChangeMin={sortingPagination.weightChangeMin}
                          onWeightChangeMin={(v) => sortingPagination.setWeightChangeMin(v === null ? undefined : v)}
                          weightChangeMax={sortingPagination.weightChangeMax}
                          onWeightChangeMax={(v) => sortingPagination.setWeightChangeMax(v === null ? undefined : v)}
                          minFundsCount={sortingPagination.minFundsCount}
                          onMinFundsCount={(v) => sortingPagination.setMinFundsCount(v === null ? undefined : v)}
                        />
                      </div>
                      <Table
                        dataSource={analyticsState.holdingChanges?.data || []}
                        columns={holdingChangesColumns}
                        rowKey="isin"
                        loading={analyticsState.loading}
                        className="sticky-table"
                        pagination={{
                          current: analyticsState.holdingChanges?.page || 1,
                          pageSize: analyticsState.holdingChanges?.pageSize || 20,
                          total: analyticsState.holdingChanges?.totalCount || 0,
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
                      Top Shares ({analyticsState.topShares?.totalCount || 0})
                    </span>
                  } 
                  key="topShares"
                >
                  <div>
                    <div style={{ marginBottom: 16 }}>
                      <TopSharesControls
                        sortBy={sortingPagination.topSharesSortBy}
                        onSortByChange={(value) => {
                          sortingPagination.setTopSharesSortBy(value);
                          loadTopShares(1, sortingPagination.topSharesPageSize, value, sortingPagination.topSharesSortDirection);
                        }}
                        sortDirection={sortingPagination.topSharesSortDirection}
                        onSortDirectionChange={(value) => {
                          sortingPagination.setTopSharesSortDirection(value);
                          loadTopShares(1, sortingPagination.topSharesPageSize, sortingPagination.topSharesSortBy, value);
                        }}
                        totalCount={analyticsState.topShares?.totalCount || 0}
                        currentCount={analyticsState.topShares?.data.length || 0}
                      />
                    </div>
                    
                    <Table
                      columns={topSharesColumns}
                      dataSource={analyticsState.topShares?.data || []}
                      className="sticky-table"
                      pagination={{
                        current: analyticsState.topShares?.page || 1,
                        pageSize: analyticsState.topShares?.pageSize || 20,
                        total: analyticsState.topShares?.totalCount || 0,
                        showSizeChanger: true,
                        showQuickJumper: true,
                        showTotal: (total, range) =>
                          `${range[0]}-${range[1]} of ${total} shares`,
                        onChange: (page, pageSize) => {
                          sortingPagination.setTopSharesPage(page);
                          if (pageSize !== sortingPagination.topSharesPageSize) {
                            sortingPagination.setTopSharesPageSize(pageSize);
                          }
                          loadTopShares(page, pageSize, sortingPagination.topSharesSortBy, sortingPagination.topSharesSortDirection);
                        },
                        onShowSizeChange: (_, size) => {
                          sortingPagination.setTopSharesPage(1);
                          sortingPagination.setTopSharesPageSize(size);
                          loadTopShares(1, size, sortingPagination.topSharesSortBy, sortingPagination.topSharesSortDirection);
                        },
                      }}
                      rowKey="isin"
                      scroll={{ x: 1000 }}
                    />
                  </div>
                </TabPane>

                <TabPane 
                  tab={
                    <span>
                      <FundOutlined />
                      Portfolio Snapshots ({analyticsState.portfolioSnapshots.length})
                    </span>
                  } 
                  key="snapshots"
                >
                  <PortfolioSnapshotsTab snapshots={analyticsState.portfolioSnapshots} />
                </TabPane>
              </Tabs>
            </Card>
          </Col>
        )}

        {analyticsState.selectedFunds.length === 0 && !analyticsState.isAllFundsSelected && (
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

      <FundHoldingsModal
        visible={modalState.showFundsModal}
        data={modalState.selectedModalData}
        onClose={modalState.closeFundsModal}
      />

      <HoldingChangeFundsModal
        visible={modalState.showHoldingFundsModal}
        data={modalState.selectedHoldingModalData}
        filterType={modalState.modalFilterType}
        onFilterChange={modalState.setModalFilterType}
        onClose={modalState.closeHoldingFundsModal}
      />
    </div>
  );
};

export default EnhancedAnalytics;
