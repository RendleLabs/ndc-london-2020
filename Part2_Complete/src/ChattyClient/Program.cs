using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
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
            var clientCertificate = new X509Certificate2("client.pfx", "secretsquirrel");

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) =>
                {
                    Console.WriteLine(certificate2.Issuer);
                    Console.WriteLine(clientCertificate.Issuer);
                    return certificate2.Issuer == clientCertificate.Issuer;
                }
            };
            handler.ClientCertificates.Add(clientCertificate);

            var http = new HttpClient(handler);

            var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
            {
                HttpClient = http
            });

            _client = new Chat.ChatClient(channel);

            var token = await SignInAsync();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var roomId = await ChooseRoomAsync();

            var cancel = new CancellationTokenSource();
            var listenTask = Task.Run(() => ListenAsync(roomId, cancel.Token));

            Console.WriteLine("Enter /q to quit.");
            
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (line?.Trim() == "/q") break;
                if (!string.IsNullOrWhiteSpace(line))
                {
                    await _client.SayAsync(new SayRequest {RoomId = roomId, Message = line});
                }
            }
            
            cancel.Cancel();
            await listenTask;
        }

        private static async Task ListenAsync(int roomId, CancellationToken cancellationToken)
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
                var password = EnterPassword();
                try
                {
                    var signInRequest = new SignInRequest
                    {
                        UserName = user,
                        Password = password
                    };
                    var response = await _client.SignInAsync(signInRequest);
                    return response.Token;
                }
                catch (RpcException e) when (e.Status.StatusCode == StatusCode.Unauthenticated)
                {
                    Console.WriteLine("Login incorrect.");
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
        
        static string EnterPassword()
        {
            while (true)
            {
                Console.Write("Password: ");
                var password = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(password)) return password;
                Console.WriteLine("Enter a password.");
            }
        }
    }
}