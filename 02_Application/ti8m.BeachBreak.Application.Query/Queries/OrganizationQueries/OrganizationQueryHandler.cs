using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.OrganizationQueries;

public class OrganizationQueryHandler :
    IQueryHandler<OrganizationListQuery, Result<IEnumerable<Organization>>>,
    IQueryHandler<OrganizationQuery, Result<Organization?>>,
    IQueryHandler<OrganizationByNumberQuery, Result<Organization?>>
{
    private readonly IOrganizationRepository repository;
    private readonly ILogger<OrganizationQueryHandler> logger;

    public OrganizationQueryHandler(IOrganizationRepository repository, ILogger<OrganizationQueryHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<Organization>>> HandleAsync(OrganizationListQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting organization list query with filters - IncludeDeleted: {IncludeDeleted}, IncludeIgnored: {IncludeIgnored}, ParentId: {ParentId}, ManagerId: {ManagerId}",
            query.IncludeDeleted, query.IncludeIgnored, query.ParentId, query.ManagerId);

        try
        {
            IEnumerable<OrganizationReadModel> organizationReadModels;

            if (query.ParentId.HasValue)
            {
                organizationReadModels = await repository.GetOrganizationsByParentIdAsync(
                    query.ParentId, query.IncludeDeleted, query.IncludeIgnored, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(query.ManagerId))
            {
                organizationReadModels = await repository.GetOrganizationsByManagerIdAsync(
                    query.ManagerId, query.IncludeDeleted, query.IncludeIgnored, cancellationToken);
            }
            else
            {
                organizationReadModels = await repository.GetAllOrganizationsAsync(
                    query.IncludeDeleted, query.IncludeIgnored, cancellationToken);
            }

            var organizations = organizationReadModels.Select(MapToOrganization);

            logger.LogInformation("Successfully retrieved {Count} organizations", organizations.Count());
            return Result<IEnumerable<Organization>>.Success(organizations);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while retrieving organization list");
            return Result<IEnumerable<Organization>>.Fail("An error occurred while retrieving organizations", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<Organization?>> HandleAsync(OrganizationQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting organization query for Id: {Id}", query.Id);

        try
        {
            var organizationReadModel = await repository.GetOrganizationByIdAsync(query.Id, cancellationToken);

            if (organizationReadModel == null)
            {
                logger.LogWarning("Organization with Id {Id} not found", query.Id);
                return Result<Organization?>.Success(null);
            }

            var organization = MapToOrganization(organizationReadModel);

            logger.LogInformation("Successfully retrieved organization with Id: {Id}", query.Id);
            return Result<Organization?>.Success(organization);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while retrieving organization with Id: {Id}", query.Id);
            return Result<Organization?>.Fail($"An error occurred while retrieving organization with Id: {query.Id}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<Organization?>> HandleAsync(OrganizationByNumberQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting organization query for Number: {Number}", query.Number);

        try
        {
            var organizationsReadModels = await repository.GetAllOrganizationsAsync(true, true, cancellationToken);
            var organizationReadModel = organizationsReadModels.FirstOrDefault(o => o.Number == query.Number);

            if (organizationReadModel == null)
            {
                logger.LogWarning("Organization with Number {Number} not found", query.Number);
                return Result<Organization?>.Success(null);
            }

            var organization = MapToOrganization(organizationReadModel);

            logger.LogInformation("Successfully retrieved organization with Number: {Number}", query.Number);
            return Result<Organization?>.Success(organization);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while retrieving organization with Number: {Number}", query.Number);
            return Result<Organization?>.Fail($"An error occurred while retrieving organization with Number: {query.Number}", StatusCodes.Status500InternalServerError);
        }
    }

    private static Organization MapToOrganization(OrganizationReadModel readModel)
    {
        return new Organization
        {
            Id = readModel.Id,
            Number = readModel.Number,
            ManagerId = readModel.ManagerId,
            ParentId = readModel.ParentId,
            Name = readModel.Name,
            IsIgnored = readModel.IsIgnored,
            IsDeleted = readModel.IsDeleted
        };
    }
}