import type { TenderImportJob } from '../types';
import { directionLabel, formatDate } from '../utils/formatters';

type JobDetailsProps = {
  job: TenderImportJob;
};

export function JobDetails({ job }: JobDetailsProps) {
  return (
    <div className="job-details">
      <div><span>ID задачі</span><strong>{job.jobId}</strong></div>
      <div><span>Напрям</span><strong>{directionLabel(job.requestDirection)}</strong></div>
      <div><span>Старт</span><strong>{formatDate(job.startedAt)}</strong></div>
      <div><span>Переглянуто</span><strong>{job.result?.feedItemsScanned ?? '-'}</strong></div>
      <div><span>Імпортовано</span><strong>{job.result?.importedCount ?? '-'}</strong></div>
      <div><span>Оновлено</span><strong>{job.result?.updatedCount ?? '-'}</strong></div>
      {job.errorMessage ? <div className="job-error"><span>Помилка</span><strong>{job.errorMessage}</strong></div> : null}
    </div>
  );
}