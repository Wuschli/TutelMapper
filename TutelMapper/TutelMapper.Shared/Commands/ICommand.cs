using System.ComponentModel;
using System.Threading.Tasks;

namespace TutelMapper.Commands
{
    public interface ICommand : INotifyPropertyChanged
    {
        bool IsApplied { get; }
        Task Do();
        Task Undo();
        string ToString();
    }
}