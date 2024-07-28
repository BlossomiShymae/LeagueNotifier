using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LeagueNotifier.Core;
using LeagueNotifier.Core.Endpoints;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace LeagueNotifier.Desktop
{
    public partial class AppViewModel : ObservableObject
    {
        public Application Application { get; set; }
        public Notifier Notifier { get; } = new();
        public HttpClient HttpClient { get; set; } = new();

        public AppViewModel(Application application)
        {
            Application = application;
            Notifier.FriendOnline += OnFriendOnline;
            Notifier.FriendRemoved += OnFriendRemoved;

            Task.Run(ProcessBackgroundAsync);
        }

        private async Task ProcessBackgroundAsync()
        {
            while (true)
            {
                try
                {
                    var friends = await LolChat.GetFriendsAsync();
                    Notifier.ProcessFriends(friends);
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.Contains("LCUx"))
                        Notifier.Clear();
                    else
                        throw;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    Trace.WriteLine(ex.StackTrace);
                }

                await Task.Delay(1000);
            }
        }

        private void OnFriendOnline(object? sender, FriendStatusEventArgs e)
        {
            var imagePath = GetIconPath(e.Friend.Icon);
            DownloadIconToPath(e.Friend.Icon, imagePath);

            ShowToast($"{e.Friend.GameName}#{e.Friend.GameTag} is online!", imagePath);
        }

        private void OnFriendRemoved(object? sender, FriendStatusEventArgs e)
        {
            var imagePath = GetIconPath(e.Friend.Icon);
            DownloadIconToPath(e.Friend.Icon, imagePath);

            ShowToast($"{e.Friend.GameName}#{e.Friend.GameTag} was removed!", imagePath);
        }

        private static void ShowToast(string message, string imagePath)
        {
            var contentBuilder = new ToastContentBuilder()
                .AddText("LeagueNotifier")
                .AddText(message)
                .AddAppLogoOverride(new Uri(imagePath));

            contentBuilder.AddAudio(new Uri("ms-winsoundevent:Notification.IM"));

            contentBuilder.Show();
            Trace.WriteLine(message);
        }

        private static string GetIconPath(int icon)
        {
            return Path.Join(Path.GetTempPath(), $"leaguenotifier-{icon}"); ;
        }

        private void DownloadIconToPath(int icon, string path)
        {
            if (!File.Exists(path))
            {
                Task task = Task.Run(async () =>
                {
                    var bytes = await HttpClient.GetByteArrayAsync($"https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/profile-icons/{icon}.jpg");
                    await File.WriteAllBytesAsync(path, bytes);
                });
                task.Wait();
            }
        }

        [RelayCommand]
        private void Quit()
        {
            if (Application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }
}