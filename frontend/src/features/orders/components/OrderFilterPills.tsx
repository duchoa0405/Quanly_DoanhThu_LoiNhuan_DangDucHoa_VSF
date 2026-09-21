import React from 'react';

interface OrderFilterPillsProps {
  selectedChannel: string;
  onSelectChannel: (channel: string) => void;
}

export const OrderFilterPills: React.FC<OrderFilterPillsProps> = ({ selectedChannel, onSelectChannel }) => {
  const pills = [
    { id: 'ALL', label: 'Tất cả kênh' },
    { id: 'TikTokShop', label: 'TikTok Shop' },
    { id: 'Shopee', label: 'Shopee' },
    { id: 'Pos', label: 'In-Store POS' },
  ];

  return (
    <div style={{ display: 'flex', gap: '8px', marginBottom: '16px' }}>
      {pills.map((p) => (
        <button
          key={p.id}
          type="button"
          onClick={() => onSelectChannel(p.id)}
          style={{
            padding: '6px 14px',
            borderRadius: '20px',
            border: '1px solid',
            borderColor: selectedChannel === p.id ? 'var(--color-primary)' : 'var(--color-border-subtle)',
            background: selectedChannel === p.id ? '#ede9fe' : '#ffffff',
            color: selectedChannel === p.id ? 'var(--color-primary)' : 'var(--color-text-primary)',
            fontSize: '13px',
            fontWeight: 500,
            cursor: 'pointer',
            transition: 'all 0.15s ease',
          }}
        >
          {p.label}
        </button>
      ))}
    </div>
  );
};
