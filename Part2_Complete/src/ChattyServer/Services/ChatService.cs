using System;
using System.Threading.Tasks;
using ChattyHub;
using ChattyServer.Protos;
using ChattyServer.Services;
using ChattyServerAuth;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace ChattyServer
{
    public class ChatService : Protos.Chat.ChatBase
    {
        private readonly IUsers _users;
        private readonly IRooms _rooms;
        private readonly IChatter _chatter;

        public ChatService(IUsers users, IRooms rooms, IChatter chatter)
        {
            _users = users;
            _rooms = rooms;
            _chatter = chatter;
        }

        public override Task<SignInResponse> SignIn(SignInRequest request, ServerCallContext context)
        {
            if (request.Password != "secret")
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Login incorrect"));
            }
            
            var token = JwtHelper.GenerateJwtToken(request.UserName);

            return Task.FromResult(new SignInResponse
            {
                Token = token
            });
        }

        [Authorize]
        public override async Task<GetRoomListResponse> GetRoomList(GetRoomListRequest request, ServerCallContext context)
        {
            var response = new GetRoomListResponse();
            
            foreach (var room in await _rooms.GetAsync())
            {
                response.Rooms.Add(new Protos.Room
                {
                    Id = room.Id,
                    Name = room.Name
                });
            }

            return response;
        }

        [Authorize]
        public override async Task<SayResponse> Say(SayRequest request, ServerCallContext context)
        {
            await _chatter.SayAsync(request.RoomId, context.GetUserName(), request.Message);
            return new SayResponse();
        }

        [Authorize]
        public override async Task Listen(ListenRequest request, IServerStreamWriter<ListenResponse> responseStream, ServerCallContext context)
        {
            var userName = context.GetUserName();
            var channel = _chatter.Listen(request.RoomId);
            while (await channel.WaitToReadAsync())
            {
                while (channel.TryRead(out var message))
                {
                    if (message.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)) continue;
                    
                    await responseStream.WriteAsync(new ListenResponse
                    {
                        UserName = message.UserName,
                        Message = message.Text
                    });
                }
            }
        }
    }
}