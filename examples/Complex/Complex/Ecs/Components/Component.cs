using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Complex.Ecs.Components;

public abstract class Component
{
    public Entity? Entity { get; set; }

    public event Action<Component>? ComponentChanged;

    protected void SetValue<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        NotifyPropertyChanged(propertyName);
    }

    private void NotifyPropertyChanged(string? propertyName)
    {
        var componentChanged = ComponentChanged;
        componentChanged?.Invoke(this);
    }
}