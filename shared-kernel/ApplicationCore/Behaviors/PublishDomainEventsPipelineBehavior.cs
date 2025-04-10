using MediatR;
using PlatformPlatform.SharedKernel.ApplicationCore.Cqrs;
using PlatformPlatform.SharedKernel.DomainCore.DomainEvents;

namespace PlatformPlatform.SharedKernel.ApplicationCore.Behaviors;

/// <summary>
///     This method publishes any domain events that were generated during the execution of a command (and added to
///     aggregates). It's crucial to understand that domain event handlers are not supposed to produce any external side
///     effects outside the current transaction/UnitOfWork. For instance, they can be utilized to create, update, or
///     delete aggregates, or they could be used to update read models. However, they should not be used to invoke other
///     services (e.g., send emails) that are not part of the same database transaction. For such tasks, use Integration
///     Events instead.
/// </summary>
public sealed class PublishDomainEventsPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand where TResponse : ResultBase
{
    private readonly IDomainEventCollector _domainEventCollector;
    private readonly IPublisher _mediatr;

    public PublishDomainEventsPipelineBehavior(IDomainEventCollector domainEventCollector, IPublisher mediatr)
    {
        _domainEventCollector = domainEventCollector;
        _mediatr = mediatr;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        var domainEvents = new Queue<IDomainEvent>();

        EnqueueAndClearDomainEvents(domainEvents);

        while (domainEvents.Any())
        {
            var domainEvent = domainEvents.Dequeue();

            // Publish the domain event to the MediatR pipeline. Any registered event handlers will be invoked. These
            // event handlers can then carry out any necessary actions, such as managing side effects, updating read
            // models, and so forth.
            await _mediatr.Publish(domainEvent, cancellationToken);

            // It is possible that a domain event handler creates a new domain event, so we need to check if there are
            // any new domain events that need to be published and handled before continuing.
            EnqueueAndClearDomainEvents(domainEvents);
        }

        return response;
    }

    /// <summary>
    ///     Adds any new domain events to the processing queue and clears them from the originating aggregates.
    /// </summary>
    private void EnqueueAndClearDomainEvents(Queue<IDomainEvent> domainEvents)
    {
        foreach (var aggregate in _domainEventCollector.GetAggregatesWithDomainEvents())
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                domainEvents.Enqueue(domainEvent);
            }

            aggregate.ClearDomainEvents();
        }
    }
}