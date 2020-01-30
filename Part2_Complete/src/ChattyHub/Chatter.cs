using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChattyHub
{
    public interface IChatter
    {
        Task SayAsync(int roomId, string userName, string message);
        ChannelReader<Message> Listen(int roomId);
    }

    public class Chatter : IChatter
    {
        private readonly ConcurrentDictionary<int, Channel<Message>> _channels = new ConcurrentDictionary<int, Channel<Message>>();

        public async Task SayAsync(int roomId, string userName, string message)
        {
            var channel = _channels.GetOrAdd(roomId, Create);
            await channel.Writer.WriteAsync(new Message(userName, message));
        }

        public ChannelReader<Message> Listen(int roomId)
        {
            return _channels.GetOrAdd(roomId, Create);
        }

        private static Channel<Message> Create(int _)
        {
            return Channel.CreateBounded<Message>(new BoundedChannelOptions(32)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                AllowSynchronousContinuations = false,
                SingleReader = false,
                SingleWriter = false
            });
        }
    }
}