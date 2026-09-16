import React from 'react';

interface PermissionGateProps {
  role: 'SalesOps' | 'FinanceManager' | 'ShopOwner';
  children: React.ReactNode;
}

export const PermissionGate: React.FC<PermissionGateProps> = ({ role, children }) => {
  // Simple mock check
  const currentRole = 'ShopOwner';
  if (role !== currentRole && currentRole !== 'ShopOwner') return null;
  return <>{children}</>;
};
