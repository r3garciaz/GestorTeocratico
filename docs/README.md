# Gestor Teocrático — Documentation

Documentation for humans **and** AI coding agents working on this repository.

Start with the root [`AGENTS.md`](../AGENTS.md) for the operating rules, then use the map
below. These docs describe **what the code actually does today**, not an aspirational
design. When a doc and the code disagree, the code wins — then fix the doc.

## Map

| Document | What it covers | Read it when… |
|---|---|---|
| [ARCHITECTURE.md](./ARCHITECTURE.md) | Composition root, DI lifetimes, render modes, request pipeline | You touch `Program.cs`, DI, or app startup |
| [DATA-MODEL.md](./DATA-MODEL.md) | Entities, keys, relationships, soft delete, EF configuration, migrations | You change schema or write queries |
| [FEATURES.md](./FEATURES.md) | Every domain: handlers, page, rules | You add or modify a feature |
| [UI-GUIDE.md](./UI-GUIDE.md) | Blazor Blueprint + Tailwind conventions and the CRUD page template | You build or restyle UI |
| [AUTH.md](./AUTH.md) | Google OAuth, Identity, roles, claims, emergency admin | You touch login/authorization |
| [DEVELOPMENT.md](./DEVELOPMENT.md) | Setup, commands, env vars, migrations, seeding, Docker, CI | You set up or run the project |
| [API.md](./API.md) | Real HTTP endpoints | You expose or consume HTTP |
| [CONVENTIONS.md](./CONVENTIONS.md) | C# style, naming, logging, error handling | You write any C#/Razor |
| [TESTING.md](./TESTING.md) | Test project, strategy and scenarios | You add tests |
| [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) | Known pitfalls and fixes | Something is broken |
| [PRD.md](./PRD.md) | Product requirements | You need the "why"/scope |
| [ROADMAP.md](./ROADMAP.md) | Status and next steps | You plan work |

## Conventions for these docs

- **Language:** English. Product/UI copy quoted from the app stays in Spanish.
- **Accuracy over aspiration:** describe what exists. Mark future work explicitly as *Planned*.
- **File paths** are relative to the repository root.
- **Keep docs close to code:** when you change behavior, update the relevant doc in the same
  change. `AGENTS.md` links here and must not drift.
- Prefer tables and short code snippets over prose.

## Source of truth

- Application code: `GestorTeocratico/`
- Composition root / DI: `GestorTeocratico/Program.cs`
- Database schema: `GestorTeocratico/Data/ApplicationDbContext.cs` + `Data/Migrations/`
- This documentation: `docs/` + `AGENTS.md`
