import React, { useState, useEffect } from 'react';
import { 
  Upload, 
  Select, 
  Button, 
  Card, 
  Space, 
  Typography, 
  message, 
  Progress, 
  Table, 
  Tag,
  Row,
  Col,
  Alert,
  DatePicker
} from 'antd';
import dayjs from 'dayjs';
import { 
  InboxOutlined, 
  UploadOutlined, 
  DeleteOutlined, 
  FileExcelOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons';
import { mutualFundService, fileUploadService } from '../../services/api';
import { MutualFundDto, ProcessedFileDto } from '../../types';

const { Dragger } = Upload;
const { Title, Text } = Typography;
const { Option } = Select;

interface FileUploadItem {
  id: string;
  file: File;
  mutualFundId?: number;
  mutualFundName?: string;
  reportDate?: dayjs.Dayjs;
  status: 'pending' | 'uploading' | 'success' | 'error';
  progress: number;
  error?: string;
  result?: ProcessedFileDto;
}

const EnhancedFileUpload: React.FC = () => {
  const [mutualFunds, setMutualFunds] = useState<MutualFundDto[]>([]);
  const [selectedMutualFund, setSelectedMutualFund] = useState<number | undefined>();
  const [fileList, setFileList] = useState<FileUploadItem[]>([]);
  const [uploading, setUploading] = useState(false);
  const [, setSearchValue] = useState('');

  useEffect(() => {
    loadMutualFunds();
  }, []);

  const loadMutualFunds = async () => {
    try {
      const funds = await mutualFundService.getMutualFundsForDropdown();
      setMutualFunds(funds);
    } catch (error) {
      message.error('Failed to load mutual funds');
    }
  };

  const handleMutualFundSearch = async (value: string) => {
    setSearchValue(value);
    if (value) {
      try {
        const funds = await mutualFundService.getMutualFundsForDropdown(value);
        setMutualFunds(funds);
      } catch (error) {
        message.error('Failed to search mutual funds');
      }
    } else {
      loadMutualFunds();
    }
  };

  const beforeUpload = (file: File) => {
    const isValidType = file.type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' ||
                       file.type === 'application/vnd.ms-excel' ||
                       file.type === 'text/csv';
    
    if (!isValidType) {
      message.error('You can only upload Excel or CSV files!');
      return false;
    }

    const isLt10M = file.size / 1024 / 1024 < 10;
    if (!isLt10M) {
      message.error('File must be smaller than 10MB!');
      return false;
    }

    // Add file to list
    const newFile: FileUploadItem = {
      id: Date.now().toString() + Math.random(),
      file,
      mutualFundId: selectedMutualFund,
      mutualFundName: mutualFunds.find(f => f.id === selectedMutualFund)?.fundName,
      reportDate: dayjs(), // Default to current date
      status: 'pending',
      progress: 0
    };

    setFileList(prev => [...prev, newFile]);
    return false; // Prevent automatic upload
  };

  const removeFile = (id: string) => {
    setFileList(prev => prev.filter(item => item.id !== id));
  };

  const updateFileStatus = (id: string, updates: Partial<FileUploadItem>) => {
    setFileList(prev => prev.map(item => 
      item.id === id ? { ...item, ...updates } : item
    ));
  };

  const assignMutualFund = (fileId: string, mutualFundId: number) => {
    const fund = mutualFunds.find(f => f.id === mutualFundId);
    updateFileStatus(fileId, {
      mutualFundId,
      mutualFundName: fund?.fundName
    });
  };

  const assignReportDate = (fileId: string, date: dayjs.Dayjs | null) => {
    updateFileStatus(fileId, {
      reportDate: date || undefined
    });
  };

  const uploadFiles = async () => {
    const pendingFiles = fileList.filter(f => f.status === 'pending' && f.mutualFundId && f.reportDate);
    
    if (pendingFiles.length === 0) {
      const missingMutualFund = fileList.some(f => f.status === 'pending' && !f.mutualFundId);
      const missingReportDate = fileList.some(f => f.status === 'pending' && !f.reportDate);
      
      if (missingMutualFund && missingReportDate) {
        message.warning('Please select mutual funds and report dates for your files.');
      } else if (missingMutualFund) {
        message.warning('Please select mutual funds for your files.');
      } else if (missingReportDate) {
        message.warning('Please select report dates for your files.');
      }
      return;
    }

    setUploading(true);

    // Group files by mutual fund
    const filesByFund = pendingFiles.reduce((acc, file) => {
      const fundId = file.mutualFundId!;
      if (!acc[fundId]) acc[fundId] = [];
      acc[fundId].push(file);
      return acc;
    }, {} as Record<number, FileUploadItem[]>);

    // Prepare files for upload with report dates
    const filesWithDates = pendingFiles.map(file => ({
      mutualFundId: file.mutualFundId!,
      file: file.file,
      reportDate: file.reportDate?.format('YYYY-MM-DD')
    }));

    try {
      // Update status to uploading
      pendingFiles.forEach(file => {
        updateFileStatus(file.id, { status: 'uploading', progress: 50 });
      });

      const response = await fileUploadService.uploadFilesWithDates(filesWithDates);

      // Update status based on response
      pendingFiles.forEach((file, index) => {
        const processedFile = response.processedFiles[index];
        if (processedFile && processedFile.status === 'Success') {
          updateFileStatus(file.id, {
            status: 'success',
            progress: 100,
            result: processedFile
          });
        } else {
          updateFileStatus(file.id, {
            status: 'error',
            progress: 0,
            error: processedFile?.errorMessage || 'Upload failed'
          });
        }
      });

      if (response.success) {
        message.success(`Successfully uploaded ${pendingFiles.length} files`);
      } else {
        message.warning(`Upload completed with some errors: ${response.message}`);
      }

    } catch (error: any) {
      pendingFiles.forEach(file => {
        updateFileStatus(file.id, {
          status: 'error',
          progress: 0,
          error: error.message || 'Upload failed'
        });
      });
      message.error('Failed to upload files');
    }

    setUploading(false);
  };

  const clearCompleted = () => {
    setFileList(prev => prev.filter(f => f.status === 'pending'));
  };

  const columns = [
    {
      title: 'File Name',
      dataIndex: 'file',
      key: 'fileName',
      render: (file: File) => (
        <Space>
          <FileExcelOutlined />
          <Text>{file.name}</Text>
        </Space>
      ),
    },
    {
      title: 'Size',
      dataIndex: 'file',
      key: 'size',
      render: (file: File) => `${(file.size / 1024 / 1024).toFixed(2)} MB`,
    },
    {
      title: 'Mutual Fund',
      key: 'mutualFund',
      render: (record: FileUploadItem) => (
        record.status === 'pending' ? (
          <Select
            style={{ width: 200 }}
            placeholder="Select mutual fund"
            showSearch
            filterOption={false}
            onSearch={handleMutualFundSearch}
            value={record.mutualFundId}
            onChange={(value) => assignMutualFund(record.id, value)}
          >
            {mutualFunds.map(fund => (
              <Option key={fund.id} value={fund.id}>
                {fund.fundName}
              </Option>
            ))}
          </Select>
        ) : (
          <Text>{record.mutualFundName}</Text>
        )
      ),
    },
    {
      title: 'Report Date',
      key: 'reportDate',
      render: (record: FileUploadItem) => (
        record.status === 'pending' ? (
          <DatePicker
            style={{ width: 140 }}
            placeholder="Select date"
            value={record.reportDate}
            onChange={(date) => assignReportDate(record.id, date)}
            format="YYYY-MM-DD"
          />
        ) : (
          <Text>{record.reportDate?.format('YYYY-MM-DD') || 'N/A'}</Text>
        )
      ),
    },
    {
      title: 'Status',
      key: 'status',
      render: (record: FileUploadItem) => {
        const statusConfig = {
          pending: { color: 'default', text: 'Pending' },
          uploading: { color: 'processing', text: 'Uploading' },
          success: { color: 'success', text: 'Success' },
          error: { color: 'error', text: 'Error' }
        };
        
        const config = statusConfig[record.status];
        return (
          <Space direction="vertical" size="small">
            <Tag color={config.color} icon={
              record.status === 'success' ? <CheckCircleOutlined /> :
              record.status === 'error' ? <ExclamationCircleOutlined /> : undefined
            }>
              {config.text}
            </Tag>
            {record.status === 'uploading' && (
              <Progress percent={record.progress} size="small" />
            )}
            {record.error && (
              <Text type="danger" style={{ fontSize: '12px' }}>{record.error}</Text>
            )}
            {record.result && (
              <Text type="success" style={{ fontSize: '12px' }}>
                {record.result.holdingsCount} holdings processed
              </Text>
            )}
          </Space>
        );
      },
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (record: FileUploadItem) => (
        record.status === 'pending' && (
          <Button 
            type="text" 
            danger 
            icon={<DeleteOutlined />}
            onClick={() => removeFile(record.id)}
          />
        )
      ),
    },
  ];

  const hasFilesReadyToUpload = fileList.some(f => f.status === 'pending' && f.mutualFundId && f.reportDate);
  const hasCompletedFiles = fileList.some(f => f.status === 'success' || f.status === 'error');

  return (
    <div>
      <Title level={2}>Upload Portfolio Files</Title>
      <Text type="secondary">
        Upload Excel or CSV files containing mutual fund portfolio data. You can upload multiple files for different mutual funds.
      </Text>

      <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
        <Col span={24}>
          <Card title="Select Default Mutual Fund (Optional)">
            <Select
              style={{ width: '100%' }}
              placeholder="Select a mutual fund to pre-assign to uploaded files"
              showSearch
              allowClear
              filterOption={false}
              onSearch={handleMutualFundSearch}
              value={selectedMutualFund}
              onChange={setSelectedMutualFund}
            >
              {mutualFunds.map(fund => (
                <Option key={fund.id} value={fund.id}>
                  <Space>
                    <Text strong>{fund.fundName}</Text>
                    <Text type="secondary">({fund.fundCode})</Text>
                  </Space>
                </Option>
              ))}
            </Select>
          </Card>
        </Col>

        <Col span={24}>
          <Card title="Upload Files">
            <Dragger
              multiple
              beforeUpload={beforeUpload}
              showUploadList={false}
              disabled={uploading}
            >
              <p className="ant-upload-drag-icon">
                <InboxOutlined />
              </p>
              <p className="ant-upload-text">Click or drag files to this area to upload</p>
              <p className="ant-upload-hint">
                Support for Excel (.xlsx, .xls) and CSV files. Maximum file size: 10MB
              </p>
            </Dragger>
          </Card>
        </Col>

        {fileList.length > 0 && (
          <Col span={24}>
            <Card 
              title="Files to Upload"
              extra={
                <Space>
                  {hasCompletedFiles && (
                    <Button onClick={clearCompleted}>
                      Clear Completed
                    </Button>
                  )}
                  <Button
                    type="primary"
                    icon={<UploadOutlined />}
                    loading={uploading}
                    disabled={!hasFilesReadyToUpload}
                    onClick={uploadFiles}
                  >
                    Upload Files ({fileList.filter(f => f.status === 'pending' && f.mutualFundId && f.reportDate).length})
                  </Button>
                </Space>
              }
            >
              {!hasFilesReadyToUpload && fileList.some(f => f.status === 'pending') && (
                <Alert
                  message="Please assign mutual funds and report dates to your files before uploading"
                  type="warning"
                  style={{ marginBottom: 16 }}
                />
              )}
              
              <Table
                dataSource={fileList}
                columns={columns}
                rowKey="id"
                pagination={false}
                size="small"
              />
            </Card>
          </Col>
        )}
      </Row>
    </div>
  );
};

export default EnhancedFileUpload;
