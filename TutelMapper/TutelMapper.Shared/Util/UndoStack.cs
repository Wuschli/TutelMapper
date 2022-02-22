using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TutelMapper.Annotations;
using TutelMapper.Commands;

namespace TutelMapper.Util
{
    public class UndoStack : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int StackPointer { get; private set; } = -1;
        public ObservableCollection<ICommand> Stack { get; } = new ObservableCollection<ICommand>();

        public async Task Do(ICommand command)
        {
            while (Stack.Count - 1 > StackPointer)
                Stack.RemoveAt(Stack.Count - 1);

            await command.Do();
            Stack.Add(command);
            StackPointer = Stack.Count - 1;
        }

        public async Task Undo()
        {
            if (StackPointer <= -1)
                return;

            var command = Stack[StackPointer];
            await command.Undo();
            StackPointer--;
        }

        public async Task Redo()
        {
            if (StackPointer >= Stack.Count - 1)
                return;
            var command = Stack[StackPointer + 1];
            await command.Do();
            StackPointer++;
        }

        [NotifyPropertyChangedInvocator]
        [UsedImplicitly]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}