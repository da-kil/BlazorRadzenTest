using Microsoft.Extensions.Logging;
using Npgsql;

namespace ti8m.BeachBreak.Core.Infrastructure.Database;

public class DatabaseInitializer
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(NpgsqlDataSource dataSource, ILogger<DatabaseInitializer> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Starting database initialization...");
            
            await using var connection = await _dataSource.OpenConnectionAsync();
            await CreateTablesAsync(connection);
            
            _logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database initialization");
            throw;
        }
    }

    private async Task CreateTablesAsync(NpgsqlConnection connection)
    {
        await CreateCategoriesTableAsync(connection);
        await CreateQuestionnaireTemplatesTableAsync(connection);
        await CreateEmployeesTableAsync(connection);
        await CreateAssignmentsTableAsync(connection);
    }

    private async Task CreateQuestionnaireTemplatesTableAsync(NpgsqlConnection connection)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS questionnaire_templates (
                id UUID PRIMARY KEY,
                name VARCHAR(255) NOT NULL,
                description TEXT,
                category VARCHAR(100),
                is_active BOOLEAN NOT NULL DEFAULT true,
                sections JSONB NOT NULL,
                settings JSONB NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL,
                updated_at TIMESTAMP WITH TIME ZONE NOT NULL
            );

            -- Add the is_active column if it doesn't exist (for existing tables)
            DO $$
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                              WHERE table_name = 'questionnaire_templates' AND column_name = 'is_active') THEN
                    ALTER TABLE questionnaire_templates ADD COLUMN is_active BOOLEAN NOT NULL DEFAULT true;
                END IF;
            END $$;

            -- Create indexes for better performance
            CREATE INDEX IF NOT EXISTS idx_questionnaire_templates_name ON questionnaire_templates(name);
            CREATE INDEX IF NOT EXISTS idx_questionnaire_templates_category ON questionnaire_templates(category);
            CREATE INDEX IF NOT EXISTS idx_questionnaire_templates_is_active ON questionnaire_templates(is_active);
            CREATE INDEX IF NOT EXISTS idx_questionnaire_templates_created_at ON questionnaire_templates(created_at);
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        _logger.LogInformation("Creating questionnaire_templates table...");
        await command.ExecuteNonQueryAsync();
        _logger.LogInformation("questionnaire_templates table created successfully.");
    }

    private async Task CreateCategoriesTableAsync(NpgsqlConnection connection)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS categories (
                id UUID PRIMARY KEY,
                name_en VARCHAR(100) NOT NULL,
                name_de VARCHAR(100) NOT NULL,
                description_en TEXT,
                description_de TEXT,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                last_modified TIMESTAMP WITH TIME ZONE,
                sort_order INTEGER NOT NULL DEFAULT 0
            );

            -- Create indexes for better performance
            CREATE INDEX IF NOT EXISTS idx_categories_name_en ON categories(name_en);
            CREATE INDEX IF NOT EXISTS idx_categories_name_de ON categories(name_de);
            CREATE INDEX IF NOT EXISTS idx_categories_is_active ON categories(is_active);
            CREATE INDEX IF NOT EXISTS idx_categories_sort_order ON categories(sort_order);

            -- Insert some default categories if they don't exist
            INSERT INTO categories (id, name_en, name_de, description_en, description_de, is_active, created_date, sort_order)
            SELECT * FROM (VALUES
                ('A84F5D93-EDC4-4F8E-BB30-F03DC7A983C2'::uuid, 'Performance Review', 'Leistungsbeurteilung', 'Annual and periodic performance evaluations', 'Jährliche und regelmäßige Leistungsbewertungen', true, NOW(), 10),
                ('2C2419A0-5368-4F3C-B112-84CF9F2AAB66'::uuid, 'Employee Feedback', 'Mitarbeiterfeedback', 'Employee satisfaction and feedback surveys', 'Mitarbeiterzufriedenheit und Feedback-Umfragen', true, NOW(), 20),
                ('C24C4AF5-B66F-4FE4-9229-4C7BF3FCAA33'::uuid, 'Training Assessment', 'Schulungsbewertung', 'Training effectiveness and skill assessments', 'Schulungseffektivität und Kompetenzbewertungen', true, NOW(), 30),
                ('D0238E7D-863A-4915-9472-1C2E46D33806'::uuid, 'Goal Setting', 'Zielsetzung', 'Objective setting and goal planning', 'Zielsetzung und Zielplanung', true, NOW(), 40),
                ('62FF06CC-CD5D-4332-89EB-93B2A4B18842'::uuid, 'Skills Assessment', 'Kompetenzbewertung', 'Competency and skills evaluation', 'Kompetenz- und Fertigkeitsbewertung', true, NOW(), 50),
                ('68772D86-9737-4D71-8B1C-005BE5AA257D'::uuid, 'Team Evaluation', 'Teambewertung', 'Team performance and collaboration assessment', 'Teamleistung und Bewertung der Zusammenarbeit', true, NOW(), 60),
                ('3EDD49B9-16FF-470D-9554-FE930958DBAE'::uuid, 'Exit Interview', 'Austrittsgespräch', 'Employee departure feedback collection', 'Sammlung von Feedback beim Mitarbeiterausscheiden', true, NOW(), 70),
                ('EBBC50BC-1642-4E3B-A756-EEB731ECB8F4'::uuid, 'Onboarding', 'Einarbeitung', 'New employee orientation and feedback', 'Einführung neuer Mitarbeiter und Feedback', true, NOW(), 80),
                ('078821A0-6C8F-4B5F-9EA7-88318424FC78'::uuid, 'Other', 'Sonstiges', 'Miscellaneous questionnaires', 'Verschiedene Fragebögen', true, NOW(), 90)
            ) AS default_categories(id, name_en, name_de, description_en, description_de, is_active, created_date, sort_order)
            WHERE NOT EXISTS (SELECT 1 FROM categories WHERE name_en = default_categories.name_en);
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        _logger.LogInformation("Creating categories table...");
        await command.ExecuteNonQueryAsync();
        _logger.LogInformation("Categories table created successfully.");
    }

    private async Task CreateEmployeesTableAsync(NpgsqlConnection connection)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS employees (
                id UUID PRIMARY KEY,
                first_name VARCHAR(100) NOT NULL,
                last_name VARCHAR(100) NOT NULL,
                role VARCHAR(100) NOT NULL,
                email VARCHAR(255) NOT NULL,
                start_date DATE NOT NULL,
                end_date DATE,
                last_start_date DATE,
                manager_id UUID,
                manager VARCHAR(200) NOT NULL,
                login_name VARCHAR(100) NOT NULL,
                employee_number VARCHAR(50) NOT NULL,
                organization_number INTEGER NOT NULL,
                organization VARCHAR(200) NOT NULL,
                is_deleted BOOLEAN NOT NULL DEFAULT false,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
            );

            -- Create indexes for better performance
            CREATE INDEX IF NOT EXISTS idx_employees_first_name ON employees(first_name);
            CREATE INDEX IF NOT EXISTS idx_employees_last_name ON employees(last_name);
            CREATE INDEX IF NOT EXISTS idx_employees_email ON employees(email);
            CREATE INDEX IF NOT EXISTS idx_employees_employee_number ON employees(employee_number);
            CREATE INDEX IF NOT EXISTS idx_employees_organization_number ON employees(organization_number);
            CREATE INDEX IF NOT EXISTS idx_employees_manager_id ON employees(manager_id);
            CREATE INDEX IF NOT EXISTS idx_employees_is_deleted ON employees(is_deleted);

            -- Insert synthetic employee data if they don't exist
            INSERT INTO employees (id, first_name, last_name, role, email, start_date, end_date, last_start_date, manager_id, manager, login_name, employee_number, organization_number, organization, is_deleted)
            SELECT * FROM (VALUES
                ('b0f388c2-6294-4116-a8b2-eccafa29b3fb'::uuid, 'John', 'Smith', 'CEO', 'john.smith@company.com', '2020-01-01'::date, NULL::date, NULL::date, NULL::uuid, '', 'jsmith', 'EMP001', 1000, 'Executive Management', false),
                ('104cc744-c766-4d5c-8f4d-f6da4577a4ef'::uuid, 'Sarah', 'Johnson', 'CTO', 'sarah.johnson@company.com', '2020-02-15'::date, NULL::date, NULL::date, 'b0f388c2-6294-4116-a8b2-eccafa29b3fb'::uuid, 'John Smith', 'sjohnson', 'EMP002', 1000, 'Executive Management', false),
                ('edc6894b-aad8-4691-8ba6-350fe532e052'::uuid, 'Michael', 'Brown', 'VP Engineering', 'michael.brown@company.com', '2020-03-01'::date, NULL::date, NULL::date, '104cc744-c766-4d5c-8f4d-f6da4577a4ef'::uuid, 'Sarah Johnson', 'mbrown', 'EMP003', 2000, 'Engineering', false),
                ('5c438c77-9f19-4ecb-9d1a-e9d8a09b4dcb'::uuid, 'Emily', 'Davis', 'Senior Developer', 'emily.davis@company.com', '2021-01-15'::date, NULL::date, NULL::date, 'edc6894b-aad8-4691-8ba6-350fe532e052'::uuid, 'Michael Brown', 'edavis', 'EMP004', 2000, 'Engineering', false),
                ('5ef0ebe1-3745-4066-a902-a0edac23da33'::uuid, 'David', 'Wilson', 'Senior Developer', 'david.wilson@company.com', '2021-02-01'::date, NULL::date, NULL::date, 'edc6894b-aad8-4691-8ba6-350fe532e052'::uuid, 'Michael Brown', 'dwilson', 'EMP005', 2000, 'Engineering', false),
                ('2ee3dee7-b580-4dd2-98e1-6dd4a148ec7f'::uuid, 'Lisa', 'Miller', 'Product Manager', 'lisa.miller@company.com', '2021-03-15'::date, NULL::date, NULL::date, '104cc744-c766-4d5c-8f4d-f6da4577a4ef'::uuid, 'Sarah Johnson', 'lmiller', 'EMP006', 3000, 'Product Management', false),
                ('2f0f172b-8960-45f7-bca6-4d59136eba71'::uuid, 'James', 'Garcia', 'UX Designer', 'james.garcia@company.com', '2021-04-01'::date, NULL::date, NULL::date, '2ee3dee7-b580-4dd2-98e1-6dd4a148ec7f'::uuid, 'Lisa Miller', 'jgarcia', 'EMP007', 3000, 'Product Management', false),
                ('a830022c-d377-4909-98c9-2d4e88bfe9d5'::uuid, 'Jennifer', 'Martinez', 'QA Engineer', 'jennifer.martinez@company.com', '2021-05-15'::date, NULL::date, NULL::date, 'edc6894b-aad8-4691-8ba6-350fe532e052'::uuid, 'Michael Brown', 'jmartinez', 'EMP008', 2000, 'Engineering', false),
                ('0d04e8cf-e5b7-42ac-af4a-7662444ce55c'::uuid, 'Robert', 'Anderson', 'DevOps Engineer', 'robert.anderson@company.com', '2021-06-01'::date, NULL::date, NULL::date, 'edc6894b-aad8-4691-8ba6-350fe532e052'::uuid, 'Michael Brown', 'randerson', 'EMP009', 2000, 'Engineering', false),
                ('06b7f4cb-3896-4552-b914-147e01a33ee3'::uuid, 'Michelle', 'Taylor', 'HR Manager', 'michelle.taylor@company.com', '2020-07-01'::date, NULL::date, NULL::date, 'b0f388c2-6294-4116-a8b2-eccafa29b3fb'::uuid, 'John Smith', 'mtaylor', 'EMP010', 4000, 'Human Resources', false),
                ('da34ee86-c247-46cf-b7c5-f8ab9b1978a7'::uuid, 'Christopher', 'Thomas', 'Finance Manager', 'christopher.thomas@company.com', '2020-08-15'::date, NULL::date, NULL::date, 'b0f388c2-6294-4116-a8b2-eccafa29b3fb'::uuid, 'John Smith', 'cthomas', 'EMP011', 5000, 'Finance', false),
                ('99637a70-da14-4af1-a65e-b5ddea51e4a3'::uuid, 'Amanda', 'Jackson', 'Marketing Manager', 'amanda.jackson@company.com', '2021-07-01'::date, NULL::date, NULL::date, 'b0f388c2-6294-4116-a8b2-eccafa29b3fb'::uuid, 'John Smith', 'ajackson', 'EMP012', 6000, 'Marketing', false),
                ('5785e9b3-adbd-4d62-a950-47f3886971a5'::uuid, 'Daniel', 'White', 'Sales Manager', 'daniel.white@company.com', '2021-08-15'::date, NULL::date, NULL::date, 'b0f388c2-6294-4116-a8b2-eccafa29b3fb'::uuid, 'John Smith', 'dwhite', 'EMP013', 7000, 'Sales', false),
                ('773f2ca1-e300-4658-bd0e-054009dbd98e'::uuid, 'Ashley', 'Harris', 'Junior Developer', 'ashley.harris@company.com', '2022-01-15'::date, NULL::date, NULL::date, '5c438c77-9f19-4ecb-9d1a-e9d8a09b4dcb'::uuid, 'Emily Davis', 'aharris', 'EMP014', 2000, 'Engineering', false),
                ('073d3dc3-d6fc-41b0-8f98-425e10f82342'::uuid, 'Matthew', 'Clark', 'Junior Developer', 'matthew.clark@company.com', '2022-02-01'::date, NULL::date, NULL::date, '5ef0ebe1-3745-4066-a902-a0edac23da33'::uuid, 'David Wilson', 'mclark', 'EMP015', 2000, 'Engineering', false),
                ('3cec7c5c-7e56-45a6-bfa9-6811e4802cb3'::uuid, 'Jessica', 'Lewis', 'Business Analyst', 'jessica.lewis@company.com', '2022-03-15'::date, NULL::date, NULL::date, '2ee3dee7-b580-4dd2-98e1-6dd4a148ec7f'::uuid, 'Lisa Miller', 'jlewis', 'EMP016', 3000, 'Product Management', false),
                ('47393fdd-e994-43a4-8ccb-e11360b319c4'::uuid, 'Andrew', 'Robinson', 'Technical Writer', 'andrew.robinson@company.com', '2022-04-01'::date, NULL::date, NULL::date, '2f0f172b-8960-45f7-bca6-4d59136eba71'::uuid, 'James Garcia', 'arobinson', 'EMP017', 3000, 'Product Management', false),
                ('49d2fa92-758b-453e-a4cf-2009101717e0'::uuid, 'Nicole', 'Walker', 'Data Analyst', 'nicole.walker@company.com', '2022-05-15'::date, NULL::date, NULL::date, 'edc6894b-aad8-4691-8ba6-350fe532e052'::uuid, 'Michael Brown', 'nwalker', 'EMP018', 2000, 'Engineering', false),
                ('24bbb5d3-dca3-4d77-8adf-efed334f5c7a'::uuid, 'Ryan', 'Hall', 'Security Engineer', 'ryan.hall@company.com', '2022-06-01'::date, NULL::date, NULL::date, '0d04e8cf-e5b7-42ac-af4a-7662444ce55c'::uuid, 'Robert Anderson', 'rhall', 'EMP019', 2000, 'Engineering', false),
                ('b0df0da8-b1f3-4c15-8cb0-63b473dd9bcd'::uuid, 'Stephanie', 'Allen', 'Customer Success Manager', 'stephanie.allen@company.com', '2022-07-15'::date, NULL::date, NULL::date, '5785e9b3-adbd-4d62-a950-47f3886971a5'::uuid, 'Daniel White', 'sallen', 'EMP020', 7000, 'Sales', false),
                ('e4cc9b6f-5b0f-4c30-ac79-6a478725b685'::uuid, 'Kevin', 'Young', 'Operations Manager', 'kevin.young@company.com', '2021-09-01'::date, NULL::date, NULL::date, 'b0f388c2-6294-4116-a8b2-eccafa29b3fb'::uuid, 'John Smith', 'kyoung', 'EMP021', 8000, 'Operations', false),
                ('360c147b-43ca-4877-9af5-d0cd804d3a92'::uuid, 'Rachel', 'King', 'Compliance Officer', 'rachel.king@company.com', '2022-08-01'::date, NULL::date, NULL::date, 'da34ee86-c247-46cf-b7c5-f8ab9b1978a7'::uuid, 'Christopher Thomas', 'rking', 'EMP022', 5000, 'Finance', false)
            ) AS default_employees(id, first_name, last_name, role, email, start_date, end_date, last_start_date, manager_id, manager, login_name, employee_number, organization_number, organization, is_deleted)
            WHERE NOT EXISTS (SELECT 1 FROM employees WHERE employee_number = default_employees.employee_number);
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        _logger.LogInformation("Creating employees table...");
        await command.ExecuteNonQueryAsync();
        _logger.LogInformation("Employees table created successfully with synthetic data.");
    }

    private async Task CreateAssignmentsTableAsync(NpgsqlConnection connection)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS questionnaire_assignments (
                id UUID PRIMARY KEY,
                template_id UUID NOT NULL,
                employee_id UUID NOT NULL,
                employee_name VARCHAR(200) NOT NULL,
                employee_email VARCHAR(255) NOT NULL,
                assigned_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                due_date TIMESTAMP WITH TIME ZONE,
                completed_date TIMESTAMP WITH TIME ZONE,
                status VARCHAR(50) NOT NULL DEFAULT 'Assigned',
                assigned_by VARCHAR(200),
                notes TEXT,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
            );

            -- Create indexes for better performance
            CREATE INDEX IF NOT EXISTS idx_assignments_template_id ON questionnaire_assignments(template_id);
            CREATE INDEX IF NOT EXISTS idx_assignments_employee_id ON questionnaire_assignments(employee_id);
            CREATE INDEX IF NOT EXISTS idx_assignments_status ON questionnaire_assignments(status);
            CREATE INDEX IF NOT EXISTS idx_assignments_assigned_date ON questionnaire_assignments(assigned_date);
            CREATE INDEX IF NOT EXISTS idx_assignments_due_date ON questionnaire_assignments(due_date);
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        _logger.LogInformation("Creating questionnaire_assignments table...");
        await command.ExecuteNonQueryAsync();
        _logger.LogInformation("Questionnaire assignments table created successfully with sample data.");
    }
}