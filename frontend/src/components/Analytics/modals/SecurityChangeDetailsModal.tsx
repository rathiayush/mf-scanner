import React, { useState, useMemo } from 'react';
import { Modal, Button, Space, Row, Col, Tag, Select, Typography, Table, Pagination } from 'antd';
import { SecurityMonthlyChangeDto, SecurityFundChangeDto } from '../../../types';
import dayjs from 'dayjs';

const { Text } = Typography;

interface SecurityChangeDetailsModalProps {
  visible: boolean;
  data: SecurityMonthlyChangeDto | null;
  onClose: () => void;
}

const SecurityChangeDetailsModal: React.FC<SecurityChangeDetailsModalProps> = ({ visible, data, onClose }) => {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [filterType, setFilterType] = useState('all');

  const filteredData = useMemo(() => {
    if (!data) return [];
    if (filterType === 'all') return data.allFundChanges;
    return data.allFundChanges.filter(fund => fund.changeType === filterType);
  }, [data, filterType]);

  const pagedData = useMemo(() => {
    const start = (page - 1) * pageSize;
    return filteredData.slice(start, start + pageSize);
  }, [filteredData, page, pageSize]);

  const columns = [
    {
      title: '#',
      key: 'index',
      width: 60,
      render: (_: any, __: any, index: number) => <Tag color="blue">{((page - 1) * pageSize) + index + 1}</Tag>
    },
    {
      title: 'Mutual Fund',
      dataIndex: 'mutualFundName',
      key: 'mutualFundName',
      sorter: (a: SecurityFundChangeDto, b: SecurityFundChangeDto) => a.mutualFundName.localeCompare(b.mutualFundName),
    },
    {
      title: 'Change Type',
      dataIndex: 'changeType',
      key: 'changeType',
      render: (type: string) => <Tag color={type === 'Added' ? 'green' : type === 'Removed' ? 'red' : type === 'Increased' ? 'blue' : type === 'Decreased' ? 'orange' : 'default'}>{type}</Tag>,
      sorter: (a: SecurityFundChangeDto, b: SecurityFundChangeDto) => a.changeType.localeCompare(b.changeType),
    },
    {
        title: 'Weight %',
        children: [
            { title: 'Prev', dataIndex: 'previousWeight', key: 'previousWeight', render: (val: number) => `${val.toFixed(2)}%`, sorter: (a:any, b:any) => a.previousWeight - b.previousWeight },
            { title: 'Curr', dataIndex: 'currentWeight', key: 'currentWeight', render: (val: number) => `${val.toFixed(2)}%`, sorter: (a:any, b:any) => a.currentWeight - b.currentWeight },
            { title: 'Δ', dataIndex: 'weightChange', key: 'weightChange', render: (val: number) => <Text type={val > 0 ? 'success' : 'danger'}>{`${val > 0 ? '+' : ''}${val.toFixed(2)}%`}</Text>, sorter: (a:any, b:any) => a.weightChange - b.weightChange },
        ]
    },
    {
        title: 'Quantity',
        children: [
            { title: 'Prev', dataIndex: 'previousQuantity', key: 'previousQuantity', render: (val?: number) => val?.toLocaleString('en-IN') ?? '-', sorter: (a:any, b:any) => (a.previousQuantity ?? 0) - (b.previousQuantity ?? 0) },
            { title: 'Curr', dataIndex: 'currentQuantity', key: 'currentQuantity', render: (val: number) => val.toLocaleString('en-IN'), sorter: (a:any, b:any) => a.currentQuantity - b.currentQuantity },
            { title: 'Δ', dataIndex: 'quantityChange', key: 'quantityChange', render: (val: number) => <Text type={val > 0 ? 'success' : 'danger'}>{`${val > 0 ? '+' : ''}${val.toLocaleString('en-IN')}`}</Text>, sorter: (a:any, b:any) => a.quantityChange - b.quantityChange },
        ]
    },
    {
        title: 'Value (Cr)',
        children: [
            { title: 'Prev', dataIndex: 'previousValue', key: 'previousValue', render: (val?: number) => val ? (val / 10000000).toFixed(2) : '-', sorter: (a:any, b:any) => (a.previousValue ?? 0) - (b.previousValue ?? 0) },
            { title: 'Curr', dataIndex: 'currentValue', key: 'currentValue', render: (val: number) => (val / 10000000).toFixed(2), sorter: (a:any, b:any) => a.currentValue - b.currentValue },
            { title: 'Δ', dataIndex: 'valueChange', key: 'valueChange', render: (val: number) => <Text type={val > 0 ? 'success' : 'danger'}>{`${val > 0 ? '+' : ''}${(val / 10000000).toFixed(2)}`}</Text>, sorter: (a:any, b:any) => a.valueChange - b.valueChange },
        ]
    },
  ];

  return (
    <Modal
      title={`Monthly Change Details - ${data ? dayjs(data.monthYear).format('MMMM YYYY') : ''}`}
      open={visible}
      onCancel={onClose}
      footer={[
        <Button key="close" onClick={onClose}>Close</Button>
      ]}
      width={1200}
      style={{ top: 20 }}
    >
      {data && (
        <div>
          <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
            <Col>
                <Space>
                    <Text strong>Filter by Change Type:</Text>
                    <Select value={filterType} onChange={setFilterType} style={{ width: 150 }}>
                        <Select.Option value="all">All</Select.Option>
                        <Select.Option value="Added">Added</Select.Option>
                        <Select.Option value="Removed">Removed</Select.Option>
                        <Select.Option value="Increased">Increased</Select.Option>
                        <Select.Option value="Decreased">Decreased</Select.Option>
                        <Select.Option value="DataNotPresent">Data Not Present</Select.Option>
                    </Select>
                </Space>
            </Col>
            <Col>
                <Pagination
                    current={page}
                    pageSize={pageSize}
                    total={filteredData.length}
                    onChange={(p, ps) => { setPage(p); if (ps) setPageSize(ps); }}
                    showSizeChanger
                    pageSizeOptions={['10', '20', '50', '100']}
                />
            </Col>
          </Row>
          <Table
            columns={columns}
            dataSource={pagedData}
            rowKey="mutualFundId"
            pagination={false}
            size="small"
            scroll={{ x: 'max-content' }}
          />
        </div>
      )}
    </Modal>
  );
};

export default SecurityChangeDetailsModal;
