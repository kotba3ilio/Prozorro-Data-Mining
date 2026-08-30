import { FormEvent, useEffect, useMemo, useRef, useState } from 'react';
import { apiClient } from './api/client';
import { AnalyticsView } from './components/AnalyticsView';
import { AppHeader } from './components/AppHeader';
import { DashboardView } from './components/DashboardView';
import { FiltersBar } from './components/FiltersBar';
import { ImportView } from './components/ImportView';
import { TendersView } from './components/TendersView';
import type {
  AnalyticsSummary,
  CursorPagedResponse,
  Filters,
  ImportJobsStatus,
  TenderDetails,
  TenderListItem,
  View,
} from './types';

const defaultFilters: Filters = {
  classificationId: '09310000-5',
  createdFrom: '2025-12-01',
  createdTo: '2026-01-01',
  limit: 10,
};

const emptyStatus: ImportJobsStatus = {
  hasActiveJobs: false,
  queuedCount: 0,
  runningCount: 0,
  activeJob: null,
  recentJobs: [],
};

export function App() {
  const [view, setView] = useState<View>('dashboard');
  const [filters, setFilters] = useState<Filters>(defaultFilters);
  const [appliedFilters, setAppliedFilters] = useState<Filters>(defaultFilters);
  const [importStatus, setImportStatus] = useState<ImportJobsStatus>(emptyStatus);
  const [summary, setSummary] = useState<AnalyticsSummary | null>(null);
  const [tenders, setTenders] = useState<CursorPagedResponse<TenderListItem> | null>(null);
  const [selectedTender, setSelectedTender] = useState<TenderDetails | null>(null);
  const [selectedTenderId, setSelectedTenderId] = useState<string | null>(null);
  const [isTenderDetailsLoading, setIsTenderDetailsLoading] = useState(false);
  const [tenderDetailsError, setTenderDetailsError] = useState<string | null>(null);
  const [tenderCursorStack, setTenderCursorStack] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isStarting, setIsStarting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const selectedTenderRequestId = useRef(0);

  const currentTenderCursor = tenderCursorStack.length > 0
    ? tenderCursorStack[tenderCursorStack.length - 1]
    : null;
  const currentTenderPage = tenderCursorStack.length + 1;

  const activeLabel = useMemo(() => {
    if (importStatus.runningCount > 0) {
      return 'Імпорт виконується';
    }

    if (importStatus.queuedCount > 0) {
      return 'Імпорт очікує в черзі';
    }

    return 'Фонових задач немає';
  }, [importStatus.queuedCount, importStatus.runningCount]);

  const maxEntityAmount = Math.max(
    0,
    ...(summary?.topProcuringEntities.map((item) => item.contractAmount) ?? []),
  );
  const maxSupplierAmount = Math.max(
    0,
    ...(summary?.topSuppliers.map((item) => item.contractAmount) ?? []),
  );

  async function loadImportStatus(signal?: AbortSignal) {
    setImportStatus(await apiClient.getImportStatus(signal));
  }

  async function loadAll(signal?: AbortSignal) {
    setIsLoading(true);
    setError(null);

    try {
      const [nextImportStatus, nextSummary, nextTenders] = await Promise.all([
        apiClient.getImportStatus(signal),
        apiClient.getSummary(appliedFilters, signal),
        apiClient.getTenders(appliedFilters, currentTenderCursor, appliedFilters.limit, signal),
      ]);

      setImportStatus(nextImportStatus);
      setSummary(nextSummary);
      setTenders(nextTenders);
    } catch (exception) {
      if (!signal?.aborted) {
        setError(exception instanceof Error ? exception.message : 'Не вдалося прочитати дані');
      }
    } finally {
      if (!signal?.aborted) {
        setIsLoading(false);
      }
    }
  }

  function changeView(nextView: View) {
    setView(nextView);

    if (nextView === 'import') {
      loadImportStatus().catch((exception) => {
        setError(exception instanceof Error ? exception.message : 'Не вдалося оновити статус імпорту');
      });
      return;
    }

    const controller = new AbortController();
    loadAll(controller.signal);
  }

  async function startImport() {
    setIsStarting(true);
    setError(null);

    try {
      await apiClient.startImport(appliedFilters, "Backward");
      await loadImportStatus();
      setView('import');
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не вдалося створити задачу');
    } finally {
      setIsStarting(false);
    }
  }

  async function selectTender(tenderId: string) {
    const requestId = selectedTenderRequestId.current + 1;
    selectedTenderRequestId.current = requestId;
    setSelectedTenderId(tenderId);
    setSelectedTender(null);
    setTenderDetailsError(null);
    setIsTenderDetailsLoading(true);

    try {
      const tender = await apiClient.getTender(tenderId);

      if (selectedTenderRequestId.current === requestId) {
        setSelectedTender(tender);
      }
    } catch (exception) {
      if (selectedTenderRequestId.current === requestId) {
        setTenderDetailsError(exception instanceof Error ? exception.message : 'Не вдалося відкрити тендер');
      }
    } finally {
      if (selectedTenderRequestId.current === requestId) {
        setIsTenderDetailsLoading(false);
      }
    }
  }

  function resetSelectedTender() {
    selectedTenderRequestId.current += 1;
    setSelectedTender(null);
    setSelectedTenderId(null);
    setTenderDetailsError(null);
    setIsTenderDetailsLoading(false);
  }

  function applyFilters(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    resetSelectedTender();
    setTenderCursorStack([]);
    setAppliedFilters(filters);
  }

  function nextTenderPage() {
    if (!tenders?.nextCursor) {
      return;
    }

    resetSelectedTender();
    setTenderCursorStack((cursors) => [...cursors, tenders.nextCursor!]);
  }

  function previousTenderPage() {
    resetSelectedTender();
    setTenderCursorStack((cursors) => cursors.slice(0, -1));
  }

  useEffect(() => {
    const controller = new AbortController();
    loadAll(controller.signal);

    return () => controller.abort();
  }, [appliedFilters, currentTenderCursor]);

  useEffect(() => {
    const intervalId = window.setInterval(() => {
      loadImportStatus().catch((exception) => {
        setError(exception instanceof Error ? exception.message : 'Не вдалося оновити статус імпорту');
      });
    }, 5000);

    return () => window.clearInterval(intervalId);
  }, []);

  return (
    <main className="app-shell">
      <AppHeader
        activeLabel={activeLabel}
        hasActiveJobs={importStatus.hasActiveJobs}
        isLoading={isLoading}
        view={view}
        onViewChange={changeView}
      />

      <FiltersBar filters={filters} onChange={setFilters} onSubmit={applyFilters} />

      {error ? <p className="error-message">Помилка запиту: {error}</p> : null}

      {view === 'dashboard' ? (
        <DashboardView
          importStatus={importStatus}
          maxEntityAmount={maxEntityAmount}
          summary={summary}
          tendersCount={tenders?.items.length ?? 0}
          onViewChange={changeView}
        />
      ) : null}

      {view === 'import' ? (
        <ImportView
          importStatus={importStatus}
          isStarting={isStarting}
          onStartImport={startImport}
        />
      ) : null}

      {view === 'analytics' ? (
        <AnalyticsView
          maxEntityAmount={maxEntityAmount}
          maxSupplierAmount={maxSupplierAmount}
          summary={summary}
        />
      ) : null}

      {view === 'tenders' ? (
        <TendersView
          currentPage={currentTenderPage}
          isTenderDetailsLoading={isTenderDetailsLoading}
          selectedTender={selectedTender}
          selectedTenderId={selectedTenderId}
          tenderDetailsError={tenderDetailsError}
          tenders={tenders}
          onNextPage={nextTenderPage}
          onPreviousPage={previousTenderPage}
          onSelectTender={selectTender}
        />
      ) : null}
    </main>
  );
}