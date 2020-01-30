using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChattyServer.Protos;
using Grpc.Core;
using Grpc.Net.Client;

namespace ChattyClient
{
    class Program
    {
        private static Chat.ChatClient _client;

        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            _client = new Chat.ChatClient(channel);

            var userName = await SignInAsync();

            var roomId = await ChooseRoomAsync();

            var cancel = new CancellationTokenSource();
            var listenTask = Task.Run(() => ListenAsync(roomId, userName, cancel.Token));

            Console.WriteLine("Enter /q to quit.");
            
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line?.Trim() == "/q") break;
                if (!string.IsNullOrWhiteSpace(line))
                {
                    await _client.SayAsync(new SayRequest {RoomId = roomId, UserName = userName, Message = line});
                }
            }
            
            cancel.Cancel();
            await listenTask;
        }

        private static async Task ListenAsync(int roomId, string userName, CancellationToken cancellationToken)
        {
            var stream = _client.Listen(new ListenRequest {RoomId = roomId});

            try
            {
                await foreach (var item in stream.ResponseStream.ReadAllAsync(cancellationToken))
                {
                    Console.WriteLine();
                    Console.WriteLine($"{item.UserName}: {item.Message}");
                    Console.WriteLine("> ");
                }
            }
            catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
            {
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                stream.Dispose();
            }
        }

        private static async Task<int> ChooseRoomAsync()
        {
            var roomList = await _client.GetRoomListAsync(new GetRoomListRequest());

            Console.WriteLine();
            Console.WriteLine("Rooms");
            Console.WriteLine("=====");
            Console.WriteLine();
            foreach (var room in roomList.Rooms)
            {
                Console.WriteLine($"{room.Id} : {room.Name}");
            }

            while (true)
            {
                Console.Write("Choose room: ");
                var input = Console.ReadLine();
                if (int.TryParse(input, out int id))
                {
                    if (roomList.Rooms.Any(r => r.Id == id))
                    {
                        return id;
                    }

                    Console.WriteLine("That is not a valid room number.");
                }
                else
                {
                    Console.WriteLine("That is not even a number.");
                }
            }
        }

        static async Task<string> SignInAsync()
        {
            Console.WriteLine("Sign in");
            Console.WriteLine("=======");

            while (true)
            {
                var user = EnterUserName();
                try
                {
                    await _client.SignInAsync(new SignInRequest {UserName = user});
                    return user;
                }
                catch (RpcException e)
                {
                    if (e.Status.StatusCode == StatusCode.AlreadyExists)
                    {
                        Console.WriteLine("A user with that name already exists. Try a different name.");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        static string EnterUserName()
        {
            while (true)
            {
                Console.Write("User: ");
                var user = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(user)) return user;
                Console.WriteLine("Enter a proper user name.");
            }
        }
    }
}