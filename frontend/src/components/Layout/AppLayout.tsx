import React from 'react';
import { Layout, Typography, Space, Button, Menu } from 'antd';
import { FileTextOutlined, ApiOutlined, UploadOutlined, BarChartOutlined, UnorderedListOutlined, RobotOutlined, DatabaseOutlined, StockOutlined } from '@ant-design/icons';

const { Header, Content, Footer, Sider } = Layout;
const { Title } = Typography;

interface AppLayoutProps {
  children: React.ReactNode;
  selectedKey?: string;
  onMenuSelect?: (key: string) => void;
}

const AppLayout: React.FC<AppLayoutProps> = ({ children, selectedKey = '1', onMenuSelect }) => {
  const menuItems = [
    {
      key: '1',
      icon: <UploadOutlined />,
      label: 'Upload File',
    },
    {
      key: '2',
      icon: <BarChartOutlined />,
      label: 'Analyse',
    },
    {
      key: '3',
      icon: <StockOutlined />,
      label: 'Security Analytics',
    },
    {
      key: '4',
      icon: <UnorderedListOutlined />,
      label: 'Mutual Fund List',
    },
    {
      key: '7',
      icon: <DatabaseOutlined />,
      label: 'Concrete Mutual Funds',
    },
    {
      key: '6',
      icon: <RobotOutlined />,
      label: 'Smart Upload V2',
    },
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ 
        background: '#fff', 
        padding: '0 24px', 
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
        display: 'flex',
        alignItems: 'center'
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
            onClick={() => window.open('https://localhost:7052/swagger/index.html', '_blank')}
          >
            API Documentation
          </Button>
        </Space>
      </Header>
      
      <Layout>
        <Sider width={250} style={{ background: '#fff' }}>
          <Menu
            mode="inline"
            selectedKeys={[selectedKey]}
            style={{ height: '100%', borderRight: 0 }}
            items={menuItems}
            onClick={({ key }) => onMenuSelect?.(key)}
          />
        </Sider>
        
        <Layout style={{ padding: '0 24px 24px' }}>
          <Content style={{ 
            padding: '24px',
            background: '#f5f5f5',
            margin: 0,
            minHeight: 280,
          }}>
            <div style={{
              background: '#fff',
              padding: '24px',
              borderRadius: '8px',
              boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
            }}>
              {children}
            </div>
          </Content>
        </Layout>
      </Layout>
      
      <Footer style={{ 
        textAlign: 'center',
        background: '#fff',
        borderTop: '1px solid #f0f0f0'
      }}>
        Mutual Fund Scanner ©2024 - Upload and analyze mutual fund portfolio files
      </Footer>
    </Layout>
  );
};

export default AppLayout;
