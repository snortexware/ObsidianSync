using System;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Apis.Util.Store;
using G.Sync.Repository;

namespace G.Sync.Google.Api
{
    public class TokenContext : IDataStore
    {
        public async Task StoreAsync<T>(string key, T value)
        {
            var tokenJson = JsonSerializer.Serialize(value);
            var repo = new SecurityRepository();

            repo.CreateOrUpdateToken(key, tokenJson);

            await Task.CompletedTask;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var repo = new SecurityRepository();

            var token = repo.GetTokenByKey(key);

            return await Task.FromResult(JsonSerializer.Deserialize<T>(token.Token));
        }

        public async Task ClearAsync()
        {
            var repo = new SecurityRepository();

            repo.DeleteAllTokens();

            await Task.CompletedTask;
        }

        public Task DeleteAsync<T>(string key)
        {
            var repo = new SecurityRepository();

            repo.DeleteTokenByKey(key);

            return Task.CompletedTask;
        }
    }
}
