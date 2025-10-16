namespace ti8m.BeachBreak.CommandApi.Dto;

public class SubmitQuestionnaireDto
{
    public string SubmittedBy { get; set; } = string.Empty;
    public int ExpectedVersion { get; set; }
}
