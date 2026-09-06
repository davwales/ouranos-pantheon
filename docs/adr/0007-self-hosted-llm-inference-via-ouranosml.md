# ADR 0007: Self-hosted LLM inference via OuranosMl

**Status:** accepted

## Context

All three modules want AI capability: Hermes (chat), Hestia (recipe normalization from
scraped pages), Plutus (price forecasting). Using a cloud LLM API would add cost, latency,
and privacy concerns for personal data; each module integrating a different vendor would
scatter configuration and client code.

## Decision Drivers

- One inference endpoint for chat, structured output, and ML forecasting
- Personal data must not leave the homelab
- Zero per-token cost
- The frontend/Hermes UX needs streaming completions

## Considered Options

- **Self-hosted OuranosMl**: dedicated inference host exposing an OpenAI-compatible API
  (chat, streaming, structured output, model listing) plus a `POST /plutus/forecast`
  endpoint
- Cloud LLM APIs (OpenAI, Anthropic, OpenRouter): easy to adopt, but recurring cost and personal
  data leaving the homelab
- Per-module direct integrations with self-hosted runtimes: no shared client or model
  catalog

## Decision

Run **OuranosMl** as the single inference service. The shared kernel provides one client
(`IOuranosMachineLearningClient`, OpenAI-compatible SDK with a custom endpoint) used by
all modules. Hermes syncs the model catalog hourly; Hestia uses structured output for
recipe normalization; Plutus calls the dedicated forecasting endpoint. This enables a centralized location for all AI related features, including custom models like `plutus-forecasting-v1`.

## Consequences

- All AI features degrade together if the inference host is offline, surfaced by the
  OuranosMl health check
- One place to swap or upgrade models; no vendor keys in this codebase
- Forecast and extraction quality depends on the locally hosted model, an accepted
  trade-off for privacy and cost
