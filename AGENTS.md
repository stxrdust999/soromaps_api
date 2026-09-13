# Agent Rules

## Restrictions

_(hard stops — never bypass)_

1. No code changes without explicit permission.
2. No task creation without explicit permission.
3. No implementation plans without explicit permission.
4. Never read `.env` files or `appsettings.*.json` secrets.

---

## Behavior

### Before responding

- Map the full context of the request (controller, model, scope, affected areas).
- Validate suggestions against the current implementation; anticipate edge cases.
- Search the codebase for existing solutions before proposing new ones (DTOs, extensions, validations, etc.).
- Ensure answers are brief and objective, without sacrificing details; do not skip complex steps or assume context.
- If there is any ambiguity on a request made by the user, clarify it before proceeding with changes.

### When writing code

- Follow the project's naming conventions (files, classes, properties, methods).
- Code in English, comments in Portuguese.
- Use full, descriptive names — no abbreviations (`userResponse` not `usrRes`; `user => user.Id` not `u => u.Id`).
- No `{}` for single-line `if`/`else` blocks.
- No `dynamic`, and no `object` as a typing escape — use concrete types or generics.
- Never return raw entities carrying sensitive fields; expose a DTO from `DTO/`.
- No comments unless explicitly requested.
- Omit boilerplate and standard `using` directives in snippets; show only the change + minimal surrounding context for placement.
- Prioritize clean code and performance.

### When suggesting

- Solve the root problem — don't suppress compiler/analyzer warnings with workarounds.
- Prefer existing patterns over new abstractions.
- Avoid workarounds; find the real fix.

### Communication

- Portuguese only.
- Use tools autonomously — avoid asking permission to run commands. Ask only when absolutely necessary.
- Be thorough and comprehensive; never omit critical implementation details or logic.
