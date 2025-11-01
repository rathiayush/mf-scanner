import React, { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route, useLocation, Navigate } from 'react-router-dom';
import { ConfigProvider, theme } from 'antd';
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import AppLayout from './components/Layout';
import { AnalysePage, MutualFundListPage } from './pages';
import UploadV2 from './components/Upload/UploadV2';
import ConcreteMutualFunds from './pages/ConcreteMutualFunds';
import SnapshotManagementPage from './pages/SnapshotManagementPage';
import { SecurityAnalytics } from './components/Analytics';
import BackgroundProcessingPage from './components/BackgroundProcessing/BackgroundProcessingPage';
import Login from './pages/Login';
import UserManagement from './pages/UserManagement';

interface AppContentProps {
  isDarkMode: boolean;
  toggleTheme: () => void;
}

const AppContent: React.FC<AppContentProps> = ({ isDarkMode, toggleTheme }) => {
  const location = useLocation();
  
  // Map routes to menu keys
  const getSelectedKey = () => {
    const path = location.pathname;
    if (path === '/' || path === '/analyze') return '2';
    if (path === '/security-analytics') return '3';
    if (path === '/mutual-funds') return '4';
    if (path === '/snapshot-management') return '10';
    if (path === '/upload-v2') return '6';
    if (path === '/concrete-funds') return '7';
    if (path === '/background-processing') return '9';
    if (path === '/user-management') return '11';
    return '2';
  };

  return (
    <Routes>
      {/* Public route - Login */}
      <Route path="/login" element={<Login />} />
      
      {/* Protected routes - All application pages */}
      <Route path="/*" element={
        <ProtectedRoute>
          <AppLayout selectedKey={getSelectedKey()} isDarkMode={isDarkMode} toggleTheme={toggleTheme}>
            <Routes>
              <Route path="/" element={<Navigate to="/analyze" replace />} />
              <Route path="/analyze" element={<AnalysePage />} />
              <Route path="/security-analytics" element={<SecurityAnalytics />} />
              <Route path="/mutual-funds" element={<MutualFundListPage />} />
              <Route path="/snapshot-management" element={<SnapshotManagementPage />} />
              <Route path="/upload-v2" element={
                <ProtectedRoute requireAdmin>
                  <UploadV2 />
                </ProtectedRoute>
              } />
              <Route path="/concrete-funds" element={
                <ProtectedRoute requireAdmin>
                  <ConcreteMutualFunds />
                </ProtectedRoute>
              } />
              <Route path="/background-processing" element={
                <ProtectedRoute requireAdmin>
                  <BackgroundProcessingPage />
                </ProtectedRoute>
              } />
              
              {/* Admin-only route */}
              <Route path="/user-management" element={
                <ProtectedRoute requireAdmin>
                  <UserManagement />
                </ProtectedRoute>
              } />
              
              <Route path="*" element={<Navigate to="/analyze" replace />} />
            </Routes>
          </AppLayout>
        </ProtectedRoute>
      } />
    </Routes>
  );
};

const App: React.FC = () => {
  const [isDarkMode, setIsDarkMode] = useState(() => {
    const saved = localStorage.getItem('theme');
    return saved === 'dark';
  });

  useEffect(() => {
    localStorage.setItem('theme', isDarkMode ? 'dark' : 'light');
  }, [isDarkMode]);

  const toggleTheme = () => {
    setIsDarkMode(!isDarkMode);
  };

  const { darkAlgorithm, defaultAlgorithm } = theme;

  return (
    <BrowserRouter>
      <AuthProvider>
        <ConfigProvider
          theme={{
            algorithm: isDarkMode ? darkAlgorithm : defaultAlgorithm,
            token: {
              colorPrimary: '#1890ff',
              borderRadius: 8,
              fontFamily: '"Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
              fontSize: 14,
              lineHeight: 1.5715,
              controlHeight: 32,
              controlHeightSM: 24,
              controlHeightLG: 40,
            },
          }}
        >
          <AppContent isDarkMode={isDarkMode} toggleTheme={toggleTheme} />
        </ConfigProvider>
      </AuthProvider>
    </BrowserRouter>
  );
};

export default App;
