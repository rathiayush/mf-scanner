import React, { useCallback, useState } from 'react';
import { Upload, Typography, Space } from 'antd';
import { InboxOutlined } from '@ant-design/icons';
import type { UploadProps, UploadFile } from 'antd';
import { UploadedFile } from '../../types';

const { Dragger } = Upload;
const { Text } = Typography;

interface FileUploadAreaProps {
  onFileUpload: (files: UploadedFile[]) => void;
  uploading: boolean;
}

const FileUploadArea: React.FC<FileUploadAreaProps> = ({ onFileUpload, uploading }) => {
  const [fileList, setFileList] = useState<UploadFile[]>([]);

  const handleUpload = useCallback((file: File): UploadedFile => {
    const uploadedFile: UploadedFile = {
      id: `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      name: file.name,
      size: file.size,
      type: file.type,
      status: 'pending',
      progress: 0,
      uploadedAt: new Date(),
    };
    return uploadedFile;
  }, []);

  const props: UploadProps = {
    name: 'file',
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
        console.error('You can only upload Excel or CSV files!');
        return false;
      }

      const isLt10M = file.size / 1024 / 1024 < 10;
      if (!isLt10M) {
        console.error('File must be smaller than 10MB!');
        return false;
      }

      const uploadedFile = handleUpload(file);
      onFileUpload([uploadedFile]);
      
      return false; // Prevent default upload behavior
    },
    onChange: (info) => {
      setFileList(info.fileList);
    },
    onDrop: (e) => {
      console.log('Dropped files', e.dataTransfer.files);
    },
  };

  return (
    <div>
      <Dragger {...props} disabled={uploading}>
        <p className="ant-upload-drag-icon">
          <InboxOutlined />
        </p>
        <p className="ant-upload-text">
          Click or drag mutual fund portfolio files to this area to upload
        </p>
        <p className="ant-upload-hint">
          Support for Excel (.xlsx, .xls) and CSV files. Maximum file size: 10MB
        </p>
      </Dragger>
      
      <div style={{ marginTop: 16, textAlign: 'center' }}>
        <Space direction="vertical" size="small">
          <Text type="secondary">
            Supported formats: Excel (.xlsx, .xls), CSV (.csv)
          </Text>
          <Text type="secondary">
            Files will be automatically processed after upload
          </Text>
        </Space>
      </div>
    </div>
  );
};

export default FileUploadArea;
