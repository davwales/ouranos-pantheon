import coreWebVitalsConfig from "eslint-config-next/core-web-vitals";

export default [
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
];
