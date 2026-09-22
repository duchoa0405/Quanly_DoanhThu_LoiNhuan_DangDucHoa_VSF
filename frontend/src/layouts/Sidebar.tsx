import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

export const Sidebar: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();

  const navItems = [
    { path: '/orders', label: 'Đơn Hàng', icon: '📦' },
    { path: '/settlement', label: 'Đối Soát Ví', icon: '💳' },
    { path: '/analytics', label: 'Dashboard', icon: '📊' },
    { path: '/catalog', label: 'Danh Mục & Giá Vốn', icon: '🏷️' },
  ];

  return (
    <aside style={{ width: 'var(--sidebar-width)', background: '#ffffff', borderRight: '1px solid var(--color-border-hairline)', display: 'flex', flexDirection: 'column', alignItems: 'center', padding: '16px 0' }}>
      <div
        role="button"
        tabIndex={0}
        style={{ fontWeight: 800, fontSize: '18px', color: 'var(--color-primary)', marginBottom: '32px', cursor: 'pointer' }}
        onClick={() => navigate('/orders')}
        onKeyDown={(e) => { if (e.key === 'Enter') navigate('/orders'); }}
      >
        VSF
      </div>
      <nav style={{ display: 'flex', flexDirection: 'column', gap: '16px', width: '100%', alignItems: 'center' }}>
        {navItems.map((item) => {
          const isActive = location.pathname.startsWith(item.path);
          return (
            <button
              key={item.path}
              type="button"
              onClick={() => navigate(item.path)}
              title={item.label}
              style={{
                width: '44px',
                height: '44px',
                borderRadius: '10px',
                border: 'none',
                background: isActive ? '#ede9fe' : 'transparent',
                color: isActive ? 'var(--color-primary)' : 'inherit',
                fontSize: '20px',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                transition: 'all 0.15s ease',
              }}
            >
              {item.icon}
            </button>
          );
        })}
      </nav>
    </aside>
  );
};
