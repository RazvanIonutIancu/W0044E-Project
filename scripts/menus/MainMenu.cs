using Godot;
using System;
using System.Collections.Generic;
using Steamworks.Data;
using Steamworks;

public partial class MainMenu : Control
{

	[Export] private PackedScene lobbyElement;
	[Export] private VBoxContainer lobbyContainer;
	[Export] private ProgressBar progressBar;
	[Export] private LineEdit codeEdit;

    public override void _EnterTree()
    {
        SteamManager.OnLobbyRefreshCompleted += OnLobbyRefreshCompletedCallback;
    }

    public override void _Process(double delta)
	{
		progressBar.Value = SteamNetworkingUtils.Status == SteamNetworkingAvailability.Current ? Mathf.Lerp(progressBar.Value, 100f, (float)delta * 10f) : 0f;
	}


    public override void _ExitTree()
    {
        SteamManager.OnLobbyRefreshCompleted -= OnLobbyRefreshCompletedCallback;
    }

	private void OnLobbyRefreshCompletedCallback(List<Lobby> lobbies)
	{
		foreach(var item in lobbies)
		{
			LobbyElement element = lobbyElement.Instantiate<LobbyElement>();
			lobbyContainer.AddChild(element);
			element.SetLabels(item.Id.ToString(), item.GetData("ownerNameDataString") + "'s lobby", item);
		}
	}

	public async void CreateLobby()
	{
		await SteamManager.Manager.CreateLobby();
	}

	public async void GetLobbies()
	{
		await SteamManager.Manager.GetMultiplayerLobbies();
	}

	public async void Join() 
	{
		string code = codeEdit.Text;
		codeEdit.Text = "";
		await SteamManager.Manager.TryJoinViaCode(code);
	}
}