import { useState } from 'react';

export const useModalState = () => {
  const [showFundsModal, setShowFundsModal] = useState(false);
  const [selectedModalData, setSelectedModalData] = useState<any>(null);
  const [showHoldingFundsModal, setShowHoldingFundsModal] = useState(false);
  const [selectedHoldingModalData, setSelectedHoldingModalData] = useState<any>(null);
  const [modalFilterType, setModalFilterType] = useState<string>('all');

  const openFundsModal = (data: any) => {
    setSelectedModalData(data);
    setShowFundsModal(true);
  };

  const closeFundsModal = () => {
    setShowFundsModal(false);
    setSelectedModalData(null);
  };

  const openHoldingFundsModal = (data: any) => {
    setSelectedHoldingModalData(data);
    setModalFilterType('all');
    setShowHoldingFundsModal(true);
  };

  const closeHoldingFundsModal = () => {
    setShowHoldingFundsModal(false);
    setSelectedHoldingModalData(null);
  };

  return {
    showFundsModal,
    selectedModalData,
    showHoldingFundsModal,
    selectedHoldingModalData,
    modalFilterType,
    setModalFilterType,
    openFundsModal,
    closeFundsModal,
    openHoldingFundsModal,
    closeHoldingFundsModal
  };
};
