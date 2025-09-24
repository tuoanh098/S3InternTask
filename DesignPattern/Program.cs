using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace PatternsDemo
{
    // ----------- Domain -----------
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; }
    }

    // ----------- Repository Pattern -----------
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
    }

    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new List<User>
        {
            new User{ Id=1, Name="Alice", IsActive=true },
            new User{ Id=2, Name="Bob",   IsActive=false },
            new User{ Id=3, Name="Cara",  IsActive=true }
        };

        public IEnumerable<User> GetAll() => _users;
    }

    // ----------- Template Method Pattern -----------
    public abstract class UserProcessor
    {
        private readonly IUserRepository _repo;

        protected UserProcessor(IUserRepository repo)
        {
            _repo = repo;
        }

        // Template Method
        public void Process()
        {
            var users = _repo.GetAll();
            foreach (var user in users)
            {
                if (ShouldProcess(user))
                {
                    HandleUser(user);
                }
            }
            AfterAll();
        }

        // "Hooks" for customization
        protected abstract bool ShouldProcess(User u);
        protected abstract void HandleUser(User u);
        protected virtual void AfterAll() { }
    }

    // Concrete implementation: only active users
    public class ActiveUserProcessor : UserProcessor
    {
        public ActiveUserProcessor(IUserRepository repo) : base(repo) { }

        protected override bool ShouldProcess(User u) => u.IsActive;

        protected override void HandleUser(User u)
        {
            Console.WriteLine($"[ActiveUserProcessor] Processing active user: {u.Name}");
        }

        protected override void AfterAll()
        {
            Console.WriteLine("Finished processing active users.\n");
        }
    }

    // Another variant: all users
    public class AllUserProcessor : UserProcessor
    {
        public AllUserProcessor(IUserRepository repo) : base(repo) { }

        protected override bool ShouldProcess(User u) => true;

        protected override void HandleUser(User u)
        {
            Console.WriteLine($"[AllUserProcessor] Handling user: {u.Name}");
        }
    }

    // ----------- Program with DI -----------
    class Program
    {
        static void Main(string[] args)
        {
            // Setup DI container
            var services = new ServiceCollection();
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            services.AddTransient<UserProcessor, ActiveUserProcessor>();
            // Try swapping with AllUserProcessor if you want:
            // services.AddTransient<UserProcessor, AllUserProcessor>();

            var provider = services.BuildServiceProvider();

            var processor = provider.GetRequiredService<UserProcessor>();
            processor.Process();

            Console.WriteLine("Done. Press any key.");
            Console.ReadKey();
        }
    }
}
