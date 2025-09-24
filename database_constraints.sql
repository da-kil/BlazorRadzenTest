-- Database constraints and validations for Questionnaire State Management
-- This script adds constraints to enforce business rules at the database level

-- Add check constraint to ensure status is valid (0=Draft, 1=Published, 2=Archived)
ALTER TABLE questionnaire_templates
ADD CONSTRAINT check_template_status
CHECK (status IN (0, 1, 2));

-- Add check constraint to ensure assignment status is valid
-- (0=Assigned, 1=InProgress, 2=Completed, 3=Overdue, 4=Cancelled)
ALTER TABLE questionnaire_assignments
ADD CONSTRAINT check_assignment_status
CHECK (status IN (0, 1, 2, 3, 4));

-- Add constraint to ensure published_date is set when status is Published
-- Note: This is a soft constraint since we can't easily check this with standard SQL
-- The application logic enforces this

-- Add foreign key constraint to ensure assignments reference valid templates
ALTER TABLE questionnaire_assignments
ADD CONSTRAINT fk_assignment_template
FOREIGN KEY (template_id) REFERENCES questionnaire_templates(id) ON DELETE CASCADE;

-- Add index on template status for better performance on status-based queries
CREATE INDEX IF NOT EXISTS idx_questionnaire_templates_status
ON questionnaire_templates(status);

-- Add index on assignment status for better performance on assignment queries
CREATE INDEX IF NOT EXISTS idx_questionnaire_assignments_status
ON questionnaire_assignments(status);

-- Add index on template_id in assignments for better performance on active assignment checks
CREATE INDEX IF NOT EXISTS idx_questionnaire_assignments_template_status
ON questionnaire_assignments(template_id, status);

-- Add index on created_at and updated_at for better performance on time-based queries
CREATE INDEX IF NOT EXISTS idx_questionnaire_templates_created_at
ON questionnaire_templates(created_at DESC);

CREATE INDEX IF NOT EXISTS idx_questionnaire_templates_updated_at
ON questionnaire_templates(updated_at DESC);

-- Add partial index for active assignments (more efficient for business rule checks)
CREATE INDEX IF NOT EXISTS idx_questionnaire_assignments_active
ON questionnaire_assignments(template_id)
WHERE status IN (0, 1, 3); -- Assigned, InProgress, Overdue

-- Add constraint to ensure due_date is in the future for new assignments
-- Note: This is handled in application logic since it's time-dependent

-- Add constraint to ensure published_by is not null when status is Published
-- Note: This would require a complex check constraint, handled in application logic

COMMENT ON TABLE questionnaire_templates IS 'Questionnaire templates with three states: Draft (0), Published (1), Archived (2)';
COMMENT ON COLUMN questionnaire_templates.status IS 'Template status: 0=Draft (editable, not assignable), 1=Published (read-only, assignable), 2=Archived (read-only, not assignable)';
COMMENT ON TABLE questionnaire_assignments IS 'Questionnaire assignments to employees';
COMMENT ON COLUMN questionnaire_assignments.status IS 'Assignment status: 0=Assigned, 1=InProgress, 2=Completed, 3=Overdue, 4=Cancelled';