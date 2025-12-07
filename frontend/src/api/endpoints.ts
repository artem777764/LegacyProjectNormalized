import type { IssTrend } from '@/models/IssTrend';
import { apiClient } from './client';
import type { AxiosRequestConfig } from 'axios';
import type { OsdrItem } from '@/models/OsdrItem';

export type ApiResponse<T> = {
  data: T;
};

export async function getJson<T = unknown>(
  url: string,
  config?: AxiosRequestConfig
): Promise<T> {
  const resp = await apiClient.get<T>(url, config);
  return resp.data;
}

export async function getIssTrend(): Promise<IssTrend> {
  return getJson<IssTrend>('/Iss/trend');
}

export async function getOsdrListAll(): Promise<OsdrItem[]> {
  const data = await getJson<OsdrItem[]>('/Osdr/list/all');
  return data.map(item => ({
    ...item,
    payload: typeof item.payload === 'string' ? JSON.parse(item.payload) : item.payload
  }));
}

export const endpoints = {
  getIssTrend,
  getOsdrListAll
};

export default endpoints;