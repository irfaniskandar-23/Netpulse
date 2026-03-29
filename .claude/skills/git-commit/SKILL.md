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
- `learn` — Phase 1 or Phase 2 output (concept file, checklist — no runnable code)
- `refactor` — restructuring without behaviour change
- `fix` — fixing a bug
- `docs` — module markdown files, concept files, checklists
- `chore` — tooling, packages, project setup

### Scopes (use these, don't invent new ones without updating this file)

- `pipeline` — middleware order, Program.cs, request flow
- `exceptions` — GlobalExceptionHandler, DomainExceptions
- `response` — API response structure, ProblemDetails, DTOs
- `tracing` — TraceIdMiddleware, W3C Trace Context
- `correlation` — downstream HTTP propagation
- `logging` — Serilog configuration, RequestEnricher
- `skills` — .claude/skills changes
- `config` — CLAUDE.md, settings.json, .claude/ non-skill changes
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
learn(pipeline): document middleware order concept and checklist

Covers how ASP.NET Core processes requests through the middleware
pipeline and why order matters. Includes concept file and build plan.

Learning point 1 — Phase 2 complete.
```

```
chore(setup): scaffold .NET 10 Web API project and install NuGet packages
```

## Steps to follow

All git commands must use `git -C "<repo-path>"` format — never `cd && git`. This ensures
the command string starts with `git` and matches the allow rule in `.claude/settings.json`.

1. Run `git -C "<repo-path>" status` to see what has changed
2. Run `git -C "<repo-path>" diff` to understand what was modified (summarise for the user if large)
3. Stage all relevant files with `git -C "<repo-path>" add <files>`
4. Run `git -C "<repo-path>" status` again after staging — confirm nothing relevant was missed
5. Write the commit message following the format above
6. Run `git -C "<repo-path>" commit -m "<message>"` — use a single inline `-m`, no heredoc
7. Run `git -C "<repo-path>" push` to push the commit to the remote
8. Output the commit hash and a one-line summary of what learning point was completed

## Rules

- Never commit secrets, connection strings with passwords, or API keys
- Always check `git status` before staging — never blindly `git add .` without reviewing
- One commit per learning point when possible — keep the history clean and readable
- The commit history should read like a learning journal
