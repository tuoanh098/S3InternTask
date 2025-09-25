using System.ComponentModel.DataAnnotations;

public class Student
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string FullName { get; set; } = default!;
    [EmailAddress, MaxLength(255)] public string? Email { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    [ConcurrencyCheck]
    public DateTime UpdatedAt { get; private set; } // optional: make setter private
}
