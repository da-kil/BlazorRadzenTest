using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using ti8m.BeachBreak.Application.Query.Models;
using ti8m.BeachBreak.QueryApi.Dto;
using AssessmentConfig = ti8m.BeachBreak.Core.Domain.QuestionConfiguration.AssessmentConfiguration;
using DtoQuestionType = ti8m.BeachBreak.QueryApi.Dto.QuestionType;

namespace ti8m.BeachBreak.QueryApi.Services.Pdf;

public class QuestionnairePdfDocument
{
    private static readonly Color PrimaryColor = new(0xFF1A5276);
    private static readonly Color SectionHeadingColor = new(0xFF1B4F72);
    private static readonly Color TableHeaderColor = new(0xFF2E86C1);
    private static readonly Color TableHeaderTextColor = Colors.White;
    private static readonly Color ZebraColor = new(0xFFEBF5FB);
    private static readonly Color NoteCardBorderColor = new(0xFFAED6F1);
    private static readonly Color NoteCardBgColor = new(0xFFF4F6F7);
    private static readonly Color FooterColor = new(0xFF7F8C8D);
    private static readonly Color SubtleGray = new(0xFFD5D8DC);
    private static readonly Color RuleGray = new(0xFFBFC9CA);
    private static readonly Color NoteMetaColor = new(0xFF5D6D7E);

    private const double FontSizeBody = 9;
    private const double FontSizeSmall = 8;
    private const double FontSizeHeading = 11;
    private const double FontSizeTitle = 16;
    private const string FontName = "Arial";

    // A4 content width at 1.5cm margins = 18.0cm
    private const double ContentWidth = 18.0;

    private readonly QuestionnairePdfData data;
    private readonly PdfLabels labels;

    public QuestionnairePdfDocument(QuestionnairePdfData data, PdfLabels labels)
    {
        this.data = data;
        this.labels = labels;
    }

    private string ManagerName =>
        data.Assignment.ManagerSubmittedByEmployeeName
        ?? data.Assignment.FinalizedByEmployeeName
        ?? "—";

    public byte[] GeneratePdf()
    {
        var document = BuildDocument();
        var renderer = new PdfDocumentRenderer { Document = document };
        renderer.RenderDocument();

        using var stream = new MemoryStream();
        renderer.PdfDocument.Save(stream, false);
        return stream.ToArray();
    }

    private Document BuildDocument()
    {
        var document = new Document();

        var normalStyle = document.Styles["Normal"];
        normalStyle.Font.Name = FontName;
        normalStyle.Font.Size = FontSizeBody;

        var section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.TopMargin = Unit.FromCentimeter(3.2);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(2.0);
        section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
        section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);

        BuildHeader(section);
        BuildFooter(section);
        BuildContent(section);

        return document;
    }

    // ─── Header ────────────────────────────────────────────────────────────

    private void BuildHeader(Section section)
    {
        var header = section.Headers.Primary;

        var table = header.AddTable();
        table.Borders.Visible = false;
        table.AddColumn(Unit.FromCentimeter(12.0));
        table.AddColumn(Unit.FromCentimeter(6.0));

        var row = table.AddRow();
        row.VerticalAlignment = VerticalAlignment.Bottom;

        var empPara = row.Cells[0].AddParagraph(data.Assignment.EmployeeName);
        empPara.Format.Font.Bold = true;
        empPara.Format.Font.Size = FontSizeTitle;
        empPara.Format.Font.Color = PrimaryColor;
        empPara.Format.Font.Name = FontName;

        var templateName = data.Language == Core.Domain.QuestionConfiguration.Language.German
            ? data.Template.NameGerman
            : data.Template.NameEnglish;
        var tmplPara = row.Cells[1].AddParagraph(templateName);
        tmplPara.Format.Font.Italic = true;
        tmplPara.Format.Font.Size = 10;
        tmplPara.Format.Font.Color = PrimaryColor;
        tmplPara.Format.Font.Name = FontName;
        tmplPara.Format.Alignment = ParagraphAlignment.Right;

        var rule = header.AddParagraph();
        rule.Format.Borders.Bottom.Width = Unit.FromPoint(1.5);
        rule.Format.Borders.Bottom.Color = PrimaryColor;
        rule.Format.SpaceBefore = Unit.FromPoint(4);
    }

    // ─── Footer ────────────────────────────────────────────────────────────

    private void BuildFooter(Section section)
    {
        var footer = section.Footers.Primary;

        var rule = footer.AddParagraph();
        rule.Format.Borders.Top.Width = Unit.FromPoint(0.5);
        rule.Format.Borders.Top.Color = RuleGray;

        var table = footer.AddTable();
        table.Borders.Visible = false;
        table.AddColumn(Unit.FromCentimeter(6.0));
        table.AddColumn(Unit.FromCentimeter(6.0));
        table.AddColumn(Unit.FromCentimeter(6.0));

        var row = table.AddRow();
        row.Format.Font.Size = 7;
        row.Format.Font.Color = FooterColor;
        row.Format.Font.Name = FontName;

        row.Cells[0].AddParagraph($"{labels.GeneratedPrefix}{data.GeneratedAt:yyyy-MM-dd HH:mm} UTC");

        var footerTemplateName = data.Language == Core.Domain.QuestionConfiguration.Language.German
            ? data.Template.NameGerman
            : data.Template.NameEnglish;
        var centerPara = row.Cells[1].AddParagraph($"{data.Assignment.EmployeeName} \u2014 {footerTemplateName}");
        centerPara.Format.Alignment = ParagraphAlignment.Center;

        var rightPara = row.Cells[2].AddParagraph();
        rightPara.Format.Alignment = ParagraphAlignment.Right;
        rightPara.AddText("Page ");
        rightPara.AddPageField();
        rightPara.AddText(" / ");
        rightPara.AddNumPagesField();
    }

    // ─── Content ───────────────────────────────────────────────────────────

    private void BuildContent(Section section)
    {
        BuildCoverBlock(section);

        foreach (var sectionDto in data.Template.Sections.OrderBy(s => s.Order))
        {
            data.Response.SectionResponses.TryGetValue(sectionDto.Id, out var sectionResponse);
            BuildSectionBlock(section, sectionDto, sectionResponse);
        }

        if (data.Assignment.InReviewNotes.Count > 0)
            BuildInReviewNotes(section);

        if (!string.IsNullOrWhiteSpace(data.Assignment.ManagerReviewSummary))
            BuildSummarySection(section, labels.ReviewSummary, data.Assignment.ManagerReviewSummary!);

        if (!string.IsNullOrWhiteSpace(data.Assignment.ManagerFinalNotes))
            BuildSummarySection(section, labels.ManagerFinalNotes, data.Assignment.ManagerFinalNotes!);

        BuildSignOffSection(section);
    }

    // ─── Cover block ───────────────────────────────────────────────────────

    private void BuildCoverBlock(Section section)
    {
        // 4-column table: [label] [value] [label] [value]
        var table = section.AddTable();
        table.Borders.Width = Unit.FromPoint(1);
        table.Borders.Color = SubtleGray;
        table.TopPadding = Unit.FromPoint(3);
        table.BottomPadding = Unit.FromPoint(3);

        table.AddColumn(Unit.FromCentimeter(2.5));
        table.AddColumn(Unit.FromCentimeter(6.5));
        table.AddColumn(Unit.FromCentimeter(2.5));
        table.AddColumn(Unit.FromCentimeter(6.5));

        // Heading row (spans all 4 columns)
        var headRow = table.AddRow();
        headRow.Cells[0].MergeRight = 3;
        headRow.Cells[0].Borders.Bottom.Width = Unit.FromPoint(0.5);
        headRow.Cells[0].Borders.Bottom.Color = SubtleGray;

        var headPara = headRow.Cells[0].AddParagraph(labels.AssignmentDetails);
        headPara.Format.Font.Bold = true;
        headPara.Format.Font.Size = FontSizeHeading;
        headPara.Format.Font.Color = SectionHeadingColor;
        headPara.Format.Font.Name = FontName;

        AddCoverRow(table, labels.Manager, ManagerName, labels.Assigned, data.Assignment.AssignedDate.ToString("yyyy-MM-dd"));
        AddCoverRow(table, labels.Email, data.Assignment.EmployeeEmail, labels.Due, data.Assignment.DueDate?.ToString("yyyy-MM-dd") ?? "—");
        AddCoverRow(table, labels.Role, data.Assignment.EmployeeRole, labels.Finalized, data.Assignment.FinalizedDate?.ToString("yyyy-MM-dd") ?? "—");
        AddCoverRow(table, labels.Organisation, data.Assignment.EmployeeOrganisationName, labels.FinalizedBy, data.Assignment.FinalizedByEmployeeName ?? "—");

        Spacer(section);
    }

    private static void AddCoverRow(Table table, string label1, string value1, string label2, string value2)
    {
        var row = table.AddRow();
        row.Format.Font.Size = FontSizeSmall;
        row.Format.Font.Name = FontName;

        row.Cells[0].AddParagraph(label1 + ":").Format.Font.Bold = true;
        row.Cells[1].AddParagraph(value1);
        row.Cells[2].AddParagraph(label2 + ":").Format.Font.Bold = true;
        row.Cells[3].AddParagraph(value2);
    }

    // ─── Section block ─────────────────────────────────────────────────────

    private void BuildSectionBlock(Section section, QuestionSectionDto sectionDto, SectionResponseDto? sectionResponse)
    {
        var sectionTitle = data.Language == Core.Domain.QuestionConfiguration.Language.German
            ? sectionDto.TitleGerman
            : sectionDto.TitleEnglish;
        var heading = section.AddParagraph(sectionTitle);
        heading.Format.Font.Bold = true;
        heading.Format.Font.Size = FontSizeHeading;
        heading.Format.Font.Color = SectionHeadingColor;
        heading.Format.Font.Name = FontName;
        heading.Format.Borders.Bottom.Width = Unit.FromPoint(1.5);
        heading.Format.Borders.Bottom.Color = SectionHeadingColor;
        heading.Format.SpaceBefore = Unit.FromPoint(12);
        heading.Format.SpaceAfter = Unit.FromPoint(6);

        switch (sectionDto.Type)
        {
            case DtoQuestionType.Assessment:
                BuildAssessmentSection(section, sectionDto, sectionResponse, labels, data.Language);
                break;
            case DtoQuestionType.Goal:
                BuildGoalSection(section, sectionResponse);
                break;
            default:
                BuildTextSection(section, sectionResponse, labels);
                break;
        }
    }

    private static void BuildAssessmentSection(Section section, QuestionSectionDto sectionDto, SectionResponseDto? sectionResponse, PdfLabels labels, Core.Domain.QuestionConfiguration.Language language)
    {
        if (sectionDto.Configuration is not AssessmentConfig config || config.Evaluations.Count == 0)
        {
            NoData(section, labels.NoAssessmentItems);
            return;
        }

        // Scale legend
        var scalePara = section.AddParagraph($"{labels.ScalePrefix}{config.GetRatingScaleDescription()}");
        scalePara.Format.Font.Italic = true;
        scalePara.Format.Font.Size = FontSizeSmall;
        scalePara.Format.Font.Name = FontName;
        scalePara.Format.Font.Color = NoteMetaColor;
        scalePara.Format.SpaceAfter = Unit.FromPoint(4);

        Dictionary<Guid, QuestionResponseDto>? empResponses = null;
        Dictionary<Guid, QuestionResponseDto>? mgrResponses = null;
        sectionResponse?.RoleResponses.TryGetValue(ResponseRole.Employee, out empResponses);
        sectionResponse?.RoleResponses.TryGetValue(ResponseRole.Manager, out mgrResponses);

        var empData = empResponses?.Values.FirstOrDefault()?.ResponseData as AssessmentResponseDataDto;
        var mgrData = mgrResponses?.Values.FirstOrDefault()?.ResponseData as AssessmentResponseDataDto;

        // Columns: Competency(7) | Emp(2) | Mgr(2) | EmpComment(3.5) | MgrComment(3.5) = 18cm
        var table = section.AddTable();
        table.Borders.Visible = false;
        table.AddColumn(Unit.FromCentimeter(7.0));
        table.AddColumn(Unit.FromCentimeter(2.0));
        table.AddColumn(Unit.FromCentimeter(2.0));
        table.AddColumn(Unit.FromCentimeter(3.5));
        table.AddColumn(Unit.FromCentimeter(3.5));

        TableHeader(table, labels.Competency, labels.EmployeeAbbr, labels.ManagerAbbr, labels.EmployeeComment, labels.ManagerComment);

        var idx = 0;
        foreach (var item in config.Evaluations.OrderBy(e => e.Order))
        {
            EvaluationRatingDto? empRating = null;
            EvaluationRatingDto? mgrRating = null;
            empData?.Evaluations.TryGetValue(item.Key, out empRating);
            mgrData?.Evaluations.TryGetValue(item.Key, out mgrRating);

            var row = DataRow(table, idx++);
            var itemTitle = language == Core.Domain.QuestionConfiguration.Language.German ? item.TitleGerman : item.TitleEnglish;
            var itemDesc = language == Core.Domain.QuestionConfiguration.Language.German ? item.DescriptionGerman : item.DescriptionEnglish;
            CompetencyCell(row.Cells[0], itemTitle, itemDesc);
            CenteredSmallCell(row.Cells[1], empRating?.Rating > 0 ? empRating.Rating.ToString() : "—");
            CenteredSmallCell(row.Cells[2], mgrRating?.Rating > 0 ? mgrRating.Rating.ToString() : "—");
            SmallCell(row.Cells[3], empRating?.Comment ?? "");
            SmallCell(row.Cells[4], mgrRating?.Comment ?? "");
        }
    }

    private static void BuildTextSection(Section section, SectionResponseDto? sectionResponse, PdfLabels labels)
    {
        Dictionary<Guid, QuestionResponseDto>? empResponses = null;
        Dictionary<Guid, QuestionResponseDto>? mgrResponses = null;
        sectionResponse?.RoleResponses.TryGetValue(ResponseRole.Employee, out empResponses);
        sectionResponse?.RoleResponses.TryGetValue(ResponseRole.Manager, out mgrResponses);

        var empText = (empResponses?.Values.FirstOrDefault()?.ResponseData as TextResponseDataDto)?.TextSections.FirstOrDefault();
        var mgrText = (mgrResponses?.Values.FirstOrDefault()?.ResponseData as TextResponseDataDto)?.TextSections.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(empText) && string.IsNullOrWhiteSpace(mgrText))
        {
            NoData(section, labels.NoResponses);
            return;
        }

        if (!string.IsNullOrWhiteSpace(empText))
            RoleParagraph(section, labels.EmployeeLabel, empText);

        if (!string.IsNullOrWhiteSpace(mgrText))
            RoleParagraph(section, labels.ManagerLabel, mgrText);
    }

    private void BuildGoalSection(Section section, SectionResponseDto? sectionResponse)
    {
        Dictionary<Guid, QuestionResponseDto>? empResponses = null;
        sectionResponse?.RoleResponses.TryGetValue(ResponseRole.Employee, out empResponses);
        var goalData = empResponses?.Values.FirstOrDefault()?.ResponseData as GoalResponseDataDto;
        var goals = goalData?.Goals ?? [];
        var ratings = goalData?.PredecessorRatings ?? [];

        SubHeading(section, labels.CurrentGoals);

        if (goals.Count == 0)
        {
            NoData(section, labels.NoGoals);
        }
        else
        {
            // Columns: Objective(7) | From(3) | To(3) | Weight(2) | AddedBy(3) = 18cm
            var table = section.AddTable();
            table.Borders.Visible = false;
            table.AddColumn(Unit.FromCentimeter(7.0));
            table.AddColumn(Unit.FromCentimeter(3.0));
            table.AddColumn(Unit.FromCentimeter(3.0));
            table.AddColumn(Unit.FromCentimeter(2.0));
            table.AddColumn(Unit.FromCentimeter(3.0));

            TableHeader(table, labels.Objective, labels.From, labels.To, labels.Weight, labels.AddedBy);

            var idx = 0;
            foreach (var goal in goals)
            {
                var row = DataRow(table, idx++);
                SmallCell(row.Cells[0], goal.ObjectiveDescription);
                SmallCell(row.Cells[1], goal.TimeframeFrom.ToString("yyyy-MM-dd"));
                SmallCell(row.Cells[2], goal.TimeframeTo.ToString("yyyy-MM-dd"));
                CenteredSmallCell(row.Cells[3], $"{goal.WeightingPercentage:F0}%");
                SmallCell(row.Cells[4], goal.AddedByRole == ApplicationRole.Employee
                    ? data.Assignment.EmployeeName
                    : ManagerName);

            }
        }

        if (ratings.Count > 0)
        {
            SubHeading(section, labels.PredecessorRatings, topSpace: 8);

            // Columns: Objective(7.2) | DoA(1.8) | Justification(7.2) | RatedBy(1.8) = 18cm
            var table = section.AddTable();
            table.Borders.Visible = false;
            table.AddColumn(Unit.FromCentimeter(7.2));
            table.AddColumn(Unit.FromCentimeter(1.8));
            table.AddColumn(Unit.FromCentimeter(7.2));
            table.AddColumn(Unit.FromCentimeter(1.8));

            TableHeader(table, labels.Objective, labels.DegreeOfAchievement, labels.Justification, labels.RatedBy);

            var idx = 0;
            foreach (var rating in ratings)
            {
                var row = DataRow(table, idx++);
                SmallCell(row.Cells[0], rating.OriginalObjective);
                CenteredSmallCell(row.Cells[1], rating.DegreeOfAchievement.ToString());
                SmallCell(row.Cells[2], rating.Justification);
                SmallCell(row.Cells[3], rating.RatedByRole.ToString());
            }
        }
    }

    private void BuildInReviewNotes(Section section)
    {
        SectionHeading(section, labels.InReviewNotes);

        foreach (var note in data.Assignment.InReviewNotes.OrderBy(n => n.Timestamp))
        {
            var noteTable = section.AddTable();
            noteTable.Borders.Width = Unit.FromPoint(1);
            noteTable.Borders.Color = NoteCardBorderColor;
            noteTable.TopPadding = Unit.FromPoint(8);
            noteTable.BottomPadding = Unit.FromPoint(8);
            noteTable.LeftPadding = Unit.FromPoint(8);
            noteTable.RightPadding = Unit.FromPoint(8);
            noteTable.AddColumn(Unit.FromCentimeter(ContentWidth));

            var noteRow = noteTable.AddRow();
            noteRow.Shading.Color = NoteCardBgColor;

            // Author + timestamp on same line using tab stop
            var headerPara = noteRow.Cells[0].AddParagraph();
            headerPara.Format.TabStops.ClearAll();
            headerPara.Format.TabStops.AddTabStop(Unit.FromCentimeter(ContentWidth - 0.3), TabAlignment.Right);
            var authorText = headerPara.AddFormattedText(note.AuthorName, TextFormat.Bold);
            authorText.Font.Size = FontSizeSmall;
            authorText.Font.Name = FontName;
            headerPara.AddTab();
            var timeText = headerPara.AddFormattedText(note.Timestamp.ToString("yyyy-MM-dd HH:mm"));
            timeText.Font.Size = FontSizeSmall;
            timeText.Font.Color = FooterColor;

            if (!string.IsNullOrWhiteSpace(note.SectionTitle))
            {
                var sectionPara = noteRow.Cells[0].AddParagraph($"{labels.SectionPrefix}{note.SectionTitle}");
                sectionPara.Format.Font.Italic = true;
                sectionPara.Format.Font.Size = FontSizeSmall;
                sectionPara.Format.Font.Color = NoteMetaColor;
            }

            var contentPara = noteRow.Cells[0].AddParagraph(note.Content);
            contentPara.Format.Font.Size = FontSizeSmall;
            contentPara.Format.SpaceBefore = Unit.FromPoint(3);

            Spacer(section, 6);
        }
    }

    private void BuildSummarySection(Section section, string heading, string content)
    {
        SectionHeading(section, heading, borderWidth: 1.0);

        var contentPara = section.AddParagraph(content);
        contentPara.Format.Font.Size = FontSizeBody;
        contentPara.Format.Font.Name = FontName;
    }

    // ─── Shared helpers ────────────────────────────────────────────────────

    private void SectionHeading(Section section, string text, double borderWidth = 1.5)
    {
        var heading = section.AddParagraph(text);
        heading.Format.Font.Bold = true;
        heading.Format.Font.Size = FontSizeHeading;
        heading.Format.Font.Color = SectionHeadingColor;
        heading.Format.Font.Name = FontName;
        heading.Format.Borders.Bottom.Width = Unit.FromPoint(borderWidth);
        heading.Format.Borders.Bottom.Color = SectionHeadingColor;
        heading.Format.SpaceBefore = Unit.FromPoint(12);
        heading.Format.SpaceAfter = Unit.FromPoint(6);
    }

    private static void SubHeading(Section section, string text, double topSpace = 0)
    {
        var para = section.AddParagraph(text);
        para.Format.Font.Bold = true;
        para.Format.Font.Size = FontSizeSmall;
        para.Format.Font.Name = FontName;
        if (topSpace > 0) para.Format.SpaceBefore = Unit.FromPoint(topSpace);
        para.Format.SpaceAfter = Unit.FromPoint(4);
    }

    private static void NoData(Section section, string message)
    {
        var para = section.AddParagraph(message);
        para.Format.Font.Italic = true;
        para.Format.Font.Size = FontSizeSmall;
    }

    private static void RoleParagraph(Section section, string label, string text)
    {
        var para = section.AddParagraph();
        var labelText = para.AddFormattedText(label + " ", TextFormat.Bold);
        labelText.Font.Size = FontSizeSmall;
        labelText.Font.Name = FontName;
        var bodyText = para.AddFormattedText(text);
        bodyText.Font.Size = FontSizeSmall;
        para.Format.SpaceAfter = Unit.FromPoint(4);
    }

    private static void TableHeader(Table table, params string[] headings)
    {
        var headerRow = table.AddRow();
        headerRow.HeadingFormat = true;
        headerRow.Shading.Color = TableHeaderColor;
        headerRow.Format.Font.Bold = true;
        headerRow.Format.Font.Size = FontSizeSmall;
        headerRow.Format.Font.Color = TableHeaderTextColor;
        headerRow.Format.Font.Name = FontName;
        headerRow.TopPadding = Unit.FromPoint(4);
        headerRow.BottomPadding = Unit.FromPoint(4);

        for (var i = 0; i < headings.Length; i++)
            headerRow.Cells[i].AddParagraph(headings[i]);
    }

    private static Row DataRow(Table table, int idx)
    {
        var row = table.AddRow();
        if (idx % 2 == 1) row.Shading.Color = ZebraColor;
        row.TopPadding = Unit.FromPoint(3);
        row.BottomPadding = Unit.FromPoint(3);
        return row;
    }

    private static void SmallCell(Cell cell, string text)
    {
        var para = cell.AddParagraph(text);
        para.Format.Font.Size = FontSizeSmall;
        para.Format.Font.Name = FontName;
    }

    private static void CenteredSmallCell(Cell cell, string text)
    {
        var para = cell.AddParagraph(text);
        para.Format.Font.Size = FontSizeSmall;
        para.Format.Font.Name = FontName;
        para.Format.Alignment = ParagraphAlignment.Center;
    }

    private static void Spacer(Section section, double points = 12)
    {
        var spacer = section.AddParagraph();
        spacer.Format.SpaceBefore = Unit.FromPoint(points);
        spacer.Format.SpaceAfter = Unit.FromPoint(0);
    }

    private static void CompetencyCell(Cell cell, string title, string? description)
    {
        var titlePara = cell.AddParagraph(title);
        titlePara.Format.Font.Size = FontSizeSmall;
        titlePara.Format.Font.Name = FontName;

        if (!string.IsNullOrWhiteSpace(description))
        {
            var descPara = cell.AddParagraph(description);
            descPara.Format.Font.Size = 7;
            descPara.Format.Font.Name = FontName;
            descPara.Format.Font.Italic = true;
            descPara.Format.Font.Color = NoteMetaColor;
            descPara.Format.SpaceBefore = Unit.FromPoint(1);
        }
    }

    private void BuildSignOffSection(Section section)
    {
        SectionHeading(section, labels.SignOff, borderWidth: 1.0);

        var employeeDate = data.Assignment.EmployeeReviewConfirmedDate
                           ?? data.Assignment.EmployeeSubmittedDate;

        // Label(3cm) | Employee(7.5cm) | Manager(7.5cm) = 18cm
        var table = section.AddTable();
        table.Borders.Width = Unit.FromPoint(0.5);
        table.Borders.Color = SubtleGray;
        table.TopPadding = Unit.FromPoint(4);
        table.BottomPadding = Unit.FromPoint(4);
        table.AddColumn(Unit.FromCentimeter(3.0));
        table.AddColumn(Unit.FromCentimeter(7.5));
        table.AddColumn(Unit.FromCentimeter(7.5));

        var headerRow = table.AddRow();
        headerRow.Shading.Color = TableHeaderColor;
        headerRow.Format.Font.Bold = true;
        headerRow.Format.Font.Size = FontSizeSmall;
        headerRow.Format.Font.Color = TableHeaderTextColor;
        headerRow.Format.Font.Name = FontName;
        headerRow.Cells[0].AddParagraph("");
        headerRow.Cells[1].AddParagraph(labels.Employee);
        headerRow.Cells[2].AddParagraph(labels.Manager);

        var nameRow = table.AddRow();
        nameRow.Format.Font.Size = FontSizeSmall;
        nameRow.Format.Font.Name = FontName;
        nameRow.Cells[0].AddParagraph(labels.Name).Format.Font.Bold = true;
        nameRow.Cells[1].AddParagraph(data.Assignment.EmployeeName);
        nameRow.Cells[2].AddParagraph(ManagerName);

        var dateRow = table.AddRow();
        dateRow.Shading.Color = ZebraColor;
        dateRow.Format.Font.Size = FontSizeSmall;
        dateRow.Format.Font.Name = FontName;
        dateRow.Cells[0].AddParagraph(labels.Date).Format.Font.Bold = true;
        dateRow.Cells[1].AddParagraph(employeeDate.HasValue ? employeeDate.Value.ToString("yyyy-MM-dd HH:mm") + " UTC" : "—");
        dateRow.Cells[2].AddParagraph(data.Assignment.FinalizedDate.HasValue ? data.Assignment.FinalizedDate.Value.ToString("yyyy-MM-dd HH:mm") + " UTC" : "—");

        var sigRow = table.AddRow();
        sigRow.Height = Unit.FromCentimeter(1.5);
        sigRow.Format.Font.Size = FontSizeSmall;
        sigRow.Format.Font.Name = FontName;
        sigRow.Cells[0].AddParagraph(labels.Signature).Format.Font.Bold = true;
        sigRow.Cells[1].AddParagraph("");
        sigRow.Cells[2].AddParagraph("");
    }
}
