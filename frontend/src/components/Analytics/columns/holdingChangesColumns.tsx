import React from 'react';
import { Space, Tag, Button, Typography } from 'antd';

const { Text } = Typography;
import { EyeOutlined } from '@ant-design/icons';
import { HoldingChangeDto } from '../../../types';
import { calculateConvictionTrend, getChangeTypeColor } from '../utils/trendCalculator';

interface HoldingChangesColumnsProps {
  requestSort: (key: string) => void;
  getSortOrder: (key: string) => string | null;
  onViewFundDetails: (data: any) => void;
}

export const getHoldingChangesColumns = ({
  requestSort,
  getSortOrder,
  onViewFundDetails
}: HoldingChangesColumnsProps) => [
  {
    title: 'Security Name',
    dataIndex: 'securityName',
    key: 'securityName',
    width: 200,
    render: (text: string, record: HoldingChangeDto) => (
      <Space direction="vertical" size="small">
        <Text strong>{text}</Text>
        <Text type="secondary" style={{ fontSize: '12px' }}>{record.isin}</Text>
      </Space>
    ),
  },
  {
    title: 'Sector',
    dataIndex: 'sector',
    key: 'sector',
    width: 120,
  },
  {
    title: 'Change Type',
    dataIndex: 'changeType',
    key: 'changeType',
    width: 150,
    render: (type: string, record: HoldingChangeDto) => {
      if (record.changeTypeCounts) {
        const changeCounts = Object.entries(record.changeTypeCounts);
        
        if (changeCounts.length === 1) {
          const [changeType, count] = changeCounts[0];
          return (
            <Tag color={getChangeTypeColor(changeType)}>
              {changeType} ({count})
            </Tag>
          );
        }
        
        return (
          <Space size={[0, 4]} wrap>
            {changeCounts.map(([changeType, count]) => (
              <Tag 
                key={changeType}
                color={getChangeTypeColor(changeType)}
                style={{ margin: '0 4px 4px 0' }}
              >
                {changeType} ({count})
              </Tag>
            ))}
          </Space>
        );
      }
      
      return <Tag color={getChangeTypeColor(type)}>{type}</Tag>;
    },
  },
  {
    title: (
      <span 
        onClick={() => requestSort('score')} 
        style={{ cursor: 'pointer', userSelect: 'none' }}
      >
        Trend
        {getSortOrder('score') === 'ascend' && ' ↑'}
        {getSortOrder('score') === 'descend' && ' ↓'}
      </span>
    ),
    key: 'trend',
    width: 140,
    sorter: (a: any, b: any) => {
      const trendA = calculateConvictionTrend(a).score;
      const trendB = calculateConvictionTrend(b).score;
      return trendA - trendB;
    },
    sortOrder: getSortOrder('score') as any,
    render: (_: any, record: any) => {
      const trend = calculateConvictionTrend(record);
      return <Tag color={trend.color}>{trend.label}</Tag>;
    },
  },
  {
    title: 'Previous Weight',
    dataIndex: 'previousWeight',
    key: 'previousWeight',
    width: 120,
    sorter: (a: any, b: any) => (a.previousWeight ?? -Infinity) - (b.previousWeight ?? -Infinity),
    render: (weight: number) => weight ? `${weight.toFixed(2)}%` : '-',
  },
  {
    title: 'Current Weight',
    dataIndex: 'currentWeight',
    key: 'currentWeight',
    width: 120,
    sorter: (a: any, b: any) => (a.currentWeight ?? 0) - (b.currentWeight ?? 0),
    render: (weight: number) => `${weight.toFixed(2)}%`,
  },
  {
    title: 'Weight Change',
    dataIndex: 'weightChange',
    key: 'weightChange',
    width: 120,
    sorter: (a: any, b: any) => (a.weightChange ?? 0) - (b.weightChange ?? 0),
    render: (change: number) => (
      <Text type={change > 0 ? 'success' : change < 0 ? 'danger' : undefined}>
        {change > 0 ? '+' : ''}{change.toFixed(2)}%
      </Text>
    ),
  },
  {
    title: 'Quantity Change',
    dataIndex: 'quantityChange',
    key: 'quantityChange',
    width: 130,
    sorter: (a: any, b: any) => (a.quantityChange ?? 0) - (b.quantityChange ?? 0),
    render: (change: number) => (
      <Text type={change > 0 ? 'success' : change < 0 ? 'danger' : undefined}>
        {change > 0 ? '+' : ''}{change.toLocaleString()}
      </Text>
    ),
  },
  {
    title: 'Value Change (Cr)',
    dataIndex: 'valueChange',
    key: 'valueChange',
    width: 130,
    sorter: (a: any, b: any) => (a.valueChange ?? 0) - (b.valueChange ?? 0),
    render: (change: number) => {
      const valueInCr = change / 100;
      return (
        <Text type={valueInCr > 0 ? 'success' : valueInCr < 0 ? 'danger' : undefined}>
          {valueInCr > 0 ? '+' : ''}₹{Math.abs(valueInCr).toLocaleString('en-IN', { maximumFractionDigits: 2 })}
        </Text>
      );
    },
  },
  {
    title: 'Total Value (Cr)',
    dataIndex: 'totalMarketValue',
    key: 'totalMarketValue',
    width: 130,
    sorter: (a: HoldingChangeDto, b: HoldingChangeDto) => (a.totalMarketValue ?? 0) - (b.totalMarketValue ?? 0),
    render: (value: number) => {
      if (value === undefined || value === null) return '-';
      const valueInCr = value / 100;
      return (
        <Text>
          ₹{valueInCr.toLocaleString('en-IN', { maximumFractionDigits: 2 })}
        </Text>
      );
    },
  },
  {
    title: 'Total Funds',
    dataIndex: 'mutualFundNames',
    key: 'totalFunds',
    width: 100,
    align: 'center' as const,
    render: (names: string[], record: HoldingChangeDto) => (
      <Space direction="vertical" size="small" style={{ alignItems: 'center' }}>
        <Tag color="blue" style={{ fontSize: '12px', fontWeight: 'bold' }}>
          {names.length}
        </Tag>
        <Button 
          type="link" 
          size="small" 
          icon={<EyeOutlined />}
          onClick={() => {
            const fundDetails = record.fundChangeDetails || [];
            const hasIncreased = fundDetails.some((f: any) => f.changeType === 'Increased');
            const hasDecreased = fundDetails.some((f: any) => f.changeType === 'Decreased');
            const hasAdded = fundDetails.some((f: any) => f.changeType === 'Added');
            const hasRemoved = fundDetails.some((f: any) => f.changeType === 'Removed');
            
            let finalChangeType = record.changeType;
            if ((hasIncreased && hasDecreased) || (hasAdded && hasRemoved) || 
                (hasIncreased && hasRemoved) || (hasDecreased && hasAdded)) {
              finalChangeType = 'Mixed';
            }
            
            onViewFundDetails({
              securityName: record.securityName,
              isin: record.isin,
              sector: record.sector,
              changeType: finalChangeType,
              previousWeight: record.previousWeight,
              currentWeight: record.currentWeight,
              weightChange: record.weightChange,
              previousQuantity: record.previousQuantity,
              currentQuantity: record.currentQuantity,
              quantityChange: record.quantityChange,
              previousValue: record.previousValue,
              currentValue: record.currentValue,
              valueChange: record.valueChange,
              fundChangeDetails: fundDetails,
              constituentChangeTypes: record.constituentChangeTypes
            });
          }}
          style={{ padding: 0, fontSize: '10px' }}
        >
          Details
        </Button>
      </Space>
    ),
  },
  {
    title: 'Mutual Funds',
    dataIndex: 'mutualFundNames',
    key: 'mutualFundNames',
    render: (names: string[]) => (
      <Space direction="vertical" size="small">
        {names.slice(0, 2).map(name => (
          <Tag key={name} style={{ fontSize: '11px' }}>{name}</Tag>
        ))}
        {names.length > 2 && (
          <Text type="secondary" style={{ fontSize: '11px' }}>+{names.length - 2} more</Text>
        )}
      </Space>
    ),
  },
];
