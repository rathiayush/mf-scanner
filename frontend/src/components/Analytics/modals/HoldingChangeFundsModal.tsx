import React from 'react';
import { Modal, Button, Space, Alert, Card, Row, Col, Tag, Select, Divider, Statistic, Typography } from 'antd';
const { Text } = Typography;
import { SwapOutlined, FundOutlined } from '@ant-design/icons';
import { getChangeTypeColor } from '../utils/trendCalculator';

const { Option } = Select;

interface HoldingChangeFundsModalProps {
  visible: boolean;
  data: any | null;
  filterType: string;
  onFilterChange: (value: string) => void;
  onClose: () => void;
}

const HoldingChangeFundsModal: React.FC<HoldingChangeFundsModalProps> = ({ 
  visible, 
  data, 
  filterType,
  onFilterChange,
  onClose 
}) => {
  return (
    <Modal
      title={
        <Space>
          <SwapOutlined />
          <Text strong>Holding Change Fund Details</Text>
        </Space>
      }
      open={visible}
      onCancel={onClose}
      footer={[
        <Button key="close" onClick={onClose}>
          Close
        </Button>
      ]}
      width={900}
      style={{ top: 20 }}
    >
      {data && (
        <div style={{ maxHeight: '60vh', overflowY: 'auto' }}>
          <Alert
            message={`Holding Change: ${data.securityName}`}
            description={
              <Space direction="vertical" size={2}>
                <Text>ISIN: {data.isin}</Text>
                <Text>Sector: {data.sector}</Text>
                <Space>
                  <Tag color={getChangeTypeColor(data.changeType)}>
                    {data.changeType}
                  </Tag>
                  {data.changeType === 'Mixed' && data.constituentChangeTypes && (
                    <Space>
                      <Text type="secondary">Contains:</Text>
                      {data.constituentChangeTypes.map((type: string) => (
                        <Tag 
                          key={type}
                          color={getChangeTypeColor(type)}
                          style={{ fontSize: '11px', padding: '1px 6px' }}
                        >
                          {type}
                        </Tag>
                      ))}
                    </Space>
                  )}
                </Space>
              </Space>
            }
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
          />
          
          <Row style={{ marginBottom: 16 }}>
            <Col span={24}>
              <Space>
                <Text strong>Filter by Change Type:</Text>
                <Select
                  value={filterType}
                  onChange={onFilterChange}
                  style={{ width: 150 }}
                  size="small"
                >
                  <Option value="all">All Funds</Option>
                  <Option value="Added">Added</Option>
                  <Option value="Removed">Removed</Option>
                  <Option value="Increased">Increased</Option>
                  <Option value="Decreased">Decreased</Option>
                </Select>
              </Space>
            </Col>
          </Row>
          
          <Space direction="vertical" style={{ width: '100%' }} size="middle">
            {data.fundChangeDetails
              ?.filter((fund: any) => filterType === 'all' || fund.changeType === filterType)
              ?.map((fund: any, index: number) => (
              <Card 
                key={fund.mutualFundId} 
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
                        {fund.mutualFundName}
                      </Text>
                      <Text type="secondary" style={{ fontSize: '12px' }}>
                        Fund ID: {fund.mutualFundId}
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
                      <Tag 
                        color={getChangeTypeColor(fund.changeType)}
                        style={{ fontSize: '11px' }}
                      >
                        {fund.changeType}
                      </Tag>
                    </Space>
                  </Col>
                </Row>
              </Card>
            )) || []}
          </Space>
          
          <Divider />
          
          <Row gutter={[16, 16]}>
            <Col span={4}>
              <Statistic
                title="Total Funds"
                value={data.fundChangeDetails?.length || 0}
                prefix={<FundOutlined />}
              />
            </Col>
            <Col span={5}>
              <Statistic
                title="Weight Change"
                value={data.weightChange?.toFixed(2) || '0.00'}
                suffix="%"
                valueStyle={{ 
                  color: (data.weightChange || 0) > 0 ? '#52c41a' : 
                         (data.weightChange || 0) < 0 ? '#ff4d4f' : '#666666'
                }}
              />
            </Col>
            <Col span={5}>
              <Statistic
                title="Quantity Change"
                value={data.quantityChange?.toLocaleString() || '0'}
                valueStyle={{ 
                  color: (data.quantityChange || 0) > 0 ? '#52c41a' : 
                         (data.quantityChange || 0) < 0 ? '#ff4d4f' : '#666666'
                }}
              />
            </Col>
            <Col span={5}>
              <Statistic
                title="Value Change"
                value={`₹${Math.abs(data.valueChange || 0).toLocaleString('en-IN', { maximumFractionDigits: 0 })}`}
                valueStyle={{ 
                  color: (data.valueChange || 0) > 0 ? '#52c41a' : 
                         (data.valueChange || 0) < 0 ? '#ff4d4f' : '#666666'
                }}
              />
            </Col>
            <Col span={5}>
              <Statistic
                title="Current Weight"
                value={data.currentWeight?.toFixed(2) || '0.00'}
                suffix="%"
                valueStyle={{ color: '#1890ff' }}
              />
            </Col>
          </Row>
        </div>
      )}
    </Modal>
  );
};

export default HoldingChangeFundsModal;
