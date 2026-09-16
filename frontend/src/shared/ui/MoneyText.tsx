import React from 'react';
import { formatMoney } from '../lib/formatters';

interface MoneyTextProps {
  amount: number;
  color?: string;
}

export const MoneyText: React.FC<MoneyTextProps> = ({ amount, color }) => {
  return (
    <span className="tabular-nums" style={{ color: color || 'inherit', fontWeight: 600 }}>
      {formatMoney(amount)}
    </span>
  );
};
