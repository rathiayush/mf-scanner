import { useState } from 'react';

export const useSortingAndPagination = () => {
  // Holding Changes pagination and sorting
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [selectedStatusFilters, setSelectedStatusFilters] = useState<string[]>([]);
  const [holdingChangesSortBy, setHoldingChangesSortBy] = useState<string>('weightChange');
  const [holdingChangesSortDirection, setHoldingChangesSortDirection] = useState<string>('desc');
  const [changeTypeMatchMode, setChangeTypeMatchMode] = useState<'any' | 'only'>('any');
  
  // Top Shares pagination and sorting
  const [topSharesPage, setTopSharesPage] = useState<number>(1);
  const [topSharesPageSize, setTopSharesPageSize] = useState<number>(20);
  const [topSharesSortBy, setTopSharesSortBy] = useState<string>('totalWeight');
  const [topSharesSortDirection, setTopSharesSortDirection] = useState<string>('desc');
  
  // Client-side sorting for trend column
  const [sortConfig, setSortConfig] = useState<{ key: string; direction: 'ascend' | 'descend' | null }>({ 
    key: 'score', 
    direction: 'descend' 
  });

  const requestSort = (key: string) => {
    let direction: 'ascend' | 'descend' = 'ascend';
    if (sortConfig.key === key && sortConfig.direction === 'ascend') {
      direction = 'descend';
    } else if (sortConfig.key === key && sortConfig.direction === 'descend') {
      return setSortConfig({ key: '', direction: null });
    }
    setSortConfig({ key, direction });
  };

  const getSortOrder = (key: string) => {
    if (sortConfig.key === key) {
      return sortConfig.direction;
    }
    return null;
  };

  return {
    // Holding Changes
    currentPage,
    setCurrentPage,
    pageSize,
    setPageSize,
    selectedStatusFilters,
    setSelectedStatusFilters,
    holdingChangesSortBy,
    setHoldingChangesSortBy,
    holdingChangesSortDirection,
    setHoldingChangesSortDirection,
    changeTypeMatchMode,
    setChangeTypeMatchMode,
    
    // Top Shares
    topSharesPage,
    setTopSharesPage,
    topSharesPageSize,
    setTopSharesPageSize,
    topSharesSortBy,
    setTopSharesSortBy,
    topSharesSortDirection,
    setTopSharesSortDirection,
    
    // Client-side sorting
    sortConfig,
    requestSort,
    getSortOrder
  };
};
