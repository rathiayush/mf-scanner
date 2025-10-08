import React from 'react';
import { BrowserRouter, Routes, Route, useLocation, Navigate } from 'react-router-dom';
import { ConfigProvider } from 'antd';
import AppLayout from './components/Layout';
import { FileUploadPage, AnalysePage, MutualFundListPage, PortfolioHoldingsPage } from './pages';
import CurrentHoldingsPage from './pages/CurrentHoldingsPage';
import HoldingsComparison from './components/Holdings/HoldingsComparison';
import PortfolioSummary from './components/Holdings/PortfolioSummary';
import UploadV2 from './components/Upload/UploadV2';
import ConcreteMutualFunds from './pages/ConcreteMutualFunds';
import { SecurityAnalytics } from './components/Analytics';

const AppContent: React.FC = () => {
  const location = useLocation();
  
  // Map routes to menu keys
  const getSelectedKey = () => {
    const path = location.pathname;
    if (path === '/') return '1';
    if (path === '/analyze') return '2';
    if (path === '/security-analytics') return '3';
    if (path === '/mutual-funds') return '4';
    if (path === '/current-holdings') return '5-1';
    if (path === '/holdings-comparison') return '5-2';
    if (path === '/portfolio-summary') return '5-3';
    if (path === '/portfolio-holdings') return '8';
    if (path === '/upload-v2') return '6';
    if (path === '/concrete-funds') return '7';
    return '1';
  };

  return (
    <AppLayout selectedKey={getSelectedKey()}>
      <Routes>
        <Route path="/" element={<FileUploadPage />} />
        <Route path="/analyze" element={<AnalysePage />} />
        <Route path="/security-analytics" element={<SecurityAnalytics />} />
        <Route path="/mutual-funds" element={<MutualFundListPage />} />
        <Route path="/current-holdings" element={<CurrentHoldingsPage />} />
        <Route path="/holdings-comparison" element={<HoldingsComparison />} />
        <Route path="/portfolio-summary" element={<PortfolioSummary />} />
        <Route path="/portfolio-holdings" element={<PortfolioHoldingsPage />} />
        <Route path="/upload-v2" element={<UploadV2 />} />
        <Route path="/concrete-funds" element={<ConcreteMutualFunds />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </AppLayout>
  );
};

const App: React.FC = () => {
  return (
    <BrowserRouter basename={import.meta.env.REACT_APP_BASE_PATH || ''}>
      <ConfigProvider
        theme={{
          token: {
            colorPrimary: '#1890ff',
            borderRadius: 8,
          colorBgContainer: '#ffffff',
          colorText: 'rgba(0, 0, 0, 0.85)',
          colorTextSecondary: 'rgba(0, 0, 0, 0.65)',
          colorBorder: '#d9d9d9',
          colorBorderSecondary: '#f0f0f0',
          colorBgLayout: '#f5f5f5',
          colorBgElevated: '#ffffff',
          colorPrimaryHover: '#40a9ff',
          colorPrimaryActive: '#096dd9',
          colorError: '#ff4d4f',
          colorWarning: '#faad14',
          colorSuccess: '#52c41a',
          colorInfo: '#1890ff',
          fontFamily: '"Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
          fontSize: 14,
          lineHeight: 1.5715,
          controlHeight: 32,
          controlHeightSM: 24,
          controlHeightLG: 40,
        },
        components: {
          Layout: {
            headerBg: '#fff',
            headerHeight: 64,
            headerPadding: '0 24px',
            headerColor: 'rgba(0, 0, 0, 0.85)',
            siderBg: '#fff',
            triggerBg: '#f0f0f0',
            triggerColor: 'rgba(0, 0, 0, 0.65)',
            bodyBg: '#fff',
          },
          Menu: {
            itemBg: '#fff',
            itemColor: 'rgba(0, 0, 0, 0.85)',
            itemHoverBg: '#f5f5f5',
            itemSelectedBg: '#e6f7ff',
            itemSelectedColor: '#1890ff',
            itemActiveBg: '#f0f0f0',
            subMenuItemBg: '#fff',
            itemHoverColor: '#1890ff',
          },
          Button: {
            primaryColor: '#fff',
          },
          Table: {
            headerBg: '#fafafa',
            headerColor: 'rgba(0, 0, 0, 0.85)',
            rowHoverBg: '#f5f5f5',
            borderColor: '#f0f0f0',
            headerSplitColor: 'transparent',
          },
          Card: {
            paddingLG: 24,
            borderRadiusLG: 8,
            boxShadow: '0 1px 2px 0 rgba(0, 0, 0, 0.03)',
          },
        },
      }}
    >
        <AppContent />
      </ConfigProvider>
    </BrowserRouter>
  );
};

export default App;
