using System;
using System.Threading.Tasks;
using Models.GameMetaData;

namespace GnomeParty.Database
{
    public interface IDatabaseService
    {
        Task SaveAsync<T>(T item);

        Task<T> LoadAsync<T>(object hashKey);

        Task DeleteAsync<T>(T item);

        Task<GameSession> GetGameSessionByInviteCodeAsync(int inviteCode);

        Task DeleteAllEntriesFromTableAsync<T>();
    }
}