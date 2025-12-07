export interface OsdrItem {
  id: number;
  datasetId: string;
  title: string;
  status: string;
  fetchedAt: string;
  updatedAt: string;
  payload: {
    REST_URL: string;
  };
}