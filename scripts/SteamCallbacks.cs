using Godot;
using System;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;

public partial class SteamCallbacks : Node
{
	public static event Action<Friend> OnPlayerJoinLobby;
	public static event Action<Friend> OnPlayerLeftLobby;

	[Export] private SceneLoader sceneLoader;

	public async override void _EnterTree()
	{
		SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
		SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
		SteamMatchmaking.OnLobbyMemberJoined += OnMemberJoined;
		SteamMatchmaking.OnLobbyMemberDisconnected += OnMemberDisconnected;
		SteamMatchmaking.OnLobbyMemberLeave += OnMemberLeave;
		SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
		SteamFriends.OnGameLobbyJoinRequested += OnJoinRequested;
	}

	public async override void _ExitTree()
	{
		SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;
		SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
		SteamMatchmaking.OnLobbyMemberJoined -= OnMemberJoined;
		SteamMatchmaking.OnLobbyMemberDisconnected -= OnMemberDisconnected;
		SteamMatchmaking.OnLobbyMemberLeave -= OnMemberLeave;
		SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
		SteamFriends.OnGameLobbyJoinRequested -= OnJoinRequested;
	}

	private void OnMemberDisconnected(Lobby lobby, Friend friend)
	{
		GD.Print("User has disconnected from lobby: " + friend.Name);
		OnPlayerLeftLobby.Invoke(friend);
	}

	private void OnMemberLeave(Lobby lobby, Friend friend)
	{
		GD.Print("User has left lobby: " + friend.Name);
		OnPlayerLeftLobby.Invoke(friend);
	}

	private void OnMemberJoined(Lobby lobby, Friend friend)
	{
		GD.Print("User has joined the lobby: " + friend.Name);
		OnPlayerJoinLobby.Invoke(friend);
	}

	private async void OnLobbyCreated(Result result, Lobby lobby)
	{
		if(result != Result.OK)
		{
			GD.Print("Lobby creation result was not ok");
		}
		else
		{
			GD.Print($"Created lobby! id = {lobby.Id}");
		}
		SteamManager.Manager.CreateSteamSocketServer();
	}

	private async void OnLobbyEntered(Lobby lobby)
	{
		SteamManager.currentLobby = lobby;
		sceneLoader.LoadLobbyMenu();
		if(lobby.Owner.Id != SteamManager.Manager.PlayerSteamID)
		{
			GD.Print($"You joined {lobby.Owner.Name}'s lobby");

			foreach (var item in lobby.Members)
			{
				OnPlayerJoinLobby.Invoke(item);
			}
			SteamManager.Manager.JoinSteamSocketServer(lobby.Owner.Id);
		}
		else
		{
			GD.Print("You have joined your own lobby");
			OnPlayerJoinLobby.Invoke(lobby.Owner);
		}
	}

	private void OnLobbyGameCreated(Lobby lobby, uint id, ushort port, SteamId steamId)
	{
		GD.Print("Lobby game created");
	}

	private async void OnJoinRequested(Lobby lobby, SteamId id)
	{
		RoomEnter joinSuccessful = await lobby.Join();
		if(joinSuccessful != RoomEnter.Success)
		{
			GD.Print("Failed to join lobby");
		}
		else
		{
			SteamManager.currentLobby = lobby;
		}
	}
}
