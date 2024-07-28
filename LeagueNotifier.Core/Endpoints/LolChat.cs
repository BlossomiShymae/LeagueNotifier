using BlossomiShymae.GrrrLCU;
using System.Net.Http.Json;

namespace LeagueNotifier.Core.Endpoints
{
    public static class LolChat
    {
        public static async Task<List<FriendResource>> GetFriendsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/lol-chat/v1/friends");
            var response = await Connector.SendAsync(request);
            var friends = await response.Content.ReadFromJsonAsync<List<FriendResource>>() ?? throw new NullReferenceException();

            return friends;
        }
    }

    public class FriendResource
    {
        public required string Availability { get; set; }
        public required string GameName { get; set; }
        public required string GameTag { get; set; }
        public int Icon { get; set; }
        public required string Puuid { get; set; }
        public required string Product { get; set; }
    }
}