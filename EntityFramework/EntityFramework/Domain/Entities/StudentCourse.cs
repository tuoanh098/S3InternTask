using System;

public class StudentCourse
{
    public int StudentId { get; set; }
    public Student Student { get; set; } = default!;
    public int CourseId { get; set; }
    public Course Course { get; set; } = default!;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
