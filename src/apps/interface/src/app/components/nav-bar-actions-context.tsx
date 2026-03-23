"use client";

import React, { createContext, useCallback, useContext, useState } from "react";

interface NavBarActionsContextValue {
  actions: React.ReactNode;
  setActions: (actions: React.ReactNode) => void;
  clearActions: () => void;
}

const NavBarActionsContext = createContext<NavBarActionsContextValue>({
  actions: null,
  setActions: () => {},
  clearActions: () => {},
});

export function NavBarActionsProvider({ children }: React.PropsWithChildren) {
  const [actions, setActionsState] = useState<React.ReactNode>(null);

  const setActions = useCallback((node: React.ReactNode) => {
    setActionsState(node);
  }, []);

  const clearActions = useCallback(() => {
    setActionsState(null);
  }, []);

  return (
    <NavBarActionsContext.Provider
      value={{ actions, setActions, clearActions }}
    >
      {children}
    </NavBarActionsContext.Provider>
  );
}

export function useNavBarActions() {
  return useContext(NavBarActionsContext);
}
