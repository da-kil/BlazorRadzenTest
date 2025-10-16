using ti8m.BeachBreak.Client.Models;
using QuestionCardTypes = ti8m.BeachBreak.Client.Components.QuestionnaireBuilder.QuestionCard;

namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

/// <summary>
/// Handler for Goal Achievement question type.
/// Manages goal categories for performance review.
/// </summary>
public class GoalAchievementQuestionHandler : IQuestionTypeHandler
{
    private readonly QuestionConfigurationService configService;

    public GoalAchievementQuestionHandler(QuestionConfigurationService configService)
    {
        this.configService = configService;
    }

    public QuestionType SupportedType => QuestionType.GoalAchievement;

    public void InitializeQuestion(QuestionItem question)
    {
        // Initialize with one default goal category
        var goalCategories = new List<QuestionCardTypes.GoalCategory>
        {
            new QuestionCardTypes.GoalCategory
            {
                Title = "",
                Description = "",
                IsRequired = false,
                Order = 0
            }
        };
        configService.SetGoalCategories(question, goalCategories);
    }

    public void AddItem(QuestionItem question)
    {
        var goalCategories = configService.GetGoalCategories(question);
        var nextOrder = goalCategories.Count > 0 ? goalCategories.Max(g => g.Order) + 1 : 0;
        var newCategory = new QuestionCardTypes.GoalCategory
        {
            Title = "",
            Description = "",
            IsRequired = false,
            Order = nextOrder
        };

        // Create a new list to ensure change detection
        var updatedCategories = new List<QuestionCardTypes.GoalCategory>(goalCategories) { newCategory };
        configService.SetGoalCategories(question, updatedCategories);
    }

    public void RemoveItem(QuestionItem question, int index)
    {
        var goalCategories = configService.GetGoalCategories(question);
        if (index >= 0 && index < goalCategories.Count)
        {
            goalCategories.RemoveAt(index);

            // Reorder remaining categories
            for (int i = 0; i < goalCategories.Count; i++)
            {
                goalCategories[i].Order = i;
            }

            configService.SetGoalCategories(question, goalCategories);
        }
    }

    public int GetItemCount(QuestionItem question)
    {
        return configService.GetGoalCategories(question).Count;
    }

    public List<string> Validate(QuestionItem question, string questionLabel)
    {
        var errors = new List<string>();
        var goalCategories = configService.GetGoalCategories(question);

        if (goalCategories.Count == 0)
        {
            errors.Add($"{questionLabel} must have at least one goal category");
        }
        else
        {
            for (int i = 0; i < goalCategories.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(goalCategories[i].Title))
                {
                    errors.Add($"Goal category {i + 1} in {questionLabel} requires a title");
                }
            }
        }

        return errors;
    }

    public string GetDefaultTitle()
    {
        return "Goal Achievement Review";
    }
}
