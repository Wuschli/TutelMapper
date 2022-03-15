#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TutelMapper.Annotations;
using TutelMapper.Commands;

namespace TutelMapper.Util;

public class UndoStack : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public int StackPointer { get; private set; } = -1;
    public bool HasUnsavedChanges { get; set; }
    public ObservableCollection<ICommand> Stack { get; } = new();

    public async Task Do(ICommand command)
    {
        while (Stack.Count - 1 > StackPointer)
            Stack.RemoveAt(Stack.Count - 1);

        await command.Do();
        Stack.Add(command);
        StackPointer = Stack.Count - 1;
        HasUnsavedChanges = true;
    }

    public async Task Undo()
    {
        if (StackPointer <= -1)
            return;

        var command = Stack[StackPointer];
        await command.Undo();
        StackPointer--;
        HasUnsavedChanges = true;
    }

    public async Task Redo()
    {
        if (StackPointer >= Stack.Count - 1)
            return;
        var command = Stack[StackPointer + 1];
        await command.Do();
        StackPointer++;
        HasUnsavedChanges = true;
    }

    [NotifyPropertyChangedInvocator]
    [UsedImplicitly]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Clear()
    {
        StackPointer = -1;
        Stack.Clear();
    }
}