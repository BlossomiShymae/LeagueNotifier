using LeagueNotifier.Core.Endpoints;
using System.Text.Json;

namespace LeagueNotifier.Core
{
    public class Notifier
    {
        public event EventHandler<FriendStatusEventArgs>? FriendOnline;
        public event EventHandler<FriendStatusEventArgs>? FriendRemoved;

        private List<FriendResource> _oldFriends = [];
        private List<FriendResource> _friendList = [];

        public Notifier()
        {
            var filePath = Path.Join(Path.GetTempPath(), "leaguenotifier-friendlist");
            if (File.Exists(filePath))
            {
                Task.Run(async () =>
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    _friendList = JsonSerializer.Deserialize<List<FriendResource>>(json) ?? throw new NullReferenceException();
                });
            }
        }

        public void ProcessFriends(List<FriendResource> friends)
        {
            if (_oldFriends.Count == 0)
                _oldFriends = new List<FriendResource>(friends);
            if (_friendList.Count == 0)
                _friendList = new List<FriendResource>(friends);

            foreach (var friend in _oldFriends)
            {
                var isRemoved = friends.Find(x => friend.Puuid == x.Puuid) == null;
                if (isRemoved)
                    FriendRemoved?.Invoke(this, new(friend));
            }

            foreach (var friend in friends)
            {
                var span = _oldFriends.Find(x => x.Puuid == friend.Puuid) ?? throw new NullReferenceException();

                // Friend status changed
                if (!span.Availability.Equals(friend.Availability))
                {
                    // Friend is not online using the League client
                    if (!friend.Product.Equals("league_of_legends"))
                        continue;

                    switch (span.Availability)
                    {
                        case "mobile" or "offline":
                            switch (friend.Availability)
                            {
                                // Friend is online
                                case "chat" or "dnd" or "away":
                                    FriendOnline?.Invoke(this, new(friend));
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }

                }
            }

            _oldFriends = new List<FriendResource>(friends);
            _friendList = new List<FriendResource>(friends);

            var json = JsonSerializer.Serialize(_friendList);
            var task = Task.Run(async () => await File.WriteAllTextAsync(Path.Join(Path.GetTempPath(), "leaguenotifier-friendlist"), json));
            task.Wait();
        }

        public void Clear()
        {
            _oldFriends.Clear();
        }
    }

    public class FriendStatusEventArgs(FriendResource Friend) : EventArgs
    {
        public FriendResource Friend { get; } = Friend;
    }
}