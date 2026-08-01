# DietPlanner

DietPlanner is a Windows recipe organizer and weekly meal planner.

## What it does

Paste a recipe URL to save its nutrition, servings, meal types, ingredients, preparation steps, and useful notes.

Choose meals from your saved recipes. DietPlanner creates a randomized seven-day plan with nutrition totals and one combined ingredient list.

## Install

DietPlanner needs Windows and the [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).

1. Download `release.zip` from the [latest release](https://github.com/maramizo/DietPlanner/releases/latest).
2. Extract all files to a folder where you have write access.
3. Start `DietPlanner.exe` from the extracted folder.

Do not start the application from inside the ZIP file. Keep all extracted files together.

DietPlanner checks for a new release when it starts. An update keeps your saved data and restarts the application.

## Add a recipe

1. Select **Add New Recipe**.
2. Paste a public recipe URL into **Recipe Link**.
3. Select **Scrape**.
4. Complete the ChatGPT sign-in if Codex requests it.
5. Review the extracted recipe.
6. Select **Save**.

The scrape fills the name, servings, calories, nutrition, meal types, ingredients, and cooking steps. It also adds relevant storage, freezing, reheating, and variation notes.

DietPlanner stores each source amount, range, normalized measurement, and original unit text. Codex does not convert ingredient measurements.

DietPlanner installs Codex CLI when it first needs it. Codex uses your ChatGPT sign-in, so you do not need an API key file.

Codex CLI runs on your computer. Recipe extraction still uses OpenAI's online service.

The recipe page must be available without a website sign-in. DietPlanner marks a permanently unavailable source and does not retry it at each start.

## Add recipes from your browser

1. Select **Browser Extension** in DietPlanner.
2. Select **Set Up Google Chrome** or **Set Up Microsoft Edge**.
3. On the browser's Extensions page, turn on **Developer mode** and select **Load unpacked**.
4. Choose the bundled `BrowserExtension` folder that DietPlanner opened. Its path is also copied to the clipboard.
5. Open a recipe page, select the DietPlanner extension, and select **Add Current Recipe**.

The popup keeps a status list: yellow means the recipe is in progress, green means it was added, and red shows an error. You can send pages from several tabs without waiting; DietPlanner processes them in parallel. The Windows app does not need to be open.

## Choose meals for a day

Use the main window to choose a recipe for Breakfast, Brunch, Lunch, Dinner, or Snack.

- **View Details** shows servings, nutrition, ingredients, cooking steps, and notes.
- **View Recipe** opens the saved recipe page.
- **Clear** removes one selection.
- **Clear All** removes all selections.
- **View Daily Facts** compares the selected meals with your nutrition targets.

## Manage saved recipes

Select **View All Recipes** to review your recipe collection. Use the check boxes to change the meal types for each recipe.

Select **View Details** to inspect a recipe. Select **Save Changes** after you change any meal types.

## Plan a week

1. Select **Plan My Week**.
2. Choose the meal types that you want to plan.
3. Choose a generation mode.
4. Check the recipes that the plan must use.
5. Open the **Ingredients** tab and clear ingredients that you do not want.
6. Select **Generate / Shuffle and Save Week**.

The two generation modes use your recipe choices differently:

- **Only selected recipes** uses only checked recipes. Each checked recipe appears at least once.
- **Generate freely from all recipes** can use the full collection. Each checked recipe still appears at least once.

The result shows each day, calorie totals, nutrition coverage, and a combined ingredient list. The list includes enough whole recipe batches.

Matching ingredients use one compatible measurement. Change a row's **Measurement** list to convert its displayed amount locally.

DietPlanner saves these measurement choices with the weekly plan. Select the generate button again to create another random plan.

## Change the appearance and units

Select **Settings** to choose a theme, font, font size, and ingredient measurement system.

Ingredient amounts can use standardized source units, US customary units, or metric units. DietPlanner converts these amounts without another Codex request.

## Build from source

Install the [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0). Run these commands in Windows PowerShell:

```powershell
git clone https://github.com/maramizo/DietPlanner.git
cd DietPlanner
dotnet run --project .\src\DietPlanner.vbproj
```

## Images

### Theme concepts

![DietPlanner theme concepts](docs/theme-concepts.png)
