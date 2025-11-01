import React from 'react';
import EnhancedAnalytics from '../components/Analytics/EnhancedAnalytics';

const AnalysePage: React.FC = () => {
  return (
    <div 
      id="analyse-page-container" 
      style={{ 
        padding: window.innerWidth < 768 ? '8px' : '24px',
        maxWidth: '100%',
        overflow: 'hidden'
      }}
    >
      <EnhancedAnalytics />
    </div>
  );
};

export default AnalysePage;
