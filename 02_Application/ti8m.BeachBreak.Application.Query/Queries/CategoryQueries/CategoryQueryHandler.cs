using Microsoft.Extensions.Logging;
using Npgsql;

namespace ti8m.BeachBreak.Application.Query.Queries.CategoryQueries;

public class CategoryQueryHandler :
    IQueryHandler<CategoryListQuery, Result<IEnumerable<Category>>>,
    IQueryHandler<CategoryQuery, Result<Category>>
{
    private readonly NpgsqlDataSource dataSource;
    private readonly ILogger<CategoryQueryHandler> logger;

    public CategoryQueryHandler(NpgsqlDataSource dataSource, ILogger<CategoryQueryHandler> logger)
    {
        this.dataSource = dataSource;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<Category>>> HandleAsync(CategoryListQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            var whereClause = query.IncludeInactive ? "" : "WHERE is_active = true";

            cmd.CommandText = $"""
                SELECT id, name, description, is_active, created_date, last_modified, sort_order
                FROM categories
                {whereClause}
                ORDER BY sort_order ASC, name ASC
                """;

            var categories = new List<Category>();

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var category = new Category
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    IsActive = reader.GetBoolean(3),
                    CreatedDate = reader.GetDateTime(4),
                    LastModified = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    SortOrder = reader.GetInt32(6)
                };

                categories.Add(category);
            }

            logger.LogInformation("Retrieved {Count} categories", categories.Count);
            return Result<IEnumerable<Category>>.Success(categories);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve categories");
            return Result<IEnumerable<Category>>.Fail($"Failed to retrieve categories: {ex.Message}", 500);
        }
    }

    public async Task<Result<Category>> HandleAsync(CategoryQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                SELECT id, name, description, is_active, created_date, last_modified, sort_order
                FROM categories
                WHERE id = @id
                """;

            cmd.Parameters.AddWithValue("@id", query.CategoryId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var category = new Category
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    IsActive = reader.GetBoolean(3),
                    CreatedDate = reader.GetDateTime(4),
                    LastModified = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    SortOrder = reader.GetInt32(6)
                };

                logger.LogInformation("Retrieved category with ID {Id}", query.CategoryId);
                return Result<Category>.Success(category);
            }

            logger.LogWarning("Category with ID {Id} not found", query.CategoryId);
            return Result<Category>.Fail($"Category with ID {query.CategoryId} not found", 404);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve category with ID {Id}", query.CategoryId);
            return Result<Category>.Fail($"Failed to retrieve category: {ex.Message}", 500);
        }
    }
}