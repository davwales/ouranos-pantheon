# Technical Context - Ouranos Pantheon

## Technology Stack

- **Frontend Framework**: Next.js 14 with App Router
- **Language**: TypeScript 5.0
- **Styling**: Tailwind CSS 3.3
- **UI Components**: Shadcn UI
- **State Management**: React Context API
- **Data Fetching**: GraphQL (URQL)
- **Utility Libraries**: date-fns, lodash

## Development Tools

- **Package Manager**: npm 9.0
- **Linting**: ESLint with TypeScript
- **Bundler**: Webpack (via Next.js)
- **Testing**: Jest (planned)
- **CI/CD**: GitLab Pipeline

## Architecture

- **Module System**: ES Modules
- **Component Structure**: Atomic design pattern
- **Routing**: Next.js App Router
- **API Layer**: GraphQL with URQL
- **Error Handling**: Custom error boundaries

## Performance

- **Code Splitting**: Automatic via Next.js
- **Image Optimization**: Next.js Image
- **Bundle Analysis**: Webpack Bundle Analyzer (planned)
- **Lazy Loading**: Dynamic imports for components

## Environment Variables

- NEXT_PUBLIC_API_URL: API base URL
- NEXT_PUBLIC_ANALYTICS_ID: Google Analytics ID
- NODE_ENV: development/production
