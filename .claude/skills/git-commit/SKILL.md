---
name: git-commit
description: Stages and commits changes using Conventional Commits format with correct scope and type. Use when the user says "commit", "save my progress", or "commit this learning point".
disable-model-invocation: true
---

# Git commit skill for Netpulse

You are committing changes to the Netpulse learning project.
Follow these rules exactly every time.

## Commit message format

Use Conventional Commits: https://www.conventionalcommits.org

```
<type>(<scope>): <short description>

[optional body — what was implemented and what concept it teaches]

[optional footer — references]
```

### Types

- `feat` — a new implementation (new middleware, new class)
- `learn` — a learning checkpoint (concept understood, documented)
- `refactor` — restructuring without behaviour change
- `fix` — fixing a bug
- `docs` — CLAUDE.md, README, or comments only
- `chore` — tooling, packages, project setup

### Scopes (use these, don't invent new ones)

- `pipeline` — middleware order, Program.cs, request flow
- `exceptions` — GlobalExceptionHandler, DomainExceptions
- `problemdetails` — RFC 7807 response format
- `tracing` — TraceIdMiddleware, W3C Trace Context
- `correlation` — downstream HTTP propagation
- `logging` — Serilog configuration, RequestEnricher
- `enrichers` — Serilog enrichers
- `mongodb` — MongoDB sink, log models
- `skills` — .claude/skills changes
- `setup` — repo setup, project scaffold, packages

### Examples

```
feat(tracing): add TraceIdMiddleware with W3C Trace Context support

Implements X-Trace-Id generation and propagation. Honours incoming
trace headers from upstream services. Pushes TraceId to Serilog
LogContext so it appears on every log entry in the request scope.

Learning point 4 complete.
```

```
feat(exceptions): implement GlobalExceptionHandler using IExceptionHandler

Maps domain exceptions to HTTP status codes via pattern matching.
Returns RFC 7807 ProblemDetails with traceId in extensions.
Only exposes exception detail in Development environment.

Learning point 2 complete.
```

```
chore(setup): scaffold .NET 10 Web API project and install NuGet packages
```

## Steps to follow

1. Run `git status` to see what has changed
2. Run `git diff` to understand what was modified (summarise for the user if large)
3. Stage all relevant files with `git add`
4. Write the commit message following the format above
5. Run `git commit -m "..."`
6. Confirm with the user what was committed and what learning point it completes

## Rules

- Never commit secrets, connection strings with passwords, or API keys
- Always check `git status` before staging — never blindly `git add .` without reviewing
- One commit per learning point when possible — keep the history clean and readable
- The commit history should read like a learning journal
