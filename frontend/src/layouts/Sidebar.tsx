import React from 'react';

interface SidebarProps {
  currentTab: string;
  onTabChange: (tab: string) => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ currentTab, onTabChange }) => {
  const navItems = [
    { id: 'orders', label: 'Đơn Hàng', icon: '📦' },
    { id: 'settlements', label: 'Đối Soát Ví', icon: '💳' },
    { id: 'discrepancies', label: 'Biên Bản Lệch', icon: '⚖️' },
    { id: 'analytics', label: 'Dashboard', icon: '📊' },
  ];

  return (
    <aside style={{ width: 'var(--sidebar-width)', background: '#ffffff', borderRight: '1px solid var(--color-border-hairline)', display: 'flex', flexDirection: 'column', alignItems: 'center', padding: '16px 0' }}>
      <div style={{ fontWeight: 800, fontSize: '18px', color: 'var(--color-primary)', marginBottom: '32px' }}>
        VSF
      </div>
      <nav style={{ display: 'flex', flexDirection: 'column', gap: '16px', width: '100%', alignItems: 'center' }}>
        {navItems.map((item) => (
          <button
            key={item.id}
            onClick={() => onTabChange(item.id)}
            title={item.label}
            style={{
              width: '44px',
              height: '44px',
              borderRadius: '10px',
              border: 'none',
              background: currentTab === item.id ? 'var(--color-canvas-bg)' : 'transparent',
              fontSize: '20px',
              cursor: 'pointer',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              transition: 'background 0.15s ease',
            }}
          >
            {item.icon}
          </button>
        ))}
      </nav>
    </aside>
  );
};
