using Steamworks;
using System.Threading.Tasks;

public interface INetworkManager
{
	public bool IsHost();
	public SceneLoader GetSceneLoader();
	public void OpenFriendOverlayForInvite();
	public SteamId GetPlayerSteamID();
	public void Disconnect();
	public string GetPlayerName();
	public Task<bool> CreateLobby(LobbyVisibility.LobbyVisibilityEnum lobbyVisibility = LobbyVisibility.DefaultValue);
	public Task<bool> GetMultiplayerLobbies();
	public Task<bool> TryJoinViaCode(string code);
	public void CreateSteamSocketServer();
	public void JoinSteamSocketServer(SteamId host);
}
