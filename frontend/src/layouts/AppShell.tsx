import React, { useState } from 'react';
import { Sidebar } from './Sidebar';
import { Topbar } from './Topbar';
import { OrdersPage } from '../features/orders/OrdersPage';
import { SettlementsPage } from '../features/settlements/SettlementsPage';
import { DiscrepanciesPage } from '../features/discrepancies/DiscrepanciesPage';
import { AnalyticsDashboardPage } from '../features/analytics/AnalyticsDashboardPage';

export const AppShell: React.FC = () => {
  const [currentTab, setCurrentTab] = useState('orders');

  return (
    <div className="app-shell" style={{ display: 'flex', minHeight: '100vh' }}>
      <Sidebar currentTab={currentTab} onTabChange={setCurrentTab} />
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
        <Topbar />
        <main style={{ flex: 1, padding: '24px', background: 'var(--color-canvas-bg)' }}>
          {currentTab === 'orders' && <OrdersPage />}
          {currentTab === 'settlements' && <SettlementsPage />}
          {currentTab === 'discrepancies' && <DiscrepanciesPage />}
          {currentTab === 'analytics' && <AnalyticsDashboardPage />}
        </main>
      </div>
    </div>
  );
};
