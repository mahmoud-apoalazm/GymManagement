using System.Reflection;
using GymManagement.Application.Common.Interfaces;
using GymManagement.Domain.Admins;
using GymManagement.Domain.Common;
using GymManagement.Domain.Gyms;
using GymManagement.Domain.Subscriptions;
using GymManagement.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Common.Persistence;

public class GymManagementDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor) : DbContext(options), IUnitOfWork
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    public DbSet<Admin> Admins { get; set; } 
    public DbSet<Subscription> Subscriptions { get; set; } 
    public DbSet<Gym> Gyms { get; set; } 
    public DbSet<User> Users { get; set; } 


    public async Task CommitChangesAsync()
    {
        var domainEvents = ChangeTracker.Entries<Entity>()
            .Select(entry => entry.Entity.PopDomainEvents())
            .SelectMany(x => x).ToList();
        // store them in the http context for later if user is waiting online
        //if (IsUserWaitingOnline())
        //{
        //    AddDomainEventsToOfflineProcessingQueue(domainEvents);
        //}
        //else
        //{
        //    await PublishDomainEvents(_publisher, domainEvents);
        //}
        
        await SaveChangesAsync();
    }

    private void AddDomainEventsToOfflineProcessingQueue(List<IDomainEvent> domainEvents)
    {
        // fetch queue from http context or create a new queue if it doesn't exist
        var domainEventsQueue = _httpContextAccessor.HttpContext!.Items
            .TryGetValue("DomainEventsQueue", out var value) && value is Queue<IDomainEvent> existingDomainEvents
                ? existingDomainEvents
                : new Queue<IDomainEvent>();

        // add the domain events to the end of the queue
        domainEvents.ForEach(domainEventsQueue.Enqueue);

        // store the queue in the http context
        _httpContextAccessor.HttpContext!.Items["DomainEventsQueue"] = domainEventsQueue;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}