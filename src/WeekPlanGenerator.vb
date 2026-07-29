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
        "Lunch",
        "Brunch",
        "Dinner",
        "Snack"
    }

    Private Const MaximumSelectedMeals As Integer = 35
    Private Const OptimizationPasses As Integer = 10

    Private Sub New()
    End Sub

    Public Shared Function GetMissingMealTypes(
        selectedMeals As IEnumerable(Of Meal)
    ) As List(Of String)
        Dim meals = If(selectedMeals, Enumerable.Empty(Of Meal)).ToList()
        Return MealTypes.Where(
            Function(mealType) Not meals.Any(
                Function(meal) SupportsExplicitMealType(meal, mealType)
            )
        ).ToList()
    End Function

    Public Shared Function Generate(
        selectedMeals As IEnumerable(Of Meal),
        targetDailyIntakes As IDictionary(Of String, Double)
    ) As WeeklyPlan
        Dim meals = If(selectedMeals, Enumerable.Empty(Of Meal)).
            Distinct().
            OrderBy(Function(meal) meal.Name, StringComparer.CurrentCultureIgnoreCase).
            ToList()

        If meals.Count = 0 Then
            Throw New WeeklyPlanException(
                "Select at least one recipe before planning the week."
            )
        End If
        If meals.Count > MaximumSelectedMeals Then
            Throw New WeeklyPlanException(
                "A week has 35 meal slots. Select no more than 35 recipes so every selected recipe can appear."
            )
        End If

        Dim missingMealTypes = GetMissingMealTypes(meals)
        If missingMealTypes.Count > 0 Then
            Throw New WeeklyPlanException(
                "The selected recipes do not cover: " &
                String.Join(", ", missingMealTypes) &
                ". Select at least one recipe for every meal type."
            )
        End If

        Dim targets = NormalizeTargets(targetDailyIntakes)
        Dim assignments = CreateInitialAssignments(meals)
        OptimizeAssignments(assignments, meals, targets)
        Return CreatePlan(assignments, meals, targets)
    End Function

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
        meals As List(Of Meal)
    ) As Meal(,)
        Dim positionOwners(DayNames.Length * MealTypes.Length - 1) As Meal
        Dim orderedForMatching = meals.OrderBy(
            Function(meal) MealTypes.Count(
                Function(mealType) SupportsExplicitMealType(meal, mealType)
            )
        ).ThenBy(Function(meal) meal.Name, StringComparer.CurrentCultureIgnoreCase).ToList()

        For Each meal In orderedForMatching
            Dim visited(positionOwners.Length - 1) As Boolean
            If Not TryPlaceRequiredMeal(meal, positionOwners, visited) Then
                Throw New WeeklyPlanException(
                    "The selected recipes cannot all fit into one week while respecting their meal types. " &
                    "Choose fewer recipes from the overrepresented category or recategorize some recipes."
                )
            End If
        Next

        Dim assignments(DayNames.Length - 1, MealTypes.Length - 1) As Meal
        Dim usage = meals.ToDictionary(Function(meal) meal, Function(meal) 0)
        For position As Integer = 0 To positionOwners.Length - 1
            Dim dayIndex = position \ MealTypes.Length
            Dim mealTypeIndex = position Mod MealTypes.Length
            assignments(dayIndex, mealTypeIndex) = positionOwners(position)
            If positionOwners(position) IsNot Nothing Then
                usage(positionOwners(position)) += 1
            End If
        Next

        For dayIndex As Integer = 0 To DayNames.Length - 1
            For mealTypeIndex As Integer = 0 To MealTypes.Length - 1
                If assignments(dayIndex, mealTypeIndex) IsNot Nothing Then Continue For
                Dim currentDayIndex = dayIndex
                Dim currentMealTypeIndex = mealTypeIndex

                Dim candidates = meals.Where(
                    Function(meal) SupportsExplicitMealType(
                        meal,
                        MealTypes(currentMealTypeIndex)
                    )
                ).OrderBy(Function(meal) usage(meal)).
                    ThenBy(
                        Function(meal)
                            Dim mealIndex = meals.IndexOf(meal)
                            Return (
                                mealIndex -
                                currentDayIndex -
                                currentMealTypeIndex +
                                meals.Count
                            ) Mod meals.Count
                        End Function
                    ).ToList()

                Dim selected = candidates(0)
                assignments(dayIndex, mealTypeIndex) = selected
                usage(selected) += 1
            Next
        Next

        Return assignments
    End Function

    Private Shared Function TryPlaceRequiredMeal(
        meal As Meal,
        positionOwners As Meal(),
        visited As Boolean()
    ) As Boolean
        For position As Integer = 0 To positionOwners.Length - 1
            If visited(position) Then Continue For
            Dim mealTypeIndex = position Mod MealTypes.Length
            If Not SupportsExplicitMealType(meal, MealTypes(mealTypeIndex)) Then Continue For

            visited(position) = True
            If positionOwners(position) Is Nothing OrElse
                TryPlaceRequiredMeal(positionOwners(position), positionOwners, visited) Then
                positionOwners(position) = meal
                Return True
            End If
        Next

        Return False
    End Function

    Private Shared Sub OptimizeAssignments(
        assignments As Meal(,),
        meals As List(Of Meal),
        targets As Dictionary(Of String, Double)
    )
        Dim usage = CountUsage(assignments, meals)
        Dim bestScore = EvaluatePlan(assignments, meals, targets, usage)

        For pass As Integer = 1 To OptimizationPasses
            Dim changed As Boolean = False

            For dayIndex As Integer = 0 To DayNames.Length - 1
                For mealTypeIndex As Integer = 0 To MealTypes.Length - 1
                    Dim current = assignments(dayIndex, mealTypeIndex)
                    Dim bestMeal = current
                    Dim positionBestScore = bestScore

                    For Each candidate In meals
                        If candidate Is current OrElse
                            Not SupportsExplicitMealType(
                                candidate,
                                MealTypes(mealTypeIndex)
                            ) Then
                            Continue For
                        End If
                        If usage(current) <= 1 Then Continue For

                        assignments(dayIndex, mealTypeIndex) = candidate
                        usage(current) -= 1
                        usage(candidate) += 1
                        Dim score = EvaluatePlan(assignments, meals, targets, usage)
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
            Next

            If ImproveWithSwaps(
                assignments,
                meals,
                targets,
                usage,
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
        targets As Dictionary(Of String, Double),
        usage As Dictionary(Of Meal, Integer),
        ByRef bestScore As Double
    ) As Boolean
        Dim changed As Boolean = False
        Dim positionCount = DayNames.Length * MealTypes.Length

        For firstPosition As Integer = 0 To positionCount - 2
            Dim firstDay = firstPosition \ MealTypes.Length
            Dim firstMealType = firstPosition Mod MealTypes.Length
            Dim firstMeal = assignments(firstDay, firstMealType)

            For secondPosition As Integer = firstPosition + 1 To positionCount - 1
                Dim secondDay = secondPosition \ MealTypes.Length
                Dim secondMealType = secondPosition Mod MealTypes.Length
                Dim secondMeal = assignments(secondDay, secondMealType)
                If firstMeal Is secondMeal Then Continue For
                If Not SupportsExplicitMealType(
                    firstMeal,
                    MealTypes(secondMealType)
                ) OrElse Not SupportsExplicitMealType(
                    secondMeal,
                    MealTypes(firstMealType)
                ) Then
                    Continue For
                End If

                assignments(firstDay, firstMealType) = secondMeal
                assignments(secondDay, secondMealType) = firstMeal
                Dim score = EvaluatePlan(assignments, meals, targets, usage)
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
        usage As Dictionary(Of Meal, Integer)
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

        For dayIndex As Integer = 0 To DayNames.Length - 1
            Dim dayTotals = targets.Keys.ToDictionary(
                Function(name) name,
                Function(name) 0.0,
                StringComparer.OrdinalIgnoreCase
            )

            For mealTypeIndex As Integer = 0 To MealTypes.Length - 1
                Dim meal = assignments(dayIndex, mealTypeIndex)
                dailyCalories(dayIndex) += meal.Calory
                For Each target In targets
                    Dim amount = GetNutrientAmount(meal, target.Key)
                    dayTotals(target.Key) += amount
                    weeklyTotals(target.Key) += amount
                Next
            Next
            Dim currentDayIndex = dayIndex
            duplicateMealPenalty += Enumerable.Range(0, MealTypes.Length).
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
                    Dim ratio = weeklyTotals(target.Key) / (target.Value * DayNames.Length)
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
            Function(count) Math.Pow((count - expectedUsage) / Math.Max(1, expectedUsage), 2)
        )

        Return weeklyTargetError * 14 +
            (dailyTargetError / DayNames.Length) * 2 +
            calorieBalance * 10 +
            nutrientBalance * 18 +
            calorieRangePenalty * 40 +
            nutrientRangePenalty * 40 +
            duplicateMealPenalty * 20 +
            usageVariance * 0.35
    End Function

    Private Shared Function NormalizedVariance(values As IEnumerable(Of Double)) As Double
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
        Return Math.Pow(Math.Max(0, relativeRange - allowedFraction * 2), 2)
    End Function

    Private Shared Function GetNutrientAmount(meal As Meal, name As String) As Double
        If meal.Nutritionals Is Nothing Then Return 0
        For Each item In meal.Nutritionals
            If String.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase) Then
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

    Private Shared Function CreatePlan(
        assignments As Meal(,),
        selectedMeals As List(Of Meal),
        targets As Dictionary(Of String, Double)
    ) As WeeklyPlan
        Dim plan As New WeeklyPlan With {
            .GeneratedAt = DateTime.Now,
            .TargetDailyIntakes = New Dictionary(Of String, Double)(
                targets,
                StringComparer.OrdinalIgnoreCase
            ),
            .SelectedRecipeUrls = selectedMeals.Select(
                Function(meal) meal.Recipe
            ).Where(Function(url) Not String.IsNullOrWhiteSpace(url)).
                Distinct(StringComparer.OrdinalIgnoreCase).
                ToList(),
            .SelectedRecipeNames = selectedMeals.Select(
                Function(meal) meal.Name
            ).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList()
        }

        For dayIndex As Integer = 0 To DayNames.Length - 1
            Dim day As New PlannedDay With {
                .Name = DayNames(dayIndex)
            }
            For mealTypeIndex As Integer = 0 To MealTypes.Length - 1
                Dim meal = assignments(dayIndex, mealTypeIndex)
                day.Meals.Add(
                    New PlannedMeal With {
                        .MealType = MealTypes(mealTypeIndex),
                        .MealName = meal.Name,
                        .RecipeUrl = meal.Recipe,
                        .Calories = meal.Calory,
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
