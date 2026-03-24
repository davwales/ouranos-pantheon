"use client";

import React, { useCallback, useEffect, useState } from "react";

export type ApiState<T> =
  | { status: "idle"; data: undefined }
  | { status: "loading"; data: T | undefined }
  | { status: "success"; data: T }
  | { status: "error"; data: T | undefined; error: Error };

export function useApi<T>(
  fetcher: () => Promise<T>,
  deps: React.DependencyList,
): [ApiState<T>, () => void] {
  const [state, setState] = useState<ApiState<T>>({
    status: "idle",
    data: undefined,
  });

  const execute = useCallback(() => {
    setState((prev) => ({ status: "loading", data: prev.data }));
    fetcher()
      .then((data) => setState({ status: "success", data }))
      .catch((error) =>
        setState((prev) => ({ status: "error", data: prev.data, error })),
      );
    // eslint-disable-next-line react-hooks/use-memo
  }, deps);

  useEffect(() => {
    execute();
  }, [execute]);

  return [state, execute];
}
