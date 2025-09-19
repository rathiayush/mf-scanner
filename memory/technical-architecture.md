# Technical Architecture Memory Bank

## System Overview
The Mutual Fund Scanner is built using a modern full-stack architecture with clear separation of concerns.

## Technology Stack

### Frontend
- **Framework**: React 18+
- **UI Library**: Ant Design (antd)
- **State Management**: React Context API / Redux Toolkit
- **HTTP Client**: Axios
- **Build Tool**: Vite
- **TypeScript**: For type safety

### Backend
- **Framework**: .NET Core 8 Web API
- **ORM**: Entity Framework Core
- **Database**: SQL Server
- **Authentication**: JWT Bearer tokens
- **File Processing**: EPPlus for Excel files
- **Logging**: Serilog
- **Background Services**: Hosted Services for file monitoring

### Architecture Patterns
- **Clean Architecture**: Domain, Application, Infrastructure layers
- **CQRS**: Command Query Responsibility Segregation
- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Built-in .NET DI container

## Key Components

### File Scanner Service
- Monitors designated folders for new Excel files
- Implements FileSystemWatcher for real-time detection
- Queues files for processing to avoid blocking

### Excel Processing Engine
- Supports multiple Excel formats
- Column mapping strategies for different fund formats
- Data validation and normalization
- Error handling and recovery

### Duplicate Detection
- File hash comparison
- Metadata-based duplicate checking
- Database constraints for data integrity

### Logging System
- Structured logging with Serilog
- File processing audit trail
- Error tracking and alerting
- Performance monitoring
