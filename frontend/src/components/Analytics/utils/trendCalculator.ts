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
  
  // Calculate total positive and negative signals
  const positive = added * 2 + increased;
  const negative = removed * 2 + decreased;
  
  // Calculate trend score (-1 to 1)
  const total = positive + negative || 1;
  const score = (positive - negative) / total;
  
  // Base style for all tags
  const baseStyle = {
    fontWeight: 'bold' as const,
    padding: '0 8px',
    borderRadius: '4px',
    ...(hasMultipleChangeTypes && {
      animation: 'blink 1s step-end infinite'
    })
  };
  
  // Determine trend based on score
  if (score >= 0.8) {
    return {
      score,
      label: 'EXTREMELY BULLISH',
      color: '',
      style: {
        ...baseStyle,
        backgroundColor: '#00FF00',
        color: 'black',
        border: '1px solid #00CC00',
        boxShadow: hasMultipleChangeTypes ? '0 0 10px #00FF00' : 'none'
      }
    };
  }
  if (score >= 0.6) return { score, label: 'VERY BULLISH', color: '#52c41a', style: { fontWeight: 'bold' } };
  if (score >= 0.4) return { score, label: 'Strongly Bullish', color: '#73d13d' };
  if (score >= 0.2) return { score, label: 'Bullish', color: '#95de64' };
  if (score >= 0.1) return { score, label: 'Slightly Bullish', color: '#b7eb8f' };
  
  if (score <= -0.8) {
    return {
      score,
      label: 'EXTREMELY BEARISH',
      color: '',
      style: {
        ...baseStyle,
        backgroundColor: '#8B0000',
        color: 'white',
        border: '1px solid #FF0000',
        boxShadow: hasMultipleChangeTypes ? '0 0 10px #FF0000' : 'none'
      }
    };
  }
  if (score <= -0.6) return { score, label: 'VERY BEARISH', color: '#ff4d4f', style: { fontWeight: 'bold' } };
  if (score <= -0.4) return { score, label: 'Strongly Bearish', color: '#ff7a45' };
  if (score <= -0.2) return { score, label: 'Bearish', color: '#ffbb96' };
  if (score <= -0.1) return { score, label: 'Slightly Bearish', color: '#ffd8bf' };
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
