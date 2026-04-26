namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

/// <summary>
/// Configuration for a binary (Yes/No) question.
/// Always renders exactly two radio button options. Labels default to "Yes"/"No" but can be overridden.
/// </summary>
public sealed class BinaryConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.Binary;
    public string OptionALabelEnglish { get; set; } = "Yes";
    public string OptionALabelGerman { get; set; } = "Ja";
    public string OptionBLabelEnglish { get; set; } = "No";
    public string OptionBLabelGerman { get; set; } = "Nein";

    /// <summary>When true, one option must be selected for the question to be considered complete.</summary>
    public bool IsRequired { get; set; } = false;

    public bool IsValid() => true;
}
