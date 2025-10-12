import React, { useState } from 'react';
import { Upload, Button, DatePicker, Card, Table, Tag, message, Space, Typography, Switch, Statistic, Row, Col } from 'antd';
import { InboxOutlined, UploadOutlined, CheckCircleOutlined, ExclamationCircleOutlined, FileOutlined, FundOutlined, LineChartOutlined } from '@ant-design/icons';
import { Dayjs } from 'dayjs';
import { UploadV2RequestDto, UploadV2ResponseDto, ProcessedFileInfo, SheetProcessingResult } from '../../types';
import { uploadV2Service } from '../../services/api';
import type { UploadFile } from 'antd/es/upload/interface';

const { Dragger } = Upload;
const { Title, Text } = Typography;

interface UploadV2Props {
  onUploadComplete?: (response: UploadV2ResponseDto) => void;
}

const UploadV2: React.FC<UploadV2Props> = ({ onUploadComplete }) => {
  const [files, setFiles] = useState<File[]>([]);
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [reportDate, setReportDate] = useState<Dayjs | null>(null);
  const [performDateCheck, setPerformDateCheck] = useState<boolean>(true);
  const [uploading, setUploading] = useState(false);
  const [uploadResponse, setUploadResponse] = useState<UploadV2ResponseDto | null>(null);

  const handleFileChange = (info: any) => {
    // Control AntD Upload file list and maintain raw File[] for submission
    setFileList(info.fileList);
    const newFiles = info.fileList
      .map((f: any) => f.originFileObj as File)
      .filter(Boolean);
    setFiles(newFiles);
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
    setFileList([]);
    setReportDate(null);
    setPerformDateCheck(true);
    setUploadResponse(null);
  };

  const processedFilesColumns = [
    {
      title: 'File Name',
      dataIndex: 'fileName',
      key: 'fileName',
      width: 250,
    },
    {
      title: 'Mutual Fund',
      dataIndex: 'mutualFundName',
      key: 'mutualFundName',
      width: 300,
    },
    {
      title: 'Status',
      key: 'status',
      width: 120,
      render: (record: ProcessedFileInfo) => {
        if (record.status === 2) {
          return <Tag color="green">Success</Tag>;
        } else if (record.status === 1) {
          return <Tag color="orange">Partial</Tag>;
        }
        return <Tag color="red">Failed</Tag>;
      },
    },
    {
      title: 'Fund Status',
      key: 'fundStatus',
      width: 120,
      render: (record: ProcessedFileInfo) => {
        return record.wasNewFundCreated ? 
          <Tag color="green">New Fund</Tag> : 
          <Tag color="blue">Existing</Tag>;
      },
    },
    {
      title: 'Holdings',
      dataIndex: 'holdingsCount',
      key: 'holdingsCount',
      width: 100,
      align: 'center' as const,
    },
    {
      title: 'Changes',
      dataIndex: 'holdingChangesCount',
      key: 'holdingChangesCount',
      width: 100,
      align: 'center' as const,
    },
    {
      title: 'Portfolio ID',
      dataIndex: 'portfolioFileId',
      key: 'portfolioFileId',
      width: 120,
      align: 'center' as const,
    },
    {
      title: 'Snapshot ID',
      dataIndex: 'snapshotId',
      key: 'snapshotId',
      width: 120,
      align: 'center' as const,
    },
  ];

  const sheetResultsColumns = [
    {
      title: 'File Name',
      dataIndex: 'fileName',
      key: 'fileName',
      width: 200,
    },
    {
      title: 'Sheet',
      key: 'sheet',
      width: 150,
      render: (record: SheetProcessingResult) => (
        <div>
          <div><strong>{record.sheetName}</strong></div>
          <Text type="secondary">Index: {record.sheetIndex}</Text>
        </div>
      ),
    },
    {
      title: 'Status',
      key: 'status',
      width: 100,
      render: (record: SheetProcessingResult) => {
        if (record.status === 2) {
          return <Tag color="green">Success</Tag>;
        } else if (record.status === 1) {
          return <Tag color="orange">Partial</Tag>;
        }
        return <Tag color="red">Failed</Tag>;
      },
    },
    {
      title: 'Mutual Fund Detection',
      key: 'detection',
      width: 300,
      render: (record: SheetProcessingResult) => {
        if (!record.detectionInfo?.wasDetected) {
          return <Tag color="red">Not Detected</Tag>;
        }
        return (
          <div>
            <div><strong>{record.detectionInfo.detectedFundName}</strong></div>
            <div>
              <Text type="secondary">
                Method: {record.detectionInfo.detectionMethod} | 
                Confidence: {(record.detectionInfo.matchConfidence * 100).toFixed(1)}%
              </Text>
            </div>
            {record.detectionInfo.rowFound !== undefined && (
              <div>
                <Text type="secondary">
                  Found at: Row {record.detectionInfo.rowFound + 1}, Col {record.detectionInfo.columnFound! + 1}
                </Text>
              </div>
            )}
          </div>
        );
      },
    },
    {
      title: 'Fund Status',
      key: 'fundStatus',
      width: 100,
      render: (record: SheetProcessingResult) => {
        if (!record.detectionInfo?.wasDetected) return null;
        return record.detectionInfo.wasNewFundCreated ? 
          <Tag color="green">New</Tag> : 
          <Tag color="blue">Existing</Tag>;
      },
    },
    {
      title: 'Holdings',
      dataIndex: 'holdingsExtracted',
      key: 'holdingsExtracted',
      width: 80,
      align: 'center' as const,
    },
    {
      title: 'Changes',
      dataIndex: 'holdingChangesTracked',
      key: 'holdingChangesTracked',
      width: 80,
      align: 'center' as const,
    },
    {
      title: 'Processing Time',
      dataIndex: 'processingDuration',
      key: 'processingDuration',
      width: 120,
      align: 'center' as const,
    },
    {
      title: 'Error',
      dataIndex: 'errorMessage',
      key: 'errorMessage',
      width: 200,
      render: (text: string) => text ? (
        <Text type="danger" ellipsis={{ tooltip: text }}>
          {text}
        </Text>
      ) : null,
    },
  ];

  const uploadProps = {
    multiple: true,
    accept: '.xlsx,.xls',
    beforeUpload: () => false, // Prevent auto upload
    onChange: handleFileChange,
    fileList: fileList,
    onRemove: (file: any) => {
      // Keep both UploadFile[] and File[] in sync when removing
      const newUploadList = fileList.filter((f: any) => f.uid !== file.uid);
      setFileList(newUploadList);
      const newFiles = newUploadList
        .map((f: any) => f.originFileObj as File)
        .filter(Boolean);
      setFiles(newFiles);
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

          {/* Statistics */}
          {uploadResponse.statistics && (
            <Card style={{ marginBottom: '24px', backgroundColor: '#f6ffed' }}>
              <Row gutter={16}>
                <Col span={6}>
                  <Statistic
                    title="Files Submitted"
                    value={uploadResponse.statistics.totalFilesSubmitted}
                    prefix={<FileOutlined />}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="Successfully Processed"
                    value={uploadResponse.statistics.filesProcessedSuccessfully}
                    valueStyle={{ color: '#3f8600' }}
                    prefix={<CheckCircleOutlined />}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="Failed"
                    value={uploadResponse.statistics.filesFailed}
                    valueStyle={{ color: '#cf1322' }}
                    prefix={<ExclamationCircleOutlined />}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="New Funds Created"
                    value={uploadResponse.statistics.newMutualFundsCreated}
                    prefix={<FundOutlined />}
                  />
                </Col>
              </Row>
              <Row gutter={16} style={{ marginTop: '16px' }}>
                <Col span={6}>
                  <Statistic
                    title="Existing Funds Updated"
                    value={uploadResponse.statistics.existingMutualFundsUpdated}
                    prefix={<FundOutlined />}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="Total Holdings"
                    value={uploadResponse.statistics.totalHoldingsProcessed}
                    prefix={<LineChartOutlined />}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="Holding Changes"
                    value={uploadResponse.statistics.totalHoldingChangesTracked}
                    prefix={<LineChartOutlined />}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="Processing Time"
                    value={uploadResponse.statistics.processingDuration}
                    valueStyle={{ fontSize: '16px' }}
                  />
                </Col>
              </Row>
            </Card>
          )}

          {/* Errors */}
          {uploadResponse.errors && uploadResponse.errors.length > 0 && (
            <Card style={{ marginBottom: '24px', backgroundColor: '#fff2e8', borderColor: '#ffbb96' }}>
              <Title level={4}>
                <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} /> Errors
              </Title>
              {uploadResponse.errors.map((error, index) => (
                <div key={index} style={{ marginBottom: '8px' }}>
                  <Text strong>{error.fileName}</Text>
                  <Text type="danger"> - {error.errorMessage}</Text>
                  {error.detailedError && (
                    <div style={{ marginLeft: '16px', marginTop: '4px' }}>
                      <Text type="secondary">{error.detailedError}</Text>
                    </div>
                  )}
                </div>
              ))}
            </Card>
          )}

          {/* Sheet Processing Results Table */}
          {uploadResponse.sheetResults && uploadResponse.sheetResults.length > 0 && (
            <>
              <Title level={4}>Sheet Processing Details</Title>
              <Table
                columns={sheetResultsColumns}
                dataSource={uploadResponse.sheetResults}
                rowKey={(record) => `${record.fileName}-${record.sheetIndex}`}
                pagination={false}
                size="small"
                scroll={{ x: 1600 }}
                expandable={{
                  expandedRowRender: (record) => (
                    <div style={{ margin: 0 }}>
                      <Row gutter={16}>
                        <Col span={8}>
                          <Text strong>Processing Details:</Text>
                          <div>Start: {new Date(record.processingStartTime).toLocaleTimeString()}</div>
                          <div>End: {new Date(record.processingEndTime).toLocaleTimeString()}</div>
                          <div>Duration: {record.processingDuration}</div>
                        </Col>
                        {record.detectionInfo?.matchedText && (
                          <Col span={16}>
                            <Text strong>Matched Text:</Text>
                            <div style={{ backgroundColor: '#f6ffed', padding: '8px', borderRadius: '4px' }}>
                              "{record.detectionInfo.matchedText}"
                            </div>
                          </Col>
                        )}
                      </Row>
                    </div>
                  ),
                  rowExpandable: (record) => !!record.detectionInfo?.matchedText || !!record.processingStartTime,
                }}
              />
            </>
          )}

          {/* Processed Files Summary Table */}
          {uploadResponse.processedFiles && uploadResponse.processedFiles.length > 0 && (
            <>
              <Title level={4} style={{ marginTop: '24px' }}>File Processing Summary</Title>
              <Table
                columns={processedFilesColumns}
                dataSource={uploadResponse.processedFiles}
                rowKey="portfolioFileId"
                pagination={false}
                size="small"
                scroll={{ x: 1400 }}
              />
            </>
          )}
        </Card>
      )}
    </div>
  );
};

export default UploadV2;
