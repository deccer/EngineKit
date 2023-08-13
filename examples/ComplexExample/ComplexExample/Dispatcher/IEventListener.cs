using System;

namespace ComplexExample.Dispatcher;

public interface IEventListener : IDisposable
{
    void AddListener<TEvent>(EventHandlerDelegate<TEvent> handler) where TEvent : IEvent;

    void RemoveListener<TEvent>(EventHandlerDelegate<TEvent> handler) where TEvent : IEvent;
}