import type { FormEvent } from 'react';
import type { Filters } from '../types';

type FiltersBarProps = {
  filters: Filters;
  onChange: (filters: Filters) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
};

export function FiltersBar({ filters, onChange, onSubmit }: FiltersBarProps) {
  return (
    <form className="filters" onSubmit={onSubmit}>
      <label>
        <span>CPV</span>
        <input
          value={filters.classificationId}
          onChange={(event) => onChange({ ...filters, classificationId: event.target.value })}
        />
      </label>
      <label>
        <span>Дата від</span>
        <input
          type="date"
          value={filters.createdFrom}
          onChange={(event) => onChange({ ...filters, createdFrom: event.target.value })}
        />
      </label>
      <label>
        <span>Дата до</span>
        <input
          type="date"
          value={filters.createdTo}
          onChange={(event) => onChange({ ...filters, createdTo: event.target.value })}
        />
      </label>
      <label>
        <span>Ліміт</span>
        <input
          min={1}
          max={50}
          type="number"
          value={filters.limit}
          onChange={(event) => onChange({ ...filters, limit: Number(event.target.value) })}
        />
      </label>
      <button type="submit">Застосувати</button>
    </form>
  );
}