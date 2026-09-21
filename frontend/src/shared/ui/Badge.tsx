import React from 'react';

interface BadgeProps {
  label: string;
  variant?: 'tiktok' | 'shopee' | 'pos' | 'success' | 'warning' | 'danger' | 'default';
}

export const Badge: React.FC<BadgeProps> = ({ label, variant }) => {
  const getBadgeStyle = () => {
    const key = (variant || label).toLowerCase();
    switch (key) {
      case 'tiktok':
      case 'tiktokshop':
        return { background: '#000000', color: '#ffffff' };
      case 'shopee':
        return { background: '#ee4d2d', color: '#ffffff' };
      case 'pos':
        return { background: '#4f46e5', color: '#ffffff' };
      case 'success':
        return { background: '#ecfdf5', color: '#059669' };
      case 'warning':
        return { background: '#fffbeb', color: '#d97706' };
      case 'danger':
        return { background: '#fef2f2', color: '#dc2626' };
      default:
        return { background: '#e2e8f0', color: '#1e293b' };
    }
  };

  const style = getBadgeStyle();

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
        background: style.background,
        color: style.color,
      }}
    >
      {label}
    </span>
  );
};
