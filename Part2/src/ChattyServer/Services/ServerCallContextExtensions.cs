using Grpc.Core;

namespace ChattyServer.Services
{
    public static class ServerCallContextExtensions
    {
        public static string GetUserName(this ServerCallContext context) => context.GetHttpContext().User?.Identity.Name;
    }
}