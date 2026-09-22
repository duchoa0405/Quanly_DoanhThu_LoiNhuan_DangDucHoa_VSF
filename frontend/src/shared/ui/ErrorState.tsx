import React from 'react';
import { Button } from './Button';

interface ErrorStateProps {
  message?: string;
  onRetry?: () => void;
}

export const ErrorState: React.FC<ErrorStateProps> = ({
  message = 'Đã xảy ra lỗi trong quá trình xử lý.',
  onRetry,
}) => {
  return (
    <div
      style={{
        padding: '16px 20px',
        marginBottom: '16px',
        borderRadius: '8px',
        background: '#fef2f2',
        border: '1px solid #fecaca',
        color: '#b91c1c',
        fontSize: '13px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: '16px',
      }}
    >
      <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
        <span style={{ fontSize: '18px' }}>⚠️</span>
        <span>{message}</span>
      </div>
      {onRetry && (
        <Button variant="secondary" onClick={onRetry}>
          Thử lại
        </Button>
      )}
    </div>
  );
};
