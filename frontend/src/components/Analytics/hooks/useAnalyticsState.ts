import { useState } from 'react';
import { Dayjs } from 'dayjs';
import {
  MutualFundDto,
  AnalyticsResponseDto,
  HoldingChangeDto,
  TopShareDto,
  AvailableDatesResponseDto,
  PagedResult
} from '../../../types';

export const useAnalyticsState = () => {
  const [mutualFunds, setMutualFunds] = useState<MutualFundDto[]>([]);
  const [selectedFunds, setSelectedFunds] = useState<number[]>([]);
  const [isAllFundsSelected, setIsAllFundsSelected] = useState(false);
  const [dateRange, setDateRange] = useState<[Dayjs, Dayjs] | null>(null);
  const [availableDates, setAvailableDates] = useState<AvailableDatesResponseDto | null>(null);
  const [analyticsData, setAnalyticsData] = useState<AnalyticsResponseDto | null>(null);
  const [holdingChanges, setHoldingChanges] = useState<PagedResult<HoldingChangeDto> | null>(null);
  const [topShares, setTopShares] = useState<PagedResult<TopShareDto> | null>(null);
  const [portfolioSnapshots, setPortfolioSnapshots] = useState<any[]>([]);
  const [currentHoldings, setCurrentHoldings] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);

  return {
    mutualFunds,
    setMutualFunds,
    selectedFunds,
    setSelectedFunds,
    isAllFundsSelected,
    setIsAllFundsSelected,
    dateRange,
    setDateRange,
    availableDates,
    setAvailableDates,
    analyticsData,
    setAnalyticsData,
    holdingChanges,
    setHoldingChanges,
    topShares,
    setTopShares,
    portfolioSnapshots,
    setPortfolioSnapshots,
    currentHoldings,
    setCurrentHoldings,
    loading,
    setLoading
  };
};
