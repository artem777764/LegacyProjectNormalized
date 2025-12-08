import { apiClient } from './client';

export type JwstItem = {
  caption?: string;
  inst?: string[];
  link?: string;
  obs?: string;
  program?: string;
  suffix?: string;
  url: string;
};

export type JwstFeedResponse = {
  source?: string;
  count?: number;
  items?: JwstItem[];
};

export type JwstFeedParams = {
  source?: string;
  suffix?: string;
  program?: string;
  instrument?: string;
  perPage?: number | string;
};

export async function getJwstFeed(params?: JwstFeedParams): Promise<JwstFeedResponse> {
  const resp = await apiClient.get<JwstFeedResponse>('/jwst/feed', { params });
  return resp.data;
}