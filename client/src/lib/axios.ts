import axios from "axios";
import { useAuthStore } from "../store/authStore";

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true,
});

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

let isRefreshing = false;
let pendingQueue: Array<{
  resolve: (token: string) => void;
  reject: (err: unknown) => void;
}> = [];

function processQueue(token: string | null, err: unknown) {
  pendingQueue.forEach((p) => (token ? p.resolve(token) : p.reject(err)));
  pendingQueue = [];
}

api.interceptors.response.use(
  (r) => r,
  async (error) => {
    if (!error.config) return Promise.reject(error);

    if (
      error.response?.status !== 401 ||
      error.config?.url?.startsWith("/auth/")
    ) {
      if (error.response?.data?.detail) {
        error.message = error.response.data.detail;
      }
      return Promise.reject(error);
    }

    // if already refreshing, queue this request
    if (isRefreshing) {
      return new Promise<string>((resolve, reject) => {
        pendingQueue.push({ resolve, reject });
      }).then((token) => {
        error.config.headers.Authorization = `Bearer ${token}`;
        return api(error.config);
      });
    }

    isRefreshing = true;

    try {
      const { data } = await axios.post(
        `${import.meta.env.VITE_API_URL}/auth/refresh`,
        {},
        { withCredentials: true },
      );

      const newToken: string = data.accessToken;
      useAuthStore.getState().setAuth(newToken);
      processQueue(newToken, null);

      error.config.headers.Authorization = `Bearer ${newToken}`;
      return api(error.config);
    } catch (refreshError) {
      processQueue(null, refreshError);
      useAuthStore.getState().clearAuth();
      window.location.href = "/login";
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  },
);
