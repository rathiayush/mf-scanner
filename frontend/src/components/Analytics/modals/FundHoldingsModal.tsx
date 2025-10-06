import React from 'react';
import { Modal, Button, Space, Alert, Card, Row, Col, Tag, Progress, Divider, Statistic, Typography } from 'antd';

const { Text } = Typography;
import { FundOutlined } from '@ant-design/icons';

interface FundHoldingsModalProps {
  visible: boolean;
  data: any[] | null;
  onClose: () => void;
}

const FundHoldingsModal: React.FC<FundHoldingsModalProps> = ({ visible, data, onClose }) => {
  return (
    <Modal
      title={
        <Space>
          <FundOutlined />
          <Text strong>Mutual Fund Holdings Details</Text>
        </Space>
      }
      open={visible}
      onCancel={onClose}
      footer={[
        <Button key="close" onClick={onClose}>
          Close
        </Button>
      ]}
      width={800}
      style={{ top: 20 }}
    >
      {data && (
        <div style={{ maxHeight: '60vh', overflowY: 'auto' }}>
          <Alert
            message="Fund Weight Distribution"
            description={`Showing holdings across ${data.length} mutual funds`}
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
          />
          
          <Space direction="vertical" style={{ width: '100%' }} size="middle">
            {data
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
                value={data.length}
                prefix={<FundOutlined />}
              />
            </Col>
            <Col span={8}>
              <Statistic
                title="Average Weight"
                value={(data.reduce((sum: number, h: any) => sum + h.weight, 0) / data.length).toFixed(2)}
                suffix="%"
              />
            </Col>
            <Col span={8}>
              <Statistic
                title="Total Weight"
                value={data.reduce((sum: number, h: any) => sum + h.weight, 0).toFixed(2)}
                suffix="%"
              />
            </Col>
          </Row>
        </div>
      )}
    </Modal>
  );
};

export default FundHoldingsModal;
