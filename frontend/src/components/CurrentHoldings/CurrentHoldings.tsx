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
  message
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import {
  MutualFundDto,
  CurrentHoldingDto,
  CurrentHoldingsResponseDto
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

  useEffect(() => {
    loadMutualFunds();
  }, []);

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

  const handleViewHoldings = async () => {
    if (!selectedFund || !selectedDate) {
      message.warning('Please select both a mutual fund and date');
      return;
    }

    setLoading(true);
    try {
      const response = await analyticsService.getCurrentHoldings({
        mutualFundId: selectedFund,
        date: selectedDate.format('YYYY-MM-DD')
      });

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
          <Space size="large" wrap>
            <div>
              <Text strong>Mutual Fund:</Text>
              <Select
                style={{ width: 300, marginTop: 8 }}
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

            <div>
              <Text strong>As of Date:</Text>
              <DatePicker
                style={{ width: 200, marginTop: 8 }}
                placeholder="Select date"
                value={selectedDate}
                onChange={setSelectedDate}
                disabledDate={(current) => current && current > dayjs().endOf('day')}
              />
            </div>

            <Button
              type="primary"
              onClick={handleViewHoldings}
              loading={loading}
              disabled={!selectedFund || !selectedDate}
              style={{ marginTop: 32 }}
            >
              View Holdings
            </Button>
          </Space>
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
