using Godot;
using Steamworks.Data;
using System;
using Steamworks;

public partial class LobbyElement : Node
{

	private Lobby lobby {get; set;}

	[Export] private RichTextLabel nameLabel;

	public void SetLabels(string id, string name, Lobby _lobby)
	{
		nameLabel.Text = name;
		lobby = _lobby;
	}

	public void Join()
	{
		if(SteamNetworkingUtils.Status != SteamNetworkingAvailability.Current) 
		{ 
			GD.Print("Try again later. Steam networking availability is pending");
			return;
		}
		lobby.Join();
	}
}