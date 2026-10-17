using Godot;
using System;
using System.Threading.Tasks;
using Steamworks;

public partial class DummyNetworkManager : Node3D, INetworkManager
{
	public bool IsHost() { return true; }
	public SceneLoader GetSceneLoader() { return null; }
	public void OpenFriendOverlayForInvite() { }
	public SteamId GetPlayerSteamID() { return new SteamId(); }
	public void Disconnect() {  }
	public string GetPlayerName() { return ""; }
	public Task<bool> CreateLobby(LobbyVisibility.LobbyVisibilityEnum lobbyVisibility = LobbyVisibility.DefaultValue) { return Task.FromResult<bool>(false); }
	public Task<bool> GetMultiplayerLobbies() { return Task.FromResult<bool>(false); }
	public Task<bool> TryJoinViaCode(string code) { return Task.FromResult<bool>(false); }
	public void CreateSteamSocketServer() { }
	public void JoinSteamSocketServer(SteamId host) {}
}
