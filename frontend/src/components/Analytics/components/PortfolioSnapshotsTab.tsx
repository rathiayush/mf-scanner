import React from 'react';
import { Row, Col, Card, Statistic, Alert, Typography } from 'antd';

const { Text } = Typography;

interface PortfolioSnapshotsTabProps {
  snapshots: any[];
}

const PortfolioSnapshotsTab: React.FC<PortfolioSnapshotsTabProps> = ({ snapshots }) => {
  if (snapshots.length === 0) {
    return (
      <Alert
        message="No Portfolio Snapshots"
        description="Run analysis to see portfolio snapshots for selected funds and dates."
        type="info"
        showIcon
      />
    );
  }

  return (
    <Row gutter={[16, 16]}>
      {snapshots.map((snapshot, index) => (
        <Col xs={24} sm={24} md={12} lg={12} xl={12} key={index}>
          <Card 
            title={`${snapshot.fundName} - ${snapshot.date}`}
            size="small"
          >
            <Row gutter={[8, 8]}>
              <Col xs={12} sm={12} md={12}>
                <Statistic
                  title="Total Value"
                  value={snapshot.totalValue}
                  prefix="₹"
                  precision={2}
                />
              </Col>
              <Col xs={12} sm={12} md={12}>
                <Statistic
                  title="Holdings Count"
                  value={snapshot.holdingsCount}
                />
              </Col>
            </Row>
            <div style={{ marginTop: 16 }}>
              <Text strong>Top 5 Holdings:</Text>
              {Array.isArray(snapshot.holdings) ? 
                snapshot.holdings.slice(0, 5).map((holding: any, idx: number) => (
                  <div key={idx} style={{ marginTop: 4 }}>
                    <Text style={{ fontSize: '12px' }}>
                      {holding.companyName}: {holding.weight?.toFixed(2)}%
                    </Text>
                  </div>
                )) : 
                <div style={{ marginTop: 4 }}>
                  <Text type="secondary" style={{ fontSize: '12px' }}>
                    No holdings data available
                  </Text>
                </div>
              }
            </div>
          </Card>
        </Col>
      ))}
    </Row>
  );
};

export default PortfolioSnapshotsTab;
