import axios from 'axios';
import {
  BackgroundProcessingRequest,
  BackgroundProcessingStatus,
  ProcessingSummary,
  DirectoryValidation,
  StartJobResponse
} from '../types/backgroundProcessing';

// Default to localhost if environment variable is not set
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7001';

class BackgroundProcessingService {
  private baseUrl = `${API_BASE_URL.replace(/\/$/, '')}/background-processing`;

  /**
   * Start background processing of files from a directory
   */
  async startProcessing(request: BackgroundProcessingRequest): Promise<StartJobResponse> {
    const response = await axios.post<StartJobResponse>(`${this.baseUrl}/start`, request);
    return response.data;
  }

  /**
   * Get the status of a specific background processing job
   */
  async getJobStatus(jobId: string): Promise<BackgroundProcessingStatus> {
    const response = await axios.get<BackgroundProcessingStatus>(`${this.baseUrl}/status/${jobId}`);
    return response.data;
  }

  /**
   * Get all background processing jobs
   */
  async getAllJobs(): Promise<BackgroundProcessingStatus[]> {
    const response = await axios.get<BackgroundProcessingStatus[]>(`${this.baseUrl}/jobs`);
    return response.data;
  }

  /**
   * Cancel a running background processing job
   */
  async cancelJob(jobId: string): Promise<{ message: string }> {
    const response = await axios.post<{ message: string }>(`${this.baseUrl}/cancel/${jobId}`);
    return response.data;
  }

  /**
   * Download the Excel report for a completed job
   */
  async downloadReport(jobId: string): Promise<Blob> {
    const response = await axios.get(`${this.baseUrl}/report/${jobId}`, {
      responseType: 'blob'
    });
    return response.data;
  }

  /**
   * Get processing statistics summary for dashboard
   */
  async getProcessingSummary(): Promise<ProcessingSummary> {
    const response = await axios.get<ProcessingSummary>(`${this.baseUrl}/summary`);
    return response.data;
  }

  /**
   * Validate a directory path for processing
   */
  async validateDirectory(directoryPath: string): Promise<DirectoryValidation> {
    const response = await axios.post<DirectoryValidation>(`${this.baseUrl}/validate-directory`, 
      JSON.stringify(directoryPath), 
      {
        headers: {
          'Content-Type': 'application/json'
        }
      }
    );
    return response.data;
  }

  /**
   * Helper method to download and save Excel report
   */
  async downloadAndSaveReport(jobId: string, fileName?: string): Promise<void> {
    try {
      const blob = await this.downloadReport(jobId);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = fileName || `ProcessingReport_${jobId}_${new Date().toISOString().slice(0, 10)}.xlsx`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Error downloading report:', error);
      throw error;
    }
  }

  /**
   * Helper method to format file size
   */
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  /**
   * Helper method to format duration
   */
  formatDuration(duration: string): string {
    if (!duration) return 'N/A';
    
    // Parse duration string (e.g., "00:05:30" or "PT5M30S")
    const parts = duration.split(':');
    if (parts.length === 3) {
      const hours = parseInt(parts[0]);
      const minutes = parseInt(parts[1]);
      const seconds = parseInt(parts[2]);
      
      if (hours > 0) {
        return `${hours}h ${minutes}m ${seconds}s`;
      } else if (minutes > 0) {
        return `${minutes}m ${seconds}s`;
      } else {
        return `${seconds}s`;
      }
    }
    
    return duration;
  }

  /**
   * Helper method to get status color
   */
  getStatusColor(status: number): string {
    switch (status) {
      case 0: return 'orange'; // Queued
      case 1: return 'blue'; // Running
      case 2: return 'green'; // Completed
      case 3: return 'red'; // Failed
      case 4: return 'gray'; // Cancelled
      default: return 'gray';
    }
  }

  /**
   * Helper method to get status text
   */
  getStatusText(status: number): string {
    switch (status) {
      case 0: return 'Queued';
      case 1: return 'Running';
      case 2: return 'Completed';
      case 3: return 'Failed';
      case 4: return 'Cancelled';
      default: return 'Unknown';
    }
  }
}

export const backgroundProcessingService = new BackgroundProcessingService();
export default backgroundProcessingService;
