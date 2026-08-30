import type { ImportJobStatus } from '../types';
import { normalizeJobStatus, statusLabel } from '../utils/formatters';

type JobBadgeProps = {
  status: ImportJobStatus;
};

export function JobBadge({ status }: JobBadgeProps) {
  const normalizedStatus = normalizeJobStatus(status);

  return <span className={'job-status ' + normalizedStatus.toLowerCase()}>{statusLabel(status)}</span>;
}