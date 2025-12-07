import type { IssTrend } from '@/models/IssTrend';
import { apiClient } from './client';
import type { AxiosRequestConfig } from 'axios';

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

export const endpoints = {
  getIssTrend
};

export default endpoints;