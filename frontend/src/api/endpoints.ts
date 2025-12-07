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

export const endpoints = {
  
};

export default endpoints;