using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<StudentCourse> StudentCourses => Set<StudentCourse>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Student

        b.Entity<Student>(e =>
        {
            e.ToTable("students");
            e.HasKey(x => x.Id);

            e.Property(x => x.FullName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(255);

            e.Property(x => x.EnrolledAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            // ✅ Concurrency via TIMESTAMP(6); default on insert, auto-update on UPDATE
            e.Property(x => x.UpdatedAt)
                .HasColumnType("timestamp(6)")
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            // DO NOT call HasComputedColumnSql() here.
        });

        // Course
        b.Entity<Course>(e =>
        {
            e.ToTable("courses");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(120);
        });

        // StudentCourse (composite key)
        b.Entity<StudentCourse>(e =>
        {
            e.ToTable("student_courses");
            e.HasKey(x => new { x.StudentId, x.CourseId });

            e.HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Course)
                .WithMany(c => c.StudentCourses)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(x => x.RegisteredAt)
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });

        // Seed data (deterministic timestamps)
        b.Entity<Student>().HasData(
            new Student { Id = 1, FullName = "Alice Nguyen", Email = "alice@example.com", EnrolledAt = new DateTime(2024,1,1,0,0,0, DateTimeKind.Utc) },
            new Student { Id = 2, FullName = "Bob Tran",    Email = "bob@example.com",   EnrolledAt = new DateTime(2024,1,2,0,0,0, DateTimeKind.Utc) }
        );

        b.Entity<Course>().HasData(
            new Course { Id = 1, Title = "Piano 101" },
            new Course { Id = 2, Title = "Guitar 101" }
        );

        b.Entity<StudentCourse>().HasData(
            new StudentCourse { StudentId = 1, CourseId = 1, RegisteredAt = new DateTime(2024,1,3,0,0,0, DateTimeKind.Utc) },
            new StudentCourse { StudentId = 2, CourseId = 2, RegisteredAt = new DateTime(2024,1,4,0,0,0, DateTimeKind.Utc) }
        );
    }
}
