import React, { useState } from 'react';
import { Upload, Button, DatePicker, Card, Table, Tag, message, Space, Typography, Switch } from 'antd';
import { InboxOutlined, UploadOutlined, CheckCircleOutlined, ExclamationCircleOutlined } from '@ant-design/icons';
import { Dayjs } from 'dayjs';
import { UploadV2RequestDto, UploadV2ResponseDto, MutualFundDetectionResult } from '../../types';
import { uploadV2Service } from '../../services/api';

const { Dragger } = Upload;
const { Title, Text } = Typography;

interface UploadV2Props {
  onUploadComplete?: (response: UploadV2ResponseDto) => void;
}

const UploadV2: React.FC<UploadV2Props> = ({ onUploadComplete }) => {
  const [files, setFiles] = useState<File[]>([]);
  const [reportDate, setReportDate] = useState<Dayjs | null>(null);
  const [performDateCheck, setPerformDateCheck] = useState<boolean>(true);
  const [uploading, setUploading] = useState(false);
  const [uploadResponse, setUploadResponse] = useState<UploadV2ResponseDto | null>(null);

  const handleFileChange = (info: any) => {
    const fileList = info.fileList.map((file: any) => file.originFileObj).filter(Boolean);
    setFiles(fileList);
  };

  const handleUpload = async () => {
    if (!reportDate) {
      message.error('Please select a report date');
      return;
    }

    if (files.length === 0) {
      message.error('Please select at least one file');
      return;
    }

    setUploading(true);
    try {
      const request: UploadV2RequestDto = {
        reportDate: reportDate.format('YYYY-MM-DD'),
        files: files,
        performDateCheck: performDateCheck
      };

      const response = await uploadV2Service.uploadFiles(request);
      
      if (response.success) {
        message.success(response.message);
        setUploadResponse(response);
        onUploadComplete?.(response);
      } else {
        message.error(response.message);
        setUploadResponse(response);
      }
    } catch (error) {
      console.error('Upload error:', error);
      message.error('Upload failed. Please try again.');
    } finally {
      setUploading(false);
    }
  };

  const handleReset = () => {
    setFiles([]);
    setReportDate(null);
    setPerformDateCheck(true);
    setUploadResponse(null);
  };

  const detectionColumns = [
    {
      title: 'File Name',
      dataIndex: 'fileName',
      key: 'fileName',
      width: 200,
    },
    {
      title: 'Detected Fund',
      dataIndex: 'detectedFundName',
      key: 'detectedFundName',
      width: 300,
      render: (text: string) => text || 'Not detected',
    },
    {
      title: 'Status',
      key: 'status',
      width: 120,
      render: (record: MutualFundDetectionResult) => {
        if (!record.mutualFundId) {
          return <Tag color="red">Failed</Tag>;
        }
        return record.wasCreated ? 
          <Tag color="green">Created</Tag> : 
          <Tag color="blue">Existing</Tag>;
      },
    },
    {
      title: 'Match Type',
      key: 'matchType',
      width: 100,
      render: (record: MutualFundDetectionResult) => {
        if (!record.mutualFundId) {
          return <Tag color="red">No Match</Tag>;
        }
        return <Tag color="green">Exact Match</Tag>;
      },
    },
    {
      title: 'Matched Text',
      dataIndex: 'matchedText',
      key: 'matchedText',
      width: 250,
      render: (text: string) => (
        <Text ellipsis={{ tooltip: text }} style={{ maxWidth: 200 }}>
          {text || 'N/A'}
        </Text>
      ),
    },
    {
      title: 'Location',
      key: 'location',
      width: 100,
      render: (record: MutualFundDetectionResult) => 
        record.rowFound >= 0 ? `R${record.rowFound + 1}C${record.columnFound + 1}` : 'N/A',
    },
  ];

  const uploadProps = {
    multiple: true,
    accept: '.xlsx,.xls',
    beforeUpload: () => false, // Prevent auto upload
    onChange: handleFileChange,
    onRemove: (file: any) => {
      const index = files.indexOf(file.originFileObj);
      if (index > -1) {
        const newFiles = [...files];
        newFiles.splice(index, 1);
        setFiles(newFiles);
      }
    },
  };

  return (
    <div style={{ padding: '24px' }}>
      <Card>
        <Title level={2}>
          <UploadOutlined /> Upload V2 - Auto Mutual Fund Detection
        </Title>
        <Text type="secondary">UploadFilesTriggerPage
          Upload Excel files and the system will automatically detect mutual funds using exact name matching, 
          create missing funds, and process the files.
        </Text>

        <div style={{ marginTop: '24px' }}>
          <Space direction="vertical" size="large" style={{ width: '100%' }}>
            {/* Date Selection */}
            <div>
              <Title level={4}>Select Report Date</Title>
              <DatePicker
                value={reportDate}
                onChange={setReportDate}
                format="YYYY-MM-DD"
                placeholder="Select report date"
                style={{ width: 200 }}
                disabled={uploading}
              />
            </div>

            {/* Date Validation Toggle */}
            <div>
              <Title level={4}>Date Validation</Title>
              <Space>
                <Switch
                  checked={performDateCheck}
                  onChange={setPerformDateCheck}
                  disabled={uploading}
                  checkedChildren="Enabled"
                  unCheckedChildren="Disabled"
                />
                <Text>
                  {performDateCheck 
                    ? 'Date validation enabled - filename dates must match report date'
                    : 'Date validation disabled - files will be processed regardless of filename dates'}
                </Text>
              </Space>
            </div>

            {/* File Upload */}
            <div>
              <Title level={4}>Upload Files</Title>
              <Dragger {...uploadProps} disabled={uploading}>
                <p className="ant-upload-drag-icon">
                  <InboxOutlined />
                </p>
                <p className="ant-upload-text">
                  Click or drag Excel files to this area to upload
                </p>
                <p className="ant-upload-hint">
                  Supports .xlsx and .xls files. Multiple files can be uploaded at once.
                  The system will scan the first 50 rows for exact mutual fund name matches.
                </p>
              </Dragger>
            </div>

            {/* Upload Actions */}
            <div>
              <Space>
                <Button
                  type="primary"
                  onClick={handleUpload}
                  loading={uploading}
                  disabled={!reportDate || files.length === 0}
                  size="large"
                >
                  {uploading ? 'Processing...' : 'Upload & Process'}
                </Button>
                <Button onClick={handleReset} disabled={uploading}>
                  Reset
                </Button>
              </Space>
            </div>

            {/* Upload Summary */}
            {files.length > 0 && (
              <Card size="small" style={{ backgroundColor: '#f6ffed' }}>
                <Text>
                  <strong>Ready to upload:</strong> {files.length} file(s) for {reportDate?.format('YYYY-MM-DD')}
                </Text>
              </Card>
            )}
          </Space>
        </div>
      </Card>

      {/* Results */}
      {uploadResponse && (
        <Card style={{ marginTop: '24px' }}>
          <Title level={3}>
            {uploadResponse.success ? (
              <><CheckCircleOutlined style={{ color: '#52c41a' }} /> Upload Results</>
            ) : (
              <><ExclamationCircleOutlined style={{ color: '#ff4d4f' }} /> Upload Results</>
            )}
          </Title>

          {/* Summary Stats */}
          <div style={{ marginBottom: '16px' }}>
            <Space size="large">
              <Text><strong>Files Processed:</strong> {uploadResponse.processedFiles}</Text>
              <Text><strong>Mutual Funds Created:</strong> {uploadResponse.createdMutualFunds}</Text>
              <Text><strong>Total Detections:</strong> {uploadResponse.detectionResults.length}</Text>
            </Space>
          </div>

          {/* Detection Results Table */}
          <Table
            columns={detectionColumns}
            dataSource={uploadResponse.detectionResults}
            rowKey="fileName"
            pagination={false}
            size="small"
            scroll={{ x: 1200 }}
          />
        </Card>
      )}
    </div>
  );
};

export default UploadV2;
