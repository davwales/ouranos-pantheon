# Ouranos Pantheon

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![Next.js](https://img.shields.io/badge/Next.js-black?logo=next.js&logoColor=white)](https://nextjs.org)

An extensible modular monolith platform for building centralized personal services across any domain.

<!-- Replace with an actual screenshot -->
<!-- ![Dashboard Preview](docs/assets/dashboard-preview.png) -->

## Overview

Ouranos Pantheon is a personal platform built to grow. It provides a structured foundation - a modular monolith with explicit domain boundaries - that makes it straightforward to add new functionality without touching existing modules. Each module is a self-contained vertical slice; adding a new domain means adding a new module and registering it in the gateway.

The platform currently includes two modules: one for aggregating and analyzing market data across game economies and financial markets, and one for interacting with locally-hosted LLMs via a configurable chat interface. Future modules can address any domain that benefits from a centralized, always-available personal service.

This project exists to demonstrate modern .NET architecture patterns in a practical, real-world application that actively makes daily life more convenient.

## Modules

### Plutus - Market Data & Analysis

Aggregates trade data from multiple external sources and provides tools for analysis and decision-making.

- Real-time trade ingestion from FFXIV (Universalis), OSRS (Wiki API), and US stock markets (Alpaca)
- Trade snapshots across configurable time frames
- Quantitative investment signal analysis (RSI, Bollinger Bands, Moving Average Crossover, Volume Anomaly, and more)
- ML-powered price forecasting via an external inference service
- Crafting recipe cost analysis for game economies

### Hermes - AI Chat Assistants

A chat interface for locally-hosted LLMs with configurable assistant profiles.

- Create and manage multiple assistant configurations (model, system prompt, temperature, etc.)
- Streaming chat completions

## Architecture

Ouranos Pantheon is a **modular monolith** - a single deployable application composed of isolated domain modules. Each module enforces its own boundaries and communicates through explicit contracts rather than shared state.

**Patterns:** Vertical Slice Architecture · Domain-Driven Design · Message-driven data pipelines

**Module contract:** Every module implements `IPantheonModule`, which provides hooks for service registration, middleware configuration, and endpoint mapping. The gateway composes all registered modules at startup.

**Data flow:** External APIs → Data loader workers → RabbitMQ → Consumer → PostgreSQL → REST API → Next.js dashboard

## Tech Stack

| Category   | Technologies                             |
| ---------- | ---------------------------------------- |
| Backend    | .NET, ASP.NET Core Minimal APIs, EF Core |
| Frontend   | Next.js, TypeScript, Tailwind CSS        |
| Data       | PostgreSQL                               |
| Messaging  | RabbitMQ, Wolverine                      |
| Scheduling | TickerQ                                  |

## Project Structure

```
src/
  apps/
    gateway/          # REST API host - composes all modules
    interface/        # Next.js dashboard
  modules/
    plutus/           # Market data, trades, signals, forecasts, recipes
    hermes/           # AI chat assistants
    Shared/           # Cross-cutting infrastructure and base abstractions
tests/
automation/           # Git hooks (pre-commit formatting, pre-push build)
```

## Getting Started

### Prerequisites

- .NET SDK
- Node.js
- Infrastructure: PostgreSQL, RabbitMQ - the recommended setup is the Docker Compose configuration in the [ouranos-docker-infrastructure](https://github.com/ouranos-labs/ouranos-docker-infrastructure) repository

### Backend

```bash
dotnet restore
dotnet build Ouranos.Pantheon.sln
```

Override connection strings and API keys in the relevant `appsettings.json` files before running.

### Frontend

```bash
cd src/apps/interface
npm install
npm run dev
```

## Contributing

This is a personal project and portfolio showcase. I am not currently accepting contributions.

That said, you are welcome to fork the repository, explore the architecture, or draw inspiration from any patterns you find useful.
