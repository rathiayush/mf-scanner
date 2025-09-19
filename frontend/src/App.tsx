import React, { useState } from 'react';
import AppLayout from './components/Layout';
import { FileUploadPage, AnalysePage, UploadFilesTriggerPage, MutualFundListPage } from './pages';
import CurrentHoldingsPage from './pages/CurrentHoldingsPage';

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
        return <CurrentHoldingsPage />;
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
