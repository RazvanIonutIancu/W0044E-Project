using GdUnit4;
using static GdUnit4.Assertions;
using Godot;
using System;
using System.Threading.Tasks;
using Steamworks;
using System.Threading;
using Steamworks.Data;

[TestSuite]
public partial class LobbyTests : Node
{

	private PackedScene mainScene = GD.Load<PackedScene>("res://scenes/MainScene.tscn");

	private SteamManager steam;

	private Lobby lobby;

	public override void _Ready()
	{
	}

	[TestCase]
	[RequireGodotRuntime]
	[GodotExceptionMonitor]
	public async Task FindNetworkAvailability()
	{
		steam = mainScene.Instantiate<SteamManager>();
		AddChild(steam);
		int i = 0;
		while(SteamNetworkingUtils.Status != SteamNetworkingAvailability.Current && i < 10)
		{
			await Task.Delay(1000);
			i++;
		}
		AssertBool(SteamNetworkingUtils.Status == SteamNetworkingAvailability.Current).IsTrue();
	}

	[TestCase]
	[RequireGodotRuntime]
	[GodotExceptionMonitor]
	public async Task HostLobby()
	{
		bool lobbyCreationAttempted = false;
		bool lobbyCreationResult = false;
		(new Thread(() => 
			{ 
				lobbyCreationResult = steam.CreateLobby(LobbyVisibility.LobbyVisibilityEnum.Public).Result;
				lobbyCreationAttempted = true; 
			}
		)).Start();
		while(!lobbyCreationAttempted)
		{
			await Task.Delay(1);
			SteamClient.RunCallbacks();
		}
		AssertBool(lobbyCreationResult).IsTrue();
		lobby = SteamManager.currentLobby.Value;
	}

	[TestCase]
	[RequireGodotRuntime]
	[GodotExceptionMonitor]
	public async Task FindLobby()
	{
		await Task.Delay(1000);
		await steam.GetMultiplayerLobbies();
		AssertBool(steam.availableLobbies.Contains(lobby)).IsTrue();
	}

	[TestCase]
	[RequireGodotRuntime]
	[GodotExceptionMonitor]
	public async Task LobbyMenuIntegration()
	{
		steam.GetNode<SteamCallbacks>("Callbacks").OnLobbyEntered(lobby);
		await Task.Delay(1000);
		AssertBool(steam.GetNode<LobbyMenu>("MenuSceneContainer/LobbyMenu") != null).IsTrue();
	}
}
