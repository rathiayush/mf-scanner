import React from 'react';
import { Space, Select, Typography, Radio } from 'antd';

const { Text } = Typography;

const { Option } = Select;

interface HoldingChangesControlsProps {
  selectedStatusFilters: string[];
  onStatusFilterChange: (value: string[]) => void;
  matchMode: 'any' | 'only';
  onMatchModeChange: (value: 'any' | 'only') => void;
  sortBy: string;
  onSortByChange: (value: string) => void;
  sortDirection: string;
  onSortDirectionChange: (value: string) => void;
  totalCount: number;
  currentCount: number;
}

const HoldingChangesControls: React.FC<HoldingChangesControlsProps> = ({
  selectedStatusFilters,
  onStatusFilterChange,
  matchMode,
  onMatchModeChange,
  sortBy,
  onSortByChange,
  sortDirection,
  onSortDirectionChange,
  totalCount,
  currentCount
}) => {
  return (
    <Space wrap>
      <Text strong>Filter by Status:</Text>
      <Select
        mode="multiple"
        value={selectedStatusFilters}
        onChange={onStatusFilterChange}
        allowClear
        placeholder="All statuses"
        style={{ minWidth: 220 }}
      >
        <Option value="Added">Added</Option>
        <Option value="Removed">Removed</Option>
        <Option value="Increased">Increased</Option>
        <Option value="Decreased">Decreased</Option>
        <Option value="Mixed">Mixed</Option>
      </Select>

      <Text strong>Match Mode:</Text>
      <Radio.Group
        value={matchMode}
        onChange={(e) => onMatchModeChange(e.target.value)}
      >
        <Radio.Button value="any">Any</Radio.Button>
        <Radio.Button value="only">Only</Radio.Button>
      </Radio.Group>
      
      <Text strong>Sort by:</Text>
      <Select
        value={sortBy}
        onChange={onSortByChange}
        style={{ width: 150 }}
      >
        <Option value="securityName">Security Name</Option>
        <Option value="sector">Sector</Option>
        <Option value="industry">Industry</Option>
        <Option value="previousWeight">Previous Weight</Option>
        <Option value="currentWeight">Current Weight</Option>
        <Option value="weightChange">Weight Change</Option>
        <Option value="changeType">Change Type</Option>
        <Option value="mutualFundsCount">Mutual Funds Count</Option>
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
        Showing {currentCount} of {totalCount} holdings
      </Text>
    </Space>
  );
};

export default HoldingChangesControls;
