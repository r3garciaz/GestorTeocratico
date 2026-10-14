# UI Guide

The UI is **Blazor** with **Blazor Blueprint** (`Bb*`) components, **Lucide** icons, and
**Tailwind CSS v4**. Radzen and Bootstrap were removed; do not reintroduce them.

## Styling setup

Styles are loaded in `Components/App.razor`, in this order:

1. `styles/themes/default.css` — project theme (shadcn CSS variables, OKLCH, light + `.dark`).
2. `_content/BlazorBlueprint.Components/blazorblueprint.css` — component styles from the package.
3. `_content/BlazorBlueprint.Components/css/themes.css` — base/primary palettes for the theme switcher.
4. `css/app.css` — Tailwind output **generated** by `npm run build:css`.
5. `_content/BlazorBlueprint.Components/js/theme-init.js` — applies the saved theme before paint.

### Tailwind v4

- Input: `wwwroot/css/app-input.css`. Output: `wwwroot/css/app.css` (do not edit the output).
- The input `@import 'tailwindcss'` and declares `@source` globs for the folders scanned for
  utility classes (`Components`, `App.razor`). **Add a `@source` line if you put markup in a new
  folder**, then rerun the build.
- Dark mode is **class-based** (`@custom-variant dark (&:where(.dark, .dark *))`) so it matches
  the library's `.dark` toggle. Do not rely on `prefers-color-scheme`.
- `@theme` maps the shadcn variables (`--background`, `--primary`, `--border`, `--radius`, …) to
  Tailwind tokens (`bg-background`, `text-muted-foreground`, `border-border`, `rounded-lg`, …).

Use the semantic tokens (`bg-background`, `text-muted-foreground`, `border`, `bg-muted`,
`text-destructive`, `bg-primary/10`, …) instead of raw colors, so theming and dark mode keep
working.

### Commands

```bash
npm run watch:css   # during development
npm run build:css   # before committing (minified)
```

## Theme variables

`wwwroot/styles/themes/default.css` defines light (`:root`) and dark (`.dark`) values for the
shadcn tokens (background, foreground, card, popover, primary, secondary, muted, accent,
destructive, border, input, ring, sidebar-*, chart-*, alert-*). Customize there to restyle the
whole app. The brand accent used across the shell is the theme `primary`
(`bg-primary/10 text-primary` with the `book-open` Lucide icon). Do not hardcode Tailwind
palette colors for branding: use the semantic tokens so light/dark keep working.

## Icons

```razor
@using BlazorBlueprint.Icons.Lucide.Components
<LucideIcon Name="plus" Class="size-4"/>
```

Common names used in this app: `book-open`, `house`, `landmark`, `building-2`, `clipboard-list`,
`users`, `user-cog`, `shield`, `shield-check`, `calendar-days`, `file-text`, `file-down`,
`refresh-cw`, `copy`, `plus`, `pencil`, `trash-2`, `eye`, `x`, `log-out`, `log-in`, `user`,
`sparkles`, `arrow-down`, `chevrons-up-down`, `badge-check`, `mail`.

## Components in use

| Area | Components |
|---|---|
| Actions | `BbButton` (`Variant` Default/Outline/Secondary/Ghost/Destructive, `Size` Icon/Small/Large, `Type` Submit, `Href` for links) |
| Data | `BbDataTable` + `BbDataTableColumn` (Sortable/Filterable/Width/CellTemplate), `BbBadge`, `EntityChips` (local, in `Components/Ui/`) |
| Forms | `EditForm`, `DataAnnotationsValidator`, `ValidationSummary`, `ValidationMessage`, `InputText`, `InputCheckbox`, `BbLabel`, `BbInput`, `BbSelect`, `BbMultiSelect`, `BbNumericInput` |
| Overlays | `BbDialog` + `BbDialogContent/Header/Title/Description/Footer`, `ToastService` |
| Layout | `BbSidebar*`, `BbCard*`, `BbAvatar`, `BbDropdownMenu*`, `BbThemeSwitcher`, `BbDarkModeToggle` |

Selects take `SelectOption<T>` values, e.g.:

```csharp
private readonly SelectOption<Gender>[] _genderOptions =
[
    new(Gender.Male, "Masculino"),
    new(Gender.Female, "Femenino")
];
```

`BbMultiSelect` binds via `Values` + `ValuesChanged` (not `@bind-Value`) and takes
`MaxDisplayTags`/`SingleLine`. Sort option texts with a Spanish comparer before rendering.

`EntityChips` renders a list of names as badges, capping at `Max` with a `+N` badge:

```razor
<EntityChips Items="@names" Max="5"/>          <!-- compact (tables) -->
<EntityChips Items="@names" Max="int.MaxValue"/><!-- full (details) -->
```

## CRUD page template

Feature pages follow a consistent pattern (see `Publisher/Publishers.razor`):

1. `@page "/route"` + `@attribute [Authorize(Roles = "Admin, Manager")]` + `@inject` services.
2. A heading (`h1` + muted subtitle) inside `<div class="space-y-6">`.
3. `BbDataTable TData="T" Data="@items"` with a `<ToolbarActions>` create button and
   `<Columns>` with `BbDataTableColumn`s. Relation columns render `EntityChips`; action columns
   render icon buttons (view/edit/delete).
4. Three `BbDialog`s: **create/edit** (`EditForm` + `OnValidSubmit`), **confirm delete**, and
   **details**.
5. `@code` block with `_items`, `_formOpen/_isNew/_editing/_selected*/_formError`,
   `_deleteOpen/_deleteTarget`, `_detailsOpen/_detailsTarget`; `LoadAsync` on init; `SaveAsync`
   calls the service then reloads; delete confirms then reloads.

Screen real estate convention: pages use `space-y-6`, tables sit in `overflow-x-auto` containers,
and content is Spanish.

## Culture and formatting

- Use `new CultureInfo("es-ES")` for month/day names and comparers
  (`StringComparer.Create(new CultureInfo("es-ES"), true)`).
- Short publisher names in PDFs are built by `PdfExportService.GetPublisherShortName`.

## Adding an interactive island to a static page

Static SSR pages (`[ExcludeFromInteractiveRouting]`, e.g. the landing) cannot use interactive
components inline. Mount the interactive bits explicitly:

```razor
<PublicThemeControls @rendermode="InteractiveServer"/>
```

## Related docs

- Architecture / render modes: [ARCHITECTURE.md](./ARCHITECTURE.md)
- Feature pages list: [FEATURES.md](./FEATURES.md)
