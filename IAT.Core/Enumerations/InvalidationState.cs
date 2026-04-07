using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Represents the state of an invalidation process within the system.
    /// </summary>
    /// <remarks>Use the provided static instances to represent common invalidation states, such as when
    /// invalidation is in progress, queued, or blocked. This type is intended to be used as a base for specific
    /// invalidation state records.</remarks>
    /// <param name="Name">The unique name identifying the invalidation state.</param>
    /// <param name="Description">A human-readable description of the invalidation state.</param>
    public abstract record InvalidationState(string Name, string Description)
    {
        /// <summary>
        /// Represents a state indicating that the system is not currently performing any invalidation tasks.
        /// </summary>
        public static readonly InvalidationState NotInvalidating = new _NotInvalidating("NotInvalidating", "The system is not currently performing any invalidation tasks.");

        /// <summary>
        /// Represents the state indicating that the system is currently performing invalidation tasks.
        /// </summary>
        public static readonly InvalidationState Invalidating = new _Invalidating("Invalidating", "The system is currently performing invalidation tasks.");

        /// <summary>
        /// Represents the state indicating that invalidation tasks are queued and waiting to be processed.
        /// </summary>
        public static readonly InvalidationState InvalidationQueued = new _InvalidationQueued("InvalidationQueued", "Invalidation tasks are queued and waiting to be processed.");

        /// <summary>
        /// Represents a state indicating that the system is ready to perform invalidation tasks, but no tasks are currently queued or in progress.
        /// </summary>
        public static readonly InvalidationState InvalidationReady = new _InvalidationReady("InvalidationReady", "Ready for invalidation.");

        /// <summary>
        /// Represents a state indicating that cache invalidation tasks are queued and awaiting processing.
        /// </summary>
        public static readonly InvalidationState CacheInvalidationQueued = new _CacheInvalidationQueued("CacheInvalidationQueued", "Cache invalidation tasks are queued and waiting to be processed.");

        /// <summary>
        /// Represents an invalidation state where tasks are queued but currently blocked from being processed.
        /// </summary>
        /// <remarks>Use this state to indicate that invalidation tasks are awaiting processing but cannot
        /// proceed due to a blocking condition. This can be useful for monitoring or handling scenarios where queued
        /// tasks are not actively being processed.</remarks>
        public static readonly InvalidationState BlockedInvalidationQueued = new _BlockedInvalidationQueued("BlockedInvalidationQueued", "Invalidation tasks are queued but currently blocked from being processed.");

        /// <summary>
        /// Returns the corresponding InvalidationState value for the specified state name.
        /// </summary>
        /// <param name="name">The name of the invalidation state to convert. The comparison is case-insensitive.</param>
        /// <returns>The InvalidationState value that matches the specified name.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified name does not correspond to a known invalidation state.</exception>
        private static InvalidationState FromName(string name) =>
            name?.ToLowerInvariant() switch
            {
                "notinvalidating" => NotInvalidating,
                "invalidating" => Invalidating,
                "invalidationqueued" => InvalidationQueued,
                "cacheinvalidationqueued" => CacheInvalidationQueued,
                "blockedinvalidationqueued" => BlockedInvalidationQueued,
                _ => throw new ArgumentException($"Unknown invalidation state: {name}")
            };

        /// <summary>
        /// Represents an immutable record containing a name and description that does not trigger invalidation logic.
        /// </summary>
        /// <param name="Name">The name associated with this record. Cannot be null.</param>
        /// <param name="Description">A description providing additional details about this record. Cannot be null.</param>
        public sealed record _NotInvalidating(string Name, string Description) : InvalidationState(Name, Description) { }

        /// <summary>
        /// Represents an invalidating entity with a specified name and description.
        /// </summary>
        /// <param name="Name">The name that identifies the invalidating entity. Cannot be null or empty.</param>
        /// <param name="Description">A description providing additional details about the invalidating entity. Cannot be null.</param>
        public record _Invalidating(string Name, string Description) : InvalidationState(Name, Description) { }

        /// <summary>
        /// Represents a record containing information about a queued invalidation operation.
        /// </summary>
        /// <param name="Name">The name that identifies the invalidation operation.</param>
        /// <param name="Description">A description providing additional details about the invalidation operation.</param>
        public record _InvalidationQueued(string Name, string Description) : InvalidationState(Name, Description) { }

        /// <summary>
        /// Represents a queued cache invalidation request with a specified name and description.
        /// </summary>
        /// <param name="Name">The unique name identifying the cache invalidation request. Cannot be null or empty.</param>
        /// <param name="Description">A description providing additional context or details about the cache invalidation request. Can be null or
        /// empty if no description is needed.</param>
        public record _CacheInvalidationQueued(string Name, string Description) : InvalidationState(Name, Description) { }   


        /// <summary>
        /// Represents an invalidation state indicating that the item is ready for invalidation.
        /// </summary>
        /// <param name="Name">The name that identifies the invalidation state.</param>
        /// <param name="Description">A description providing additional details about the invalidation state.</param>
        public record _InvalidationReady(String Name, string Description) : InvalidationState(Name, Description) { }
        
        /// <summary>
        /// Represents a record containing information about a queued blocked invalidation, including its name and
        /// description.
        /// </summary>
        /// <param name="Name">The name that identifies the blocked invalidation in the queue.</param>
        /// <param name="Description">A description providing additional details about the blocked invalidation.</param>
        public record _BlockedInvalidationQueued(string Name, string Description) : InvalidationState(Name, Description) { } 
    }
}
