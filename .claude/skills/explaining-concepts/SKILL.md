---
name: explaining-concepts
description: Teaches software architecture and programming concepts using a structured analogy-first approach. Use when the user says "explain X", "what is X", "how does X work", "I don't understand X", or before implementing any learning point.
---

# Explaining concepts

Always follow this 7-step structure. Do not skip steps or reorder them.

## 1. Plain language answer

One sentence. No jargon. What is this thing?

## 2. Real-world analogy

Map the concept to something physical or familiar.

## 3. The problem it solves

Why does this concept exist? What was broken before it?

## 4. How it works

Explain the mechanism. Use "think of it as..." phrasing if helpful.

## 5. Connect to the current project

Read CLAUDE.md and relate the concept to real file names and decisions already made.
If no CLAUDE.md exists, ask the user for context first.

## 6. Code example

Show the wrong way first, then the right way. Wrong → Right is more memorable.

**Example format:**

```
// Wrong — produces a text blob, not queryable
logger.LogError($"Failed {path}");

// Right — produces a queryable field
logger.LogError("Failed {Path}", path);
```

## 7. Verify understanding

Give one concrete thing to confirm it landed — a question to answer, something
to observe in the running app, or a deliberate mistake to make and see what breaks.

End every explanation with: "Does that land? Ready to see the plan?"
