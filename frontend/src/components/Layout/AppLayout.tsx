import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Layout, Typography, Space, Button, Menu } from 'antd';
import { FileTextOutlined, ApiOutlined, UploadOutlined, BarChartOutlined, UnorderedListOutlined, RobotOutlined, DatabaseOutlined, StockOutlined } from '@ant-design/icons';

const { Header, Content, Footer, Sider } = Layout;
const { Title } = Typography;

interface AppLayoutProps {
  children: React.ReactNode;
  selectedKey?: string;
  onMenuSelect?: (key: string) => void;
}

const AppLayout: React.FC<AppLayoutProps> = ({ children, selectedKey = '1' }) => {
  const navigate = useNavigate();
  const menuItems = [
    {
      key: '1',
      icon: <UploadOutlined />,
      label: <Link to="/">Upload File</Link>,
    },
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
    {
      key: '6',
      icon: <RobotOutlined />,
      label: <Link to="/upload-v2">Smart Upload V2</Link>,
    },
  ];
  
  // Submenu items for Holdings
  const holdingsSubMenuItems = [
    {
      key: '5-1',
      label: <Link to="/current-holdings">Current Holdings</Link>,
    },
    {
      key: '5-2',
      label: <Link to="/holdings-comparison">Holdings Comparison</Link>,
    },
    {
      key: '5-3',
      label: <Link to="/portfolio-summary">Portfolio Summary</Link>,
    },
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ 
        background: '#fff',
        padding: '0 24px',
        display: 'flex',
        alignItems: 'center',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
      }}>
        <Space align="center" style={{ width: '100%', justifyContent: 'space-between' }}>
          <Space align="center">
            <FileTextOutlined style={{ fontSize: '24px', color: '#1890ff' }} />
            <Title level={3} style={{ margin: 0, color: '#1890ff' }}>
              Mutual Fund Scanner
            </Title>
          </Space>
          <Button 
            type="primary" 
            icon={<ApiOutlined />}
          >
            <Link to="https://localhost:7052/swagger/index.html" target="_blank">API Documentation</Link>
          </Button>
        </Space>
      </Header>
      
      <Layout>
        <Sider collapsible>
         
          <Menu
            theme="light"
            defaultSelectedKeys={[selectedKey]}
            selectedKeys={[selectedKey]}
            mode="inline"
            style={{
              borderRight: '1px solid #f0f0f0',
              background: '#fff'
            }}
            items={[
              ...menuItems,
              {
                key: '5',
                icon: <FileTextOutlined />,
                label: 'Holdings',
                children: holdingsSubMenuItems,
              },
            ]}
            onClick={({ key }) => {
              // Handle direct navigation for submenu items
              if (key === '5-1') navigate('/current-holdings');
              if (key === '5-2') navigate('/holdings-comparison');
              if (key === '5-3') navigate('/portfolio-summary');
            }}
          />
        </Sider>
        
        <Layout style={{ 
          padding: '24px',
          background: '#f0f2f5',
          minHeight: 'calc(100vh - 128px)'
        }}>
          <Content style={{ 
            margin: 0,
            padding: 0,
            minHeight: '100%',
          }}>
            <div style={{
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
      
      <Footer style={{ 
        textAlign: 'center',
        background: '#fff',
        borderTop: '1px solid #f0f0f0',
        padding: '16px 50px'
      }}>
        Mutual Fund Scanner ©2024 - Upload and analyze mutual fund portfolio files
      </Footer>
    </Layout>
  );
};

export default AppLayout;
