using System;
using System.Collections.Generic;

namespace ComplexExample.Dispatcher;

public sealed class EventDispatcher : IEventDispatcher
{
    private bool _disposed;
    private IDictionary<Type, Delegate> _applicationEventHandlers;

    public EventDispatcher()
    {
        _applicationEventHandlers = new Dictionary<Type, Delegate>();
    }

    ~EventDispatcher() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            RemoveAllListeners();
        }
        _applicationEventHandlers = null;
        _disposed = true;
    }

    public void AddListener<TEvent>(EventHandlerDelegate<TEvent> handler) where TEvent : IEvent
    {
        if (_applicationEventHandlers.TryGetValue(typeof(TEvent), out var existingEventHandler))
        {
            _applicationEventHandlers[typeof(TEvent)] = Delegate.Combine(existingEventHandler, handler);
        }
        else
        {
            _applicationEventHandlers[typeof(TEvent)] = handler;
        }
    }

    public void RemoveListener<TEvent>(EventHandlerDelegate<TEvent> handler) where TEvent : IEvent
    {
        if (!_applicationEventHandlers.TryGetValue(typeof(TEvent), out var source))
        {
            return;
        }
        var @delegate = Delegate.Remove(source, handler);
        if ((object)@delegate == null)
        {
            _applicationEventHandlers.Remove(typeof(TEvent));
        }
        else
        {
            _applicationEventHandlers[typeof(TEvent)] = @delegate;
        }
    }

    public void Dispatch<TEvent>(TEvent @event) where TEvent : IEvent
    {
        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        if (_disposed)
        {
            throw new ObjectDisposedException("Cannot dispatch and event when disposed! ");
        }

        if (!_applicationEventHandlers.TryGetValue(typeof(TEvent), out var @delegate) ||
            @delegate is not EventHandlerDelegate<TEvent> eventHandlerDelegate)
        {
            return;
        }

        eventHandlerDelegate(@event);
    }

    private void RemoveAllListeners()
    {
        var array = new Type[_applicationEventHandlers.Keys.Count];
        _applicationEventHandlers.Keys.CopyTo(array, 0);
        foreach (var key in array)
        {
            foreach (var invocation in _applicationEventHandlers[key].GetInvocationList())
            {
                var @delegate = Delegate.Remove(_applicationEventHandlers[key], invocation);
                if ((object)@delegate == null)
                {
                    _applicationEventHandlers.Remove(key);
                }
                else
                {
                    _applicationEventHandlers[key] = @delegate;
                }
            }
        }
    }
}