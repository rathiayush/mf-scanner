import React, { useMemo, useState } from 'react';
import { Modal, Button, Space, Alert, Row, Col, Tag, Progress, Divider, Statistic, Typography, Pagination, Table } from 'antd';

const { Text } = Typography;
import { FundOutlined } from '@ant-design/icons';

interface FundHoldingsModalProps {
  visible: boolean;
  data: any[] | null;
  onClose: () => void;
}

const FundHoldingsModal: React.FC<FundHoldingsModalProps> = ({ visible, data, onClose }) => {
  const [page, setPage] = useState(1);
  const pageSize = 10;
  const sortedData = useMemo(() => (data || []).slice().sort((a: any, b: any) => b.weight - a.weight), [data]);
  const pageData = useMemo(() => sortedData.slice((page - 1) * pageSize, page * pageSize), [sortedData, page]);
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
      width={1000}
      style={{ top: 20 }}
      bodyStyle={{ maxHeight: '75vh', overflow: 'auto' }}
    >
      {data && (
        <>
          <Alert
            message="Fund Weight Distribution"
            description={`Showing holdings across ${data.length} mutual funds`}
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
          />
          <Row justify="end" style={{ marginBottom: 8 }}>
            <Pagination
              current={page}
              total={sortedData.length}
              pageSize={pageSize}
              showSizeChanger={false}
              onChange={(p) => setPage(p)}
              size="small"
            />
          </Row>
          
          <Table
            dataSource={pageData}
            rowKey={(row: any) => `${row.mutualFundId}`}
            pagination={false}
            className="sticky-table"
            scroll={{ x: 800 }}
            columns={[
              {
                title: '#',
                dataIndex: 'index',
                width: 70,
                render: (_: any, __: any, idx: number) => (
                  <Tag color="blue">{(page - 1) * pageSize + idx + 1}</Tag>
                )
              },
              {
                title: 'Mutual Fund',
                dataIndex: 'mutualFundName',
                width: 400,
                sorter: (a: any, b: any) => a.mutualFundName.localeCompare(b.mutualFundName),
                render: (text: string, row: any) => (
                  <Space direction="vertical" size={0}>
                    <Text strong>{text}</Text>
                    <Text type="secondary" style={{ fontSize: 12 }}>Fund ID: {row.mutualFundId}</Text>
                  </Space>
                )
              },
              {
                title: 'Weight %',
                dataIndex: 'weight',
                width: 160,
                sorter: (a: any, b: any) => (a.weight ?? 0) - (b.weight ?? 0),
                render: (w: number) => (
                  <Space direction="vertical" size={0} style={{ width: '100%' }}>
                    <Text strong style={{ color: '#1890ff' }}>{(w ?? 0).toFixed(2)}%</Text>
                    <Progress 
                      percent={w}
                      showInfo={false}
                      strokeColor={{ '0%': '#108ee9', '100%': '#87d068' }}
                      size="small"
                    />
                  </Space>
                )
              }
            ]}
          />
          
          <Divider />
          <Row justify="end">
            <Pagination
              current={page}
              total={sortedData.length}
              pageSize={pageSize}
              showSizeChanger={false}
              onChange={(p) => setPage(p)}
              size="small"
            />
          </Row>
          
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
        </>
      )}
    </Modal>
  );
};

export default FundHoldingsModal;
