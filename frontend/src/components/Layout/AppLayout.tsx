import React from 'react';
import { Link } from 'react-router-dom';
import { Layout, Typography, Space, Button, Menu, Switch, Dropdown, Avatar } from 'antd';
import { FileTextOutlined, ApiOutlined, BarChartOutlined, UnorderedListOutlined, RobotOutlined, DatabaseOutlined, StockOutlined, PlayCircleOutlined, SettingOutlined, BulbOutlined, BulbFilled, UserOutlined, LogoutOutlined, TeamOutlined } from '@ant-design/icons';
import { useAuth } from '../../contexts/AuthContext';

const { Header, Content, Footer, Sider } = Layout;
const { Title } = Typography;

interface AppLayoutProps {
  children: React.ReactNode;
  selectedKey?: string;
  onMenuSelect?: (key: string) => void;
  isDarkMode: boolean;
  toggleTheme: () => void;
}

const AppLayout: React.FC<AppLayoutProps> = ({ children, selectedKey = '2', isDarkMode, toggleTheme }) => {
  const { user, logout, isAdmin } = useAuth();

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
    ...(isAdmin ? [{
      key: '7',
      icon: <DatabaseOutlined />,
      label: <Link to="/concrete-funds">Concrete Mutual Funds</Link>,
    }] : []),
    {
      key: '10',
      icon: <SettingOutlined />,
      label: <Link to="/snapshot-management">Snapshot Management</Link>,
    },
    ...(isAdmin ? [{
      key: '6',
      icon: <RobotOutlined />,
      label: <Link to="/upload-v2">Smart Upload V2</Link>,
    }] : []),
    ...(isAdmin ? [{
      key: '9',
      icon: <PlayCircleOutlined />,
      label: <Link to="/background-processing">Background Processing</Link>,
    }] : []),
    ...(isAdmin ? [{
      key: '11',
      icon: <TeamOutlined />,
      label: <Link to="/user-management">User Management</Link>,
    }] : []),
  ];

  const userMenuItems = [
    {
      key: 'user-info',
      label: (
        <div>
          <div style={{ fontWeight: 'bold' }}>{user?.username}</div>
          <div style={{ fontSize: '12px', color: '#888' }}>{user?.email}</div>
          <div style={{ fontSize: '11px', marginTop: '4px' }}>
            <span style={{ 
              padding: '2px 8px', 
              borderRadius: '4px', 
              background: user?.role === 'Admin' ? '#1890ff' : '#52c41a',
              color: 'white'
            }}>
              {user?.role}
            </span>
          </div>
        </div>
      ),
      disabled: true,
    },
    {
      type: 'divider' as const,
    },
    {
      key: 'logout',
      label: 'Logout',
      icon: <LogoutOutlined />,
      onClick: logout,
      danger: true,
    },
  ];

  return (
    <Layout id="app-layout-container" style={{ minHeight: '100vh' }}>
      <Header id="app-layout-header" style={{ 
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
          <Space>
            <Space align="center" style={{ marginRight: 16 }}>
              {isDarkMode ? <BulbFilled style={{ fontSize: 16, color: '#faad14' }} /> : <BulbOutlined style={{ fontSize: 16 }} />}
              <Switch 
                checked={isDarkMode}
                onChange={toggleTheme}
                checkedChildren="Dark"
                unCheckedChildren="Light"
              />
            </Space>
            <Button 
              id="app-layout-api-docs-btn"
              type="primary" 
              icon={<ApiOutlined />}
            >
              <Link to="https://localhost:7052/swagger/index.html" target="_blank">API Documentation</Link>
            </Button>
            <Dropdown menu={{ items: userMenuItems }} placement="bottomRight" trigger={['click']}>
              <Button type="text" style={{ height: 'auto', padding: '4px 8px' }}>
                <Space>
                  <Avatar icon={<UserOutlined />} style={{ backgroundColor: '#1890ff' }} />
                  <span style={{ color: isDarkMode ? '#fff' : '#000' }}>{user?.username}</span>
                </Space>
              </Button>
            </Dropdown>
          </Space>
        </Space>
      </Header>
      
      <Layout id="app-layout-main">
        <Sider id="app-layout-sider" collapsible theme={isDarkMode ? 'dark' : 'light'}>
         
          <Menu
            id="app-layout-menu"
            theme={isDarkMode ? 'dark' : 'light'}
            defaultSelectedKeys={[selectedKey]}
            selectedKeys={[selectedKey]}
            mode="inline"
            items={menuItems}
          />
        </Sider>
        
        <Layout id="app-layout-content-wrapper" style={{ 
          padding: '24px',
          minHeight: 'calc(100vh - 128px)'
        }}>
          <Content id="app-layout-content" style={{ 
            margin: 0,
            padding: 0,
            minHeight: '100%',
          }}>
            <div id="app-layout-content-inner" style={{
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
        padding: '16px 50px'
      }}>
        <span id="app-layout-footer-text">Mutual Fund Scanner ©2024 - Upload and analyze mutual fund portfolio files</span>
      </Footer>
    </Layout>
  );
};

export default AppLayout;
