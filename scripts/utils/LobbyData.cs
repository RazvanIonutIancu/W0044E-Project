using System.Collections.Generic;

public static class LobbyData
{
    // corresponds to OptionButton.Items[idx].ID, see MainMenu.tcsn/Hosting/LobbyType::Items
    public enum LobbyVisibilityEnum
    {
        Default = -1,
        Public = 0,
        Private = 1,
        FriendsOnly = 2,
    }
	// public static string password;
    // public static readonly Dictionary<LobbyVisibilityEnum, string> LobbyVisibilityDict = new() {
    //     {LobbyVisibilityEnum.Default, LobbyVisibilityEnum.Public.ToString()},
    //     {LobbyVisibilityEnum.Public, LobbyVisibilityEnum.Public.ToString()},
    //     {LobbyVisibilityEnum.Private, LobbyVisibilityEnum.Private.ToString()},
    //     {LobbyVisibilityEnum.FriendsOnly, LobbyVisibilityEnum.FriendsOnly.ToString()},
    // };
}