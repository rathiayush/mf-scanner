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
  message,
  Spin
} from 'antd';
import {
  CompressOutlined,
  FundOutlined
} from '@ant-design/icons';
import { mutualFundService } from '../../services/api';
import { MutualFundDto } from '../../types';
import dayjs from 'dayjs';

const { Title, Text } = Typography;
const { Option } = Select;

const HoldingsComparison: React.FC = () => {
  const [mutualFunds, setMutualFunds] = useState<MutualFundDto[]>([]);
  const [selectedFunds, setSelectedFunds] = useState<number[]>([]);
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

  const handleCompare = () => {
    if (selectedFunds.length < 2) {
      message.warning('Please select at least 2 mutual funds to compare');
      return;
    }
    
    setLoading(true);
    // TODO: Implement comparison logic
    setTimeout(() => {
      setLoading(false);
      message.info('Holdings comparison functionality will be implemented here');
    }, 1000);
  };

  const columns = [
    {
      title: 'Security Name',
      dataIndex: 'securityName',
      key: 'securityName',
    },
    {
      title: 'Fund 1 Weight',
      dataIndex: 'fund1Weight',
      key: 'fund1Weight',
      render: (weight: number) => weight ? `${weight.toFixed(2)}%` : '-',
    },
    {
      title: 'Fund 2 Weight',
      dataIndex: 'fund2Weight',
      key: 'fund2Weight',
      render: (weight: number) => weight ? `${weight.toFixed(2)}%` : '-',
    },
    {
      title: 'Difference',
      dataIndex: 'difference',
      key: 'difference',
      render: (diff: number) => (
        <Tag color={diff > 0 ? 'green' : diff < 0 ? 'red' : 'default'}>
          {diff > 0 ? '+' : ''}{diff?.toFixed(2)}%
        </Tag>
      ),
    },
  ];

  return (
    <div>
      <Title level={2}>Holdings Comparison</Title>
      <Text type="secondary">
        Compare portfolio holdings between multiple mutual funds on a specific date.
      </Text>

      <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
        <Col span={24}>
          <Card title="Comparison Parameters">
            <Row gutter={[16, 16]}>
              <Col span={12}>
                <Space direction="vertical" style={{ width: '100%' }}>
                  <Text strong>Select Mutual Funds (minimum 2)</Text>
                  <Select
                    mode="multiple"
                    style={{ width: '100%' }}
                    placeholder="Select mutual funds to compare"
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
                  <Text strong>Comparison Date</Text>
                  <DatePicker
                    style={{ width: '100%' }}
                    value={selectedDate}
                    onChange={(date) => setSelectedDate(date || dayjs())}
                    placeholder="Select comparison date"
                  />
                </Space>
              </Col>
            </Row>

            <div style={{ marginTop: 16, textAlign: 'right' }}>
              <Button
                type="primary"
                icon={<CompressOutlined />}
                loading={loading}
                onClick={handleCompare}
                disabled={selectedFunds.length < 2}
              >
                Compare Holdings
              </Button>
            </div>
          </Card>
        </Col>

        <Col span={24}>
          <Card title="Comparison Results">
            <Spin spinning={loading}>
              <Table
                columns={columns}
                dataSource={[]}
                pagination={{ pageSize: 20 }}
                locale={{ emptyText: 'Select funds and click "Compare Holdings" to see results' }}
              />
            </Spin>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default HoldingsComparison;
