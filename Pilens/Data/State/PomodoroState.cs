using System;

namespace Pilens.Data.State;

public sealed class PomodoroState
{
    private int _minutes = 25;

    public event Action? OnChange;

    public int Minutes
    {
        get => _minutes;
        set
        {
            if (value < 1 || _minutes == value)
            {
                return;
            }

            _minutes = value;
            OnChange?.Invoke();
        }
    }
}