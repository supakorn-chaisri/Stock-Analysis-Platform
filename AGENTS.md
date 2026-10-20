# Stock Analysis Platform

Thai + international stock analysis web platform.

- **Backend**: .NET (latest LTS), PostgreSQL + Redis
- **Frontend**: Next.js + shadcn/ui + Tailwind CSS
- **Data**: hybrid API sources via adapter pattern, ETL background workers
- **AI**: Gemini API with structured JSON Schema output over a RAG + structured financial DB

## Agent skills

### Issue tracker

Issues live in GitHub Issues for `supakorn-chaisri/Stock-Analysis-Platform`, managed via the `gh` CLI. See `docs/agents/issue-tracker.md`.

### Triage labels

Five canonical triage roles map to the labels `needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context layout: root `GLOSSARY.md` plus `docs/adr/`. See `docs/agents/domain.md`.
