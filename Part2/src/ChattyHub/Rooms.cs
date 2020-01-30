using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChattyHub
{
    public class Rooms : IRooms
    {
        private readonly IReadOnlyList<Room> _rooms = new List<Room>
        {
            new Room(1, "Lobby"),
            new Room(2, "Marvel"),
            new Room(3, "DC"),
        }.AsReadOnly();

        public Task<IReadOnlyList<Room>> GetAsync() => Task.FromResult(_rooms);
    }
}
