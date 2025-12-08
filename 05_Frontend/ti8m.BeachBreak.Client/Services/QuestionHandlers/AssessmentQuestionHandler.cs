using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services.QuestionHandlers;

/// <summary>
/// Handler for Assessment question type.
/// Manages evaluations with rating scales - can be completed by employee or manager.
/// </summary>
public class AssessmentQuestionHandler : IQuestionTypeHandler
{
    public QuestionType SupportedType => QuestionType.Assessment;

    public void InitializeQuestion(QuestionItem question)
    {
        // Initialize with one default evaluation and rating scale settings
        question.Configuration = new AssessmentConfiguration
        {
            Evaluations = new List<EvaluationItem>
            {
                new EvaluationItem("evaluation_1", "", "", false, 0)
            },
            RatingScale = 4,
            ScaleLowLabel = "Poor",
            ScaleHighLabel = "Excellent"
        };
    }

    public void AddItem(QuestionItem question)
    {
        if (question.Configuration is AssessmentConfiguration config)
        {
            var nextOrder = config.Evaluations.Count > 0 ? config.Evaluations.Max(e => e.Order) + 1 : 0;
            var newEvaluation = new EvaluationItem(
                $"evaluation_{config.Evaluations.Count + 1}",
                "",
                "",
                false,
                nextOrder
            );

            config.Evaluations.Add(newEvaluation);
        }
    }

    public void RemoveItem(QuestionItem question, int index)
    {
        if (question.Configuration is AssessmentConfiguration config)
        {
            if (index >= 0 && index < config.Evaluations.Count)
            {
                config.Evaluations.RemoveAt(index);

                // Reorder remaining evaluations
                for (int i = 0; i < config.Evaluations.Count; i++)
                {
                    config.Evaluations[i].Order = i;
                }
            }
        }
    }

    public int GetItemCount(QuestionItem question)
    {
        if (question.Configuration is AssessmentConfiguration config)
        {
            return config.Evaluations.Count;
        }
        return 0;
    }

    public List<string> Validate(QuestionItem question, string questionLabel)
    {
        var errors = new List<string>();

        if (question.Configuration is AssessmentConfiguration config)
        {
            if (config.Evaluations.Count == 0)
            {
                errors.Add($"{questionLabel} must have at least one evaluation");
            }
            else
            {
                for (int i = 0; i < config.Evaluations.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(config.Evaluations[i].TitleEnglish))
                    {
                        errors.Add($"Evaluation {i + 1} in {questionLabel} requires a title");
                    }
                }
            }
        }

        return errors;
    }

    public string GetDefaultTitle()
    {
        return "Competency Assessment";
    }
}
