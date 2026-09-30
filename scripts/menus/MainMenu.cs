using Godot;
using System;
using System.Collections.Generic;
using Steamworks.Data;
using Steamworks;
using static LobbyData;

public partial class MainMenu : Control
{

	[Export] private PackedScene lobbyElement;
	[Export] private VBoxContainer lobbyContainer;
	[Export] private ProgressBar progressBar;
	[Export] private LineEdit codeEdit;
	[Export] private OptionButton lobbyVisibility;
	[Export] private LineEdit HostPasswordEdit;


	private void OnLobbyVisibilitySelected(int index){
		switch ((LobbyVisibilityEnum)index)
		{
			case LobbyVisibilityEnum.Public:
			case LobbyVisibilityEnum.Default:
				HostPasswordEdit.Editable = false;
				break;
			case LobbyVisibilityEnum.Private:
			case LobbyVisibilityEnum.FriendsOnly:
				HostPasswordEdit.Editable = true;
				break;
		}
	}
	LobbyVisibilityEnum GetSelectedLobbyVisibility(){
		return (LobbyVisibilityEnum)lobbyVisibility.Selected;
	}
	string GetSelectedLobbyVisibilityStr(){
		return ((LobbyVisibilityEnum)lobbyVisibility.Selected).ToString();
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
			if(item.GetData("visibility") == "public") {
				LobbyElement element = lobbyElement.Instantiate<LobbyElement>();
				lobbyContainer.AddChild(element);
				element.SetLabels(item.Id.ToString(), item.GetData("ownerNameDataString") + "'s lobby", item);
			}
		}
	}

	public async void CreateLobby()
	{
		GD.PrintS(GetSelectedLobbyVisibilityStr(),"|",lobbyVisibility.Selected);
		await SteamManager.Manager.CreateLobby(GetSelectedLobbyVisibility());
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