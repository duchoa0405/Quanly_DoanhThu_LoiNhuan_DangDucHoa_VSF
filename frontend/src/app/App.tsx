import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { AppShell } from '../layouts/AppShell';
import { OrdersPage } from '../features/orders/OrdersPage';
import { SettlementsPage } from '../features/settlements/SettlementsPage';
import { AnalyticsDashboardPage } from '../features/analytics/AnalyticsDashboardPage';
import { CatalogPage } from '../features/catalog/CatalogPage';

export const App: React.FC = () => {
  return (
    <AppShell>
      <Routes>
        <Route path="/" element={<Navigate to="/orders" replace />} />
        <Route path="/orders" element={<OrdersPage />} />
        <Route path="/settlement" element={<SettlementsPage />} />
        <Route path="/analytics" element={<AnalyticsDashboardPage />} />
        <Route path="/catalog" element={<CatalogPage />} />
        <Route path="*" element={<Navigate to="/orders" replace />} />
      </Routes>
    </AppShell>
  );
};

export default App;
