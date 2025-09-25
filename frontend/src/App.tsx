import React, { useState } from 'react';
import AppLayout from './components/Layout';
import { FileUploadPage, AnalysePage, UploadFilesTriggerPage, MutualFundListPage } from './pages';
import CurrentHoldingsPage from './pages/CurrentHoldingsPage';
import HoldingsComparison from './components/Holdings/HoldingsComparison';
import PortfolioSummary from './components/Holdings/PortfolioSummary';
import UploadV2 from './components/Upload/UploadV2';
import ConcreteMutualFunds from './pages/ConcreteMutualFunds';

const App: React.FC = () => {
  const [selectedMenuKey, setSelectedMenuKey] = useState('1');

  const handleMenuSelect = (key: string) => {
    setSelectedMenuKey(key);
  };

  const renderPage = () => {
    switch (selectedMenuKey) {
      case '1':
        return <FileUploadPage />;
      case '2':
        return <AnalysePage />;
      case '3':
        return <UploadFilesTriggerPage />;
      case '4':
        return <MutualFundListPage />;
      case '5':
      case '5-1':
        return <CurrentHoldingsPage />;
      case '5-2':
        return <HoldingsComparison />;
      case '5-3':
        return <PortfolioSummary />;
      case '6':
        return <UploadV2 />;
      case '7':
        return <ConcreteMutualFunds />;
      default:
        return <FileUploadPage />;
    }
  };

  return (
    <AppLayout selectedKey={selectedMenuKey} onMenuSelect={handleMenuSelect}>
      {renderPage()}
    </AppLayout>
  );
};

export default App;
