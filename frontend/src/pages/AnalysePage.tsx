import React from 'react';
import EnhancedAnalytics from '../components/Analytics/EnhancedAnalytics';

const AnalysePage: React.FC = () => {
  return (
    <div 
      id="analyse-page-container" 
      style={{ 
        maxWidth: '100%',
        overflow: 'hidden'
      }}
    >
      <EnhancedAnalytics />
    </div>
  );
};

export default AnalysePage;
