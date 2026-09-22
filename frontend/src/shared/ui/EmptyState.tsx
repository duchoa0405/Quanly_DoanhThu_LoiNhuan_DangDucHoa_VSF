import React from 'react';
import { Button } from './Button';

interface EmptyStateProps {
  title?: string;
  description?: string;
  actionLabel?: string;
  onAction?: () => void;
}

export const EmptyState: React.FC<EmptyStateProps> = ({
  title = 'Không có dữ liệu',
  description = 'Chưa có bản ghi nào phù hợp với điều kiện hiển thị hiện tại.',
  actionLabel,
  onAction,
}) => {
  return (
    <div
      style={{
        padding: '48px 24px',
        textAlign: 'center',
        color: '#64748b',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        gap: '12px',
      }}
    >
      <div style={{ fontSize: '32px', marginBottom: '4px' }}>📭</div>
      <h3 style={{ fontSize: '15px', fontWeight: 600, color: '#1e293b', margin: 0 }}>{title}</h3>
      <p style={{ fontSize: '13px', margin: 0, maxWidth: '400px', lineHeight: 1.5 }}>{description}</p>
      {actionLabel && onAction && (
        <div style={{ marginTop: '8px' }}>
          <Button variant="primary" onClick={onAction}>
            {actionLabel}
          </Button>
        </div>
      )}
    </div>
  );
};
