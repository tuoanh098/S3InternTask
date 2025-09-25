using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Course
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Title { get; set; } = default!;

    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
}
