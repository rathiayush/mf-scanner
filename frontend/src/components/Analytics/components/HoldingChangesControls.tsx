import React from 'react';
import { Space, Select, Typography, Radio, InputNumber, Input, Row, Col } from 'antd';

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
    <div style={{ width: '100%' }}>
      <Row gutter={[8, 8]}>
        <Col xs={24} sm={12} md={8} lg={6}>
          <Space direction="vertical" size="small" style={{ width: '100%' }}>
            <Text strong style={{ fontSize: '12px' }}>Filter by Status:</Text>
            <Select
              mode="multiple"
              value={selectedStatusFilters}
              onChange={onStatusFilterChange}
              allowClear
              placeholder="All statuses"
              style={{ width: '100%' }}
              size="small"
            >
              <Option value="Added">Added</Option>
              <Option value="Removed">Removed</Option>
              <Option value="Increased">Increased</Option>
              <Option value="Decreased">Decreased</Option>
              <Option value="Mixed">Mixed</Option>
            </Select>
          </Space>
        </Col>

        <Col xs={24} sm={12} md={8} lg={4}>
          <Space direction="vertical" size="small" style={{ width: '100%' }}>
            <Text strong style={{ fontSize: '12px' }}>Match Mode:</Text>
            <Radio.Group
              value={matchMode}
              onChange={(e) => onMatchModeChange(e.target.value)}
              size="small"
              style={{ width: '100%' }}
            >
              <Radio.Button value="any">Any</Radio.Button>
              <Radio.Button value="only">Only</Radio.Button>
            </Radio.Group>
          </Space>
        </Col>

        <Col xs={12} sm={12} md={8} lg={5}>
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
              <Option value="previousWeight">Previous Weight</Option>
              <Option value="currentWeight">Current Weight</Option>
              <Option value="weightChange">Weight Change</Option>
              <Option value="quantityChange">Quantity Change</Option>
              <Option value="valueChange">Value Change</Option>
              <Option value="changeType">Change Type</Option>
              <Option value="mutualFundsCount">Mutual Funds Count</Option>
              <Option value="valueImpactRatio">Value Impact Ratio</Option>
            </Select>
          </Space>
        </Col>

        <Col xs={12} sm={12} md={8} lg={3}>
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

        <Col xs={24} sm={12} md={8} lg={6}>
          <Space direction="vertical" size="small" style={{ width: '100%' }}>
            <Text strong style={{ fontSize: '12px' }}>Sector:</Text>
            <Select
              mode="multiple"
              value={selectedSectors}
              onChange={onSectorsChange}
              allowClear
              placeholder="All sectors"
              style={{ width: '100%' }}
              maxTagCount={1}
              size="small"
            >
              {availableSectors.map((s) => (
                <Option key={s} value={s}>{s}</Option>
              ))}
            </Select>
          </Space>
        </Col>

        <Col xs={24} sm={12} md={8} lg={6}>
          <Space direction="vertical" size="small" style={{ width: '100%' }}>
            <Text strong style={{ fontSize: '12px' }}>Search Security:</Text>
            <Input
              placeholder="Search by name or ISIN"
              value={securityNameSearch}
              onChange={(e) => onSecurityNameSearch(e.target.value)}
              allowClear
              style={{ width: '100%' }}
              size="small"
            />
          </Space>
        </Col>

        <Col xs={12} sm={8} md={6} lg={4}>
          <Space direction="vertical" size="small" style={{ width: '100%' }}>
            <Text strong style={{ fontSize: '12px' }}>Δ Weight (%):</Text>
            <Space.Compact style={{ width: '100%' }}>
              <InputNumber
                placeholder="Min"
                value={weightChangeMin}
                onChange={(v) => onWeightChangeMin(v === null ? null : Number(v))}
                style={{ width: '50%' }}
                size="small"
              />
              <InputNumber
                placeholder="Max"
                value={weightChangeMax}
                onChange={(v) => onWeightChangeMax(v === null ? null : Number(v))}
                style={{ width: '50%' }}
                size="small"
              />
            </Space.Compact>
          </Space>
        </Col>

        <Col xs={12} sm={8} md={6} lg={4}>
          <Space direction="vertical" size="small" style={{ width: '100%' }}>
            <Text strong style={{ fontSize: '12px' }}>Min Funds:</Text>
            <InputNumber
              placeholder="e.g. 2"
              value={minFundsCount}
              onChange={(v) => onMinFundsCount(v === null ? null : Number(v))}
              style={{ width: '100%' }}
              min={1}
              size="small"
            />
          </Space>
        </Col>

        <Col xs={24} sm={24} md={12} lg={24}>
          <Text type="secondary" style={{ fontSize: '12px', display: 'block', marginTop: '8px' }}>
            Showing {currentCount} of {totalCount} holdings
          </Text>
        </Col>
      </Row>
    </div>
  );
};

export default HoldingChangesControls;
