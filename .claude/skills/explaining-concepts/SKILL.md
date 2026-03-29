---
name: explaining-concepts
description: Teaches software architecture and programming concepts using a structured analogy-first approach. Use when the user says "explain X", "what is X", "how does X work", "I don't understand X", or before implementing any learning point.
---

## IMPORTANT

- Always refer to Microsoft documentation and best practices as the source of truth for implementation and concepts.
- Do not reinvent the wheel when Microsoft already provided an interface for that particular use case.

# Explaining concepts

Always follow this 7-step structure. Do not skip steps or reorder them.

## 1. Plain language answer

One sentence. No jargon. What is this thing?

## 2. Real-world analogy

Map the concept to something physical or familiar.

## 3. The problem it solves

- Explain the problem overview first, then the details.
- How does this concept solve the problem?
- Impact of not using this concept? What can go wrong? Use real-world examples if possible.

## 4. How it works

- Explain the mechanism. Use "think of it as..." phrasing if helpful.
- If there are multiple components, break it down and explain how they work together.
- Always produce a diagram using Excalidraw — visuals are more memorable than text.

### Diagram steps (always follow this order)

1. Call `mcp__claude_ai_Excalidraw__read_me` to load the element format reference.
2. Call `mcp__claude_ai_Excalidraw__create_view` to render the diagram inline.
3. Call `mcp__claude_ai_Excalidraw__export_to_excalidraw` to generate a shareable URL for the developer to save or revisit.

## 5. Connect to the current project

Read CLAUDE.md and relate the concept to real file names and decisions already made.

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
