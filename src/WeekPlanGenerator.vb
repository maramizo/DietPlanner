Public Enum WeekPlanGenerationMode
    SelectedRecipesOnly = 0
    FullCatalogWithGuarantees = 1
End Enum

Public NotInheritable Class WeekPlanGenerator
    Public Shared ReadOnly DayNames As String() = {
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday",
        "Saturday",
        "Sunday"
    }

    Public Shared ReadOnly MealTypes As String() = {
        "Breakfast",
        "Brunch",
        "Lunch",
        "Dinner",
        "Snack"
    }

    Private Const OptimizationPasses As Integer = 10
    Private Const RandomPreferenceWeight As Double = 0.08

    Private Sub New()
    End Sub

    Public Shared Function GetMissingMealTypes(
        selectedMeals As IEnumerable(Of Meal),
        Optional plannedMealTypes As IEnumerable(Of String) = Nothing
    ) As List(Of String)
        Dim meals = NormalizeMeals(selectedMeals)
        Dim normalizedMealTypes = NormalizePlannedMealTypes(plannedMealTypes)
        Return normalizedMealTypes.Where(
            Function(mealType) Not meals.Any(
                Function(meal) SupportsExplicitMealType(meal, mealType)
            )
        ).ToList()
    End Function

    Public Shared Function Generate(
        selectedMeals As IEnumerable(Of Meal),
        targetDailyIntakes As IDictionary(Of String, Double),
        Optional randomSeed As Integer? = Nothing
    ) As WeeklyPlan
        Dim selected = NormalizeMeals(selectedMeals)
        Return Generate(
            selected,
            selected,
            WeekPlanGenerationMode.SelectedRecipesOnly,
            MealTypes,
            targetDailyIntakes,
            randomSeed
        )
    End Function

    Public Shared Function Generate(
        selectedMeals As IEnumerable(Of Meal),
        availableMeals As IEnumerable(Of Meal),
        generationMode As WeekPlanGenerationMode,
        targetDailyIntakes As IDictionary(Of String, Double),
        Optional randomSeed As Integer? = Nothing
    ) As WeeklyPlan
        Return Generate(
            selectedMeals,
            availableMeals,
            generationMode,
            MealTypes,
            targetDailyIntakes,
            randomSeed
        )
    End Function

    Public Shared Function Generate(
        selectedMeals As IEnumerable(Of Meal),
        availableMeals As IEnumerable(Of Meal),
        generationMode As WeekPlanGenerationMode,
        plannedMealTypes As IEnumerable(Of String),
        targetDailyIntakes As IDictionary(Of String, Double),
        Optional randomSeed As Integer? = Nothing
    ) As WeeklyPlan
        Dim guaranteedMeals = NormalizeMeals(selectedMeals)
        Dim catalogMeals = NormalizeMeals(availableMeals)
        Dim normalizedMealTypes = NormalizePlannedMealTypes(plannedMealTypes)
        Dim candidateMeals As List(Of Meal)

        If generationMode = WeekPlanGenerationMode.SelectedRecipesOnly Then
            candidateMeals = New List(Of Meal)(guaranteedMeals)
        Else
            candidateMeals = New List(Of Meal)(catalogMeals)
            For Each meal In guaranteedMeals
                If Not candidateMeals.Contains(meal) Then candidateMeals.Add(meal)
            Next
            candidateMeals = NormalizeMeals(candidateMeals)
        End If

        ValidateInputs(
            candidateMeals,
            guaranteedMeals,
            generationMode,
            normalizedMealTypes
        )
        candidateMeals = candidateMeals.Where(
            Function(meal) normalizedMealTypes.Any(
                Function(mealType) SupportsExplicitMealType(meal, mealType)
            )
        ).ToList()

        Dim seed = If(
            randomSeed.HasValue,
            randomSeed.Value,
            System.Security.Cryptography.RandomNumberGenerator.GetInt32(
                Integer.MaxValue
            )
        )
        Dim random As New Random(seed)
        Dim targets = NormalizeTargets(targetDailyIntakes)
        Dim assignments = CreateInitialAssignments(
            candidateMeals,
            guaranteedMeals,
            normalizedMealTypes,
            random
        )
        Dim randomCosts = CreateRandomPreferenceCosts(
            candidateMeals,
            normalizedMealTypes.Count,
            random
        )
        OptimizeAssignments(
            assignments,
            candidateMeals,
            guaranteedMeals,
            normalizedMealTypes,
            targets,
            randomCosts,
            random
        )
        Return CreatePlan(
            assignments,
            guaranteedMeals,
            normalizedMealTypes,
            targets,
            generationMode,
            seed
        )
    End Function

    Private Shared Function NormalizeMeals(
        source As IEnumerable(Of Meal)
    ) As List(Of Meal)
        Return If(source, Enumerable.Empty(Of Meal)).
            Where(Function(meal) meal IsNot Nothing).
            Distinct().
            OrderBy(
                Function(meal) meal.Name,
                StringComparer.CurrentCultureIgnoreCase
            ).
            ToList()
    End Function

    Private Shared Function NormalizePlannedMealTypes(
        source As IEnumerable(Of String)
    ) As List(Of String)
        If source Is Nothing Then Return New List(Of String)(MealTypes)

        Dim requested = source.Where(
            Function(value) Not String.IsNullOrWhiteSpace(value)
        ).ToList()
        Return MealTypes.Where(
            Function(optionName) requested.Any(
                Function(value) String.Equals(
                    value,
                    optionName,
                    StringComparison.OrdinalIgnoreCase
                )
            )
        ).ToList()
    End Function

    Private Shared Sub ValidateInputs(
        candidateMeals As List(Of Meal),
        guaranteedMeals As List(Of Meal),
        generationMode As WeekPlanGenerationMode,
        plannedMealTypes As List(Of String)
    )
        If plannedMealTypes.Count = 0 Then
            Throw New WeeklyPlanException(
                "Select at least one meal type to plan."
            )
        End If

        If candidateMeals.Count = 0 Then
            If generationMode = WeekPlanGenerationMode.SelectedRecipesOnly Then
                Throw New WeeklyPlanException(
                    "Select at least one recipe before planning the week."
                )
            End If
            Throw New WeeklyPlanException(
                "Add at least one recipe before generating freely from the recipe catalog."
            )
        End If

        Dim unsupportedGuaranteed = guaranteedMeals.Where(
            Function(meal) Not plannedMealTypes.Any(
                Function(mealType) SupportsExplicitMealType(meal, mealType)
            )
        ).Select(Function(meal) meal.Name).ToList()
        If unsupportedGuaranteed.Count > 0 Then
            Throw New WeeklyPlanException(
                "These guaranteed recipes do not match any selected meal type: " &
                String.Join(", ", unsupportedGuaranteed) &
                ". Select a matching meal type or uncheck those recipes."
            )
        End If

        Dim availableSlots = DayNames.Length * plannedMealTypes.Count
        If guaranteedMeals.Count > availableSlots Then
            Throw New WeeklyPlanException(
                "This week has " &
                availableSlots &
                " selected meal slots. Guarantee no more than " &
                availableSlots &
                " recipes so every selected recipe can appear."
            )
        End If

        Dim missingMealTypes = GetMissingMealTypes(
            candidateMeals,
            plannedMealTypes
        )
        If missingMealTypes.Count > 0 Then
            Dim sourceName = If(
                generationMode = WeekPlanGenerationMode.SelectedRecipesOnly,
                "selected recipes",
                "recipe catalog"
            )
            Throw New WeeklyPlanException(
                "The " & sourceName & " do not cover: " &
                String.Join(", ", missingMealTypes) &
                ". Add or select at least one recipe for every selected meal type."
            )
        End If
    End Sub

    Private Shared Function NormalizeTargets(
        source As IDictionary(Of String, Double)
    ) As Dictionary(Of String, Double)
        Dim normalized As New Dictionary(Of String, Double)(
            StringComparer.OrdinalIgnoreCase
        )
        If source Is Nothing Then Return normalized

        For Each item In source
            If item.Value <= 0 OrElse String.IsNullOrWhiteSpace(item.Key) Then Continue For
            Dim nutrient = New Nutrition(item.Key, item.Value)
            normalized(nutrient.Name) = item.Value
        Next
        Return normalized
    End Function

    Private Shared Function CreateInitialAssignments(
        candidateMeals As List(Of Meal),
        guaranteedMeals As List(Of Meal),
        plannedMealTypes As List(Of String),
        random As Random
    ) As Meal(,)
        Dim positionOwners(
            DayNames.Length * plannedMealTypes.Count - 1
        ) As Meal
        Dim randomizedGuaranteed = Shuffle(guaranteedMeals, random)
        Dim orderedForMatching = randomizedGuaranteed.OrderBy(
            Function(meal) plannedMealTypes.Where(
                Function(mealType) SupportsExplicitMealType(meal, mealType)
            ).Count()
        ).ToList()

        For Each meal In orderedForMatching
            Dim visited(positionOwners.Length - 1) As Boolean
            If Not TryPlaceRequiredMeal(
                meal,
                positionOwners,
                visited,
                plannedMealTypes,
                random
            ) Then
                Throw New WeeklyPlanException(
                    "The guaranteed recipes cannot all fit into one week while respecting their meal types. " &
                    "Choose fewer recipes from the overrepresented category or recategorize some recipes."
                )
            End If
        Next

        Dim assignments(
            DayNames.Length - 1,
            plannedMealTypes.Count - 1
        ) As Meal
        Dim usage = candidateMeals.ToDictionary(
            Function(meal) meal,
            Function(meal) 0
        )
        For position As Integer = 0 To positionOwners.Length - 1
            Dim dayIndex = position \ plannedMealTypes.Count
            Dim mealTypeIndex = position Mod plannedMealTypes.Count
            assignments(dayIndex, mealTypeIndex) = positionOwners(position)
            If positionOwners(position) IsNot Nothing Then
                usage(positionOwners(position)) += 1
            End If
        Next

        For Each position In ShuffleIndexes(positionOwners.Length, random)
            Dim dayIndex = position \ plannedMealTypes.Count
            Dim mealTypeIndex = position Mod plannedMealTypes.Count
            If assignments(dayIndex, mealTypeIndex) IsNot Nothing Then Continue For
            Dim currentDayIndex = dayIndex
            Dim currentMealTypeIndex = mealTypeIndex

            Dim candidates = Shuffle(
                candidateMeals.Where(
                    Function(meal) SupportsExplicitMealType(
                        meal,
                        plannedMealTypes(currentMealTypeIndex)
                    )
                ),
                random
            ).OrderBy(Function(meal) usage(meal)).
                ThenBy(
                    Function(meal) If(
                        IsUsedOnDay(assignments, currentDayIndex, meal),
                        1,
                        0
                    )
                ).
                ToList()

            Dim selected = candidates(0)
            assignments(dayIndex, mealTypeIndex) = selected
            usage(selected) += 1
        Next

        Return assignments
    End Function

    Private Shared Function IsUsedOnDay(
        assignments As Meal(,),
        dayIndex As Integer,
        meal As Meal
    ) As Boolean
        For mealTypeIndex As Integer = 0 To assignments.GetLength(1) - 1
            If assignments(dayIndex, mealTypeIndex) Is meal Then Return True
        Next
        Return False
    End Function

    Private Shared Function TryPlaceRequiredMeal(
        meal As Meal,
        positionOwners As Meal(),
        visited As Boolean(),
        plannedMealTypes As List(Of String),
        random As Random
    ) As Boolean
        For Each position In ShuffleIndexes(positionOwners.Length, random)
            If visited(position) Then Continue For
            Dim mealTypeIndex = position Mod plannedMealTypes.Count
            If Not SupportsExplicitMealType(
                meal,
                plannedMealTypes(mealTypeIndex)
            ) Then
                Continue For
            End If

            visited(position) = True
            If positionOwners(position) Is Nothing OrElse
                TryPlaceRequiredMeal(
                    positionOwners(position),
                    positionOwners,
                    visited,
                    plannedMealTypes,
                    random
                ) Then
                positionOwners(position) = meal
                Return True
            End If
        Next

        Return False
    End Function

    Private Shared Function CreateRandomPreferenceCosts(
        meals As List(Of Meal),
        mealTypeCount As Integer,
        random As Random
    ) As Double(,,)
        Dim costs(
            DayNames.Length - 1,
            mealTypeCount - 1,
            meals.Count - 1
        ) As Double
        For dayIndex As Integer = 0 To DayNames.Length - 1
            For mealTypeIndex As Integer = 0 To mealTypeCount - 1
                For mealIndex As Integer = 0 To meals.Count - 1
                    costs(dayIndex, mealTypeIndex, mealIndex) = random.NextDouble()
                Next
            Next
        Next
        Return costs
    End Function

    Private Shared Sub OptimizeAssignments(
        assignments As Meal(,),
        meals As List(Of Meal),
        guaranteedMeals As List(Of Meal),
        plannedMealTypes As List(Of String),
        targets As Dictionary(Of String, Double),
        randomCosts As Double(,,),
        random As Random
    )
        Dim guaranteed As New HashSet(Of Meal)(guaranteedMeals)
        Dim mealIndexes = meals.Select(
            Function(meal, index) New With {
                .Meal = meal,
                .Index = index
            }
        ).ToDictionary(
            Function(item) item.Meal,
            Function(item) item.Index
        )
        Dim usage = CountUsage(assignments, meals)
        Dim bestScore = EvaluatePlan(
            assignments,
            meals,
            targets,
            usage,
            mealIndexes,
            randomCosts
        )
        Dim positionCount = DayNames.Length * plannedMealTypes.Count

        For pass As Integer = 1 To OptimizationPasses
            Dim changed As Boolean = False

            For Each position In ShuffleIndexes(positionCount, random)
                Dim dayIndex = position \ plannedMealTypes.Count
                Dim mealTypeIndex = position Mod plannedMealTypes.Count
                Dim current = assignments(dayIndex, mealTypeIndex)
                Dim bestMeal = current
                Dim positionBestScore = bestScore

                For Each candidate In Shuffle(meals, random)
                    If candidate Is current OrElse
                        Not SupportsExplicitMealType(
                            candidate,
                            plannedMealTypes(mealTypeIndex)
                        ) Then
                        Continue For
                    End If
                    If guaranteed.Contains(current) AndAlso usage(current) <= 1 Then
                        Continue For
                    End If

                    assignments(dayIndex, mealTypeIndex) = candidate
                    usage(current) -= 1
                    usage(candidate) += 1
                    Dim score = EvaluatePlan(
                        assignments,
                        meals,
                        targets,
                        usage,
                        mealIndexes,
                        randomCosts
                    )
                    usage(candidate) -= 1
                    usage(current) += 1

                    If score + 0.0000001 < positionBestScore Then
                        positionBestScore = score
                        bestMeal = candidate
                    End If
                Next

                assignments(dayIndex, mealTypeIndex) = current
                If bestMeal IsNot current Then
                    assignments(dayIndex, mealTypeIndex) = bestMeal
                    usage(current) -= 1
                    usage(bestMeal) += 1
                    bestScore = positionBestScore
                    changed = True
                End If
            Next

            If ImproveWithSwaps(
                assignments,
                meals,
                plannedMealTypes,
                targets,
                usage,
                mealIndexes,
                randomCosts,
                random,
                bestScore
            ) Then
                changed = True
            End If

            If Not changed Then Exit For
        Next
    End Sub

    Private Shared Function ImproveWithSwaps(
        assignments As Meal(,),
        meals As List(Of Meal),
        plannedMealTypes As List(Of String),
        targets As Dictionary(Of String, Double),
        usage As Dictionary(Of Meal, Integer),
        mealIndexes As Dictionary(Of Meal, Integer),
        randomCosts As Double(,,),
        random As Random,
        ByRef bestScore As Double
    ) As Boolean
        Dim changed As Boolean = False
        Dim positionCount = DayNames.Length * plannedMealTypes.Count

        For Each firstPosition In ShuffleIndexes(positionCount, random)
            Dim firstDay = firstPosition \ plannedMealTypes.Count
            Dim firstMealType = firstPosition Mod plannedMealTypes.Count
            Dim firstMeal = assignments(firstDay, firstMealType)

            For Each secondPosition In ShuffleIndexes(positionCount, random)
                If secondPosition <= firstPosition Then Continue For
                Dim secondDay = secondPosition \ plannedMealTypes.Count
                Dim secondMealType = secondPosition Mod plannedMealTypes.Count
                Dim secondMeal = assignments(secondDay, secondMealType)
                If firstMeal Is secondMeal Then Continue For
                If Not SupportsExplicitMealType(
                    firstMeal,
                    plannedMealTypes(secondMealType)
                ) OrElse Not SupportsExplicitMealType(
                    secondMeal,
                    plannedMealTypes(firstMealType)
                ) Then
                    Continue For
                End If

                assignments(firstDay, firstMealType) = secondMeal
                assignments(secondDay, secondMealType) = firstMeal
                Dim score = EvaluatePlan(
                    assignments,
                    meals,
                    targets,
                    usage,
                    mealIndexes,
                    randomCosts
                )
                If score + 0.0000001 < bestScore Then
                    bestScore = score
                    changed = True
                    firstMeal = assignments(firstDay, firstMealType)
                Else
                    assignments(firstDay, firstMealType) = firstMeal
                    assignments(secondDay, secondMealType) = secondMeal
                End If
            Next
        Next

        Return changed
    End Function

    Private Shared Function CountUsage(
        assignments As Meal(,),
        meals As List(Of Meal)
    ) As Dictionary(Of Meal, Integer)
        Dim usage = meals.ToDictionary(Function(meal) meal, Function(meal) 0)
        For Each meal In assignments
            usage(meal) += 1
        Next
        Return usage
    End Function

    Private Shared Function EvaluatePlan(
        assignments As Meal(,),
        meals As List(Of Meal),
        targets As Dictionary(Of String, Double),
        usage As Dictionary(Of Meal, Integer),
        mealIndexes As Dictionary(Of Meal, Integer),
        randomCosts As Double(,,)
    ) As Double
        Dim dailyCalories(DayNames.Length - 1) As Double
        Dim dailyNutrientScores(DayNames.Length - 1) As Double
        Dim weeklyTotals = targets.Keys.ToDictionary(
            Function(name) name,
            Function(name) 0.0,
            StringComparer.OrdinalIgnoreCase
        )
        Dim dailyTargetError As Double = 0
        Dim duplicateMealPenalty As Double = 0
        Dim randomPreferenceCost As Double = 0

        For dayIndex As Integer = 0 To DayNames.Length - 1
            Dim dayTotals = targets.Keys.ToDictionary(
                Function(name) name,
                Function(name) 0.0,
                StringComparer.OrdinalIgnoreCase
            )

            For mealTypeIndex As Integer = 0 To assignments.GetLength(1) - 1
                Dim meal = assignments(dayIndex, mealTypeIndex)
                dailyCalories(dayIndex) += meal.Calory
                randomPreferenceCost += randomCosts(
                    dayIndex,
                    mealTypeIndex,
                    mealIndexes(meal)
                )
                For Each target In targets
                    Dim amount = GetNutrientAmount(meal, target.Key)
                    dayTotals(target.Key) += amount
                    weeklyTotals(target.Key) += amount
                Next
            Next
            Dim currentDayIndex = dayIndex
            duplicateMealPenalty += Enumerable.Range(
                0,
                assignments.GetLength(1)
            ).
                Select(Function(index) assignments(currentDayIndex, index)).
                GroupBy(Function(meal) meal).
                Sum(Function(group) Math.Max(0, group.Count() - 1))

            If targets.Count > 0 Then
                Dim ratios = targets.Select(
                    Function(target) dayTotals(target.Key) / target.Value
                ).ToList()
                dailyNutrientScores(dayIndex) = ratios.Average()
                dailyTargetError += ratios.Sum(
                    Function(ratio) Math.Pow(ratio - 1, 2)
                ) / targets.Count
            End If
        Next

        Dim weeklyTargetError As Double = 0
        If targets.Count > 0 Then
            weeklyTargetError = targets.Sum(
                Function(target)
                    Dim ratio = weeklyTotals(target.Key) /
                        (target.Value * DayNames.Length)
                    Return Math.Pow(ratio - 1, 2)
                End Function
            ) / targets.Count
        End If

        Dim calorieBalance = NormalizedVariance(dailyCalories)
        Dim nutrientBalance = NormalizedVariance(dailyNutrientScores)
        Dim calorieRangePenalty = RangePenalty(dailyCalories, 0.15)
        Dim nutrientRangePenalty = RangePenalty(dailyNutrientScores, 0.2)

        Dim expectedUsage = assignments.Length / CDbl(meals.Count)
        Dim usageVariance = usage.Values.Average(
            Function(count) Math.Pow(
                (count - expectedUsage) / Math.Max(1, expectedUsage),
                2
            )
        )

        Return weeklyTargetError * 14 +
            (dailyTargetError / DayNames.Length) * 2 +
            calorieBalance * 10 +
            nutrientBalance * 18 +
            calorieRangePenalty * 40 +
            nutrientRangePenalty * 40 +
            duplicateMealPenalty * 20 +
            usageVariance * 0.35 +
            randomPreferenceCost / assignments.Length * RandomPreferenceWeight
    End Function

    Private Shared Function NormalizedVariance(
        values As IEnumerable(Of Double)
    ) As Double
        Dim list = values.ToList()
        If list.Count = 0 Then Return 0
        Dim average = list.Average()
        If Math.Abs(average) < 0.000001 Then Return 0
        Return list.Average(
            Function(value) Math.Pow((value - average) / average, 2)
        )
    End Function

    Private Shared Function RangePenalty(
        values As IEnumerable(Of Double),
        allowedFraction As Double
    ) As Double
        Dim list = values.ToList()
        If list.Count = 0 Then Return 0
        Dim average = list.Average()
        If Math.Abs(average) < 0.000001 Then Return 0
        Dim relativeRange = (list.Max() - list.Min()) / average
        Return Math.Pow(
            Math.Max(0, relativeRange - allowedFraction * 2),
            2
        )
    End Function

    Private Shared Function GetNutrientAmount(
        meal As Meal,
        name As String
    ) As Double
        If meal.Nutritionals Is Nothing Then Return 0
        For Each item In meal.Nutritionals
            If String.Equals(
                item.Key,
                name,
                StringComparison.OrdinalIgnoreCase
            ) Then
                Return item.Value
            End If
        Next
        Return 0
    End Function

    Private Shared Function SupportsExplicitMealType(
        meal As Meal,
        mealType As String
    ) As Boolean
        Return meal IsNot Nothing AndAlso meal.MealTypes IsNot Nothing AndAlso
            meal.MealTypes.Any(
                Function(value) String.Equals(
                    value,
                    mealType,
                    StringComparison.OrdinalIgnoreCase
                )
            )
    End Function

    Private Shared Function Shuffle(Of T)(
        source As IEnumerable(Of T),
        random As Random
    ) As List(Of T)
        Dim items = source.ToList()
        For index As Integer = items.Count - 1 To 1 Step -1
            Dim swapIndex = random.Next(index + 1)
            Dim value = items(index)
            items(index) = items(swapIndex)
            items(swapIndex) = value
        Next
        Return items
    End Function

    Private Shared Function ShuffleIndexes(
        count As Integer,
        random As Random
    ) As List(Of Integer)
        Return Shuffle(Enumerable.Range(0, count), random)
    End Function

    Private Shared Function CreatePlan(
        assignments As Meal(,),
        guaranteedMeals As List(Of Meal),
        plannedMealTypes As List(Of String),
        targets As Dictionary(Of String, Double),
        generationMode As WeekPlanGenerationMode,
        randomSeed As Integer
    ) As WeeklyPlan
        Dim plan As New WeeklyPlan With {
            .GeneratedAt = DateTime.Now,
            .GenerationMode = generationMode.ToString(),
            .PlannedMealTypes = New List(Of String)(plannedMealTypes),
            .RandomSeed = randomSeed,
            .TargetDailyIntakes = New Dictionary(Of String, Double)(
                targets,
                StringComparer.OrdinalIgnoreCase
            ),
            .SelectedRecipeUrls = guaranteedMeals.Select(
                Function(meal) meal.Recipe
            ).Where(Function(url) Not String.IsNullOrWhiteSpace(url)).
                Distinct(StringComparer.OrdinalIgnoreCase).
                ToList(),
            .SelectedRecipeNames = guaranteedMeals.Select(
                Function(meal) meal.Name
            ).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList()
        }

        For dayIndex As Integer = 0 To DayNames.Length - 1
            Dim day As New PlannedDay With {
                .Name = DayNames(dayIndex)
            }
            For mealTypeIndex As Integer = 0 To plannedMealTypes.Count - 1
                Dim meal = assignments(dayIndex, mealTypeIndex)
                day.Meals.Add(
                    New PlannedMeal With {
                        .MealType = plannedMealTypes(mealTypeIndex),
                        .MealName = meal.Name,
                        .RecipeUrl = meal.Recipe,
                        .Calories = meal.Calory,
                        .RecipeServings = Math.Max(1, meal.Servings),
                        .Ingredients = If(
                            meal.Ingredients,
                            New List(Of RecipeIngredient)
                        ).Where(
                            Function(ingredient) ingredient IsNot Nothing
                        ).Select(
                            Function(ingredient) ingredient.Clone()
                        ).ToList(),
                        .Nutritionals = New Dictionary(Of String, Double)(
                            If(
                                meal.Nutritionals,
                                New Dictionary(Of String, Double)
                            ),
                            StringComparer.OrdinalIgnoreCase
                        )
                    }
                )
            Next
            plan.Days.Add(day)
        Next

        Return plan
    End Function
End Class
