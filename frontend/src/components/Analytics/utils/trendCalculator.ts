import { HoldingChangeDto } from '../../../types';

export interface TrendScore {
  score: number;
  label: string;
  color: string;
  style?: React.CSSProperties;
}

export const calculateTrendScore = (record: HoldingChangeDto, hasMultipleChangeTypes: boolean): TrendScore => {
  if (!record.changeTypeCounts) {
    return { score: 0, label: 'Neutral', color: 'default' };
  }

  const counts = record.changeTypeCounts;
  const added = counts['Added'] || 0;
  const removed = counts['Removed'] || 0;
  const increased = counts['Increased'] || 0;
  const decreased = counts['Decreased'] || 0;
  
  const positive = added * 2 + increased;
  const negative = removed * 2 + decreased;
  
  const total = positive + negative || 1;
  const score = (positive - negative) / total;
  
  const baseStyle = {
    fontWeight: 'bold' as const,
    padding: '0 8px',
    borderRadius: '4px',
  };
  
  if (score >= 0.8) {
    return {
      score,
      label: 'EXTREMELY BULLISH',
      color: '',
      style: {
        ...baseStyle,
        backgroundColor: '#237804',
        color: 'white',
        border: '1px solid #389e0d',
      }
    };
  }
  if (score >= 0.6) return { score, label: 'VERY BULLISH', color: '#389e0d', style: { fontWeight: 'bold' } };
  if (score >= 0.4) return { score, label: 'Strongly Bullish', color: '#52c41a' };
  if (score >= 0.2) return { score, label: 'Bullish', color: '#73d13d' };
  if (score >= 0.1) return { score, label: 'Slightly Bullish', color: '#95de64' };
  
  if (score <= -0.8) {
    return {
      score,
      label: 'EXTREMELY BEARISH',
      color: '',
      style: {
        ...baseStyle,
        backgroundColor: '#a8071a',
        color: 'white',
        border: '1px solid #cf1322',
      }
    };
  }
  if (score <= -0.6) return { score, label: 'VERY BEARISH', color: '#cf1322', style: { fontWeight: 'bold' } };
  if (score <= -0.4) return { score, label: 'Strongly Bearish', color: '#f5222d' };
  if (score <= -0.2) return { score, label: 'Bearish', color: '#ff4d4f' };
  if (score <= -0.1) return { score, label: 'Slightly Bearish', color: '#ff7a45' };
  if (Math.abs(positive - negative) <= 1) return { score, label: 'Neutral', color: '#d9d9d9' };
  
  return { score, label: 'MIXED SIGNALS', color: '#722ed1', style: { color: 'white' } };
};

export const getChangeTypeColor = (changeType: string): string => {
  const colors: Record<string, string> = {
    Added: 'green',
    Removed: 'red',
    Increased: 'blue',
    Decreased: 'orange',
    Mixed: 'purple'
  };
  return colors[changeType] || 'default';
};

interface ConvictionData {
  previousWeight?: number | null;
  currentWeight: number;
  previousQuantity?: number | null;
  currentQuantity: number;
  previousValue?: number | null;
  currentValue: number;
  previousSnapshotTotalValue?: number | null;
  currentSnapshotTotalValue: number;
}

export const calculateConvictionTrend = (data: ConvictionData): TrendScore => {
  let score = 0;

  const { 
    previousValue, currentValue, 
    previousQuantity, currentQuantity, 
    previousWeight, currentWeight,
    previousSnapshotTotalValue, currentSnapshotTotalValue
  } = data;

  const hasPreviousData = previousValue != null && previousQuantity != null && previousWeight != null;

  if (hasPreviousData) {
    const valueChange = currentValue - previousValue!;
    const quantityChange = currentQuantity - previousQuantity!;
    const weightChange = currentWeight - previousWeight!;

    // 1. Primary Signal: Value Change interpreted with Quantity Change
    if (quantityChange > 0) {
        if (valueChange > 0) score += 4; // High Conviction Buy
        else score += 2; // Buying on Weakness
    } else if (quantityChange < 0) {
        if (valueChange < 0) score -= 4; // High Conviction Sell
        else score -= 2; // Trimming a Winner
    } else { // quantityChange === 0
        if (valueChange > 0) score += 1; // Passive Gain
        else if (valueChange < 0) score -= 1; // Passive Loss
    }

    // 2. Secondary Signal: Quantity Change vs. AUM Change
    if (previousSnapshotTotalValue != null && previousSnapshotTotalValue > 0) {
        const aumChangePercentage = (currentSnapshotTotalValue - previousSnapshotTotalValue) / previousSnapshotTotalValue;
        const quantityChangePercentage = (currentQuantity - previousQuantity!) / previousQuantity!;

        if (isFinite(quantityChangePercentage) && isFinite(aumChangePercentage)) {
            if (quantityChangePercentage > aumChangePercentage) {
                score += 2; // Outpacing AUM Growth (Active Buying)
            } else if (quantityChangePercentage < 0 && quantityChangePercentage < aumChangePercentage) {
                score -= 2; // Aggressive Selling
            }
        }
    }

    // 3. Tertiary Signal: Weight Change
    if (Math.abs(weightChange) > 0.25) {
        if (weightChange > 0) score += 2; // Significant Weight Increase
        else score -= 2; // Significant Weight Decrease
    }
  } else {
    // Handle 'Added' or 'Removed' cases where there's no previous data
    if (data.currentWeight > 0) score = 5; // Strong 'Added' signal
  }

  // Map score to labels
  if (score >= 6) return { score, label: 'EXTREMELY BULLISH', color: '#237804' };
  if (score >= 3) return { score, label: 'Bullish', color: '#52c41a' };
  if (score > 0) return { score, label: 'Slightly Bullish', color: '#95de64' };
  if (score === 0) return { score, label: 'Neutral', color: '#d9d9d9' };
  if (score <= -6) return { score, label: 'EXTREMELY BEARISH', color: '#a8071a' };
  if (score <= -3) return { score, label: 'Bearish', color: '#f5222d' };
  if (score < 0) return { score, label: 'Slightly Bearish', color: '#ff7a45' };
  
  return { score, label: 'Neutral', color: '#d9d9d9' }; // Default fallback
};
