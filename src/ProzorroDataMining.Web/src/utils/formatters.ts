import type { ImportDirection, ImportJobStatus, TenderStatus } from '../types';

export function formatDate(value: string | null | undefined) {
  if (!value) {
    return '-';
  }

  return new Intl.DateTimeFormat('uk-UA', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(value));
}

export function formatMoney(value: number | null | undefined, currency = 'UAH') {
  if (value === null || value === undefined) {
    return '-';
  }

  return new Intl.NumberFormat('uk-UA', {
    style: 'currency',
    currency,
    maximumFractionDigits: 0,
  }).format(value);
}

export function normalizeJobStatus(status: ImportJobStatus) {
  const numericLabels: Record<number, 'Queued' | 'Running' | 'Completed' | 'Failed' | 'Unknown'> = {
    1: 'Queued',
    2: 'Running',
    3: 'Completed',
    4: 'Failed',
  };

  if (typeof status === 'number') {
    return numericLabels[status] ?? 'Unknown';
  }

  if (status === 'Queued' || status === 'Running' || status === 'Completed' || status === 'Failed') {
    return status;
  }

  return 'Unknown';
}

export function statusLabel(status: ImportJobStatus) {
  switch (normalizeJobStatus(status)) {
    case 'Queued':
      return 'У черзі';
    case 'Running':
      return 'Виконується';
    case 'Completed':
      return 'Завершено';
    case 'Failed':
      return 'Помилка';
    default:
      return 'Невідомо';
  }
}

export function directionLabel(direction: ImportDirection | null | undefined) {
  if (direction === 1 || direction === 'Backward') {
    return 'Імпорт назад';
  }

  if (direction === 2 || direction === 'Forward') {
    return 'Синхронізація вперед';
  }

  return '-';
}

export function tenderStatusLabel(status: TenderStatus) {
  if (typeof status === 'string') {
    return status;
  }

  const labels: Record<number, string> = {
    0: 'Невідомо',
    1: 'Чернетка',
    2: 'Активний',
    3: 'Уточнення',
    4: 'Подання пропозицій',
    5: 'Прекваліфікація',
    6: 'Період оскарження',
    7: 'Аукціон',
    8: 'Кваліфікація',
    9: 'Визначено переможця',
    10: 'Очікує другий етап',
    11: 'Неуспішний',
    12: 'Завершено',
    13: 'Скасовано',
  };

  return labels[status] ?? String(status);
}

export function relativeWidth(value: number, max: number) {
  if (max <= 0) {
    return '0%';
  }

  return Math.max(6, Math.round((value / max) * 100)) + '%';
}