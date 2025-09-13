using Npgsql;
using Microsoft.AspNetCore.Http;

namespace ti8m.BeachBreak.Application.Command.Commands.CategoryCommands;

public class CategoryCommandHandler :
    ICommandHandler<CreateCategoryCommand, Result>,
    ICommandHandler<UpdateCategoryCommand, Result>,
    ICommandHandler<DeleteCategoryCommand, Result>
{
    private readonly NpgsqlDataSource dataSource;

    public CategoryCommandHandler(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<Result> HandleAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var id = Guid.NewGuid();

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                INSERT INTO categories (id, name, description, is_active, created_date, last_modified, sort_order)
                VALUES (@id, @name, @description, @is_active, @created_date, @last_modified, @sort_order)
                """;

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", command.Category.Name);
            cmd.Parameters.AddWithValue("@description", command.Category.Description);
            cmd.Parameters.AddWithValue("@is_active", command.Category.IsActive);
            cmd.Parameters.AddWithValue("@created_date", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@last_modified", (object?)command.Category.LastModified ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sort_order", command.Category.SortOrder);

            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create category: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                UPDATE categories
                SET name = @name,
                    description = @description,
                    is_active = @is_active,
                    last_modified = @last_modified,
                    sort_order = @sort_order
                WHERE id = @id
                """;

            cmd.Parameters.AddWithValue("@id", command.Category.Id);
            cmd.Parameters.AddWithValue("@name", command.Category.Name);
            cmd.Parameters.AddWithValue("@description", command.Category.Description);
            cmd.Parameters.AddWithValue("@is_active", command.Category.IsActive);
            cmd.Parameters.AddWithValue("@last_modified", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@sort_order", command.Category.SortOrder);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                return Result.Fail($"Category with ID {command.Category.Id} not found", StatusCodes.Status404NotFound);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update category: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                DELETE FROM categories
                WHERE id = @id
                """;

            cmd.Parameters.AddWithValue("@id", command.CategoryId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                return Result.Fail($"Category with ID {command.CategoryId} not found", StatusCodes.Status404NotFound);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete category: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }
}