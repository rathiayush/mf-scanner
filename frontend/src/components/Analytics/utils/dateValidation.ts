import dayjs, { Dayjs } from 'dayjs';
import { AvailableDatesResponseDto } from '../../../types';

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
    const selectedFundAvailabilities = isAllFundsSelected 
      ? availableDates.fundDateAvailabilities
      : availableDates.fundDateAvailabilities.filter(
          fund => selectedFunds.includes(fund.mutualFundId)
        );
    
    if (selectedFundAvailabilities.length === 0) {
      return false;
    }
    
    try {
      // Find the intersection of available dates for all selected funds
      const availableDateStrings = selectedFundAvailabilities.reduce((intersection, fund) => {
        const fundDates = fund.availableDates.map(dateStr => dayjs(dateStr).format('YYYY-MM-DD'));
        return intersection.length === 0 
          ? fundDates 
          : intersection.filter(date => fundDates.includes(date));
      }, [] as string[]);
      
      if (availableDateStrings.length === 0) {
        return true;
      }
      
      const currentDateStr = current.format('YYYY-MM-DD');
      return !availableDateStrings.includes(currentDateStr);
      
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
