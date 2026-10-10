import coreWebVitalsConfig from "eslint-config-next/core-web-vitals";

const eslintConfig = [
  ...coreWebVitalsConfig,
  {
    // TanStack Table v8 is incompatible with the React Compiler. The affected
    // components already carry a "use no memo" directive so the compiler skips
    // them. The rule still fires at the call-site regardless of that directive,
    // so we disable it here until TanStack Table adds React Compiler support.
    rules: {
      "react-hooks/incompatible-library": "off",
    },
  },
  {
    // eslint-plugin-react-hooks 7.1 flags pre-existing patterns (mostly dialogs
    // resetting form state on open). Kept visible as warnings until those
    // components are refactored, rather than blocking the dependency upgrade.
    rules: {
      "react-hooks/set-state-in-effect": "warn",
      "react-hooks/static-components": "warn",
      "react-hooks/preserve-manual-memoization": "warn",
    },
  },
];

export default eslintConfig;
