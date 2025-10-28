using AirlineManager.DataAccess.Data;
using AirlineManager.Models.Domain;
using AirlineManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using UAParser;

namespace AirlineManager.Services.Implementations
{
    public class SessionManagementService : ISessionManagementService
    {
        private readonly ApplicationDbContext _context;

        public SessionManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserSession> CreateOrUpdateSessionAsync(string userId, string userEmail,
string sessionId, string? ipAddress, string? userAgent, bool isPersistent)
        {
            var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null)
            {
                session = new UserSession
                {
                    UserId = userId,
                    UserEmail = userEmail,
                    SessionId = sessionId,
                    CreatedAt = DateTime.UtcNow,
                    LastActivityAt = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    IsPersistent = isPersistent,
                    IsActive = true
                };

                // Parse user agent
                if (!string.IsNullOrEmpty(userAgent))
                {
                    try
                    {
                        var uaParser = Parser.GetDefault();
                        var clientInfo = uaParser.Parse(userAgent);

                        session.Browser = $"{clientInfo.UA.Family} {clientInfo.UA.Major}";
                        session.OperatingSystem = $"{clientInfo.OS.Family} {clientInfo.OS.Major}";
                        session.Device = clientInfo.Device.Family != "Other" ? clientInfo.Device.Family : null;
                    }
                    catch
                    {
                        // If parsing fails, just skip it
                    }
                }

                // Set expiration based on persistence
                if (isPersistent)
                {
                    session.ExpiresAt = DateTime.UtcNow.AddDays(30);
                }
                else
                {
                    session.ExpiresAt = DateTime.UtcNow.AddHours(1);
                }

                _context.UserSessions.Add(session);
            }
            else
            {
                session.LastActivityAt = DateTime.UtcNow;
                session.IsActive = true;

                // Update expiration on activity
                if (isPersistent)
                {
                    session.ExpiresAt = DateTime.UtcNow.AddDays(30);
                }
                else
                {
                    session.ExpiresAt = DateTime.UtcNow.AddHours(1);
                }

                _context.UserSessions.Update(session);
            }

            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<IEnumerable<UserSession>> GetActiveSessionsAsync(string userId)
        {
            return await _context.UserSessions
     .Where(s => s.UserId == userId && s.IsActive)
   .OrderByDescending(s => s.LastActivityAt)
          .ToListAsync();
        }

        public async Task UpdateSessionActivityAsync(string sessionId)
        {
            var session = await _context.UserSessions
        .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (session != null)
            {
                session.LastActivityAt = DateTime.UtcNow;

                // Extend expiration
                if (session.IsPersistent)
                {
                    session.ExpiresAt = DateTime.UtcNow.AddDays(30);
                }
                else
                {
                    session.ExpiresAt = DateTime.UtcNow.AddHours(1);
                }

                _context.UserSessions.Update(session);
                await _context.SaveChangesAsync();
            }
        }

        public async Task TerminateSessionAsync(string sessionId)
        {
            var session = await _context.UserSessions
                            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session != null)
            {
                session.IsActive = false;
                _context.UserSessions.Update(session);
                await _context.SaveChangesAsync();
            }
        }

        public async Task TerminateOtherSessionsAsync(string userId, string currentSessionId)
        {
            var sessions = await _context.UserSessions
               .Where(s => s.UserId == userId && s.SessionId != currentSessionId && s.IsActive)
           .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
            }

            _context.UserSessions.UpdateRange(sessions);
            await _context.SaveChangesAsync();
        }

        public async Task TerminateAllSessionsAsync(string userId)
        {
            var sessions = await _context.UserSessions
       .Where(s => s.UserId == userId && s.IsActive)
             .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
            }

            _context.UserSessions.UpdateRange(sessions);
            await _context.SaveChangesAsync();
        }

        public async Task CleanupExpiredSessionsAsync()
        {
            var expiredSessions = await _context.UserSessions
        .Where(s => s.IsActive && s.ExpiresAt.HasValue && s.ExpiresAt < DateTime.UtcNow)
        .ToListAsync();

            foreach (var session in expiredSessions)
            {
                session.IsActive = false;
            }

            _context.UserSessions.UpdateRange(expiredSessions);
            await _context.SaveChangesAsync();
        }

        public async Task<UserSession?> GetSessionByIdAsync(string sessionId)
        {
            return await _context.UserSessions
           .FirstOrDefaultAsync(s => s.SessionId == sessionId);
        }
    }
}