"use client";

import { useCallback, useEffect, useRef, useState } from "react";

const DEFAULT_THRESHOLD_PX = 48;

/**
 * Keeps the nearest scrollable ancestor of `contentRef` pinned to the bottom while the content
 * grows, but only while the user is at (or within `threshold` of) the bottom. Scrolling up
 * detaches; scrolling back down, or calling `scrollToBottom`, re-attaches.
 */
export function useStickToBottom<T extends HTMLElement>({
  threshold = DEFAULT_THRESHOLD_PX,
}: { threshold?: number } = {}) {
  const contentRef = useRef<T | null>(null);
  const scrollerRef = useRef<HTMLElement | null>(null);
  const isStuckRef = useRef(true);
  const [isAtBottom, setIsAtBottom] = useState(true);

  const setStuck = useCallback((isStuck: boolean) => {
    isStuckRef.current = isStuck;
    setIsAtBottom(isStuck);
  }, []);

  const scrollToBottom = useCallback(() => {
    setStuck(true);
    const scroller = scrollerRef.current;
    if (scroller) {
      scroller.scrollTop = scroller.scrollHeight;
    }
  }, [setStuck]);

  useEffect(() => {
    const content = contentRef.current;
    if (!content) {
      return;
    }

    const scroller = findScrollParent(content);
    scrollerRef.current = scroller;
    const scrollEvents: HTMLElement | Window =
      scroller === document.scrollingElement ? window : scroller;
    let lastTop = scroller.scrollTop;

    const handleScroll = () => {
      const top = scroller.scrollTop;
      const distance = scroller.scrollHeight - top - scroller.clientHeight;
      // Any upward move detaches, even inside the threshold, so a slow drag can escape a
      // streaming reply; shrinking content that clamps the scroll position keeps it at the bottom.
      const isMovingUp = top < lastTop;
      lastTop = top;
      setStuck(isMovingUp ? distance < 1 : distance <= threshold);
    };

    const follow = () => {
      if (isStuckRef.current) {
        scroller.scrollTop = scroller.scrollHeight;
        lastTop = scroller.scrollTop;
      }
    };

    scrollEvents.addEventListener("scroll", handleScroll, { passive: true });
    const observer = new ResizeObserver(follow);
    observer.observe(content);
    observer.observe(scroller);

    return () => {
      scrollEvents.removeEventListener("scroll", handleScroll);
      observer.disconnect();
      scrollerRef.current = null;
    };
  }, [threshold, setStuck]);

  return { contentRef, isAtBottom, scrollToBottom };
}

function findScrollParent(element: HTMLElement): HTMLElement {
  for (let parent = element.parentElement; parent; parent = parent.parentElement) {
    const { overflowY } = getComputedStyle(parent);
    if (overflowY === "auto" || overflowY === "scroll" || overflowY === "overlay") {
      return parent;
    }
  }

  return (document.scrollingElement as HTMLElement | null) ?? document.documentElement;
}
