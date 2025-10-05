import env from '../config/env';
import axios from 'axios';
const API_BASE_URL = env.api.baseUrl; // Using environment configuration
//const API_BASE_URL = 'http://192.168.29.212:5247/api';

export interface ConcreteMutualFundDto {
  id: number;
  fundName: string;
  amcName?: string;
  category?: string;
  subCategory?: string;
  isActive: boolean;
  createdDate: string;
  lastUpdated?: string;
  notes?: string;
  matchPriority: number;
}

export interface CreateConcreteMutualFundDto {
  fundName: string;
  amcName?: string;
  category?: string;
  subCategory?: string;
  isActive: boolean;
  notes?: string;
  matchPriority: number;
}

export interface UpdateConcreteMutualFundDto {
  fundName: string;
  amcName?: string;
  category?: string;
  subCategory?: string;
  isActive: boolean;
  notes?: string;
  matchPriority: number;
}

export interface ConcreteMutualFundListDto {
  id: number;
  fundName: string;
  amcName?: string;
  category?: string;
  isActive: boolean;
  matchPriority: number;
  createdDate: string;
}

export interface ConcreteMutualFundSearchDto {
  searchTerm?: string;
  amcName?: string;
  category?: string;
  isActive?: boolean;
  page: number;
  pageSize: number;
  sortBy: string;
  sortDescending: boolean;
}

export interface SearchResponse {
  items: ConcreteMutualFundListDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface BulkCreateResponse {
  message: string;
  createdCount: number;
  totalProvided: number;
}

class ConcreteMutualFundService {
  private baseUrl = `${API_BASE_URL}/ConcreteMutualFund`;

  async getAll(): Promise<ConcreteMutualFundDto[]> {
    const response = await axios.get<ConcreteMutualFundDto[]>(this.baseUrl);
    return response.data;
  }

  async search(searchDto: ConcreteMutualFundSearchDto): Promise<SearchResponse> {
    const response = await axios.post<SearchResponse>(`${this.baseUrl}/search`, searchDto);
    return response.data;
  }

  async getById(id: number): Promise<ConcreteMutualFundDto> {
    const response = await axios.get<ConcreteMutualFundDto>(`${this.baseUrl}/${id}`);
    return response.data;
  }

  async create(createDto: CreateConcreteMutualFundDto): Promise<ConcreteMutualFundDto> {
    const response = await axios.post<ConcreteMutualFundDto>(this.baseUrl, createDto);
    return response.data;
  }

  async update(id: number, updateDto: UpdateConcreteMutualFundDto): Promise<ConcreteMutualFundDto> {
    const response = await axios.put<ConcreteMutualFundDto>(`${this.baseUrl}/${id}`, updateDto);
    return response.data;
  }

  async delete(id: number): Promise<void> {
    await axios.delete(`${this.baseUrl}/${id}`);
  }

  async exists(id: number): Promise<boolean> {
    try {
      await axios.head(`${this.baseUrl}/${id}`);
      return true;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return false;
      }
      throw error;
    }
  }

  async getDistinctAMCNames(): Promise<string[]> {
    const response = await axios.get<string[]>(`${this.baseUrl}/amc-names`);
    return response.data;
  }

  async getDistinctCategories(): Promise<string[]> {
    const response = await axios.get<string[]>(`${this.baseUrl}/categories`);
    return response.data;
  }

  async bulkCreate(createDtos: CreateConcreteMutualFundDto[]): Promise<BulkCreateResponse> {
    const response = await axios.post<BulkCreateResponse>(`${this.baseUrl}/bulk`, createDtos);
    return response.data;
  }
}

export const concreteMutualFundService = new ConcreteMutualFundService();
