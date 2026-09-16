import React from 'react';

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
}

export const Button: React.FC<ButtonProps> = ({
  variant = 'primary',
  size = 'md',
  children,
  style,
  ...props
}) => {
  const baseStyle: React.CSSProperties = {
    display: 'inline-flex',
    alignItems: 'center',
    justifyContent: 'center',
    fontWeight: 500,
    borderRadius: '6px',
    border: '1px solid transparent',
    cursor: 'pointer',
    transition: 'all 0.15s ease-in-out',
    padding: size === 'sm' ? '6px 12px' : '8px 16px',
    fontSize: size === 'sm' ? '12px' : '14px',
    background: variant === 'primary' ? 'var(--color-primary)' : '#ffffff',
    color: variant === 'primary' ? '#ffffff' : 'var(--color-text-primary)',
    borderColor: variant === 'secondary' ? 'var(--color-border-subtle)' : 'transparent',
    ...style,
  };

  return (
    <button style={baseStyle} {...props}>
      {children}
    </button>
  );
};
