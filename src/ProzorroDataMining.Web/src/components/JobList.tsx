import type { TenderImportJob } from '../types';
import { directionLabel, formatDate } from '../utils/formatters';
import { JobBadge } from './JobBadge';

type JobListProps = {
  jobs: TenderImportJob[];
  compact?: boolean;
};

export function JobList({ jobs, compact = false }: JobListProps) {
  if (jobs.length === 0) {
    return <p className="empty-state">Задачі імпорту ще не створювались.</p>;
  }

  return (
    <div className={compact ? 'jobs-list compact' : 'jobs-list'}>
      {jobs.map((job) => (
        <div className="job-entry" key={job.jobId}>
          <div className="job-row">
            <JobBadge status={job.status} />
            <span>{directionLabel(job.result?.direction ?? job.requestDirection)}</span>
            <span>{job.result?.feedItemsScanned ?? '-'}</span>
            <span>{job.result?.importedCount ?? '-'}</span>
            <span>{job.result?.updatedCount ?? '-'}</span>
            <span>{formatDate(job.completedAt)}</span>
          </div>
          {job.errorMessage ? (
            <div className="job-row-error" role="alert">
              <span>Помилка імпорту</span>
              <strong>{job.errorMessage}</strong>
            </div>
          ) : null}
        </div>
      ))}
    </div>
  );
}