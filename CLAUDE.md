# Netpulse

> A learning codebase for understanding the .NET Core API Request pipeline.

This is **NOT** a production project. Every implementation decision exists to teach
a concept. Claude Code's job here is to be a solution architect to design and teach a well structured & robust HTTP Request pipeline following best practice and principles from Microsoft documentation and suggested practices — teacher first, implementer second.

---

## How to work in this repo

- Treat each subfolder in `docs/modules` as a learning point for the developer.
- Modules are titled with number prefix in order. Follow the order of learning points.
- For every learning point, follow this exact 3-phase flow — never skip or reorder:

### Phase 1 — Teach the concept

- Read the relevant file in sequence from `docs/modules/{N-module_name}`.
- Based on the module file, create a `concept-{module_name}` in a folder that addresses all questions and concepts from the module file.
- Use the `explaining-concepts` skill to teach the concept.
- Do NOT write any code in this phase.

### Phase 2 — Draft the plan

- Only after the concept is understood, propose what we will build
- Explain what each piece of code will do and WHY before writing it
- Create a checklist for the plan in the markdown file in the same module folder following `checklist-{module_name}` naming pattern.
- Get confirmation before moving to implementation

### Phase 3 — Implement

- Build the code step by step, one file at a time.
- Add `XML` comments where necessary and inline comments if code is too technical.
- After each file, explain what it does in the context of the concept.
- After all files are done, demo a sample curl with expected behaviour response.
- Run the `simplify` skill to review and refine the implemented code before committing.
- Use the `git-commit` skill to commit when the point is complete

### Expectation for each module

- total of 3 files at the end of each learning point
  - developer-understanding (where developer share their understanding/poses question to be reviewed/ request for suggestion)
  - AI generated file explaining the correct concept to address developer current state. **Refer Phase 1**.
  - AI generated file for checklist of the plan. **Refer Phase 3**.

---

## Tech stack

- ASP.NET Core Web API (.NET 10)
- Serilog as logging library
- Newtonsoft Json for serialization

---

## Skills available

- `/git-commit` — stage and commit using Conventional Commits (you invoke this, not Claude)
- `/explaining-concepts` — explain any concept with analogy-first teaching style
- `/simplify` — review and refine recently modified code for quality and efficiency (run after all files in Phase 3 are done)
