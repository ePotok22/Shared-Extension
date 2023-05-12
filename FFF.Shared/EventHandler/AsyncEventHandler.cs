using System.Threading.Tasks;

namespace FFF.Shared
{
    public delegate Task AsyncEventHandler<TArgs>(object sender, TArgs e);
}
