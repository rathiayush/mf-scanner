import React, { useState, useEffect } from 'react';
import { Card, Button, Table, Upload, message, Row, Col, Typography, Space, Tag, Progress, Alert } from 'antd';
import { CloudUploadOutlined, FolderOpenOutlined, PlayCircleOutlined, ReloadOutlined, DeleteOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import type { UploadFile, UploadProps } from 'antd/es/upload';
import dayjs from 'dayjs';
import { apiService } from '../services/api';

const { Title, Text } = Typography;
const { Dragger } = Upload;

interface BatchFile {
  id: string;
  fileName: string;
  fileSize: number;
  uploadDate: string;
  status: 'Pending' | 'Processing' | 'Completed' | 'Failed';
  progress: number;
  errorMessage?: string;
}

const UploadFilesTriggerPage: React.FC = () => {
  const [files, setFiles] = useState<BatchFile[]>([]);
  const [uploading, setUploading] = useState(false);
  const [processing, setProcessing] = useState(false);
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);

  useEffect(() => {
    fetchBatchFiles();
  }, []);

  const fetchBatchFiles = async () => {
    try {
      const batchFiles = await apiService.getBatchFiles();
      setFiles(batchFiles);
    } catch (error) {
      console.error('Error fetching batch files:', error);
      message.error('Failed to fetch batch files');
    }
  };

  const uploadProps: UploadProps = {
    name: 'files',
    multiple: true,
    accept: '.xlsx,.xls,.csv',
    fileList,
    beforeUpload: (file) => {
      const isValidType = file.type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' ||
                          file.type === 'application/vnd.ms-excel' ||
                          file.type === 'text/csv' ||
                          file.name.endsWith('.xlsx') ||
                          file.name.endsWith('.xls') ||
                          file.name.endsWith('.csv');
      
      if (!isValidType) {
        message.error(`${file.name} is not a valid file type. Please upload Excel or CSV files only.`);
        return false;
      }

      const isLt10M = file.size / 1024 / 1024 < 10;
      if (!isLt10M) {
        message.error(`${file.name} must be smaller than 10MB!`);
        return false;
      }

      return false; // Prevent automatic upload
    },
    onChange: (info) => {
      setFileList(info.fileList);
    },
    onDrop: (e) => {
      console.log('Dropped files', e.dataTransfer.files);
    },
  };

  const handleBatchUpload = async () => {
    if (fileList.length === 0) {
      message.warning('Please select files to upload');
      return;
    }

    setUploading(true);
    try {
      const formData = new FormData();
      fileList.forEach((file) => {
        if (file.originFileObj) {
          formData.append('files', file.originFileObj);
        }
      });

      await apiService.uploadBatchFiles(formData);
      message.success('Files uploaded successfully');
      setFileList([]);
      fetchBatchFiles();
    } catch (error) {
      console.error('Error uploading files:', error);
      message.error('Failed to upload files');
    } finally {
      setUploading(false);
    }
  };

  const handleTriggerProcessing = async () => {
    const pendingFiles = files.filter((f: BatchFile) => f.status === 'Pending');
    if (pendingFiles.length === 0) {
      message.warning('No pending files to process');
      return;
    }

    setProcessing(true);
    try {
      await apiService.triggerBatchProcessing();
      message.success('Batch processing triggered successfully');
      
      // Start polling for updates
      const interval = setInterval(async () => {
        await fetchBatchFiles();
        const updatedFiles = await apiService.getBatchFiles();
        const stillProcessing = updatedFiles.some((f: BatchFile) => f.status === 'Processing');
        
        if (!stillProcessing) {
          clearInterval(interval);
          setProcessing(false);
        }
      }, 2000);

    } catch (error) {
      console.error('Error triggering batch processing:', error);
      message.error('Failed to trigger batch processing');
      setProcessing(false);
    }
  };

  const handleDeleteSelected = async () => {
    if (selectedRowKeys.length === 0) {
      message.warning('Please select files to delete');
      return;
    }

    try {
      await apiService.deleteBatchFiles(selectedRowKeys as string[]);
      message.success('Selected files deleted successfully');
      setSelectedRowKeys([]);
      fetchBatchFiles();
    } catch (error) {
      console.error('Error deleting files:', error);
      message.error('Failed to delete selected files');
    }
  };

  const getStatusTag = (status: string) => {
    const statusConfig = {
      'Pending': { color: 'default', text: 'Pending' },
      'Processing': { color: 'processing', text: 'Processing' },
      'Completed': { color: 'success', text: 'Completed' },
      'Failed': { color: 'error', text: 'Failed' },
    };
    
    const config = statusConfig[status as keyof typeof statusConfig] || statusConfig.Pending;
    return <Tag color={config.color}>{config.text}</Tag>;
  };

  const columns: ColumnsType<BatchFile> = [
    {
      title: 'File Name',
      dataIndex: 'fileName',
      key: 'fileName',
      width: 300,
      ellipsis: true,
    },
    {
      title: 'File Size',
      dataIndex: 'fileSize',
      key: 'fileSize',
      width: 120,
      render: (size: number) => {
        const sizeInMB = size / (1024 * 1024);
        return sizeInMB > 1 ? `${sizeInMB.toFixed(2)} MB` : `${(size / 1024).toFixed(2)} KB`;
      },
    },
    {
      title: 'Upload Date',
      dataIndex: 'uploadDate',
      key: 'uploadDate',
      width: 150,
      render: (date: string) => dayjs(date).format('DD/MM/YYYY HH:mm'),
      sorter: (a, b) => dayjs(a.uploadDate).unix() - dayjs(b.uploadDate).unix(),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      render: (status: string) => getStatusTag(status),
      filters: [
        { text: 'Pending', value: 'Pending' },
        { text: 'Processing', value: 'Processing' },
        { text: 'Completed', value: 'Completed' },
        { text: 'Failed', value: 'Failed' },
      ],
      onFilter: (value, record) => record.status === value,
    },
    {
      title: 'Progress',
      dataIndex: 'progress',
      key: 'progress',
      width: 150,
      render: (progress: number, record: BatchFile) => {
        if (record.status === 'Processing') {
          return <Progress percent={progress} size="small" />;
        }
        if (record.status === 'Completed') {
          return <Progress percent={100} size="small" status="success" />;
        }
        if (record.status === 'Failed') {
          return <Progress percent={progress} size="small" status="exception" />;
        }
        return <Progress percent={0} size="small" />;
      },
    },
    {
      title: 'Error Message',
      dataIndex: 'errorMessage',
      key: 'errorMessage',
      width: 200,
      ellipsis: true,
      render: (error: string) => error ? <Text type="danger">{error}</Text> : '-',
    },
  ];

  const rowSelection = {
    selectedRowKeys,
    onChange: (newSelectedRowKeys: React.Key[]) => {
      setSelectedRowKeys(newSelectedRowKeys);
    },
    getCheckboxProps: (record: BatchFile) => ({
      disabled: record.status === 'Processing',
    }),
  };

  const getProcessingStats = () => {
    const pending = files.filter(f => f.status === 'Pending').length;
    const processing = files.filter(f => f.status === 'Processing').length;
    const completed = files.filter(f => f.status === 'Completed').length;
    const failed = files.filter(f => f.status === 'Failed').length;
    
    return { pending, processing, completed, failed, total: files.length };
  };

  const stats = getProcessingStats();

  return (
    <div>
      <Title level={2}>
        <CloudUploadOutlined /> Upload Files & Trigger Processing
      </Title>
      
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={24}>
          <Card title="Batch File Upload" extra={
            <Space>
              <Button
                type="primary"
                onClick={handleBatchUpload}
                loading={uploading}
                disabled={fileList.length === 0}
              >
                Upload Files
              </Button>
            </Space>
          }>
            <Dragger {...uploadProps} style={{ marginBottom: 16 }}>
              <p className="ant-upload-drag-icon">
                <FolderOpenOutlined />
              </p>
              <p className="ant-upload-text">Click or drag files to this area to upload</p>
              <p className="ant-upload-hint">
                Support for multiple file upload. Only Excel (.xlsx, .xls) and CSV files are allowed.
                Maximum file size: 10MB per file.
              </p>
            </Dragger>
            
            {fileList.length > 0 && (
              <Alert
                message={`${fileList.length} file(s) selected for upload`}
                type="info"
                showIcon
                style={{ marginTop: 16 }}
              />
            )}
          </Card>
        </Col>
      </Row>

      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Text type="secondary">Total Files</Text>
              <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#1890ff' }}>
                {stats.total}
              </div>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Text type="secondary">Pending</Text>
              <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#faad14' }}>
                {stats.pending}
              </div>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Text type="secondary">Processing</Text>
              <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#1890ff' }}>
                {stats.processing}
              </div>
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <div style={{ textAlign: 'center' }}>
              <Text type="secondary">Completed</Text>
              <div style={{ fontSize: '24px', fontWeight: 'bold', color: '#52c41a' }}>
                {stats.completed}
              </div>
            </div>
          </Card>
        </Col>
      </Row>

      <Card 
        title="Batch Processing Queue"
        extra={
          <Space>
            <Button
              icon={<ReloadOutlined />}
              onClick={fetchBatchFiles}
            >
              Refresh
            </Button>
            <Button
              icon={<DeleteOutlined />}
              onClick={handleDeleteSelected}
              disabled={selectedRowKeys.length === 0}
              danger
            >
              Delete Selected
            </Button>
            <Button
              type="primary"
              icon={<PlayCircleOutlined />}
              onClick={handleTriggerProcessing}
              loading={processing}
              disabled={stats.pending === 0}
            >
              Trigger Processing
            </Button>
          </Space>
        }
      >
        <Table
          columns={columns}
          dataSource={files}
          rowKey="id"
          rowSelection={rowSelection}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} items`,
          }}
          scroll={{ x: 1000 }}
        />
      </Card>
    </div>
  );
};

export default UploadFilesTriggerPage;
