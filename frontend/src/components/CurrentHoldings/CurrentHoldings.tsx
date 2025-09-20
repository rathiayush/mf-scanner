import React, { useState, useEffect } from 'react';
import {
  Card,
  Select,
  DatePicker,
  Button,
  Table,
  Spin,
  Alert,
  Typography,
  Space,
  Tag,
  Tooltip,
  message,
  Row,
  Col
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import {
  MutualFundDto,
  CurrentHoldingDto,
  CurrentHoldingsResponseDto,
  AvailableDatesResponseDto
} from '../../types';
import { mutualFundService, analyticsService } from '../../services/api';

const { Title, Text } = Typography;
const { Option } = Select;

interface CurrentHoldingsProps {
  className?: string;
}

const CurrentHoldings: React.FC<CurrentHoldingsProps> = ({ className }) => {
  const [mutualFunds, setMutualFunds] = useState<MutualFundDto[]>([]);
  const [selectedFund, setSelectedFund] = useState<number | null>(null);
  const [selectedDate, setSelectedDate] = useState<dayjs.Dayjs | null>(null);
  const [holdings, setHoldings] = useState<CurrentHoldingDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [fundLoading, setFundLoading] = useState(false);
  const [holdingsResponse, setHoldingsResponse] = useState<CurrentHoldingsResponseDto | null>(null);
  const [availableDates, setAvailableDates] = useState<AvailableDatesResponseDto | null>(null);
  const [datesLoading, setDatesLoading] = useState(false);

  useEffect(() => {
    loadMutualFunds();
  }, []);

  useEffect(() => {
    if (selectedFund) {
      loadAvailableDates();
    } else {
      setAvailableDates(null);
      setSelectedDate(null);
    }
  }, [selectedFund]);

  const loadMutualFunds = async () => {
    setFundLoading(true);
    try {
      const funds = await mutualFundService.getMutualFundsForDropdown();
      setMutualFunds(funds);
    } catch (error) {
      message.error('Failed to load mutual funds');
    } finally {
      setFundLoading(false);
    }
  };

  const loadAvailableDates = async () => {
    if (!selectedFund) return;
    
    setDatesLoading(true);
    try {
      const response = await analyticsService.getAvailableDates([selectedFund]);
      setAvailableDates(response);
      
      // Auto-select the latest available date if none is selected
      if (!selectedDate && response.availableDates.length > 0) {
        const latestDate = response.availableDates
          .filter(d => d.hasData)
          .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())[0];
        
        if (latestDate) {
          setSelectedDate(dayjs(latestDate.date));
        }
      }
    } catch (error) {
      message.error('Failed to load available dates');
      setAvailableDates(null);
    } finally {
      setDatesLoading(false);
    }
  };

  const handleViewHoldings = async () => {
    if (!selectedFund || !selectedDate) {
      message.warning('Please select both a mutual fund and date');
      return;
    }

    // Validate that the selected date has snapshot data
    const selectedDateStr = selectedDate.format('YYYY-MM-DD');
    const isDateValid = availableDates?.availableDates.some(d => 
      d.date === selectedDateStr && d.hasData
    );
    
    if (!isDateValid) {
      message.error('No snapshot data available for the selected date. Please select a date with available data.');
      return;
    }

    setLoading(true);
    try {
      console.log('Fetching holdings for fund:', selectedFund, 'date:', selectedDateStr);
      
      const response = await analyticsService.getCurrentHoldings({
        mutualFundId: selectedFund,
        date: selectedDateStr
      });

      console.log('Holdings response:', response);

      if (response.success) {
        setHoldingsResponse(response);
        setHoldings(response.holdings);
        message.success('Holdings loaded successfully');
      } else {
        message.error(response.message || 'Failed to load holdings');
        setHoldingsResponse(null);
        setHoldings([]);
      }
    } catch (error) {
      console.error('Error loading holdings:', error);
      message.error('Failed to load holdings');
      setHoldingsResponse(null);
      setHoldings([]);
    } finally {
      setLoading(false);
    }
  };

  const columns: ColumnsType<CurrentHoldingDto> = [
    {
      title: 'Symbol',
      dataIndex: 'symbol',
      key: 'symbol',
      width: 120,
      render: (text: string) => (
        <Tooltip title={text}>
          <Tag color="blue">{text}</Tag>
        </Tooltip>
      ),
    },
    {
      title: 'Company Name',
      dataIndex: 'companyName',
      key: 'companyName',
      width: 200,
      ellipsis: true,
      render: (text: string) => (
        <Tooltip title={text}>
          <Text>{text}</Text>
        </Tooltip>
      ),
    },
    {
      title: 'Quantity',
      dataIndex: 'quantity',
      key: 'quantity',
      width: 120,
      align: 'right',
      render: (value: number) => value.toLocaleString(),
    },
    {
      title: 'Price',
      dataIndex: 'price',
      key: 'price',
      width: 120,
      align: 'right',
      render: (value: number) => `₹${value.toFixed(2)}`,
    },
    {
      title: 'Value',
      dataIndex: 'value',
      key: 'value',
      width: 150,
      align: 'right',
      render: (value: number) => `₹${value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`,
      sorter: (a, b) => a.value - b.value,
      defaultSortOrder: 'descend',
    },
    {
      title: 'Weight',
      dataIndex: 'weight',
      key: 'weight',
      width: 100,
      align: 'right',
      render: (value: number) => `${value.toFixed(2)}%`,
      sorter: (a, b) => a.weight - b.weight,
    },
    {
      title: 'Sector',
      dataIndex: 'sector',
      key: 'sector',
      width: 150,
      render: (text: string) => text ? (
        <Tooltip title={text}>
          <Tag color="green">{text}</Tag>
        </Tooltip>
      ) : '-',
    },
    {
      title: 'Industry',
      dataIndex: 'industry',
      key: 'industry',
      width: 150,
      render: (text: string) => text ? (
        <Tooltip title={text}>
          <Tag color="orange">{text}</Tag>
        </Tooltip>
      ) : '-',
    },
  ];


  return (
    <div className={className}>
      <Card>
        <Title level={3}>Current Holdings</Title>
        <Text type="secondary">
          View all current shares held by a mutual fund on a specific date
        </Text>

        <div style={{ marginTop: 24, marginBottom: 24 }}>
          <Row gutter={[24, 16]} align="bottom">
            <Col xs={24} sm={12} md={8} lg={8}>
              <div>
                <Text strong>Mutual Fund:</Text>
                <Select
                  style={{ width: '100%', marginTop: 8 }}
                  placeholder="Select a mutual fund"
                  loading={fundLoading}
                  value={selectedFund}
                  onChange={setSelectedFund}
                  showSearch
                  filterOption={(input, option) =>
                    (option?.children as unknown as string)?.toLowerCase().includes(input.toLowerCase())
                  }
                >
                  {mutualFunds.map(fund => (
                    <Option key={fund.id} value={fund.id}>
                      {fund.fundName} ({fund.fundCode})
                    </Option>
                  ))}
                </Select>
              </div>
            </Col>

            <Col xs={24} sm={12} md={8} lg={8}>
              <div>
                <Text strong>As of Date:</Text>
                <DatePicker
                  style={{ width: '100%', marginTop: 8 }}
                  placeholder="Select date"
                  value={selectedDate}
                  onChange={setSelectedDate}
                  disabled={!selectedFund || datesLoading}
                  disabledDate={(current) => {
                    // Disable future dates
                    if (current && current > dayjs().endOf('day')) {
                      return true;
                    }
                    
                    // Disable dates without available snapshot data
                    if (availableDates && selectedFund) {
                      const dateStr = current.format('YYYY-MM-DD');
                      const isDateAvailable = availableDates.availableDates.some(d => 
                        d.date === dateStr && d.hasData
                      );
                      return !isDateAvailable;
                    }
                    
                    return false;
                  }}
                />
                {datesLoading && (
                  <div style={{ marginTop: 4 }}>
                    <Text type="secondary" style={{ fontSize: 12 }}>
                      Loading available dates...
                    </Text>
                  </div>
                )}
              </div>
            </Col>

            <Col xs={24} sm={24} md={8} lg={8}>
              <Button
                type="primary"
                onClick={handleViewHoldings}
                loading={loading}
                disabled={!selectedFund || !selectedDate}
                style={{ width: '100%', marginTop: 32 }}
                size="large"
              >
                View Holdings
              </Button>
            </Col>
          </Row>
        </div>

        {holdingsResponse && (
          <Alert
            message={`${holdingsResponse.mutualFundName} - Holdings as of ${dayjs(holdingsResponse.date).format('MMM DD, YYYY')}`}
            description={
              <Space size="large">
                <Text strong>Total Value: ₹{holdingsResponse.totalValue.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</Text>
                <Text strong>Number of Holdings: {holdingsResponse.holdingsCount}</Text>
              </Space>
            }
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
          />
        )}

        {loading ? (
          <div style={{ textAlign: 'center', padding: '50px' }}>
            <Spin size="large" />
            <div style={{ marginTop: 16 }}>
              <Text>Loading holdings...</Text>
            </div>
          </div>
        ) : holdings.length > 0 ? (
          <Table
            columns={columns}
            dataSource={holdings}
            rowKey="id"
            pagination={{
              pageSize: 20,
              showSizeChanger: true,
              showQuickJumper: true,
              showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} holdings`,
            }}
            scroll={{ x: 1200, y: 600 }}
            size="middle"
            bordered
          />
        ) : holdingsResponse ? (
          <Alert
            message="No Holdings Found"
            description="No holdings data found for the selected mutual fund and date."
            type="warning"
            showIcon
          />
        ) : (
          <Alert
            message="Select Fund and Date"
            description="Please select a mutual fund and date to view current holdings."
            type="info"
            showIcon
          />
        )}
      </Card>
    </div>
  );
};

export default CurrentHoldings;
