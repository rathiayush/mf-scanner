import React from 'react';
import { Link } from 'react-router-dom';
import { Layout, Typography, Space, Button, Menu } from 'antd';
import { FileTextOutlined, ApiOutlined, BarChartOutlined, UnorderedListOutlined, RobotOutlined, DatabaseOutlined, StockOutlined, PlayCircleOutlined, SettingOutlined } from '@ant-design/icons';

const { Header, Content, Footer, Sider } = Layout;
const { Title } = Typography;

interface AppLayoutProps {
  children: React.ReactNode;
  selectedKey?: string;
  onMenuSelect?: (key: string) => void;
}

const AppLayout: React.FC<AppLayoutProps> = ({ children, selectedKey = '2' }) => {
  const menuItems = [
    {
      key: '2',
      icon: <BarChartOutlined />,
      label: <Link to="/analyze">Analyse</Link>,
    },
    {
      key: '3',
      icon: <StockOutlined />,
      label: <Link to="/security-analytics">Security Analytics</Link>,
    },
    {
      key: '4',
      icon: <UnorderedListOutlined />,
      label: <Link to="/mutual-funds">Mutual Fund List</Link>,
    },
    {
      key: '7',
      icon: <DatabaseOutlined />,
      label: <Link to="/concrete-funds">Concrete Mutual Funds</Link>,
    },
    // Portfolio Holdings menu item removed
    {
      key: '10',
      icon: <SettingOutlined />,
      label: <Link to="/snapshot-management">Snapshot Management</Link>,
    },
    {
      key: '6',
      icon: <RobotOutlined />,
      label: <Link to="/upload-v2">Smart Upload V2</Link>,
    },
    {
      key: '9',
      icon: <PlayCircleOutlined />,
      label: <Link to="/background-processing">Background Processing</Link>,
    },
  ];

  return (
    <Layout id="app-layout-container" style={{ minHeight: '100vh' }}>
      <Header id="app-layout-header" style={{ 
        background: '#fff',
        padding: '0 24px',
        display: 'flex',
        alignItems: 'center',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
      }}>
        <Space id="app-layout-header-space" align="center" style={{ width: '100%', justifyContent: 'space-between' }}>
          <Space id="app-layout-logo-space" align="center">
            <FileTextOutlined id="app-layout-logo-icon" style={{ fontSize: '24px', color: '#1890ff' }} />
            <Title id="app-layout-title" level={3} style={{ margin: 0, color: '#1890ff' }}>
              Mutual Fund Scanner
            </Title>
          </Space>
          <Button 
            id="app-layout-api-docs-btn"
            type="primary" 
            icon={<ApiOutlined />}
          >
            <Link to="https://localhost:7052/swagger/index.html" target="_blank">API Documentation</Link>
          </Button>
        </Space>
      </Header>
      
      <Layout id="app-layout-main">
        <Sider id="app-layout-sider" collapsible>
         
          <Menu
            id="app-layout-menu"
            theme="light"
            defaultSelectedKeys={[selectedKey]}
            selectedKeys={[selectedKey]}
            mode="inline"
            style={{
              borderRight: '1px solid #f0f0f0',
              background: '#fff'
            }}
            items={menuItems}
          />
        </Sider>
        
        <Layout id="app-layout-content-wrapper" style={{ 
          padding: '24px',
          background: '#f0f2f5',
          minHeight: 'calc(100vh - 128px)'
        }}>
          <Content id="app-layout-content" style={{ 
            margin: 0,
            padding: 0,
            minHeight: '100%',
          }}>
            <div id="app-layout-content-inner" style={{
              background: '#fff',
              borderRadius: '8px',
              boxShadow: '0 1px 2px 0 rgba(0,0,0,0.03)',
              padding: '24px',
              minHeight: '100%',
              boxSizing: 'border-box'
            }}>
              {children}
            </div>
          </Content>
        </Layout>
      </Layout>
      
      <Footer id="app-layout-footer" style={{ 
        textAlign: 'center',
        background: '#fff',
        borderTop: '1px solid #f0f0f0',
        padding: '16px 50px'
      }}>
        <span id="app-layout-footer-text">Mutual Fund Scanner ©2024 - Upload and analyze mutual fund portfolio files</span>
      </Footer>
    </Layout>
  );
};

export default AppLayout;
