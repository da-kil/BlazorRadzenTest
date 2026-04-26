namespace ti8m.BeachBreak.Client.Models;

public sealed class BinaryConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.Binary;
    public string OptionALabelEnglish { get; set; } = "Yes";
    public string OptionALabelGerman { get; set; } = "Ja";
    public string OptionBLabelEnglish { get; set; } = "No";
    public string OptionBLabelGerman { get; set; } = "Nein";
    public bool IsRequired { get; set; } = false;
}
