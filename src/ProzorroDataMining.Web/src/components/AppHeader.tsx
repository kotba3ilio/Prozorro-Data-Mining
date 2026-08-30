import type { View } from '../types';

type AppHeaderProps = {
  activeLabel: string;
  hasActiveJobs: boolean;
  isLoading: boolean;
  view: View;
  onViewChange: (view: View) => void;
};

const tabs: Array<{ key: View; label: string }> = [
  { key: 'dashboard', label: 'Огляд' },
  { key: 'import', label: 'Імпорт' },
  { key: 'analytics', label: 'Аналітика' },
  { key: 'tenders', label: 'Тендери' },
];

export function AppHeader({ activeLabel, hasActiveJobs, isLoading, view, onViewChange }: AppHeaderProps) {
  return (
    <>
      <header className="page-header">
        <div>
          <p className="eyebrow">Prozorro Data Mining</p>
          <h1>Імпорт, тендери та аналітика закупівель</h1>
        </div>
        <div className={hasActiveJobs ? 'status-pill active' : 'status-pill'}>
          <span aria-hidden="true" />
          {isLoading ? 'Завантаження' : activeLabel}
        </div>
      </header>

      <nav className="tabs" aria-label="Розділи">
        {tabs.map((tab) => (
          <button
            className={view === tab.key ? 'tab active' : 'tab'}
            key={tab.key}
            type="button"
            onClick={() => onViewChange(tab.key)}
          >
            {tab.label}
          </button>
        ))}
      </nav>
    </>
  );
}