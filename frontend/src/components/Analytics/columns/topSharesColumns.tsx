import React from 'react';
import { Space, Tag, Button, Typography } from 'antd';

const { Text } = Typography;
import { EyeOutlined } from '@ant-design/icons';
import { TopShareDto } from '../../../types';

interface TopSharesColumnsProps {
  onViewFundHoldings: (holdings: any[]) => void;
}

export const getTopSharesColumns = ({ onViewFundHoldings }: TopSharesColumnsProps) => [
  {
    title: 'Security Name',
    dataIndex: 'securityName',
    key: 'securityName',
    width: 200,
    fixed: 'left' as const,
    render: (text: string, record: TopShareDto) => (
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
    title: 'Total Weight',
    dataIndex: 'totalWeight',
    key: 'totalWeight',
    width: 120,
    render: (weight: number) => `${weight.toFixed(2)}%`,
    sorter: (a: TopShareDto, b: TopShareDto) => b.totalWeight - a.totalWeight,
  },
  {
    title: 'Holding Funds',
    dataIndex: 'holdingFundsCount',
    key: 'holdingFundsCount',
    width: 100,
    render: (count: number) => <Tag color="blue">{count} funds</Tag>,
  },
  {
    title: 'Fund Holdings',
    dataIndex: 'fundHoldings',
    key: 'fundHoldings',
    render: (holdings: any[]) => (
      <Space direction="vertical" size="small">
        {holdings.slice(0, 3).map(holding => (
          <div key={holding.mutualFundId}>
            <Text style={{ fontSize: '12px' }}>
              {holding.mutualFundName}: {holding.weight.toFixed(2)}%
            </Text>
          </div>
        ))}
        {holdings.length > 3 && (
          <Button 
            type="link" 
            size="small" 
            icon={<EyeOutlined />}
            onClick={() => onViewFundHoldings(holdings)}
            style={{ padding: 0, fontSize: '12px' }}
          >
            View all {holdings.length} funds
          </Button>
        )}
      </Space>
    ),
  },
];
