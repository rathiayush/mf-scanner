export enum BackgroundJobStatus {
  Queued = 0,
  Running = 1,
  Completed = 2,
  Failed = 3,
  Cancelled = 4
}

export enum ProcessingStatus {
  Pending = 0,
  Processing = 1,
  Completed = 2,
  Failed = 3,
  Skipped = 4
}

export enum UploadErrorCode {
  // Validation Errors (1000-1999)
  ValidationError = 1000,
  DateValidationFailed = 1001,
  FileFormatInvalid = 1002,
  FileEmpty = 1003,
  FileTooLarge = 1004,
  
  // Detection Errors (2000-2999)
  DetectionError = 2000,
  MutualFundNotDetected = 2001,
  RequiredColumnsNotFound = 2002,
  SheetNotFound = 2003,
  
  // Processing Errors (3000-3999)
  ProcessingError = 3000,
  MutualFundCreationFailed = 3001,
  PortfolioFileCreationFailed = 3002,
  SnapshotCreationFailed = 3003,
  HoldingsExtractionFailed = 3004,
  HoldingChangesTrackingFailed = 3005,
  
  // Database Errors (4000-4999)
  DatabaseError = 4000,
  TransactionFailed = 4001,
  DataCleanupFailed = 4002,
  SaveChangesFailed = 4003,
  
  // System Errors (5000-5999)
  SystemError = 5000,
  UnexpectedError = 5001,
  ServiceUnavailable = 5002
}

export interface BackgroundProcessingRequest {
  directoryPath?: string;
  reportDate: string;
  performDateCheck: boolean;
  filePatterns: string[];
  generateExcelReport: boolean;
}

export interface BackgroundProcessingStatus {
  jobId: string;
  status: BackgroundJobStatus;
  message: string;
  startTime: string;
  endTime?: string;
  duration?: string;
  totalFiles: number;
  processedFiles: number;
  successfulFiles: number;
  failedFiles: number;
  currentFile?: string;
  result?: BackgroundProcessingResult;
  excelReportPath?: string;
  errors: string[];
}

export interface BackgroundProcessingResult {
  success: boolean;
  message: string;
  directoryProcessed: string;
  processingStartTime: string;
  processingEndTime: string;
  processingDuration: string;
  totalFilesFound: number;
  filesProcessedSuccessfully: number;
  filesFailed: number;
  filesSkipped: number;
  totalSheetsProcessed: number;
  sheetsProcessedSuccessfully: number;
  sheetsFailed: number;
  newMutualFundsCreated: number;
  existingMutualFundsUpdated: number;
  totalHoldingsProcessed: number;
  totalHoldingChangesTracked: number;
  fileResults: BackgroundFileResult[];
  sheetResults: BackgroundSheetResult[];
  errors: BackgroundProcessingError[];
}

export interface BackgroundFileResult {
  fileName: string;
  filePath: string;
  status: ProcessingStatus;
  errorMessage?: string;
  processingStartTime: string;
  processingEndTime: string;
  processingDuration: string;
  sheetsFound: number;
  sheetsProcessedSuccessfully: number;
  sheetsFailed: number;
  sheetResults: BackgroundSheetResult[];
}

export interface BackgroundSheetResult {
  fileName: string;
  sheetIndex: number;
  sheetName: string;
  status: ProcessingStatus;
  errorMessage?: string;
  mutualFundDetected: boolean;
  detectedFundName?: string;
  mutualFundId?: number;
  wasNewFundCreated: boolean;
  matchConfidence: number;
  portfolioFileId?: number;
  snapshotId?: number;
  holdingsExtracted: number;
  holdingChangesTracked: number;
  processingStartTime: string;
  processingEndTime: string;
  processingDuration: string;
}

export interface BackgroundProcessingError {
  fileName: string;
  sheetIndex?: number;
  sheetName?: string;
  errorCode: UploadErrorCode;
  errorMessage: string;
  detailedError?: string;
  processingStage: string;
  timestamp: string;
}

export interface ProcessingSummary {
  totalJobs: number;
  runningJobs: number;
  completedJobs: number;
  failedJobs: number;
  queuedJobs: number;
  recentJobs: RecentJob[];
}

export interface RecentJob {
  jobId: string;
  status: BackgroundJobStatus;
  startTime: string;
  duration?: string;
  totalFiles: number;
  successfulFiles: number;
  failedFiles: number;
}

export interface DirectoryValidation {
  isValid: boolean;
  directoryPath: string;
  excelFileCount: number;
  files: DirectoryFile[];
}

export interface DirectoryFile {
  fileName: string;
  filePath: string;
  fileSize: number;
  lastModified: string;
}

export interface StartJobResponse {
  jobId: string;
  message: string;
}
