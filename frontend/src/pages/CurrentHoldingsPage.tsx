import React from 'react';
import { CurrentHoldings } from '../components/CurrentHoldings';
import Layout from '../components/Layout';

const CurrentHoldingsPage: React.FC = () => {
  return (
    <Layout>
      <div style={{ padding: '24px' }}>
        <CurrentHoldings />
      </div>
    </Layout>
  );
};

export default CurrentHoldingsPage;
