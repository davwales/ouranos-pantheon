# Pantheon Interface

## Domain Overview

The intent of this module is to provide the user interface for the various Pantheon modules. At time of writing this includes Hermes to have conversations with various user-created characters and Plutus which allows users to explore market data to find potential investments.

The intention for the project structure is to promote development as a modular monolith, where common components may be created at the root and then each module can define their own pages and layouts within module directories. As the Pantheon Gateway is the primary API supplying this interface it utilizes urql to execute GraphQL queries and mutations.

## Module Structure

```
src/
  └── app/
      ├── components/
      ├── module-a/
      │   ├── components/
      │   ├── queries.tsx
      │   ├── mutations.tsx
      │   ├── layout.tsx
      │   └── page.tsx
      └── module-b/
```

## Getting Started

1. Prerequisites
    1. Ensure the schema url is properly configured in `codegen.ts`.
    2. Run `npm run codegen` to generate the GraphQL types.
2. Configuration
    1. Set the `NEXT_PUBLIC_API_HOST` variable to your hosted Pantheon API.
3. Running the Application
    1. Execute `npm run dev` to start the application and view your changes.

## Dependencies

- Pantheon Gateway
- TypeScript
- NextJS
- urql
- MaterialUI
