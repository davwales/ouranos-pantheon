const GATEWAY_PORT = 8300;

// Resolved per call so one image runs anywhere: the browser targets the gateway on the host that
// served the page, server rendering uses API_BASE from the container environment, and
// NEXT_PUBLIC_API_BASE / NEXT_PUBLIC_API_HOST (inlined at build time) override both.
function apiBase(): string {
  const override =
    process.env.NEXT_PUBLIC_API_BASE || process.env.NEXT_PUBLIC_API_HOST;
  if (override) {
    return override;
  }
  if (typeof window !== "undefined") {
    return `${window.location.protocol}//${window.location.hostname}:${GATEWAY_PORT}`;
  }
  return process.env.API_BASE || `http://localhost:${GATEWAY_PORT}`;
}

export function apiUrl(path: string): string {
  return `${apiBase()}${path}`;
}

export class ApiError extends Error {
  constructor(
    public status: number,
    message: string,
  ) {
    super(message);
  }
}

async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(apiUrl(path), {
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

export async function* streamSse<T>(
  path: string,
  body: unknown,
  signal?: AbortSignal,
): AsyncGenerator<T> {
  const res = await fetch(apiUrl(path), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
    signal,
  });

  if (!res.ok || !res.body) {
    throw new ApiError(res.status, `Streaming failed: ${res.statusText}`);
  }

  const reader = res.body.getReader();
  const decoder = new TextDecoder();
  let buffer = "";

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;
    buffer += decoder.decode(value, { stream: true });
    const lines = buffer.split("\n\n");
    buffer = lines.pop() ?? "";
    for (const line of lines) {
      if (line.startsWith("data: ")) {
        yield JSON.parse(line.slice(6)) as T;
      }
    }
  }
}

export const api = {
  get: <T>(
    path: string,
    params?: Record<
      string,
      string | number | boolean | string[] | undefined | null
    >,
    headers?: Record<string, string>,
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
    return apiFetch<T>(qstr ? `${path}?${qstr}` : path, { headers });
  },
  post: <T>(path: string, body?: unknown) =>
    apiFetch<T>(path, {
      method: "POST",
      body: body === undefined ? undefined : JSON.stringify(body),
    }),
  put: <T>(path: string, body: unknown) =>
    apiFetch<T>(path, { method: "PUT", body: JSON.stringify(body) }),
  patch: <T>(path: string, body: unknown) =>
    apiFetch<T>(path, { method: "PATCH", body: JSON.stringify(body) }),
  del: <T>(path: string, headers?: Record<string, string>) =>
    apiFetch<T>(path, { method: "DELETE", headers }),
};
