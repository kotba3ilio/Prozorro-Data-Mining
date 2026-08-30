export type View = 'dashboard' | 'import' | 'analytics' | 'tenders';
export type ImportJobStatus = 'Queued' | 'Running' | 'Completed' | 'Failed' | string | number;
export type StartImportDirection = 'Backward' | 'Forward';
export type ImportDirection = StartImportDirection | string | number;
export type TenderStatus = number | string;

export type ImportTendersResponse = {
  direction: ImportDirection;
  feedItemsScanned: number;
  candidatesFound: number;
  importedCount: number;
  updatedCount: number;
  skippedCount: number;
  isCompleted: boolean;
  nextPageUri?: string | null;
  prevPageUri?: string | null;
};

export type TenderImportJob = {
  jobId: string;
  status: ImportJobStatus;
  requestDirection: ImportDirection;
  createdAt: string;
  startedAt: string | null;
  completedAt: string | null;
  result: ImportTendersResponse | null;
  errorMessage: string | null;
};

export type ImportJobsStatus = {
  hasActiveJobs: boolean;
  queuedCount: number;
  runningCount: number;
  activeJob: TenderImportJob | null;
  recentJobs: TenderImportJob[];
};

export type AnalyticsSummary = {
  totalSavings: number;
  topProcuringEntities: TopProcuringEntity[];
  topSuppliers: TopSupplier[];
};

export type TopProcuringEntity = {
  procuringEntityName: string;
  contractAmount: number;
  tendersCount: number;
};

export type TopSupplier = {
  supplierName: string;
  contractAmount: number;
  tendersCount: number;
};

export type TenderListItem = {
  id: string;
  prozorroId: string;
  status: TenderStatus;
  dateCreated: string | null;
  procuringEntityName: string;
  expectedAmount: number;
  contractAmount: number;
  currency: string | null;
  suppliers: string[];
};

export type TenderDetails = Omit<TenderListItem, 'suppliers'> & {
  importedAt: string;
  updatedAt: string | null;
  items: Array<{ classificationId: string; description: string | null }>;
  contracts: Array<{
    prozorroContractId: string | null;
    awardId: string;
    amount: number;
    currency: string | null;
    dateSigned: string | null;
  }>;
  suppliers: Array<{
    name: string;
    identifierScheme: string | null;
    identifierId: string | null;
    awardId: string | null;
  }>;
};

export type CursorPagedResponse<T> = {
  items: T[];
  pageSize: number;
  nextCursor: string | null;
  hasNextPage: boolean;
};

export type Filters = {
  classificationId: string;
  createdFrom: string;
  createdTo: string;
  limit: number;
};