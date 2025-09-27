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
        logger.LogOrganizationListQueryStarting(query.IncludeDeleted, query.IncludeIgnored, query.ParentId, query.ManagerId);

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
            var organizationCount = organizations.Count();

            logger.LogOrganizationListQuerySucceeded(organizationCount);
            return Result<IEnumerable<Organization>>.Success(organizations);
        }
        catch (Exception ex)
        {
            logger.LogOrganizationListQueryFailed(ex);
            return Result<IEnumerable<Organization>>.Fail("An error occurred while retrieving organizations", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<Organization?>> HandleAsync(OrganizationQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogOrganizationQueryStarting(query.Id);

        try
        {
            var organizationReadModel = await repository.GetOrganizationByIdAsync(query.Id, cancellationToken);

            if (organizationReadModel == null)
            {
                logger.LogOrganizationNotFound(query.Id);
                return Result<Organization?>.Success(null);
            }

            var organization = MapToOrganization(organizationReadModel);

            logger.LogOrganizationQuerySucceeded(query.Id);
            return Result<Organization?>.Success(organization);
        }
        catch (Exception ex)
        {
            logger.LogOrganizationQueryFailed(query.Id, ex);
            return Result<Organization?>.Fail($"An error occurred while retrieving organization with Id: {query.Id}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<Organization?>> HandleAsync(OrganizationByNumberQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogOrganizationByNumberQueryStarting(query.Number);

        try
        {
            var organizationsReadModels = await repository.GetAllOrganizationsAsync(true, true, cancellationToken);
            var organizationReadModel = organizationsReadModels.FirstOrDefault(o => o.Number == query.Number);

            if (organizationReadModel == null)
            {
                logger.LogOrganizationByNumberNotFound(query.Number);
                return Result<Organization?>.Success(null);
            }

            var organization = MapToOrganization(organizationReadModel);

            logger.LogOrganizationByNumberQuerySucceeded(query.Number);
            return Result<Organization?>.Success(organization);
        }
        catch (Exception ex)
        {
            logger.LogOrganizationByNumberQueryFailed(query.Number, ex);
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