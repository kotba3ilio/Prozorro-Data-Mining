import type { CursorPagedResponse, TenderDetails, TenderListItem } from '../types';
import { formatMoney, tenderStatusLabel } from '../utils/formatters';
import { TenderDetailsPanel } from './TenderDetailsPanel';

type TendersViewProps = {
  currentPage: number;
  isTenderDetailsLoading: boolean;
  selectedTender: TenderDetails | null;
  selectedTenderId: string | null;
  tenderDetailsError: string | null;
  tenders: CursorPagedResponse<TenderListItem> | null;
  onNextPage: () => void;
  onPreviousPage: () => void;
  onSelectTender: (tenderId: string) => void;
};

export function TendersView({
  currentPage,
  isTenderDetailsLoading,
  selectedTender,
  selectedTenderId,
  tenderDetailsError,
  tenders,
  onNextPage,
  onPreviousPage,
  onSelectTender,
}: TendersViewProps) {
  return (
    <section className="tenders-layout" aria-label="Тендери">
      <div className="panel">
        <div className="panel-heading">
          <h2>Тендери</h2>
          <span className="muted">{tenders?.items.length ?? 0} записів на сторінці</span>
        </div>
        <div className="tender-list">
          {(tenders?.items ?? []).map((tender) => (
            <button
              className={selectedTenderId === tender.id ? 'tender-row selected' : 'tender-row'}
              key={tender.id}
              type="button"
              onClick={() => onSelectTender(tender.id)}
            >
              <span>
                <strong>{tender.procuringEntityName}</strong>
              </span>
              <span>{formatMoney(tender.contractAmount, tender.currency ?? 'UAH')}</span>
              <span>{tenderStatusLabel(tender.status)}</span>
            </button>
          ))}
          {(tenders?.items.length ?? 0) === 0 ? <p className="empty-state">За поточними фільтрами тендерів немає.</p> : null}
        </div>
        <div className="pagination">
          <button type="button" disabled={currentPage <= 1} onClick={onPreviousPage}>Назад</button>
          <span>Сторінка {currentPage}</span>
          <button type="button" disabled={!tenders?.hasNextPage} onClick={onNextPage}>Далі</button>
        </div>
      </div>
      <TenderDetailsPanel
        error={tenderDetailsError}
        isLoading={isTenderDetailsLoading}
        tender={selectedTender}
        selectedTenderId={selectedTenderId}
      />
    </section>
  );
}