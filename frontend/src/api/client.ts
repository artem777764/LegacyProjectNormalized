import axios, { type AxiosInstance, type AxiosRequestConfig } from 'axios';

const API_BASE = "http://localhost:5022/"

function createClient(): AxiosInstance {
  const instance = axios.create({
    baseURL: API_BASE,
    timeout: 10000,
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
  });

  return instance;
}

export const apiClient = createClient();
export type { AxiosRequestConfig };