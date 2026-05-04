import { useEffect, useRef } from "react";

type IntervalCallback = () => void;

export default function useInterval(
  callback: IntervalCallback,
  delay: number | null,
) {
  const savedCallback = useRef<IntervalCallback | null>(null);

  useEffect(() => {
    savedCallback.current = callback;
  }, [callback]);

  useEffect(() => {
    if (delay === null) {
      return;
    }

    function tick() {
      if (!savedCallback.current) {
        return;
      }
      savedCallback.current();
    }

    const id = setInterval(tick, delay);
    return () => clearInterval(id);
  }, [delay]);
}
