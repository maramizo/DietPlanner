## Diet Planner
Simple Diet Planner made with Visual Basic and Windows Forms for personal use.

Recipe pages are parsed locally and sent to Codex CLI for structured nutrition and
meal-type extraction, including ingredient amounts, preparation directions, and
focused notes for storage, freezing, reheating, make-ahead guidance, and recipe
variations.
Ingredients are stored as a canonical grocery identity, separate non-identity
details, numeric quantity, and canonical unit rather than an opaque amount string.
Existing ingredient lists are normalized together in one coordinated catalog-wide
Codex pass on startup. This gives every recipe the same vocabulary without
requiring its source URL. New recipe scrapes receive the established canonical
catalog and reuse an existing identity whenever the grocery item is equivalent.
Purpose labels do not create duplicate ingredients: entries such as
`Salt (for filling)` and `Salt (for mash)` normalize to `Salt`, and compatible
amounts are summed locally. Intrinsic varieties remain distinct, so `Kosher Salt`,
`Sea Salt`, and plain `Salt` are never merged with one another. Ordinary sizes,
package sizes, and preparation wording are retained in the separate Details field
without fragmenting the planner's holistic ingredient list.
Serving count is stored with each recipe, and scraped calories and nutrients are
normalized to a per-serving basis. View Details also calculates total batch
calories from the stored serving count without persisting a redundant total.
DietPlanner installs the native Windows Codex CLI on demand when it is missing and
uses the user's ChatGPT sign-in; no API-key file is needed. Existing recipes
without meal types are categorized automatically on startup. Legacy recipes that
do not yet have the current ingredients, preparation directions, and notes data
are also enriched once from their saved source URL.

Startup compatibility work fans out every recipe download and Codex extraction
as an independent asynchronous task. Serving/calorie/ingredient/direction/note
enrichment, the coordinated stored-ingredient catalog pass, and meal-category
migration run as separate flows at the same time, then save their results together
after every task has completed.

Every meal records its advanced-scrape status as `Pending`, `Complete`, or
`Unavailable`. Clearly invalid, inaccessible, or incomplete recipe sources are
marked `Unavailable` and skipped on future launches. Transient network or Codex
failures remain `Pending` so they can be retried later.

Scraped directions arrive as ordered Preparation and Cooking step arrays.
DietPlanner formats the headings, numbering, punctuation, and line breaks locally.

The main window can plan a complete Monday-through-Sunday week in either of two
modes:

1. Use only checked recipes and include every checked recipe at least once.
2. Generate freely from the full catalog while guaranteeing any checked recipes
   at least once.

The planner lets the user choose any subset of Breakfast, Brunch, Lunch, Dinner,
and Snack; all five are selected by default for backwards compatibility. Only
the chosen slots are generated and validated, so a Breakfast-and-Dinner plan has
14 weekly slots rather than 35. A holistic ingredient checklist is also selected
by default; unchecking an ingredient excludes every recipe that requires it.
The finished week includes a consolidated ingredient table, scaled to one serving
for each planned slot. Each click creates a fresh randomized shuffle, then
balances the saved recommended daily nutrient targets across the week while
penalizing large day-to-day calorie or nutrient variance. The generated plan,
selected meal types, ingredient constraint, mode, random seed, guaranteed
recipes, ingredient snapshots, and target snapshot are saved in
`data/week-plan.json`.

Settings includes four persistent themes with live preview: Fresh Sage, Coastal
Blue, Berry Bloom, and Midnight Kitchen. It also provides an installed-font
selector and 8–12 pt sizing; the default is Segoe UI Variable Text at 10 pt with
an automatic Segoe UI fallback. Themes now extend into the native Windows title
bar, including its caption, text, and border colors. These preferences are stored
in `data/settings.json`, which is preserved during automatic updates.
Ingredient amounts can be displayed in standardized source units, US customary
units, or metric units. Unit conversion and weekly aggregation are deterministic
local calculations; Codex only extracts and normalizes the source quantity and
unit.

![DietPlanner theme concepts](docs/theme-concepts.png)

`View All Recipes` provides an editable category matrix for the full recipe
catalog. Breakfast always implies Brunch. A one-time category migration also asks
Codex to apply Brunch more broadly to suitable Lunch and Snack recipes, after
which manual category edits remain authoritative.

Daily Facts refreshes in place after recommended-intake changes. Nutrient units
remain display formatting, so saving a selected or actively edited `g`/`mg` cell
continues to persist its numeric value correctly.

The app runs `gpt-5.6-luna` with low reasoning in Fast mode. Codex CLI is the local
Windows client, while model inference still uses OpenAI's hosted service. On the
first Codex-backed action, DietPlanner:

1. Runs OpenAI's official PowerShell installer if `codex.exe` is not found.
2. Opens the Codex browser sign-in flow if no ChatGPT login is available.
3. Runs `codex exec` with a strict JSON output schema in a read-only temporary
   workspace.

For direct CLI use, the prompt to `codex exec` is positional. The `-p` option
selects a named Codex profile; it does not mean “prompt.”

Release builds check this repository's latest GitHub Release on startup. When a
newer version is available, DietPlanner verifies the release's SHA-256 checksum,
stages it, closes, replaces the application files, and restarts automatically.
Saved files in the application's `data` directory are preserved. If replacement
fails, the updater restores its backup and does not retry that same release on
every launch. DietPlanner is single-instance, records an update as pending before
the external installer starts, and verifies the installed application assembly
against the staged copy before restarting. This prevents another open instance
or a rollback failure from creating an update/restart loop.
