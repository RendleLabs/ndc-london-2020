using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChattyHub
{
    public interface IRooms
    {
        Task<IReadOnlyList<Room>> GetAsync();
    }
}