import React from 'react';

interface BadgeProps {
  label: string;
  variant?: 'tiktok' | 'shopee' | 'pos' | 'success' | 'warning' | 'danger';
}

export const Badge: React.FC<BadgeProps> = ({ label, variant = 'success' }) => {
  return (
    <span
      style={{
        display: 'inline-block',
        padding: '2px 8px',
        borderRadius: '9999px',
        fontSize: '11px',
        fontWeight: 600,
        textTransform: 'uppercase',
        letterSpacing: '0.04em',
        background: '#e2e8f0',
        color: '#1e293b',
      }}
    >
      {label}
    </span>
  );
};
