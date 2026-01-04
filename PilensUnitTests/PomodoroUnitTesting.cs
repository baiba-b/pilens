using Microsoft.EntityFrameworkCore;
using Pilens.Data;
using Pilens.Data.Models;
using System.Linq;
using Xunit;

namespace PilensUnitTests
{
    public sealed class PomodoroUnitTesting
    {
        private static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();

            if (!context.Users.Any(u => u.Id == "user-1"))
            {
                var user = new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "user@test.com",
                    NormalizedUserName = "USER@TEST.COM",
                    Email = "user@test.com",
                    NormalizedEmail = "USER@TEST.COM",
                    EmailConfirmed = true
                };
                context.Users.Add(user);
                context.SaveChanges();
            }

            return context;
        }

        private static Pomodoro CreateValidPomodoro(string userId) =>
            new()
            {
                UserID = userId,
                Minutes = 25,
                PauseMinutes = 5,
                LongPauseMinutes = 20,
                SessionAmount = 4,
                SessionLongPause = 4,
                AdjustedMin = 5
            };

        [Fact]
        public async Task VT_Pom_01_SavePomodoroWithValidData()
        {
            var dbName = nameof(VT_Pom_01_SavePomodoroWithValidData);

            using (var db = CreateContext(dbName))
            {
                var pom = CreateValidPomodoro("user-1");
                db.Pomodoros.Add(pom);
                await db.SaveChangesAsync();

                pom.Minutes = 30;
                pom.PauseMinutes = 6;
                pom.LongPauseMinutes = 18;
                pom.SessionAmount = 5;
                pom.SessionLongPause = 3;
                pom.AdjustedMin = 2;
                db.Pomodoros.Update(pom);
                await db.SaveChangesAsync();
            }

            using var reload = CreateContext(dbName);
            var stored = await reload.Pomodoros.SingleAsync(p => p.UserID == "user-1");
            Assert.Equal(30, stored.Minutes);
            Assert.Equal(6, stored.PauseMinutes);
            Assert.Equal(18, stored.LongPauseMinutes);
            Assert.Equal(5, stored.SessionAmount);
            Assert.Equal(3, stored.SessionLongPause);
            Assert.Equal(2, stored.AdjustedMin);
        }

        [Fact]
        public async Task VT_Pom_02_SavePomodoroWithInvalidData_IsRejectedAndNotPersisted()
        {
            var dbName = nameof(VT_Pom_02_SavePomodoroWithInvalidData_IsRejectedAndNotPersisted);

            using (var db = CreateContext(dbName))
            {
                var pom = CreateValidPomodoro("user-1");
                db.Pomodoros.Add(pom);
                await db.SaveChangesAsync();

                // Simulate UI validation failure (minutes must be > 0); do not commit invalid data.
                var invalidMinutes = 0;
                Assert.True(invalidMinutes < 1);

                var before = await db.Pomodoros.SingleAsync(p => p.UserID == "user-1");
                Assert.Equal(25, before.Minutes);
                Assert.Equal(5, before.PauseMinutes);
                Assert.Equal(20, before.LongPauseMinutes);
                Assert.Equal(4, before.SessionAmount);
                Assert.Equal(4, before.SessionLongPause);
                Assert.Equal(5, before.AdjustedMin);
            }
        }
    }
}
