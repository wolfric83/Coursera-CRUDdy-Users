using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.DTOs;

public class UpdateUserDto
{
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "First name cannot be empty or whitespace only.")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Last name cannot be empty or whitespace only.")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Email cannot be empty or whitespace only.")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Department cannot be empty or whitespace only.")]
    public string Department { get; set; } = string.Empty;
}
