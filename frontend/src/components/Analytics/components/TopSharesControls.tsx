import React from 'react';
import { Space, Select, Typography, Row, Col } from 'antd';

const { Text } = Typography;

const { Option } = Select;

interface TopSharesControlsProps {
  sortBy: string;
  onSortByChange: (value: string) => void;
  sortDirection: string;
  onSortDirectionChange: (value: string) => void;
  totalCount: number;
  currentCount: number;
}

const TopSharesControls: React.FC<TopSharesControlsProps> = ({
  sortBy,
  onSortByChange,
  sortDirection,
  onSortDirectionChange,
  totalCount,
  currentCount
}) => {
  return (
    <div style={{ width: '100%' }}>
      <Row gutter={[8, 8]} align="middle">
        <Col xs={12} sm={12} md={8} lg={6}>
          <Space direction="vertical" size="small" style={{ width: '100%' }}>
            <Text strong style={{ fontSize: '12px' }}>Sort by:</Text>
            <Select
              value={sortBy}
              onChange={onSortByChange}
              style={{ width: '100%' }}
              size="small"
            >
              <Option value="securityName">Security Name</Option>
              <Option value="sector">Sector</Option>
              <Option value="industry">Industry</Option>
              <Option value="totalWeight">Total Weight</Option>
              <Option value="holdingFundsCount">Holding Funds Count</Option>
            </Select>
          </Space>
        </Col>

        <Col xs={12} sm={12} md={6} lg={4}>
          <Space direction="vertical" size="small" style={{ width: '100%' }}>
            <Text strong style={{ fontSize: '12px' }}>Direction:</Text>
            <Select
              value={sortDirection}
              onChange={onSortDirectionChange}
              style={{ width: '100%' }}
              size="small"
            >
              <Option value="asc">Asc</Option>
              <Option value="desc">Desc</Option>
            </Select>
          </Space>
        </Col>

        <Col xs={24} sm={24} md={10} lg={14}>
          <Text type="secondary" style={{ fontSize: '12px', display: 'block', marginTop: '8px' }}>
            Showing {currentCount} of {totalCount} shares
          </Text>
        </Col>
      </Row>
    </div>
  );
};

export default TopSharesControls;
