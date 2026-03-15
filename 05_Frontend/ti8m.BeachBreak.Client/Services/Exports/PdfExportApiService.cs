using System.Net.Http.Json;
using Microsoft.JSInterop;
using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.Dto;

namespace ti8m.BeachBreak.Client.Services.Exports;

public class PdfExportApiService : BaseApiService, IPdfExportApiService
{
    private readonly IJSRuntime _jsRuntime;

    public PdfExportApiService(IHttpClientFactory factory, IJSRuntime jsRuntime) : base(factory)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> ExportSinglePdfAsync(Guid assignmentId, string suggestedFileName, PdfLanguage language = PdfLanguage.English, CancellationToken ct = default)
    {
        var response = await HttpQueryClient.GetAsync($"q/api/v1/pdf-export/assignments/{assignmentId}?language={(int)language}", ct);

        if (!response.IsSuccessStatusCode)
            return false;

        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        var base64 = Convert.ToBase64String(bytes);

        await _jsRuntime.InvokeVoidAsync("downloadBase64File", ct, base64, suggestedFileName, "application/pdf");
        return true;
    }

    public async Task<bool> ExportBulkPdfAsync(IEnumerable<Guid> assignmentIds, PdfLanguage language = PdfLanguage.English, CancellationToken ct = default)
    {
        var idList = assignmentIds.ToList();
        var requestBody = new BulkPdfExportRequestDto { AssignmentIds = idList, Language = (int)language };

        var response = await HttpQueryClient.PostAsJsonAsync("q/api/v1/pdf-export/assignments/bulk", requestBody, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            Console.WriteLine($"[BulkPdfExport] {response.StatusCode}: {errorBody}");
            return false;
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        var base64 = Convert.ToBase64String(bytes);
        var fileName = $"Questionnaire_Export_{DateTime.Now:yyyy-MM-dd}.zip";

        await _jsRuntime.InvokeVoidAsync("downloadBase64File", ct, base64, fileName, "application/zip");
        return true;
    }
}
