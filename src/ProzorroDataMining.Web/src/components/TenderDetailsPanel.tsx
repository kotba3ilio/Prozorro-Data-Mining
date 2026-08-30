import type { TenderDetails } from '../types';
import { formatDate, formatMoney, tenderStatusLabel } from '../utils/formatters';

type TenderDetailsPanelProps = {
  error: string | null;
  isLoading: boolean;
  selectedTenderId: string | null;
  tender: TenderDetails | null;
};

export function TenderDetailsPanel({ error, isLoading, selectedTenderId, tender }: TenderDetailsPanelProps) {
  return (
    <aside className="panel detail-panel">
      <div className="panel-heading"><h2>Деталі тендера</h2></div>
      {!selectedTenderId ? <p className="empty-state">Обери тендер у списку.</p> : null}
      {selectedTenderId && isLoading ? <p className="empty-state">Завантаження деталей...</p> : null}
      {selectedTenderId && error ? <p className="error-message">Не вдалося завантажити деталі: {error}</p> : null}
      {selectedTenderId && !isLoading && !error && !tender ? <p className="empty-state">Деталі тендера не знайдено.</p> : null}
      {tender && !isLoading ? (
        <div className="detail-stack">
          <div>
            <span className="muted">ID Prozorro</span>
            <h3>{tender.prozorroId}</h3>
          </div>
          <div className="detail-grid">
            <div><span>Статус</span><strong>{tenderStatusLabel(tender.status)}</strong></div>
            <div><span>Створено</span><strong>{formatDate(tender.dateCreated)}</strong></div>
            <div><span>Очікувана</span><strong>{formatMoney(tender.expectedAmount, tender.currency ?? 'UAH')}</strong></div>
            <div><span>Контракти</span><strong>{formatMoney(tender.contractAmount, tender.currency ?? 'UAH')}</strong></div>
          </div>
          <div>
            <h3>Постачальники</h3>
            <ul className="plain-list">
              {tender.suppliers.map((supplier, index) => (
                <li key={supplier.name + index}>{supplier.name}<span>{supplier.identifierId ?? supplier.awardId ?? ''}</span></li>
              ))}
            </ul>
          </div>
          <div>
            <h3>Предмети</h3>
            <ul className="plain-list">
              {tender.items.map((item, index) => (
                <li key={item.classificationId + index}>{item.classificationId}<span>{item.description ?? ''}</span></li>
              ))}
            </ul>
          </div>
        </div>
      ) : null}
    </aside>
  );
}