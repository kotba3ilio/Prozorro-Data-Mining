import type { AnalyticsSummary, ImportJobsStatus, View } from '../types';
import { formatMoney } from '../utils/formatters';
import { JobList } from './JobList';
import { MetricCard } from './MetricCard';
import { RankList } from './RankList';

type DashboardViewProps = {
  importStatus: ImportJobsStatus;
  maxEntityAmount: number;
  onViewChange: (view: View) => void;
  summary: AnalyticsSummary | null;
  tendersCount: number;
};

export function DashboardView({
  importStatus,
  maxEntityAmount,
  onViewChange,
  summary,
  tendersCount,
}: DashboardViewProps) {
  return (
    <section className="dashboard-grid" aria-label="Огляд">
      <MetricCard label="Економія" value={formatMoney(summary?.totalSavings ?? 0)} />
      <MetricCard label="Тендери" value={String(tendersCount)} />
      <MetricCard label="Активні задачі" value={String(importStatus.queuedCount + importStatus.runningCount)} />
      <div className="panel wide">
        <div className="panel-heading">
          <h2>Топ замовників</h2>
          <button type="button" onClick={() => onViewChange('analytics')}>Відкрити</button>
        </div>
        <RankList
          items={summary?.topProcuringEntities ?? []}
          max={maxEntityAmount}
          nameKey="procuringEntityName"
        />
      </div>
      <div className="panel">
        <div className="panel-heading">
          <h2>Останній імпорт</h2>
          <button type="button" onClick={() => onViewChange('import')}>Задачі</button>
        </div>
        <JobList jobs={importStatus.recentJobs.slice(0, 4)} compact />
      </div>
    </section>
  );
}