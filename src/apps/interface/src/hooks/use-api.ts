"use client";

import React, { useCallback, useEffect, useRef, useState } from "react";

export type ApiState<T> =
  | { status: "idle"; data: undefined }
  | { status: "loading"; data: T | undefined }
  | { status: "success"; data: T }
  | { status: "error"; data: T | undefined; error: Error };

export function useApi<T>(
  fetcher: () => Promise<T>,
  deps: React.DependencyList = [],
): [ApiState<T>, () => void] {
  const fetcherRef = useRef(fetcher);
  const [executeCount, setExecuteCount] = useState(0);
  const depsKey = JSON.stringify(deps);

  const [state, setState] = useState<ApiState<T>>({
    status: "loading",
    data: undefined,
  });

  useEffect(() => {
    fetcherRef.current = fetcher;
  });

  const execute = useCallback(() => {
    setState((prev) => ({ ...prev, status: "loading" }));
    setExecuteCount((c) => c + 1);
  }, []);

  useEffect(() => {
    let cancelled = false;
    fetcherRef
      .current()
      .then((data) => {
        if (!cancelled) setState({ status: "success", data });
      })
      .catch((error) => {
        if (!cancelled)
          setState((prev) => ({ status: "error", data: prev.data, error }));
      });
    return () => {
      cancelled = true;
    };
  }, [depsKey, executeCount]);

  return [state, execute];
}
