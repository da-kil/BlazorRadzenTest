using Microsoft.Extensions.DependencyInjection;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Extension methods for registering questionnaire services following the separated concerns pattern
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the new separated questionnaire services (recommended approach)
    /// </summary>
    public static IServiceCollection AddQuestionnaireServices(this IServiceCollection services)
    {
        // Register the specialized services
        services.AddScoped<IQuestionnaireTemplateService, QuestionnaireTemplateService>();
        services.AddScoped<IQuestionnaireAssignmentService, QuestionnaireAssignmentService>();
        services.AddScoped<IQuestionnaireResponseService, QuestionnaireResponseService>();

        return services;
    }

}