﻿namespace NServiceBus
{
    using System;

    /// <summary>
    /// Configuration options for the in memory gateway persistence.
    /// </summary>
    public static class InMemoryGatewayPersistenceConfigurationExtensions
    {
        /// <summary>
        /// Configures the size of the LRU cache.
        /// </summary>
        /// <param name="persistenceExtensions">The persistence extensions to extend.</param>
        /// <param name="maxSize">Maximum size of the LRU cache.</param>
        [ObsoleteEx(
            Message = "Gateway persistence has been moved to the NServiceBus.Gateway dedicated package.",
            RemoveInVersion = "9.0.0",
            TreatAsErrorFromVersion = "8.0.0")]
        public static void GatewayDeduplicationCacheSize(this PersistenceExtensions<InMemoryPersistence> persistenceExtensions, int maxSize)
        {
            throw new NotImplementedException();
        }
    }
}