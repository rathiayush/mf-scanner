import React, { useMemo, useState } from 'react';
import { Modal, Button, Space, Alert, Row, Col, Tag, Select, Divider, Statistic, Typography, Table, Pagination } from 'antd';
const { Text } = Typography;
import { SwapOutlined, FundOutlined } from '@ant-design/icons';
import { getChangeTypeColor } from '../utils/trendCalculator';

// Inject minimal CSS to ensure the table body owns horizontal scrolling within the modal
const modalTableStyles = `
  #holding-change-funds-table .ant-table-body {
    overflow-x: auto !important;
  }
  #holding-change-funds-table .ant-table-content {
    overflow-x: auto !important;
  }
  #holding-change-funds-table .ant-table-container {
    overflow-x: auto !important;
  }
`;
const modalStyleEl = document.createElement('style');
if (!document.head.querySelector('style[data-hcfm-styles]')) {
  modalStyleEl.setAttribute('data-hcfm-styles', 'true');
  modalStyleEl.textContent = modalTableStyles;
  document.head.appendChild(modalStyleEl);
}

interface TopControlsProps {
  filterType: string;
  onFilterChange: (v: string) => void;
  totalCount: number;
  page: number;
  pageSize: number;
  onPageChange: (p: number) => void;
  onPageSizeChange: (ps: number) => void;
}

const TopControls: React.FC<TopControlsProps> = ({
  filterType,
  onFilterChange,
  totalCount,
  page,
  pageSize,
  onPageChange,
  onPageSizeChange,
}) => {
  return (
    <Row id="holding-change-funds-top-controls" align="middle" justify="space-between" gutter={[12, 12]}>
      <Col>
        <Space id="hcfc-filter-space">
          <Text id="hcfc-filter-label" strong>Filter by Change Type:</Text>
          <Select
            id="hcfc-filter-select"
            size="small"
            style={{ width: 160 }}
            value={filterType}
            onChange={onFilterChange}
            options={[
              { label: 'All Funds', value: 'all' },
              { label: 'Added', value: 'Added' },
              { label: 'Removed', value: 'Removed' },
              { label: 'Increased', value: 'Increased' },
              { label: 'Decreased', value: 'Decreased' },
            ]}
          />
        </Space>
      </Col>
      <Col>
        <Space id="hcfc-pagination-space" size={8}>
          <Text id="hcfc-per-page-label" type="secondary" style={{ fontSize: 12 }}>Per page:</Text>
          <Select
            id="hcfc-page-size-select"
            size="small"
            value={pageSize}
            style={{ width: 90 }}
            onChange={onPageSizeChange}
            options={[10, 20, 50, 100].map(n => ({ label: `${n}`, value: n }))}
          />
          <Pagination
            id="hcfc-pagination"
            size="small"
            current={page}
            total={totalCount}
            pageSize={pageSize}
            showSizeChanger={false}
            onChange={onPageChange}
          />
        </Space>
      </Col>
    </Row>
  );
};

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
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const filteredData = useMemo(() => {
    const list = data?.fundChangeDetails || [];
    return list.filter((f: any) => filterType === 'all' || f.changeType === filterType);
  }, [data, filterType]);

  const pagedData = useMemo(() => {
    const start = (page - 1) * pageSize;
    return filteredData.slice(start, start + pageSize);
  }, [filteredData, page, pageSize]);
  return (
    <Modal
      title={
        <Space id="holding-change-funds-modal-title">
          <SwapOutlined id="holding-change-funds-modal-title-icon" />
          <Text id="holding-change-funds-modal-title-text" strong>Holding Change Fund Details</Text>
        </Space>
      }
      open={visible}
      onCancel={onClose}
      footer={[
        <Button id="holding-change-funds-modal-close" key="close" onClick={onClose}>
          Close
        </Button>
      ]}
      width={1400}
      style={{ top: 20 }}
      bodyStyle={{ maxHeight: '80vh', overflow: 'hidden' }}
    >
      {data && (
        <div id="holding-change-funds-modal-content" style={{ height: '80vh', display: 'flex', flexDirection: 'column' }}>
          <Alert
            id="holding-change-funds-modal-alert"
            message={`Holding Change: ${data.securityName}`}
            description={
              <Space id="holding-change-funds-modal-alert-desc" direction="vertical" size={2}>
                <Text id="holding-change-funds-modal-isin">ISIN: {data.isin}</Text>
                <Text id="holding-change-funds-modal-sector">Sector: {data.sector}</Text>
                <Space id="holding-change-funds-modal-tags">
                  <Tag id="holding-change-funds-modal-change-type" color={getChangeTypeColor(data.changeType)}>
                    {data.changeType}
                  </Tag>
                  {data.changeType === 'Mixed' && data.constituentChangeTypes && (
                    <Space id="holding-change-funds-modal-mixed-types">
                      <Text id="holding-change-funds-modal-mixed-label" type="secondary">Contains:</Text>
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
          
          <Row id="holding-change-funds-controls-row" style={{ marginBottom: 12 }} align="middle" justify="space-between">
            <Col span={24}>
              <div id="holding-change-funds-top-controls-wrapper">
              <TopControls 
                filterType={filterType} 
                onFilterChange={(v) => { setPage(1); onFilterChange(v); }} 
                totalCount={filteredData.length}
                page={page}
                pageSize={pageSize}
                onPageChange={setPage}
                onPageSizeChange={(ps: number) => { setPage(1); setPageSize(ps); }}
              />
              </div>
            </Col>
          </Row>

          {/* Top Summary KPIs */}
          <Row id="holding-change-funds-kpis-row" gutter={[16, 16]} style={{ marginBottom: 12 }}>
            <Col xs={12} sm={8} md={4}>
              <Statistic
                id="hc-kpi-total-funds"
                title="Total Funds"
                value={filteredData.length}
                prefix={<FundOutlined />}
              />
            </Col>
            <Col xs={12} sm={8} md={5}>
              <Statistic
                id="hc-kpi-weight-change"
                title="Weight Change"
                value={data.weightChange?.toFixed(2) || '0.00'}
                suffix="%"
                valueStyle={{ 
                  color: (data.weightChange || 0) > 0 ? '#52c41a' : 
                         (data.weightChange || 0) < 0 ? '#ff4d4f' : '#666666'
                }}
              />
            </Col>
            <Col xs={12} sm={8} md={5}>
              <Statistic
                id="hc-kpi-quantity-change"
                title="Quantity Change"
                value={data.quantityChange?.toLocaleString('en-IN') || '0'}
                valueStyle={{ 
                  color: (data.quantityChange || 0) > 0 ? '#52c41a' : 
                         (data.quantityChange || 0) < 0 ? '#ff4d4f' : '#666666'
                }}
              />
            </Col>
            <Col xs={12} sm={8} md={5}>
              <Statistic
                id="hc-kpi-value-change"
                title="Value Change"
                value={`₹${Math.abs(data.valueChange || 0).toLocaleString('en-IN', { maximumFractionDigits: 0 })}`}
                valueStyle={{ 
                  color: (data.valueChange || 0) > 0 ? '#52c41a' : 
                         (data.valueChange || 0) < 0 ? '#ff4d4f' : '#666666'
                }}
              />
            </Col>
            <Col xs={12} sm={8} md={5}>
              <Statistic
                id="hc-kpi-current-weight"
                title="Current Weight"
                value={data.currentWeight?.toFixed(2) || '0.00'}
                suffix="%"
                valueStyle={{ color: '#1890ff' }}
              />
            </Col>
          </Row>
          
          <div id="holding-change-funds-table-container" style={{ flex: 1, paddingBottom: 80 }}>
          <div
            id="holding-change-funds-table-wrapper"
            tabIndex={0}
            style={{ outline: 'none' }}
            onKeyDown={(e) => {
              const body = (document.querySelector('#holding-change-funds-table .ant-table-body') as HTMLDivElement) || null;
              if (!body) return;
              const step = 60;
              if (e.key === 'ArrowDown') {
                body.scrollTop += step;
                e.preventDefault();
              } else if (e.key === 'ArrowUp') {
                body.scrollTop -= step;
                e.preventDefault();
              } else if (e.key === 'ArrowRight') {
                body.scrollLeft += step;
                e.preventDefault();
              } else if (e.key === 'ArrowLeft') {
                body.scrollLeft -= step;
                e.preventDefault();
              } else if (e.key === 'PageDown') {
                body.scrollTop += body.clientHeight;
                e.preventDefault();
              } else if (e.key === 'PageUp') {
                body.scrollTop -= body.clientHeight;
                e.preventDefault();
              } else if (e.key === 'Home') {
                body.scrollTop = 0;
                e.preventDefault();
              } else if (e.key === 'End') {
                body.scrollTop = body.scrollHeight;
                e.preventDefault();
              }
            }}
          >
          <Table
            id="holding-change-funds-table"
            size="small"
            rowKey={(row: any) => `${row.mutualFundId}`}
            dataSource={pagedData}
            pagination={false}
            sticky
            scroll={{ x: 'max-content' as any, y: 'calc(78vh - 320px)' as any }}
            className="sticky-table"
            columns={[
              {
                title: '#',
                dataIndex: 'index',
                width: 60,
                fixed: 'left' as const,
                render: (_: any, __: any, idx: number) => <Tag color="blue">{idx + 1}</Tag>
              },
              {
                title: 'Mutual Fund',
                dataIndex: 'mutualFundName',
                key: 'mutualFundName',
                width: 220,
                fixed: 'left' as const,
                render: (text: string, row: any) => (
                  <Space direction="vertical" size={0}>
                    <Text strong>{text}</Text>
                    <Text type="secondary" style={{ fontSize: 11 }}>ID: {row.mutualFundId}</Text>
                  </Space>
                )
              },
              {
                title: 'Type',
                dataIndex: 'changeType',
                key: 'changeType',
                width: 120,
                render: (type: string) => (
                  <Tag color={getChangeTypeColor(type)} style={{ fontSize: 11 }}>{type}</Tag>
                )
              },
              // AUM first
              {
                title: 'Prev Snapshot AUM (Cr)',
                dataIndex: 'previousSnapshotTotalValue',
                width: 180,
                sorter: (a: any, b: any) => (a.previousSnapshotTotalValue ?? -Infinity) - (b.previousSnapshotTotalValue ?? -Infinity),
                render: (v: number | undefined) => v ? (v / 100).toFixed(2) : '-'
              },
              {
                title: 'Curr Snapshot AUM (Cr)',
                dataIndex: 'currentSnapshotTotalValue',
                width: 180,
                sorter: (a: any, b: any) => (a.currentSnapshotTotalValue ?? 0) - (b.currentSnapshotTotalValue ?? 0),
                render: (v: number) => (v / 100).toFixed(2)
              },
              // Quantity next
              {
                title: 'Prev Qty',
                dataIndex: 'previousQuantity',
                width: 120,
                sorter: (a: any, b: any) => (a.previousQuantity ?? -Infinity) - (b.previousQuantity ?? -Infinity),
                render: (v: number) => v ? v.toLocaleString('en-IN') : '-'
              },
              {
                title: 'Curr Qty',
                dataIndex: 'currentQuantity',
                width: 120,
                sorter: (a: any, b: any) => (a.currentQuantity ?? 0) - (b.currentQuantity ?? 0),
                render: (v: number) => (v ?? 0).toLocaleString('en-IN')
              },
              {
                title: 'Qty Δ',
                dataIndex: 'quantityChange',
                width: 120,
                sorter: (a: any, b: any) => (a.quantityChange ?? 0) - (b.quantityChange ?? 0),
                render: (v: number) => (
                  <Text style={{ color: (v || 0) > 0 ? '#52c41a' : (v || 0) < 0 ? '#ff4d4f' : '#666' }}>
                    {v ? (v > 0 ? '+' : '') + v.toLocaleString('en-IN') : '-'}
                  </Text>
                )
              },
              // Value next
              {
                title: 'Prev Value (Cr)',
                dataIndex: 'previousValue',
                width: 140,
                sorter: (a: any, b: any) => (a.previousValue ?? -Infinity) - (b.previousValue ?? -Infinity),
                render: (v: number | null | undefined) =>
                  v == null ? '-' : (v / 100).toFixed(2)
              },
              {
                title: 'Curr Value (Cr)',
                dataIndex: 'currentValue',
                width: 140,
                sorter: (a: any, b: any) => (a.currentValue ?? 0) - (b.currentValue ?? 0),
                render: (v: number | null | undefined) =>
                  v == null ? '-' : (v / 100).toFixed(2)
              },
              {
                title: 'Value Δ (Cr)',
                dataIndex: 'valueChange',
                width: 140,
                sorter: (a: any, b: any) => (a.valueChange ?? 0) - (b.valueChange ?? 0),
                render: (v: number | null | undefined) => {
                  if (v == null) {
                    return <Text style={{ color: '#666' }}>-</Text>;
                  }
                  const num = Number(v);
                  const color = num > 0 ? '#52c41a' : num < 0 ? '#ff4d4f' : '#666';
                  const sign = num > 0 ? '+' : num < 0 ? '-' : '';
                  return (
                    <Text style={{ color }}>
                      {`${sign}${(Math.abs(num) / 100).toFixed(2)}`}
                    </Text>
                  );
                }
              },
              // Weight last
              {
                title: 'Prev Weight %',
                dataIndex: 'previousWeight',
                width: 120,
                sorter: (a: any, b: any) => (a.previousWeight ?? -Infinity) - (b.previousWeight ?? -Infinity),
                render: (v: number) => v ? v.toFixed(2) : '-'
              },
              {
                title: 'Curr Weight %',
                dataIndex: 'currentWeight',
                width: 120,
                sorter: (a: any, b: any) => (a.currentWeight ?? 0) - (b.currentWeight ?? 0),
                render: (v: number) => v?.toFixed(2)
              },
              {
                title: 'Weight Δ %',
                dataIndex: 'weightChange',
                width: 120,
                sorter: (a: any, b: any) => (a.weightChange ?? 0) - (b.weightChange ?? 0),
                render: (v: number) => (
                  <Text style={{ color: v > 0 ? '#52c41a' : v < 0 ? '#ff4d4f' : '#666' }}>
                    {v > 0 ? '+' : ''}{v.toFixed(2)}
                  </Text>
                )
              },
            ]}
          />
          </div>
          
          <Divider />
          </div>
        </div>
      )}
    </Modal>
  );
};

export default HoldingChangeFundsModal;
