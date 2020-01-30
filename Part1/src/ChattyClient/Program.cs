using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChattyClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
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
                    // TODO: Send to server
                }
            }
            
            cancel.Cancel();
            await listenTask;
        }

        private static async Task ListenAsync(int roomId, string userName, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        private static async Task<int> ChooseRoomAsync()
        {
            // TODO: Get room list...

            Console.WriteLine();
            Console.WriteLine("Rooms");
            Console.WriteLine("=====");
            Console.WriteLine();

            // TODO: Display room list...

            while (true)
            {
                Console.Write("Choose room: ");
                var input = Console.ReadLine();
                if (int.TryParse(input, out int id))
                {
                    // TODO: Validate room selection
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
                // TODO: Sign in
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