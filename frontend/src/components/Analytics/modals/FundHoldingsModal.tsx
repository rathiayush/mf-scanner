import { useMemo, useState } from 'react';
import { Modal, Button, Space, Alert, Row, Col, Tag, Progress, Divider, Statistic, Typography, Pagination, Table, theme } from 'antd';

const { Text } = Typography;
const { useToken } = theme;
import { FundOutlined } from '@ant-design/icons';

interface FundHoldingsModalProps {
  visible: boolean;
  data: any[] | null;
  onClose: () => void;
}

const FundHoldingsModal: React.FC<FundHoldingsModalProps> = ({ visible, data, onClose }) => {
  const { token } = useToken();
  const [page, setPage] = useState(1);
  const pageSize = 10;
  const sortedData = useMemo(() => (data || []).slice().sort((a: any, b: any) => b.weight - a.weight), [data]);
  const pageData = useMemo(() => sortedData.slice((page - 1) * pageSize, page * pageSize), [sortedData, page]);
  return (
    <Modal
      id="fund-holdings-modal-container"
      title={
        <Space id="fund-holdings-modal-title-space">
          <FundOutlined id="fund-holdings-modal-icon" />
          <Text id="fund-holdings-modal-title-text" strong>Mutual Fund Holdings Details</Text>
        </Space>
      }
      open={visible}
      onCancel={onClose}
      footer={[
        <Button id="fund-holdings-modal-close-btn" key="close" onClick={onClose}>
          Close
        </Button>
      ]}
      width={1000}
      style={{ top: 20 }}
      bodyStyle={{ maxHeight: '75vh', overflow: 'auto' }}
    >
      {data && (
        <div id="fund-holdings-modal-content">
          <Alert
            id="fund-holdings-modal-alert"
            message="Fund Weight Distribution"
            description={`Showing holdings across ${data.length} mutual funds`}
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
          />
          <Row id="fund-holdings-modal-pagination-row" justify="end" style={{ marginBottom: 8 }}>
            <Pagination
              id="fund-holdings-modal-pagination"
              current={page}
              total={sortedData.length}
              pageSize={pageSize}
              showSizeChanger={false}
              onChange={(p) => setPage(p)}
              size="small"
            />
          </Row>
          
          <Table
            id="fund-holdings-modal-table"
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
                    <Text strong style={{ color: token.colorPrimary }}>{(w ?? 0).toFixed(2)}%</Text>
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
          
          <Divider id="fund-holdings-modal-divider" />
          <Row id="fund-holdings-modal-bottom-pagination-row" justify="end">
            <Pagination
              id="fund-holdings-modal-bottom-pagination"
              current={page}
              total={sortedData.length}
              pageSize={pageSize}
              showSizeChanger={false}
              onChange={(p) => setPage(p)}
              size="small"
            />
          </Row>
          
          <Row id="fund-holdings-modal-stats-row" gutter={[16, 16]} style={{ marginTop: 16 }}>
            <Col id="total-funds-stat-col" span={8}>
              <Statistic
                id="total-funds-statistic"
                title="Total Funds"
                value={data.length}
                prefix={<FundOutlined />}
              />
            </Col>
            <Col id="average-weight-stat-col" span={8}>
              <Statistic
                id="average-weight-statistic"
                title="Average Weight"
                value={(data.reduce((sum: number, h: any) => sum + h.weight, 0) / data.length).toFixed(2)}
                suffix="%"
                valueStyle={{ color: token.colorPrimary }}
              />
            </Col>
            <Col id="total-weight-stat-col" span={8}>
              <Statistic
                id="total-weight-statistic"
                title="Total Weight"
                value={data.reduce((sum: number, h: any) => sum + h.weight, 0).toFixed(2)}
                suffix="%"
              />
            </Col>
          </Row>
        </div>
      )}
    </Modal>
  );
};

export default FundHoldingsModal;
