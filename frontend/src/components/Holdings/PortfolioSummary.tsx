import React, { useState, useEffect } from 'react';
import {
  Card,
  Select,
  DatePicker,
  Button,
  Row,
  Col,
  Typography,
  Space,
  Statistic,
  Progress,
  message,
  Spin,
  Table
} from 'antd';
import {
  PieChartOutlined,
  FundOutlined,
  RiseOutlined
} from '@ant-design/icons';
import { mutualFundService } from '../../services/api';
import { MutualFundDto } from '../../types';
import dayjs from 'dayjs';

const { Title, Text } = Typography;
const { Option } = Select;

const PortfolioSummary: React.FC = () => {
  const [mutualFunds, setMutualFunds] = useState<MutualFundDto[]>([]);
  const [selectedFund, setSelectedFund] = useState<number | undefined>();
  const [selectedDate, setSelectedDate] = useState(dayjs());
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadMutualFunds();
  }, []);

  const loadMutualFunds = async () => {
    try {
      const funds = await mutualFundService.getMutualFundsForDropdown();
      setMutualFunds(funds);
    } catch (error) {
      message.error('Failed to load mutual funds');
    }
  };

  const handleGenerateSummary = () => {
    if (!selectedFund) {
      message.warning('Please select a mutual fund');
      return;
    }
    
    setLoading(true);
    // TODO: Implement portfolio summary logic
    setTimeout(() => {
      setLoading(false);
      message.info('Portfolio summary functionality will be implemented here');
    }, 1000);
  };

  const sectorColumns = [
    {
      title: 'Sector',
      dataIndex: 'sector',
      key: 'sector',
    },
    {
      title: 'Allocation',
      dataIndex: 'allocation',
      key: 'allocation',
      render: (allocation: number) => (
        <Space>
          <Progress percent={allocation} size="small" style={{ width: 100 }} />
          <Text>{allocation}%</Text>
        </Space>
      ),
    },
    {
      title: 'Value',
      dataIndex: 'value',
      key: 'value',
      render: (value: number) => `₹${value?.toLocaleString()}`,
    },
  ];

  const topHoldingsColumns = [
    {
      title: 'Security Name',
      dataIndex: 'securityName',
      key: 'securityName',
    },
    {
      title: 'Weight',
      dataIndex: 'weight',
      key: 'weight',
      render: (weight: number) => `${weight}%`,
    },
    {
      title: 'Value',
      dataIndex: 'value',
      key: 'value',
      render: (value: number) => `₹${value?.toLocaleString()}`,
    },
  ];

  return (
    <div>
      <Title level={2}>Portfolio Summary</Title>
      <Text type="secondary">
        Get comprehensive portfolio summary including sector allocation and top holdings.
      </Text>

      <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
        <Col span={24}>
          <Card title="Summary Parameters">
            <Row gutter={[16, 16]}>
              <Col span={12}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text strong>Select Mutual Fund</Text>
                  <Select
                    style={{ width: '100%' }}
                    placeholder="Select a mutual fund"
                    showSearch
                    filterOption={(input, option) =>
                      (option?.label as string)?.toLowerCase().includes(input.toLowerCase())
                    }
                    value={selectedFund}
                    onChange={setSelectedFund}
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
                  <Text strong>Summary Date</Text>
                  <DatePicker
                    style={{ width: '100%' }}
                    value={selectedDate}
                    onChange={(date) => setSelectedDate(date || dayjs())}
                    placeholder="Select summary date"
                  />
                </Space>
              </Col>
            </Row>

            <div style={{ marginTop: 16, textAlign: 'right' }}>
              <Button
                type="primary"
                icon={<PieChartOutlined />}
                loading={loading}
                onClick={handleGenerateSummary}
                disabled={!selectedFund}
              >
                Generate Summary
              </Button>
            </div>
          </Card>
        </Col>

        <Col span={24}>
          <Row gutter={[16, 16]}>
            <Col span={8}>
              <Card>
                <Statistic
                  title="Total Portfolio Value"
                  value={0}
                  prefix="₹"
                  suffix="Cr"
                  precision={2}
                />
              </Card>
            </Col>
            <Col span={8}>
              <Card>
                <Statistic
                  title="Number of Holdings"
                  value={0}
                  prefix={<FundOutlined />}
                />
              </Card>
            </Col>
            <Col span={8}>
              <Card>
                <Statistic
                  title="Top 10 Holdings %"
                  value={0}
                  suffix="%"
                  precision={2}
                  prefix={<RiseOutlined />}
                />
              </Card>
            </Col>
          </Row>
        </Col>

        <Col span={12}>
          <Card title="Sector Allocation">
            <Spin spinning={loading}>
              <Table
                columns={sectorColumns}
                dataSource={[]}
                pagination={false}
                size="small"
                locale={{ emptyText: 'Generate summary to see sector allocation' }}
              />
            </Spin>
          </Card>
        </Col>

        <Col span={12}>
          <Card title="Top 10 Holdings">
            <Spin spinning={loading}>
              <Table
                columns={topHoldingsColumns}
                dataSource={[]}
                pagination={false}
                size="small"
                locale={{ emptyText: 'Generate summary to see top holdings' }}
              />
            </Spin>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default PortfolioSummary;
