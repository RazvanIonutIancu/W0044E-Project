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
	[Export] private OptionButton lobbyVisibilityOptButton;
	private LobbyVisibility lobbyVisibility = new();

	private void OnLobbyVisibilitySelected(int idx) {
		// int id =lobbyVisibilityOptButton.GetItemId(idx);
		// GD.Print($"OnLobbyVisibilitySelected enum={(LobbyVisibility.LobbyVisibilityEnum)id}, index={idx}, ID={id}");
		lobbyVisibility.SetValue(lobbyVisibilityOptButton.GetItemId(idx));
	}

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
		await SteamManager.Manager.CreateLobby(lobbyVisibility.GetValue());
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