import { formatMoney, relativeWidth } from '../utils/formatters';

type RankListProps<T extends { contractAmount: number; tendersCount: number }> = {
  items: T[];
  max: number;
  nameKey: keyof T;
};

export function RankList<T extends { contractAmount: number; tendersCount: number }>(
  { items, max, nameKey }: RankListProps<T>,
) {
  if (items.length === 0) {
    return <p className="empty-state">Даних ще немає.</p>;
  }

  return (
    <div className="rank-list">
      {items.map((item, index) => (
        <div className="rank-row" key={String(item[nameKey]) + index}>
          <div className="rank-meta">
            <strong>{String(item[nameKey])}</strong>
            <span>{item.tendersCount} тендерів</span>
          </div>
          <div className="rank-amount">{formatMoney(item.contractAmount)}</div>
          <div className="bar-track"><span style={{ width: relativeWidth(item.contractAmount, max) }} /></div>
        </div>
      ))}
    </div>
  );
}