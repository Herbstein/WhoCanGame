using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TinySteamWrapper;
using WhoCanGame.Helpers;

namespace WhoCanGame {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private IDictionary<int, SteamApp> allGames;
        private List<SteamProfileGame> games;
        private List<SteamProfile> profiles;
        private List<SteamProfile> usersWithGame;

        public MainWindow() {
            InitializeComponent();
            games = new List<SteamProfileGame>();
            profiles = new List<SteamProfile>();
            usersWithGame = new List<SteamProfile>();
            SteamManager.SteamAPIKey = "6592C07D3BED5A56B277FCC0DA0082D1";
        }

        private async void AddUser(object sender, RoutedEventArgs e) {
            var name = HelperFunctions.FindChild<TextBox>(this, "UserName").Text;
            HelperFunctions.FindChild<TextBox>(this, "UserName").Text = "";

            if (allGames == null) {
                allGames = await SteamManager.GetFullAppDictionary();
            }

            long steamID;

            if (name.IsDigitsOnly() && name.Length == 17) {
                long.TryParse(name, out steamID);
            } else {
                steamID = await SteamManager.GetSteamIDByName(name);
            }

            if (steamID != 0) {
                var profile = await SteamManager.GetSteamProfileByID(steamID);
                if (profile != null) {
                    await SteamManager.LoadGamesForProfile(profile);

                    if (profile.Games.Count == 0) {} else if (games.Count == 0) {
                        foreach (var game in profile.Games) {
                            games.Add(game);
                        }
                    } else {
                        games = games.Join(profile.Games.ToList(), la => la.App, lb => lb.App, (la, lb) => la).ToList();
                    }

                    DataContext = null;

                    var gameStringList = games.Select(game => game.App.Name + "\n").ToList();

                    gameStringList.Sort();

                    var gameString = gameStringList.Aggregate("", (current, str) => current + str);

                    DataContext = gameString;

                    profiles.Add(profile);

                    foreach (var steamProfile in profiles.Where(steamProfile => !HelperFunctions.FindChild<TextBox>(this, "Users").Text.Contains(steamProfile.PersonaName))) {
                        HelperFunctions.FindChild<TextBox>(this, "Users").Text += steamProfile.PersonaName + "\n";
                    }
                } else {
                    MessageBox.Show(this, "No profile found for the id " + steamID);
                }
            } else {
                MessageBox.Show(this, "No Steam ID found for user " + name);
            }
        }

        private void Reset(object sender, RoutedEventArgs e) {
            DataContext = "";
            games = new List<SteamProfileGame>();
            HelperFunctions.FindChild<TextBox>(this, "Users").Text = "";
            profiles = new List<SteamProfile>();
        }
    }
}
