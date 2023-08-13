namespace ComplexExample.Dispatcher;

public delegate void EventHandlerDelegate<in TEvent>(TEvent @event) where TEvent : IEvent;