using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Infrastructure.DbContext;
using Presentation.Hubs;
using Moq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Application.Dtos.Common;
using Domain.Models;

namespace Presentation.Tests
{
    public class ChatHubTests
    {
        private AppDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        private void ClearUserConnections()
        {
            var field = typeof(ChatHub).GetField("_userConnections", BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null)
            {
                var dict = field.GetValue(null) as System.Collections.Concurrent.ConcurrentDictionary<string, HashSet<string>>;
                dict?.Clear();
            }
        }

        [Fact]
        public async Task SendMessageToUser_SavesMessageAndInvokesClients()
        {
            ClearUserConnections();
            var db = CreateDbContext("hub_send_1");

            // Prepare the user connections dictionary so recipient has a tracked connection
            var field = typeof(ChatHub).GetField("_userConnections", BindingFlags.Static | BindingFlags.NonPublic);
            var dict = field.GetValue(null) as System.Collections.Concurrent.ConcurrentDictionary<string, HashSet<string>>;
            dict.TryAdd("recipient1", new HashSet<string> { "conn-2" });

            var clientsMock = new Mock<IHubCallerClients>();
            var callerProxy = new Mock<IClientProxy>();
            var recipientProxy = new Mock<IClientProxy>();

            callerProxy.Setup(p => p.SendCoreAsync("ReceiveMessage", It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            recipientProxy.Setup(p => p.SendCoreAsync("ReceiveMessage", It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            clientsMock.Setup(c => c.Caller).Returns(callerProxy.Object);
            clientsMock.Setup(c => c.Clients(It.IsAny<System.Collections.Generic.IReadOnlyList<string>>())).Returns(recipientProxy.Object);

            clientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(recipientProxy.Object);

            var loggerMock = new Mock<ILogger<ChatHub>>();

            var hub = new ChatHub(db, loggerMock.Object)
            {
                Clients = clientsMock.Object
            };

            var contextMock = new Mock<HubCallerContext>();
            contextMock.SetupGet(c => c.ConnectionId).Returns("conn-1");

            var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "sender1") }));

            contextMock.SetupGet(c => c.User).Returns(claimsPrincipal);
            contextMock.SetupGet(c => c.UserIdentifier).Returns("sender1");

            hub.Context = contextMock.Object;

            await hub.SendMessageToUser("recipient1", new SendMessageRequest("hello"));

            // verify message persisted
            var msg = await db.ChatMessages.FirstOrDefaultAsync();
            Assert.NotNull(msg);
            Assert.Equal("sender1", msg.SenderId);
            Assert.Equal("recipient1", msg.RecipientId);
            Assert.Equal("hello", msg.Text);

            // verify clients were invoked (recipient and caller)
            recipientProxy.Verify(p => p.SendCoreAsync("ReceiveMessage", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            callerProxy.Verify(p => p.SendCoreAsync("ReceiveMessage", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}
