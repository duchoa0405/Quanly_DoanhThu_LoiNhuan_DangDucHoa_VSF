import React from 'react';

export const Topbar: React.FC = () => {
  return (
    <header
      style={{
        height: 'var(--topbar-height)',
        background: '#ffffff',
        borderBottom: '1px solid var(--color-border-hairline)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '0 24px',
      }}
    >
      <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
        <span style={{ fontWeight: 700, fontSize: '15px' }}>FASHION-WEB</span>
        <span style={{ fontSize: '12px', color: 'var(--color-text-muted)' }}>Multi-Channel Settlement Platform</span>
      </div>
      <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
        <span style={{ fontSize: '12px', background: '#e0f2fe', color: '#0369a1', padding: '4px 10px', borderRadius: '12px', fontWeight: 600 }}>
          Vai trò: ShopOwner (Toàn quyền)
        </span>
      </div>
    </header>
  );
};
