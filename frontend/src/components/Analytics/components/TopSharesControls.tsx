import React from 'react';
import { Space, Select, Typography } from 'antd';

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
    <Space wrap>
      <Text strong>Sort by:</Text>
      <Select
        value={sortBy}
        onChange={onSortByChange}
        style={{ width: 150 }}
      >
        <Option value="securityName">Security Name</Option>
        <Option value="sector">Sector</Option>
        <Option value="industry">Industry</Option>
        <Option value="totalWeight">Total Weight</Option>
        <Option value="holdingFundsCount">Holding Funds Count</Option>
      </Select>
      
      <Select
        value={sortDirection}
        onChange={onSortDirectionChange}
        style={{ width: 100 }}
      >
        <Option value="asc">Asc</Option>
        <Option value="desc">Desc</Option>
      </Select>
      
      <Text type="secondary">
        Showing {currentCount} of {totalCount} shares
      </Text>
    </Space>
  );
};

export default TopSharesControls;
