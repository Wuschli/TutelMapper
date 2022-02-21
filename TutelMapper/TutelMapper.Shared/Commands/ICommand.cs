using System.Threading.Tasks;

namespace TutelMapper.Commands
{
    public interface ICommand
    {
        Task Do();
        Task Undo();
        string ToString();
    }
}