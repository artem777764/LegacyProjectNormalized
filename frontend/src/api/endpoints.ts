import type { AxiosRequestConfig } from 'axios';
import { apiClient } from './client';
import type { IssTrend } from '@/models/IssTrend';
import type { OsdrItem } from '@/models/OsdrItem';
import type { AstroResponse } from '@/models/AstroEvent';

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

export async function getAstroEvents(
  lat = 55.7558,
  lon = 37.6176,
  days = 7
): Promise<AstroResponse> {
  return getJson<AstroResponse>(`/Astro/events?lat=${lat}&lon=${lon}&days=${days}`);
}

export const endpoints = {
  getIssTrend,
  getOsdrListAll,
  getAstroEvents
};

export default endpoints;