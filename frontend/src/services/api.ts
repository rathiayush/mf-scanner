import axios from 'axios';
import { 
  ApiResponse, 
  FileUploadResponse, 
  ProcessedFileData,
  MutualFundDto,
  CreateMutualFundDto,
  UpdateMutualFundDto,
  MutualFundSearchDto,
  PagedResult,
  FileUploadResponseDto,
  ProcessedFileDto,
  AnalyticsRequestDto,
  AnalyticsResponseDto,
  TopShareDto,
  AvailableDatesResponseDto,
  AnalyticsSummaryDto,
  HoldingChangeDto,
  CurrentHoldingsRequestDto,
  CurrentHoldingsResponseDto,
  UploadV2RequestDto,
  UploadV2ResponseDto
} from '../types';

const API_BASE_URL = 'https://localhost:7052/api'; // Backend API URL

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Legacy file service (keeping for compatibility)
export const fileService = {
  uploadFile: async (file: File): Promise<ApiResponse<FileUploadResponse>> => {
    const formData = new FormData();
    formData.append('file', file);

    try {
      const response = await api.post('/files/upload', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Failed to upload file');
    }
  },

  getProcessingStatus: async (fileId: string): Promise<ApiResponse<ProcessedFileData>> => {
    try {
      const response = await api.get(`/files/${fileId}/status`);
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Failed to get processing status');
    }
  },

  getProcessedData: async (fileId: string): Promise<ApiResponse<ProcessedFileData>> => {
    try {
      const response = await api.get(`/files/${fileId}/data`);
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Failed to get processed data');
    }
  },

  getAllFiles: async (): Promise<ApiResponse<FileUploadResponse[]>> => {
    try {
      const response = await api.get('/files');
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Failed to get files');
    }
  },
};

// New API services matching backend endpoints
export const mutualFundService = {
  // CRUD operations
  createMutualFund: async (data: CreateMutualFundDto): Promise<MutualFundDto> => {
    const response = await api.post('/MutualFund', data);
    return response.data;
  },

  updateMutualFund: async (id: number, data: UpdateMutualFundDto): Promise<MutualFundDto> => {
    const response = await api.put(`/MutualFund/${id}`, data);
    return response.data;
  },

  deleteMutualFund: async (id: number): Promise<void> => {
    await api.delete(`/MutualFund/${id}`);
  },

  getMutualFundById: async (id: number): Promise<MutualFundDto> => {
    const response = await api.get(`/MutualFund/${id}`);
    return response.data;
  },

  // Search and dropdown
  searchMutualFunds: async (searchDto: MutualFundSearchDto): Promise<PagedResult<MutualFundDto>> => {
    const response = await api.post('/MutualFund/search', searchDto);
    return response.data;
  },

  getMutualFundsForDropdown: async (searchTerm?: string): Promise<MutualFundDto[]> => {
    const params = searchTerm ? `?searchTerm=${encodeURIComponent(searchTerm)}` : '';
    const response = await api.get(`/MutualFund/dropdown${params}`);
    return response.data;
  },

  // Legacy compatibility
  getAllMutualFunds: async (): Promise<MutualFundDto[]> => {
    const response = await api.get('/MutualFund');
    return response.data;
  },
};

export const fileUploadService = {
  uploadFiles: async (mutualFundId: number, files: File[]): Promise<FileUploadResponseDto> => {
    const formData = new FormData();
    formData.append('mutualFundId', mutualFundId.toString());
    files.forEach(file => {
      formData.append('files', file);
    });

    const response = await api.post('/FileUpload/single', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  uploadFilesWithDates: async (fileUploads: Array<{mutualFundId: number, file: File, reportDate?: string}>): Promise<FileUploadResponseDto> => {
    const formData = new FormData();
    
    fileUploads.forEach((upload, index) => {
      formData.append(`fileUploads[${index}].mutualFundId`, upload.mutualFundId.toString());
      formData.append(`fileUploads[${index}].file`, upload.file);
      if (upload.reportDate) {
        formData.append(`fileUploads[${index}].reportDate`, upload.reportDate);
      }
    });

    const response = await api.post('/FileUpload/multiple', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  uploadMultipleFiles: async (fileUploads: Array<{mutualFundId: number, file: File}>): Promise<FileUploadResponseDto> => {
    const formData = new FormData();
    
    fileUploads.forEach((upload, index) => {
      formData.append(`fileUploads[${index}].mutualFundId`, upload.mutualFundId.toString());
      formData.append(`fileUploads[${index}].file`, upload.file);
    });

    const response = await api.post('/FileUpload/multiple', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  getProcessedFiles: async (mutualFundId: number): Promise<ProcessedFileDto[]> => {
    const response = await api.get(`/FileUpload/processed/${mutualFundId}`);
    return response.data;
  },

  validateFile: async (file: File): Promise<{isValid: boolean, fileName: string}> => {
    const formData = new FormData();
    formData.append('file', file);

    const response = await api.post('/FileUpload/validate', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
};

export const analyticsService = {
  getAnalytics: async (request: AnalyticsRequestDto): Promise<AnalyticsResponseDto> => {
    const response = await api.post('/analytics/analyze', request);
    return response.data;
  },

  getHoldingChanges: async (request: AnalyticsRequestDto): Promise<PagedResult<HoldingChangeDto>> => {
    const response = await api.post('/analytics/holding-changes', request);
    return response.data;
  },

  getTopShares: async (
    mutualFundIds: string, 
    fromDate: string, 
    toDate: string, 
    topCount: number = 50
  ): Promise<TopShareDto[]> => {
    const response = await api.get('/analytics/top-shares', {
      params: { mutualFundIds, fromDate, toDate, topCount }
    });
    return response.data;
  },

  getAvailableDates: async (mutualFundIds?: number[]): Promise<AvailableDatesResponseDto> => {
    const params = mutualFundIds ? { mutualFundIds } : {};
    const response = await api.get('/analytics/available-dates', { params });
    return response.data;
  },

  getAnalyticsSummary: async (
    mutualFundIds: number[], 
    fromDate: string, 
    toDate: string
  ): Promise<AnalyticsSummaryDto> => {
    const response = await api.get('/analytics/summary', {
      params: { mutualFundIds, fromDate, toDate }
    });
    return response.data;
  },

  getCurrentHoldings: async (request: CurrentHoldingsRequestDto): Promise<CurrentHoldingsResponseDto> => {
    const response = await api.post('/analytics/current-holdings', request);
    return response.data;
  },

    validateDatesForFunds: async (
    mutualFundIds: number[], 
    fromDate: string, 
    toDate: string
  ): Promise<{ isValid: boolean }> => {
    console.log('API validateDatesForFunds called with:', { mutualFundIds, fromDate, toDate });
    
    const params = { 
      mutualFundIds: mutualFundIds.join(','), // Convert array to comma-separated string
      fromDate, 
      toDate 
    };
    
    console.log('API request params:', params);
    
    const response = await api.get('/analytics/validate-dates', {
      params
    });
    
    console.log('API validateDatesForFunds response:', response.data);
    return response.data;
  },
};

export const uploadV2Service = {
  uploadFiles: async (request: UploadV2RequestDto): Promise<UploadV2ResponseDto> => {
    const formData = new FormData();
    formData.append('reportDate', request.reportDate);
    
    request.files.forEach((file) => {
      formData.append('files', file);
    });

    const response = await api.post('/uploadv2/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  getDetectionPatterns: async () => {
    const response = await api.get('/uploadv2/detection-patterns');
    return response.data;
  },

  validateDatesForFunds: async (
    mutualFundIds: string, 
    fromDate: string, 
    toDate: string
  ): Promise<{ isValid: boolean }> => {
    console.log('API validateDatesForFunds called with:', { mutualFundIds, fromDate, toDate });
    
    const params = { 
      mutualFundIds: mutualFundIds.join(','), // Convert array to comma-separated string
      fromDate, 
      toDate 
    };
    
    console.log('API request params:', params);
    
    const response = await api.get('/analytics/validate-dates', {
      params
    });
    
    console.log('API validateDatesForFunds response:', response.data);
    return response.data;
  },
};

// Legacy API service (keeping for compatibility)
export const apiService = {
  // Mutual Funds (legacy)
  getMutualFunds: async () => {
    return mutualFundService.getAllMutualFunds();
  },

  // Analysis endpoints (legacy)
  getStockChanges: async (fromDate: string, toDate: string, fundId?: string) => {
    const fundIds = fundId ? [parseInt(fundId)] : [];
    return analyticsService.getHoldingChanges({
      mutualFundIds: fundIds,
      fromDate,
      toDate,
      page: 1,
      pageSize: 100
    });
  },

  getTopStocks: async (fromDate: string, toDate: string, fundId?: string) => {
    const fundIds = fundId ? fundId : '';
    return analyticsService.getTopShares(fundIds, fromDate, toDate);
  },

  getStockHoldings: async (fromDate: string, toDate: string, fundId?: string) => {
    const fundIds = fundId ? [parseInt(fundId)] : [];
    return analyticsService.getHoldingChanges({
      mutualFundIds: fundIds,
      fromDate,
      toDate,
      page: 1,
      pageSize: 100
    });
  },

  // Batch processing endpoints (legacy)
  getBatchFiles: async () => {
    const response = await api.get('/portfolio/batch-files');
    return response.data;
  },

  uploadBatchFiles: async (formData: FormData) => {
    const response = await api.post('/portfolio/batch-upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  triggerBatchProcessing: async () => {
    const response = await api.post('/portfolio/batch-process');
    return response.data;
  },

  deleteBatchFiles: async (fileIds: string[]) => {
    const response = await api.delete('/portfolio/batch-files', {
      data: { fileIds },
    });
    return response.data;
  },

  // Portfolio summary (legacy)
  getPortfolioSummary: async (fundId: string) => {
    const response = await api.get(`/portfolio/${fundId}/summary`);
    return response.data;
  },
};

export default api;
