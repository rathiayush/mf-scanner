import { 
  SnapshotListResponseDto, 
  SnapshotHoldingsRequestDto, 
  SnapshotHoldingsResponseDto,
  DeleteSnapshotRequestDto,
  DeleteSnapshotResponseDto,
  BulkDeleteSnapshotRequestDto,
  BulkDeleteSnapshotResponseDto,
  AvailableSnapshotDatesRequestDto,
  AvailableSnapshotDatesResponseDto
} from '../types';
import env from '../config/env';

const API_BASE_URL = env.api.baseUrl;

class SnapshotManagementService {
  private baseUrl = `${API_BASE_URL}/SnapshotManagement`;

  async getSnapshots(mutualFundId: number): Promise<SnapshotListResponseDto> {
    try {
      const response = await fetch(`${this.baseUrl}/snapshots/${mutualFundId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Error fetching snapshots:', error);
      throw error;
    }
  }

  async getSnapshotHoldings(request: SnapshotHoldingsRequestDto): Promise<SnapshotHoldingsResponseDto> {
    try {
      const response = await fetch(`${this.baseUrl}/holdings`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Error fetching snapshot holdings:', error);
      throw error;
    }
  }

  async deleteSnapshot(request: DeleteSnapshotRequestDto): Promise<DeleteSnapshotResponseDto> {
    try {
      const response = await fetch(`${this.baseUrl}/snapshot`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Error deleting snapshot:', error);
      throw error;
    }
  }

  async bulkDeleteSnapshots(request: BulkDeleteSnapshotRequestDto): Promise<BulkDeleteSnapshotResponseDto> {
    try {
      const response = await fetch(`${this.baseUrl}/snapshots/bulk`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Error bulk deleting snapshots:', error);
      throw error;
    }
  }

  async getAvailableSnapshotDates(request: AvailableSnapshotDatesRequestDto): Promise<AvailableSnapshotDatesResponseDto> {
    try {
      const response = await fetch(`${this.baseUrl}/available-dates`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      return await response.json();
    } catch (error) {
      console.error('Error fetching available snapshot dates:', error);
      throw error;
    }
  }
}

export const snapshotManagementService = new SnapshotManagementService();
