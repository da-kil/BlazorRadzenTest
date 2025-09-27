using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.OrganizationAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.OrganizationCommands;

public class OrganizationCommandHandler :
    ICommandHandler<BulkImportOrganizationCommand, Result>,
    ICommandHandler<BulkUpdateOrganizationsCommand, Result>,
    ICommandHandler<BulkDeleteOrganizationsCommand, Result>,
    ICommandHandler<IgnoreOrganizationCommand, Result>
{
    private static readonly Guid namespaceGuid = new("7A3D6DB8-32B3-4903-A2EA-C3925FAA31B9");
    private readonly IOrganizationAggregateRepository repository;
    private readonly ILogger<OrganizationCommandHandler> logger;

    public OrganizationCommandHandler(IOrganizationAggregateRepository repository, ILogger<OrganizationCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(BulkImportOrganizationCommand command, CancellationToken cancellationToken = default)
    {
        int countInserted = 0;
        int countUpdated = 0;
        int countUndeleted = 0;
        int countDeleted = 0;
        foreach (var o in command.Organizations)
        {
            Guid orgId = Deterministic.Create(namespaceGuid, o.Number);
            Guid? parentOrgId = o.ParentNumber == null ? null : Deterministic.Create(namespaceGuid, o.ParentNumber);

            var organization = await repository.LoadAsync<Organization>(orgId, cancellationToken: cancellationToken);

            if (organization is not null && organization.IsDeleted)
            {
                logger.LogInformation("Undeleting Organization {OrgId}, {OrgNumber}", organization.Id, organization.Number);

                organization.Undelete();
                UpdateOrganization(o, parentOrgId, organization);

                if (organization.UncommittedEvents.Any())
                {
                    await repository.StoreAsync(organization, cancellationToken);
                    countUndeleted++;
                }
            }
            else if (organization is not null)
            {
                logger.LogInformation("Update organization {OrganizationId}", organization.Id);

                UpdateOrganization(o, parentOrgId, organization);

                if (organization.UncommittedEvents.Any())
                {
                    await repository.StoreAsync(organization, cancellationToken);
                    countUpdated++;
                }
            }
            else
            {
                organization = new Organization(orgId, o.Number, o.ManagerUserId, parentOrgId, o.Name);
                logger.LogInformation("Insert organization {OrganizationId}", organization.Id);
                await repository.StoreAsync(organization, cancellationToken);

                countInserted++;
            }
        }

        var ids = command.Organizations.Select(o => Deterministic.Create(namespaceGuid, o.Number)).ToArray();
        var organizationsToDelete = await repository.FindEntriesToDeleteAsync<Employee>(ids, cancellationToken: cancellationToken);

        if (organizationsToDelete is not null)
        {
            foreach (var organization in organizationsToDelete)
            {
                organization.Delete();
                await repository.StoreAsync(organization, cancellationToken);
                countDeleted++;
            }
        }

        logger.LogInformation("Organization bulk import inserted: {CountInserted}, undeleted: {CountUndeleted}, updated: {countUpdated}, deleted: {CountDeleted}", countInserted, countUndeleted, countUpdated, countDeleted);
        return Result.Success();
    }

    public async Task<Result> HandleAsync(BulkUpdateOrganizationsCommand command, CancellationToken cancellationToken = default)
    {
        foreach (var o in command.Organizations)
        {
            Guid orgId = Deterministic.Create(namespaceGuid, o.Number);
            Guid? parentOrgId = o.ParentNumber == null ? null : Deterministic.Create(namespaceGuid, o.ParentNumber);

            var organization = await repository.LoadRequiredAsync<Organization>(orgId);
            UpdateOrganization(o, parentOrgId, organization);

            await repository.StoreAsync(organization, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> HandleAsync(BulkDeleteOrganizationsCommand command, CancellationToken cancellationToken = default)
    {
        foreach (var number in command.OrgNumbers)
        {
            var orgId = Deterministic.Create(namespaceGuid, number);
            var organization = await repository.LoadRequiredAsync<Organization>(orgId);

            organization.Delete();
            await repository.StoreAsync(organization, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> HandleAsync(IgnoreOrganizationCommand command, CancellationToken cancellationToken = default)
    {
        var organization = await repository.LoadRequiredAsync<Organization>(command.OrganizationId);

        organization.Ignore();

        await repository.StoreAsync(organization, cancellationToken);
        return Result.Success();
    }

    private static void UpdateOrganization(SyncOrganization o, Guid? parentOrgId, Organization organization)
    {
        organization.ChangeName(o.Name);
        organization.ChangeManager(o.ManagerUserId);
        organization.ChangeParentOrganization(parentOrgId);
    }
}