import React from 'react';
import { Space, Select, Typography, Radio, InputNumber, Divider, Input } from 'antd';

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
  // Filters
  availableSectors: string[];
  selectedSectors: string[];
  onSectorsChange: (value: string[]) => void;
  securityNameSearch: string;
  onSecurityNameSearch: (value: string) => void;
  weightChangeMin?: number;
  onWeightChangeMin: (value: number | null) => void;
  weightChangeMax?: number;
  onWeightChangeMax: (value: number | null) => void;
  minFundsCount?: number;
  onMinFundsCount: (value: number | null) => void;
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
  currentCount,
  // filters
  availableSectors,
  selectedSectors,
  onSectorsChange,
  securityNameSearch,
  onSecurityNameSearch,
  weightChangeMin,
  onWeightChangeMin,
  weightChangeMax,
  onWeightChangeMax,
  minFundsCount,
  onMinFundsCount
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
        style={{ width: 180 }}
      >
        <Option value="securityName">Security Name</Option>
        <Option value="sector">Sector</Option>
        <Option value="industry">Industry</Option>
        <Option value="previousWeight">Previous Weight</Option>
        <Option value="currentWeight">Current Weight</Option>
        <Option value="weightChange">Weight Change</Option>
        <Option value="quantityChange">Quantity Change</Option>
        <Option value="valueChange">Value Change</Option>
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

      <Divider type="vertical" />

      <Text strong>Sector:</Text>
      <Select
        mode="multiple"
        value={selectedSectors}
        onChange={onSectorsChange}
        allowClear
        placeholder="All sectors"
        style={{ minWidth: 180 }}
        maxTagCount={2}
      >
        {availableSectors.map((s) => (
          <Option key={s} value={s}>{s}</Option>
        ))}
      </Select>

      <Text strong>Search Security:</Text>
      <Input
        placeholder="Search by name or ISIN"
        value={securityNameSearch}
        onChange={(e) => onSecurityNameSearch(e.target.value)}
        allowClear
        style={{ width: 220 }}
      />

      <Text strong>Δ Weight (%):</Text>
      <InputNumber
        placeholder="Min"
        value={weightChangeMin}
        onChange={(v) => onWeightChangeMin(v === null ? null : Number(v))}
        style={{ width: 90 }}
      />
      <InputNumber
        placeholder="Max"
        value={weightChangeMax}
        onChange={(v) => onWeightChangeMax(v === null ? null : Number(v))}
        style={{ width: 90 }}
      />

      <Text strong>Min Funds:</Text>
      <InputNumber
        placeholder="e.g. 2"
        value={minFundsCount}
        onChange={(v) => onMinFundsCount(v === null ? null : Number(v))}
        style={{ width: 90 }}
        min={1}
      />
      
      <Text type="secondary">
        Showing {currentCount} of {totalCount} holdings
      </Text>
    </Space>
  );
};

export default HoldingChangesControls;
