using Rankora_API.Scripts.Rankora.Types;
using System;
using System.Collections.Generic;

#nullable enable
namespace Rankora_API.Scripts.Rankora.Main
{
    /// <summary>
    /// Central event bus for Rankora SDK.
    /// Allows subscribing to specific strongly-typed events and replaying the last emitted value.
    /// </summary>
    public static class RankoraEvents
    {
        /// <summary>
        /// Strongly-typed event wrapper with "last value replay" capability.
        /// </summary>
        public class RankoraEvent<T>
        {
            readonly bool raiseWithPreviousData = true;
            public RankoraEvent()
            {
                
            }
            public RankoraEvent(bool raiseWithPreviousData)
            {
                this.raiseWithPreviousData = raiseWithPreviousData;
            }
            // Stores the last raised value for replay on new subscriptions
            private T previousData = default!;

            // Action-based listeners (simpler than EventHandler<T>)
            private Action<T>? actionListeners;

            /// <summary>
            /// Emits the given value to all subscribers and stores it for replay.
            /// </summary>
            public void Raise(T data)
            {
                previousData = data;
                actionListeners?.Invoke(data);
            }

            /// <summary>
            /// Subscribes to the event. If a value has been raised before, it is immediately invoked.
            /// </summary>
            public void Subscribe(Action<T> action)
            {
                if (action == null) return;
                actionListeners += action;

                // Immediately invoke with previous data if available
                if (raiseWithPreviousData && !EqualityComparer<T>.Default.Equals(previousData, default!))
                {
                    action.Invoke(previousData);
                }
            }

            /// <summary>
            /// Unsubscribes an action listener.
            /// </summary>
            public void Unsubscribe(Action<T> action)
            {
                actionListeners -= action;
            }

            /// <summary>
            /// Clears all listeners and stored value.
            /// </summary>
            public void Clear()
            {
                previousData = default!;
                actionListeners = null;
            }
        }

        // Static initialization logic for linking certain events together
        static RankoraEvents()
        {
            OnPostPlayerUpdated.Subscribe(static (response) =>
            {
                if (!response.success) return;

                if (!string.IsNullOrEmpty(response.player_id))
                {
                    OnPlayerIdUpdated.Raise(response.player_id);
                }

                if (response.rank > 0)
                {
                    OnPlayerRankUpdated.Raise(response.rank);
                }
            });
        }

        // Global events
        public static RankoraEvent<PlayerEntry> OnPlayerFetched { get; } = new();
        public static RankoraEvent<PostPlayerResponse> OnPostPlayerUpdated { get; } = new();
        public static RankoraEvent<string> OnPlayerIdUpdated { get; } = new();
        public static RankoraEvent<LeaderboardEntriesResponse> OnEntriesFetched { get; } = new();
        public static RankoraEvent<LeaderboardMetadata> OnLeaderboardMetadataFetched { get; } = new();
        public static RankoraEvent<string> OnPlayerDeleted { get; } = new();
        public static RankoraEvent<int> OnPlayerRankUpdated { get; } = new();
    }
}
