import type {
  AnalyticsSummary,
  Filters,
  StartImportDirection,
  ImportJobsStatus,
  CursorPagedResponse,
  TenderDetails,
  TenderImportJob,
  TenderListItem,
} from "../types";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "";

async function apiGet<T>(path: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(apiBaseUrl + path, { signal });

  if (!response.ok) {
    throw new Error("Помилка HTTP " + response.status);
  }

  return response.json() as Promise<T>;
}

async function apiPost<T>(path: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(apiBaseUrl + path, { method: "POST", signal });

  if (!response.ok) {
    throw new Error("Помилка HTTP " + response.status);
  }

  return response.json() as Promise<T>;
}

export function buildQuery(
  filters: Filters,
  extras: Record<string, string | number | null | undefined> = {},
) {
  const query = new URLSearchParams({
    classificationId: filters.classificationId,
    createdFrom: filters.createdFrom,
    createdTo: filters.createdTo,
    limit: String(filters.limit),
  });

  Object.entries(extras).forEach(([key, value]) => {
    if (value !== null && value !== undefined && value !== "") {
      query.set(key, String(value));
    }
  });

  return "?" + query.toString();
}

export const apiClient = {
  getImportStatus(signal?: AbortSignal) {
    return apiGet<ImportJobsStatus>(
      "/api/import/tenders/jobs/status?limit=10",
      signal,
    );
  },
  getSummary(filters: Filters, signal?: AbortSignal) {
    return apiGet<AnalyticsSummary>(
      "/api/analytics/summary" + buildQuery(filters),
      signal,
    );
  },
  getTenders(
    filters: Filters,
    cursor: string | null,
    pageSize: number,
    signal?: AbortSignal,
  ) {
    return apiGet<CursorPagedResponse<TenderListItem>>(
      "/api/tenders/list" + buildQuery(filters, { cursor, pageSize }),
      signal,
    );
  },
  getTender(tenderId: string, signal?: AbortSignal) {
    return apiGet<TenderDetails>("/api/tenders/" + tenderId, signal);
  },
  startImport(
    filters: Filters,
    direction: StartImportDirection,
    signal?: AbortSignal,
  ) {
    return apiPost<TenderImportJob>(
      "/api/import/tenders" +
        buildQuery(filters, { direction, maxPages: 7500, pageSize: 500 }),
      signal,
    );
  },
};
