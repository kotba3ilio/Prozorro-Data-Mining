import type { AnalyticsSummary } from '../types';
import { RankList } from './RankList';

type AnalyticsViewProps = {
  maxEntityAmount: number;
  maxSupplierAmount: number;
  summary: AnalyticsSummary | null;
};

export function AnalyticsView({ maxEntityAmount, maxSupplierAmount, summary }: AnalyticsViewProps) {
  return (
    <section className="analytics-grid" aria-label="Аналітика">
      <div className="panel">
        <div className="panel-heading"><h2>Топ замовників за сумою контрактів</h2></div>
        <RankList items={summary?.topProcuringEntities ?? []} max={maxEntityAmount} nameKey="procuringEntityName" />
      </div>
      <div className="panel">
        <div className="panel-heading"><h2>Топ постачальників за сумою контрактів</h2></div>
        <RankList items={summary?.topSuppliers ?? []} max={maxSupplierAmount} nameKey="supplierName" />
      </div>
    </section>
  );
}