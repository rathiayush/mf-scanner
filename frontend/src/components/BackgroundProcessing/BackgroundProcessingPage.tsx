import React, { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Button,
  Form,
  Input,
  DatePicker,
  Switch,
  Select,
  Table,
  Tag,
  Progress,
  Modal,
  message,
  Statistic,
  Divider,
  Space,
  Tooltip,
  Alert,
  Typography
} from 'antd';
import {
  PlayCircleOutlined,
  PauseCircleOutlined,
  DownloadOutlined,
  ReloadOutlined,
  FolderOpenOutlined,
  FileExcelOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  StopOutlined
} from '@ant-design/icons';
import dayjs from 'dayjs';
import backgroundProcessingService from '../../services/backgroundProcessingService';
import {
  BackgroundProcessingRequest,
  BackgroundProcessingStatus,
  ProcessingSummary,
  DirectoryValidation,
  BackgroundJobStatus
} from '../../types/backgroundProcessing';

const { Title, Text } = Typography;
const { Option } = Select;

const BackgroundProcessingPage: React.FC = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [jobs, setJobs] = useState<BackgroundProcessingStatus[]>([]);
  const [summary, setSummary] = useState<ProcessingSummary | null>(null);
  const [selectedJob, setSelectedJob] = useState<BackgroundProcessingStatus | null>(null);
  const [directoryValidation, setDirectoryValidation] = useState<DirectoryValidation | null>(null);
  const [validatingDirectory, setValidatingDirectory] = useState(false);
  const [refreshInterval, setRefreshInterval] = useState<NodeJS.Timeout | null>(null);

  // Load initial data
  useEffect(() => {
    loadJobs();
    loadSummary();
    
    // Set up auto-refresh for running jobs
    const interval = setInterval(() => {
      loadJobs();
      loadSummary();
    }, 5000); // Refresh every 5 seconds
    
    setRefreshInterval(interval);
    
    return () => {
      if (interval) clearInterval(interval);
    };
  }, []);

  const loadJobs = async () => {
    try {
      const jobsData = await backgroundProcessingService.getAllJobs();
      setJobs(jobsData);
    } catch (error) {
      console.error('Error loading jobs:', error);
    }
  };

  const loadSummary = async () => {
    try {
      const summaryData = await backgroundProcessingService.getProcessingSummary();
      setSummary(summaryData);
    } catch (error) {
      console.error('Error loading summary:', error);
    }
  };

  const validateDirectory = async (directoryPath: string) => {
    if (!directoryPath) return;
    
    setValidatingDirectory(true);
    try {
      const validation = await backgroundProcessingService.validateDirectory(directoryPath);
      setDirectoryValidation(validation);
      
      if (validation.isValid) {
        message.success(`Found ${validation.excelFileCount} Excel files in directory`);
      } else {
        message.warning('Directory validation failed');
      }
    } catch (error: any) {
      message.error(`Directory validation failed: ${error.response?.data?.message || error.message}`);
      setDirectoryValidation(null);
    } finally {
      setValidatingDirectory(false);
    }
  };

  const handleStartProcessing = async (values: any) => {
    setLoading(true);
    try {
      const request: BackgroundProcessingRequest = {
        directoryPath: values.directoryPath,
        reportDate: values.reportDate.format('YYYY-MM-DD'),
        performDateCheck: values.performDateCheck || false,
        filePatterns: values.filePatterns || ['*.xlsx', '*.xls'],
        generateExcelReport: values.generateExcelReport !== false
      };

      const response = await backgroundProcessingService.startProcessing(request);
      message.success(`Processing started successfully! Job ID: ${response.jobId}`);
      
      // Refresh jobs list
      await loadJobs();
      await loadSummary();
      
      // Reset form
      form.resetFields();
      setDirectoryValidation(null);
    } catch (error: any) {
      message.error(`Failed to start processing: ${error.response?.data?.message || error.message}`);
    } finally {
      setLoading(false);
    }
  };

  const handleCancelJob = async (jobId: string) => {
    try {
      await backgroundProcessingService.cancelJob(jobId);
      message.success('Job cancelled successfully');
      await loadJobs();
      await loadSummary();
    } catch (error: any) {
      message.error(`Failed to cancel job: ${error.response?.data?.message || error.message}`);
    }
  };

  const handleDownloadReport = async (jobId: string) => {
    try {
      await backgroundProcessingService.downloadAndSaveReport(jobId);
      message.success('Report downloaded successfully');
    } catch (error: any) {
      message.error(`Failed to download report: ${error.response?.data?.message || error.message}`);
    }
  };

  const getStatusIcon = (status: BackgroundJobStatus) => {
    switch (status) {
      case BackgroundJobStatus.Queued:
        return <ClockCircleOutlined style={{ color: 'orange' }} />;
      case BackgroundJobStatus.Running:
        return <PlayCircleOutlined style={{ color: 'blue' }} />;
      case BackgroundJobStatus.Completed:
        return <CheckCircleOutlined style={{ color: 'green' }} />;
      case BackgroundJobStatus.Failed:
        return <ExclamationCircleOutlined style={{ color: 'red' }} />;
      case BackgroundJobStatus.Cancelled:
        return <StopOutlined style={{ color: 'gray' }} />;
      default:
        return <ClockCircleOutlined />;
    }
  };

  const jobColumns = [
    {
      title: 'Job ID',
      dataIndex: 'jobId',
      key: 'jobId',
      width: 120,
      render: (jobId: string) => (
        <Tooltip title={jobId}>
          <Text code>{jobId.substring(0, 8)}...</Text>
        </Tooltip>
      )
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      render: (status: BackgroundJobStatus) => (
        <Tag color={backgroundProcessingService.getStatusColor(status)} icon={getStatusIcon(status)}>
          {backgroundProcessingService.getStatusText(status)}
        </Tag>
      )
    },
    {
      title: 'Progress',
      key: 'progress',
      width: 200,
      render: (record: BackgroundProcessingStatus) => {
        if (record.status === BackgroundJobStatus.Running && record.totalFiles > 0) {
          const percent = Math.round((record.processedFiles / record.totalFiles) * 100);
          return (
            <div>
              <Progress percent={percent} size="small" />
              <Text type="secondary" style={{ fontSize: '12px' }}>
                {record.processedFiles}/{record.totalFiles} files
              </Text>
            </div>
          );
        }
        return record.totalFiles > 0 ? `${record.successfulFiles}/${record.totalFiles} successful` : 'N/A';
      }
    },
    {
      title: 'Current File',
      dataIndex: 'currentFile',
      key: 'currentFile',
      width: 200,
      render: (currentFile: string) => currentFile ? (
        <Tooltip title={currentFile}>
          <Text ellipsis style={{ maxWidth: 180 }}>
            <FileExcelOutlined /> {currentFile}
          </Text>
        </Tooltip>
      ) : 'N/A'
    },
    {
      title: 'Start Time',
      dataIndex: 'startTime',
      key: 'startTime',
      width: 150,
      render: (startTime: string) => dayjs(startTime).format('MMM DD, HH:mm')
    },
    {
      title: 'Duration',
      dataIndex: 'duration',
      key: 'duration',
      width: 100,
      render: (duration: string) => backgroundProcessingService.formatDuration(duration)
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 150,
      render: (record: BackgroundProcessingStatus) => (
        <Space>
          {record.status === BackgroundJobStatus.Running && (
            <Button
              size="small"
              icon={<PauseCircleOutlined />}
              onClick={() => handleCancelJob(record.jobId)}
              danger
            >
              Cancel
            </Button>
          )}
          {record.status === BackgroundJobStatus.Completed && record.excelReportPath && (
            <Button
              size="small"
              icon={<DownloadOutlined />}
              onClick={() => handleDownloadReport(record.jobId)}
              type="primary"
            >
              Report
            </Button>
          )}
          <Button
            size="small"
            onClick={() => setSelectedJob(record)}
          >
            Details
          </Button>
        </Space>
      )
    }
  ];

  return (
    <div style={{ padding: '24px' }}>
      <Title level={2}>
        <PlayCircleOutlined /> Background File Processing
      </Title>
      
      {/* Summary Statistics */}
      {summary && (
        <Row gutter={16} style={{ marginBottom: '24px' }}>
          <Col span={4}>
            <Card>
              <Statistic
                title="Total Jobs"
                value={summary.totalJobs}
                prefix={<FileExcelOutlined />}
              />
            </Card>
          </Col>
          <Col span={4}>
            <Card>
              <Statistic
                title="Running"
                value={summary.runningJobs}
                valueStyle={{ color: '#1890ff' }}
                prefix={<PlayCircleOutlined />}
              />
            </Card>
          </Col>
          <Col span={4}>
            <Card>
              <Statistic
                title="Completed"
                value={summary.completedJobs}
                valueStyle={{ color: '#52c41a' }}
                prefix={<CheckCircleOutlined />}
              />
            </Card>
          </Col>
          <Col span={4}>
            <Card>
              <Statistic
                title="Failed"
                value={summary.failedJobs}
                valueStyle={{ color: '#ff4d4f' }}
                prefix={<ExclamationCircleOutlined />}
              />
            </Card>
          </Col>
          <Col span={4}>
            <Card>
              <Statistic
                title="Queued"
                value={summary.queuedJobs}
                valueStyle={{ color: '#fa8c16' }}
                prefix={<ClockCircleOutlined />}
              />
            </Card>
          </Col>
          <Col span={4}>
            <Card style={{ textAlign: 'center' }}>
              <Button
                type="primary"
                icon={<ReloadOutlined />}
                onClick={() => { loadJobs(); loadSummary(); }}
                style={{ marginTop: '8px' }}
              >
                Refresh
              </Button>
            </Card>
          </Col>
        </Row>
      )}

      <Row gutter={24}>
        {/* Start New Job Form */}
        <Col span={8}>
          <Card title="Start New Processing Job" style={{ marginBottom: '24px' }}>
            <Form
              form={form}
              layout="vertical"
              onFinish={handleStartProcessing}
              initialValues={{
                performDateCheck: false,
                generateExcelReport: true,
                filePatterns: ['*.xlsx', '*.xls']
              }}
            >
              <Form.Item
                label="Directory Path"
                name="directoryPath"
                rules={[{ required: true, message: 'Please enter directory path' }]}
              >
                <Input
                  placeholder="C:\MutualFundFiles"
                  prefix={<FolderOpenOutlined />}
                  onBlur={(e) => validateDirectory(e.target.value)}
                />
              </Form.Item>

              {validatingDirectory && (
                <Alert message="Validating directory..." type="info" style={{ marginBottom: '16px' }} />
              )}

              {directoryValidation && (
                <Alert
                  message={`Found ${directoryValidation.excelFileCount} Excel files`}
                  description={`Directory: ${directoryValidation.directoryPath}`}
                  type="success"
                  style={{ marginBottom: '16px' }}
                />
              )}

              <Form.Item
                label="Report Date"
                name="reportDate"
                rules={[{ required: true, message: 'Please select report date' }]}
              >
                <DatePicker style={{ width: '100%' }} />
              </Form.Item>

              <Form.Item
                label="File Patterns"
                name="filePatterns"
              >
                <Select
                  mode="tags"
                  placeholder="Enter file patterns"
                  defaultValue={['*.xlsx', '*.xls']}
                >
                  <Option value="*.xlsx">*.xlsx</Option>
                  <Option value="*.xls">*.xls</Option>
                </Select>
              </Form.Item>

              <Form.Item
                label="Options"
                style={{ marginBottom: '8px' }}
              >
                <Form.Item
                  name="performDateCheck"
                  valuePropName="checked"
                  style={{ display: 'inline-block', marginBottom: '8px' }}
                >
                  <Switch /> Perform Date Check
                </Form.Item>
                <br />
                <Form.Item
                  name="generateExcelReport"
                  valuePropName="checked"
                  style={{ display: 'inline-block', marginBottom: '0' }}
                >
                  <Switch defaultChecked /> Generate Excel Report
                </Form.Item>
              </Form.Item>

              <Form.Item>
                <Button
                  type="primary"
                  htmlType="submit"
                  loading={loading}
                  icon={<PlayCircleOutlined />}
                  block
                  size="large"
                >
                  Start Processing
                </Button>
              </Form.Item>
            </Form>
          </Card>
        </Col>

        {/* Jobs List */}
        <Col span={16}>
          <Card title="Processing Jobs" style={{ marginBottom: '24px' }}>
            <Table
              columns={jobColumns}
              dataSource={jobs}
              rowKey="jobId"
              pagination={{ pageSize: 10 }}
              scroll={{ x: 1200 }}
              size="small"
            />
          </Card>
        </Col>
      </Row>

      {/* Job Details Modal */}
      <Modal
        title={`Job Details - ${selectedJob?.jobId.substring(0, 8)}...`}
        open={!!selectedJob}
        onCancel={() => setSelectedJob(null)}
        footer={[
          <Button key="close" onClick={() => setSelectedJob(null)}>
            Close
          </Button>,
          selectedJob?.status === BackgroundJobStatus.Completed && selectedJob.excelReportPath && (
            <Button
              key="download"
              type="primary"
              icon={<DownloadOutlined />}
              onClick={() => selectedJob && handleDownloadReport(selectedJob.jobId)}
            >
              Download Report
            </Button>
          )
        ]}
        width={800}
      >
        {selectedJob && (
          <div>
            <Row gutter={16}>
              <Col span={12}>
                <Statistic
                  title="Status"
                  value={backgroundProcessingService.getStatusText(selectedJob.status)}
                  prefix={getStatusIcon(selectedJob.status)}
                />
              </Col>
              <Col span={12}>
                <Statistic
                  title="Total Files"
                  value={selectedJob.totalFiles}
                />
              </Col>
            </Row>
            
            <Divider />
            
            <Row gutter={16}>
              <Col span={8}>
                <Statistic
                  title="Successful"
                  value={selectedJob.successfulFiles}
                  valueStyle={{ color: '#52c41a' }}
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Failed"
                  value={selectedJob.failedFiles}
                  valueStyle={{ color: '#ff4d4f' }}
                />
              </Col>
              <Col span={8}>
                <Statistic
                  title="Processed"
                  value={selectedJob.processedFiles}
                />
              </Col>
            </Row>

            {selectedJob.status === BackgroundJobStatus.Running && selectedJob.totalFiles > 0 && (
              <>
                <Divider />
                <Progress
                  percent={Math.round((selectedJob.processedFiles / selectedJob.totalFiles) * 100)}
                  status="active"
                />
                {selectedJob.currentFile && (
                  <Text type="secondary">
                    Currently processing: {selectedJob.currentFile}
                  </Text>
                )}
              </>
            )}

            <Divider />
            
            <div>
              <Text strong>Start Time:</Text> {dayjs(selectedJob.startTime).format('YYYY-MM-DD HH:mm:ss')}
              <br />
              {selectedJob.endTime && (
                <>
                  <Text strong>End Time:</Text> {dayjs(selectedJob.endTime).format('YYYY-MM-DD HH:mm:ss')}
                  <br />
                </>
              )}
              {selectedJob.duration && (
                <>
                  <Text strong>Duration:</Text> {backgroundProcessingService.formatDuration(selectedJob.duration)}
                  <br />
                </>
              )}
              <Text strong>Message:</Text> {selectedJob.message}
            </div>

            {selectedJob.result && (
              <>
                <Divider />
                <Title level={4}>Processing Results</Title>
                <Row gutter={16}>
                  <Col span={8}>
                    <Statistic
                      title="New Funds Created"
                      value={selectedJob.result.newMutualFundsCreated}
                    />
                  </Col>
                  <Col span={8}>
                    <Statistic
                      title="Holdings Processed"
                      value={selectedJob.result.totalHoldingsProcessed}
                    />
                  </Col>
                  <Col span={8}>
                    <Statistic
                      title="Changes Tracked"
                      value={selectedJob.result.totalHoldingChangesTracked}
                    />
                  </Col>
                </Row>
              </>
            )}

            {selectedJob.errors && selectedJob.errors.length > 0 && (
              <>
                <Divider />
                <Alert
                  message="Errors"
                  description={
                    <ul>
                      {selectedJob.errors.map((error, index) => (
                        <li key={index}>{error}</li>
                      ))}
                    </ul>
                  }
                  type="error"
                />
              </>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
};

export default BackgroundProcessingPage;
