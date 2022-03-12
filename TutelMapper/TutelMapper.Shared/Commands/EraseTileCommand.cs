using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TutelMapper.Annotations;

namespace TutelMapper.Commands;

public class EraseTileCommand : ICommand
{
    private readonly string[,] _target;
    private readonly int _x;
    private readonly int _y;

    private string _previousTile;

    public event PropertyChangedEventHandler PropertyChanged;

    public EraseTileCommand(string[,] target, int x, int y)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        _x = x;
        _y = y;
    }

    public bool IsApplied { get; private set; }

    public Task Do()
    {
        if (IsApplied)
            return Task.CompletedTask;
        _previousTile = _target[_x, _y];
        _target[_x, _y] = null;
        IsApplied = true;
        return Task.CompletedTask;
    }

    public Task Undo()
    {
        if (!IsApplied)
            return Task.CompletedTask;
        _target[_x, _y] = _previousTile;
        IsApplied = false;
        return Task.CompletedTask;
    }

    public override string ToString()
    {
        return $"Erase at [{_x}|{_y}]";
    }

    [NotifyPropertyChangedInvocator]
    [UsedImplicitly]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}