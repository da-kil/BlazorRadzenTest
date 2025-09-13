using System.ComponentModel.DataAnnotations;

namespace ti8m.BeachBreak.CommandApi.Dto;

public class CategoryDto
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string NameEn { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string NameDe { get; set; } = string.Empty;

    [StringLength(500)]
    public string DescriptionEn { get; set; } = string.Empty;

    [StringLength(500)]
    public string DescriptionDe { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0;
}