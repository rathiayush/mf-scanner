import dayjs, { Dayjs } from 'dayjs';
import { AvailableDatesResponseDto } from '../../../types';

// Cache for optimized date validation
let cachedAvailableDatesSet: Set<string> | null = null;
let cachedFundIds: string = '';
let cachedIsAllFunds = false;

// Helper to create cache key for selected funds
const createFundsCacheKey = (selectedFunds: number[], isAllFundsSelected: boolean): string => {
  return isAllFundsSelected ? 'ALL_FUNDS' : selectedFunds.sort().join(',');
};

// Optimized function to build available dates set
const buildAvailableDatesSet = (
  selectedFunds: number[],
  isAllFundsSelected: boolean,
  availableDates: AvailableDatesResponseDto
): Set<string> => {
  const fundsCacheKey = createFundsCacheKey(selectedFunds, isAllFundsSelected);
  
  // Return cached result if parameters haven't changed
  if (cachedAvailableDatesSet && cachedFundIds === fundsCacheKey && cachedIsAllFunds === isAllFundsSelected) {
    return cachedAvailableDatesSet;
  }
  
  if (!availableDates?.fundDateAvailabilities || availableDates.fundDateAvailabilities.length === 0) {
    cachedAvailableDatesSet = new Set<string>();
    cachedFundIds = fundsCacheKey;
    cachedIsAllFunds = isAllFundsSelected;
    return cachedAvailableDatesSet;
  }
  
  const selectedFundAvailabilities = isAllFundsSelected 
    ? availableDates.fundDateAvailabilities
    : availableDates.fundDateAvailabilities.filter(
        fund => selectedFunds.includes(fund.mutualFundId)
      );
  
  if (selectedFundAvailabilities.length === 0) {
    cachedAvailableDatesSet = new Set<string>();
    cachedFundIds = fundsCacheKey;
    cachedIsAllFunds = isAllFundsSelected;
    return cachedAvailableDatesSet;
  }
  
  // Convert each fund's dates to Set for O(1) lookup
  const fundDateSets = selectedFundAvailabilities.map(fund => 
    new Set(fund.availableDates.map(dateStr => dayjs(dateStr).format('YYYY-MM-DD')))
  );
  
  // Find intersection of all fund date sets
  let intersection = fundDateSets[0];
  for (let i = 1; i < fundDateSets.length; i++) {
    const newIntersection = new Set<string>();
    for (const date of intersection) {
      if (fundDateSets[i].has(date)) {
        newIntersection.add(date);
      }
    }
    intersection = newIntersection;
  }
  
  // Cache the result
  cachedAvailableDatesSet = intersection;
  cachedFundIds = fundsCacheKey;
  cachedIsAllFunds = isAllFundsSelected;
  
  return intersection;
};

export const disabledDateLogic = (
  current: Dayjs,
  selectedFunds: number[],
  isAllFundsSelected: boolean,
  availableDates: AvailableDatesResponseDto | null
): boolean => {
  // If no funds selected and not all funds selected, use global date range
  if (selectedFunds.length === 0 && !isAllFundsSelected) {
    if (!availableDates || !availableDates.minDate || !availableDates.maxDate) {
      return false;
    }
    
    try {
      const minDate = dayjs(availableDates.minDate);
      const maxDate = dayjs(availableDates.maxDate);
      
      if (!minDate.isValid() || !maxDate.isValid()) {
        return false;
      }
      
      return current.isBefore(minDate.startOf('month')) || current.isAfter(maxDate.endOf('month'));
    } catch (error) {
      return false;
    }
  }
  
  // If funds are selected or all funds selected, validate against fund-specific availability
  if (availableDates?.fundDateAvailabilities && availableDates.fundDateAvailabilities.length > 0) {
    try {
      // Use optimized set-based lookup
      const availableDatesSet = buildAvailableDatesSet(selectedFunds, isAllFundsSelected, availableDates);
      
      if (availableDatesSet.size === 0) {
        return true;
      }
      
      const currentDateStr = current.format('YYYY-MM-DD');
      return !availableDatesSet.has(currentDateStr);
      
    } catch (error) {
      // Fallback to global range if there's an error
      if (!availableDates.minDate || !availableDates.maxDate) {
        return false;
      }
      
      const minDate = dayjs(availableDates.minDate);
      const maxDate = dayjs(availableDates.maxDate);
      
      if (!minDate.isValid() || !maxDate.isValid()) {
        return false;
      }
      
      return current.isBefore(minDate.startOf('month')) || current.isAfter(maxDate.endOf('month'));
    }
  }
  
  // Fallback to global date range if no fund-specific data
  if (!availableDates || !availableDates.minDate || !availableDates.maxDate) {
    return false;
  }
  
  try {
    const minDate = dayjs(availableDates.minDate);
    const maxDate = dayjs(availableDates.maxDate);
    
    if (!minDate.isValid() || !maxDate.isValid()) {
      return false;
    }
    
    return current.isBefore(minDate.startOf('month')) || current.isAfter(maxDate.endOf('month'));
  } catch (error) {
    return false;
  }
};

// Function to clear cache when funds or dates change
export const clearDateValidationCache = (): void => {
  cachedAvailableDatesSet = null;
  cachedFundIds = '';
  cachedIsAllFunds = false;
};
