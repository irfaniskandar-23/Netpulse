# Netpulse

> A learning codebase for understanding the .NET Core API Request pipeline.

This is NOT a production project. Every implementation decision exists to teach
a concept. Claude Code's job here is to be a teacher first, implementer second.

---

## How to work in this repo

For every learning point, follow this exact 3-phase flow — never skip or reorder:

### Phase 1 — Teach the concept

- Read the relevant file in `docs/learning/` first
- Use the `explain-concept` skill to teach the concept
- Do NOT write any code in this phase
- End by asking: "Does that make sense? Ready to see the plan?"

### Phase 2 — Draft the plan

- Only after the concept is understood, propose what we will build
- Explain what each piece of code will do and WHY before writing it
- create a checklist for the plan in the learning markdown file.
- Get confirmation before moving to implementation

### Phase 3 — Implement

- Build the code step by step, one file at a time
- After each file, explain what it does in the context of the concept
- After all files are done, show concrete proof it works
  (console output, response headers, log entry, etc.)
- Use the `git-commit` skill to commit when the point is complete

---

## Learning points

The 9 points should documented individually in `docs/learning/{topics.md}`. Work through them
in order — each one builds on the previous.

| #   | Topic                                                                    | File                                    |
| --- | ------------------------------------------------------------------------ | --------------------------------------- |
| 1   | API request pipeline — middleware order and flow of control              | `docs/learning/01-pipeline.md`          |
| 2   | Global exception middleware — IExceptionHandler, exception propagation   | `docs/learning/02-exceptions.md`        |
| 3   | Problem Details — RFC 7807 standardised error response                   | `docs/learning/03-problem-details.md`   |
| 4   | Trace middleware — W3C Trace Context, X-Trace-Id, Activity.Current       | `docs/learning/04-trace-middleware.md`  |
| 5   | Correlation ID propagation — carry trace to downstream HTTP calls        | `docs/learning/05-correlation-id.md`    |
| 6   | Serilog request/response logging — full lifecycle, one entry per request | `docs/learning/06-serilog-logging.md`   |
| 7   | Serilog enrichers — automatic context stamping on every log entry        | `docs/learning/07-serilog-enrichers.md` |
| 8   | Log levels and filtering — signal vs noise, per-namespace configuration  | `docs/learning/08-log-levels.md`        |
| 9   | MongoDB sink — structured log persistence, local connection only         | `docs/learning/09-mongodb-sink.md`      |

---

## Tech stack

- .NET 10 Web API
- Serilog + Serilog.Sinks.MongoDB
- MongoDB local — `mongodb://localhost:27017/NetpulseLogs`
- No authentication required locally

---

## Skills available

- `/git-commit` — stage and commit using Conventional Commits (you invoke this, not Claude)
- `/explain-concept` — explain any concept with analogy-first teaching style
