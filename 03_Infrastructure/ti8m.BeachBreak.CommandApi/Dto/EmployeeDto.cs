using System.ComponentModel.DataAnnotations;

namespace ti8m.BeachBreak.CommandApi.Dto;

public class EmployeeDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Role { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string EMail { get; set; } = string.Empty;

    [Required]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateOnly? LastStartDate { get; set; }

    public Guid? ManagerId { get; set; }

    [StringLength(200)]
    public string Manager { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LoginName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required]
    public int OrganizationNumber { get; set; }

    [Required]
    [StringLength(200)]
    public string Organization { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;
}