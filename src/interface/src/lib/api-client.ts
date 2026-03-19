const API_BASE =
  process.env.NEXT_PUBLIC_API_BASE ??
  process.env.NEXT_PUBLIC_API_HOST ??
  "http://localhost:8300";

export class ApiError extends Error {
  constructor(
    public status: number,
    message: string,
  ) {
    super(message);
  }
}

async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    headers: { "Content-Type": "application/json", ...options?.headers },
    ...options,
  });
  if (!res.ok) {
    const problem = await res.json().catch(() => ({ title: res.statusText }));
    throw new ApiError(res.status, problem.title ?? res.statusText);
  }
  return res.json() as Promise<T>;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  skip: number;
  take: number;
}

export const api = {
  get: <T>(
    path: string,
    params?: Record<
      string,
      string | number | boolean | string[] | undefined | null
    >,
  ) => {
    const qs = new URLSearchParams();
    for (const [k, v] of Object.entries(params ?? {})) {
      if (v === undefined || v === null) continue;
      if (Array.isArray(v)) {
        for (const item of v) qs.append(k, item);
      } else {
        qs.set(k, String(v));
      }
    }
    const qstr = qs.toString();
    return apiFetch<T>(qstr ? `${path}?${qstr}` : path);
  },
  post: <T>(path: string, body: unknown) =>
    apiFetch<T>(path, { method: "POST", body: JSON.stringify(body) }),
  put: <T>(path: string, body: unknown) =>
    apiFetch<T>(path, { method: "PUT", body: JSON.stringify(body) }),
  del: <T>(path: string) => apiFetch<T>(path, { method: "DELETE" }),
};
