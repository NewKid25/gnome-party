using System;
using System.Threading.Tasks;
using Models.GameMetaData;

namespace GnomeParty.Database
{
    public interface IDatabaseService
    {
        Task DeleteAllEntriesFromTableAsync<T>();
        Task DeleteAsync<T>(T item);
        Task<GameSession> GetGameSessionByInviteCodeAsync(int inviteCode);
        Task<T> LoadAsync<T>(object hashKey);
        Task SaveAsync<T>(T item);
    }
}