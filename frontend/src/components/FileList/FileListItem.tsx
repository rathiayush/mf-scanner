import React from 'react';
import { Card, Typography, Progress, Tag, Space, Button } from 'antd';
import { FileTextOutlined, CheckCircleOutlined, ExclamationCircleOutlined, LoadingOutlined, EyeOutlined } from '@ant-design/icons';
import { UploadedFile } from '../../types';
import dayjs from 'dayjs';

const { Text } = Typography;

interface FileListItemProps {
  file: UploadedFile;
  onViewDetails?: (file: UploadedFile) => void;
}

const FileListItem: React.FC<FileListItemProps> = ({ file, onViewDetails }) => {
  const getStatusIcon = () => {
    switch (file.status) {
      case 'completed':
        return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      case 'error':
        return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />;
      case 'processing':
      case 'uploading':
        return <LoadingOutlined style={{ color: '#1890ff' }} />;
      default:
        return <FileTextOutlined style={{ color: '#d9d9d9' }} />;
    }
  };

  const getStatusColor = () => {
    switch (file.status) {
      case 'completed':
        return 'success';
      case 'error':
        return 'error';
      case 'processing':
        return 'processing';
      case 'uploading':
        return 'processing';
      default:
        return 'default';
    }
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  return (
    <Card 
      size="small" 
      style={{ marginBottom: 8 }}
      actions={file.status === 'completed' && onViewDetails ? [
        <Button 
          type="link" 
          icon={<EyeOutlined />} 
          onClick={() => onViewDetails(file)}
          key="view"
        >
          View Details
        </Button>
      ] : undefined}
    >
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Space>
          {getStatusIcon()}
          <div>
            <Text strong>{file.name}</Text>
            <br />
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {formatFileSize(file.size)} • {dayjs(file.uploadedAt).format('MMM DD, YYYY HH:mm')}
            </Text>
          </div>
        </Space>
        
        <Space direction="vertical" align="end" size="small">
          <Tag color={getStatusColor()}>
            {file.status.toUpperCase()}
          </Tag>
          
          {(file.status === 'uploading' || file.status === 'processing') && (
            <Progress 
              percent={file.progress} 
              size="small" 
              style={{ width: 100 }}
              showInfo={false}
            />
          )}
          
          {file.error && (
            <Text type="danger" style={{ fontSize: '12px', maxWidth: 200 }}>
              {file.error}
            </Text>
          )}
        </Space>
      </div>
    </Card>
  );
};

export default FileListItem;
