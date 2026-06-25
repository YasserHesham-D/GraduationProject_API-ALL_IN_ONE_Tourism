using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Infrastructure.DbContext;
using Presentation.Controllers;
using Moq;
using Microsoft.AspNetCore.SignalR;
using Presentation.Hubs;
using Microsoft.AspNetCore.Mvc;
using Domain.Models;
using Application.Dtos.Common;

namespace Presentation.Tests
{
    public class ChatControllerTests
    {
        private AppDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetChatHistory_DirectBetweenUsers_ReturnsOrderedMessages()
        {
            var db = CreateDbContext("history_direct");
            db.ChatMessages.Add(new ChatMessage { Id = Guid.NewGuid(), SenderId = "userA", RecipientId = "userB", Text = "first", SentAt = DateTime.UtcNow.AddMinutes(-5) });
            db.ChatMessages.Add(new ChatMessage { Id = Guid.NewGuid(), SenderId = "userB", RecipientId = "userA", Text = "second", SentAt = DateTime.UtcNow.AddMinutes(-1) });
            await db.SaveChangesAsync();

            var hubMock = new Mock<IHubContext<ChatHub>>();
            var controller = new ChatController(db, hubMock.Object);

            var result = await controller.GetChatHistory("userA", "userB");

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsType<System.Collections.Generic.List<ChatMessageDto>>(ok.Value);
            Assert.Equal(2, list.Count);
            Assert.Equal("first", list[0].Text);
            Assert.Equal("second", list[1].Text);
        }

        [Fact]
        public async Task GetMyConversations_MissingClaim_ReturnsBadRequestWithClaims()
        {
            var db = CreateDbContext("myconvs1");
            var hubMock = new Mock<IHubContext<ChatHub>>();
            var controller = new ChatController(db, hubMock.Object);

            // Controller.User is null by default in unit tests, so this should return BadRequest indicating missing NameIdentifier
            var result = await controller.GetMyConversations();

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(bad.Value);
        }
    }
}
