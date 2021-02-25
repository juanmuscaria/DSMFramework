using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ModLoader.Modding
{
    public class ModGameEventManager
    {
        /// <summary>
        ///     The global instance of ModGameEventManager
        /// </summary>
        public static readonly ModGameEventManager Manager = new ModGameEventManager();

        private readonly List<Type> events = new List<Type>();

        private ModGameEventManager()
        {
        }

        /// <summary>
        ///     Registers your custom random event into the game!
        /// </summary>
        /// <param name="eventType">Your event class, it must extend BaseGameEvent and have a public empty constructor</param>
        /// <exception cref="ArgumentException">If the class does not extend BaseGameEvent or no public constructor found</exception>
        public void RegisterEvent(Type eventType)
        {
            if (!typeof(BaseGameEvent).IsAssignableFrom(eventType))
                throw new ArgumentException("Event type does not extend BaseGameEvent");
            if (eventType.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException("No public empty constructor found!");
            events.Add(eventType);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        internal void InjectEventsInto(GameEventManager manager)
        {
            foreach (var @event in events)
                manager.AddEvent((BaseGameEvent) @event.GetConstructor(Type.EmptyTypes).Invoke(null));
        }
    }
}