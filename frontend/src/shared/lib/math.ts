export function calculateMargin(gross: number, fees: number): number {
  if (gross <= 0) return 0;
  return Number((((gross - fees) / gross) * 100).toFixed(1));
}
