// Legacy interfaces (keeping for compatibility)
export interface UploadedFile {
  id: string;
  name: string;
  size: number;
  type: string;
  status: 'pending' | 'uploading' | 'processing' | 'completed' | 'error';
  progress: number;
  uploadedAt: Date;
  processedData?: ProcessedFileData;
  error?: string;
}

export interface ProcessedFileData {
  fundName: string;
  reportDate: string;
  totalHoldings: number;
  totalValue: number;
  holdings: Holding[];
  summary: PortfolioSummary;
}

export interface Holding {
  securityName: string;
  isin: string;
  weight: number;
  value: number;
  quantity: number;
  sector?: string;
  industry?: string;
}

export interface PortfolioSummary {
  topHoldings: Holding[];
  sectorAllocation: SectorAllocation[];
  totalValue: number;
  numberOfHoldings: number;
}

export interface SectorAllocation {
  sector: string;
  percentage: number;
  value: number;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

export interface FileUploadResponse {
  fileId: string;
  fileName: string;
  status: string;
  message: string;
}

// New API DTOs matching backend
export interface MutualFundDto {
  id: number;
  fundCode: string;
  fundName: string;
  amc: string;
  category: string;
  subCategory: string;
  launchDate?: string;
  nav?: number;
  aum?: number;
  expenseRatio?: number;
  status: string;
  isActive: boolean;
  createdDate: string;
  lastUpdated?: string;
  portfolioCount: number;
  latestPortfolioDate?: string;
}

export interface CreateMutualFundDto {
  fundName: string;
  fundCode?: string;
  amc?: string;
  category?: string;
  subCategory?: string;
  launchDate?: string;
  nav?: number;
  aum?: number;
  expenseRatio?: number;
}

export interface UpdateMutualFundDto {
  id: number;
  fundName: string;
  fundCode?: string;
  amc?: string;
  category?: string;
  subCategory?: string;
  launchDate?: string;
  nav?: number;
  aum?: number;
  expenseRatio?: number;
  status: string;
  isActive: boolean;
}

export interface MutualFundSearchDto {
  searchTerm?: string;
  category?: string;
  amc?: string;
  isActive?: boolean;
  page: number;
  pageSize: number;
}

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface FileUploadRequestDto {
  mutualFundId: number;
  files: File[];
}

export interface FileUploadResponseDto {
  success: boolean;
  message: string;
  processedFiles: ProcessedFileDto[];
  errors: string[];
}

export interface ProcessedFileDto {
  fileId: number;
  fileName: string;
  reportDate: string;
  holdingsCount: number;
  status: string;
  errorMessage?: string;
}

export interface MultipleFileUploadRequestDto {
  fileUploads: FileUploadItem[];
}

export interface FileUploadItem {
  mutualFundId: number;
  file: File;
}

export interface AnalyticsRequestDto {
  mutualFundIds: number[];
  fromDate: string;
  toDate: string;
  page: number;
  pageSize: number;
  changeTypeFilter?: string; // Added, Removed, Increased, Decreased, Mixed, or undefined for all
}

export interface AnalyticsResponseDto {
  mutualFundIds: number[];
  fromDate: string;
  toDate: string;
  addedRemovedHoldings: PagedResult<HoldingChangeDto>;
  topShares: TopShareDto[];
  summary: AnalyticsSummaryDto;
}

export interface HoldingChangeDto {
  isin: string;
  securityName: string;
  sector: string;
  industry: string;
  previousWeight?: number;
  currentWeight: number;
  weightChange: number;
  changeType: string; // Added, Removed, Increased, Decreased
  mutualFundNames: string[];
  fromDate: string;
  toDate: string;
}

export interface TopShareDto {
  isin: string;
  securityName: string;
  sector: string;
  industry: string;
  totalWeight: number;
  holdingFundsCount: number;
  fundHoldings: FundHoldingDto[];
}

export interface FundHoldingDto {
  mutualFundId: number;
  mutualFundName: string;
  weight: number;
  previousWeight?: number;
  weightChange: number;
}

export interface AnalyticsSummaryDto {
  totalHoldingsAnalyzed: number;
  addedHoldings: number;
  removedHoldings: number;
  increasedHoldings: number;
  decreasedHoldings: number;
  unchangedHoldings: number;
  totalWeightChange: number;
}

export interface DateRangeDto {
  date: string;
  displayName: string;
  hasData: boolean;
}

export interface AvailableDatesResponseDto {
  availableDates: DateRangeDto[];
  minDate?: string;
  maxDate?: string;
  fundDateAvailabilities: MutualFundDateAvailability[];
}

export interface MutualFundDateAvailability {
  mutualFundId: number;
  mutualFundName: string;
  minDate?: string;
  maxDate?: string;
  availableDates: string[];
}

export interface CurrentHoldingsRequestDto {
  mutualFundId: number;
  date: string;
}

export interface CurrentHoldingDto {
  id: number;
  symbol: string;
  companyName: string;
  quantity: number;
  price: number;
  value: number;
  weight: number;
  sector?: string;
  industry?: string;
}

export interface CurrentHoldingsResponseDto {
  success: boolean;
  message: string;
  mutualFundId: number;
  mutualFundName: string;
  date: string;
  totalValue: number;
  holdings: CurrentHoldingDto[];
  holdingsCount: number;
}
