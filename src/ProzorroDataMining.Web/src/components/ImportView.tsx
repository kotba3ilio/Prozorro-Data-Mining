import type { ImportJobsStatus } from "../types";
import { JobBadge } from "./JobBadge";
import { JobDetails } from "./JobDetails";
import { JobList } from "./JobList";
import { MetricCard } from "./MetricCard";

type ImportViewProps = {
  importStatus: ImportJobsStatus;
  isStarting: boolean;
  onStartImport: () => void;
};

export function ImportView({
  importStatus,
  isStarting,
  onStartImport,
}: ImportViewProps) {
  return (
    <section className="stack" aria-label="Імпорт">
      <div className="toolbar" aria-label="Керування імпортом">
        <button type="button" onClick={onStartImport} disabled={isStarting}>
          Оновити
        </button>
      </div>
      <section className="status-grid" aria-label="Стан фонових задач">
        <MetricCard label="У черзі" value={String(importStatus.queuedCount)} />
        <MetricCard
          label="Виконується"
          value={String(importStatus.runningCount)}
        />
        <MetricCard
          label="Останні задачі"
          value={String(importStatus.recentJobs.length)}
        />
      </section>
      <div className="panel">
        <div className="panel-heading">
          <h2>Поточна фонова задача</h2>
          {importStatus.activeJob ? (
            <JobBadge status={importStatus.activeJob.status} />
          ) : null}
        </div>
        {importStatus.activeJob ? (
          <JobDetails job={importStatus.activeJob} />
        ) : (
          <p className="empty-state">
            Немає задач у статусі «у черзі» або «виконується».
          </p>
        )}
      </div>
      <div className="panel">
        <div className="panel-heading">
          <h2>Останні задачі</h2>
        </div>
        <JobList jobs={importStatus.recentJobs} />
      </div>
    </section>
  );
}