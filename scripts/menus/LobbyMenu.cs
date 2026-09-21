using Godot;
using System;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using Newtonsoft.Json;
using System.Linq;

public partial class LobbyMenu : Control
{
	[Export] private PackedScene lobbyPlayer;
	[Export] private VBoxContainer playerContainer;

	[Export] private RichTextLabel codeLabel;

	private bool clientIsReady = false;

	public override void _EnterTree()
    {
		SteamCallbacks.OnPlayerLeftLobby += OnPlayerLeftLobbyCallback;
		SteamCallbacks.OnPlayerJoinLobby += OnPlayerJoinLobbyCallback;
		SteamManager.OnLobbyInitialized += OnLobbyInitializedCallback;
		DataParser.OnReadyMessage += OnReadyMessageCallback;
    }

    public override void _ExitTree()
    {
		SteamCallbacks.OnPlayerLeftLobby -= OnPlayerLeftLobbyCallback;
		SteamCallbacks.OnPlayerJoinLobby -= OnPlayerJoinLobbyCallback;
		SteamManager.OnLobbyInitialized -= OnLobbyInitializedCallback;
		DataParser.OnReadyMessage -= OnReadyMessageCallback;
    }

	private void OnPlayerLeftLobbyCallback(Friend friend)
	{
		GetNode<LobbyPlayer>($"Players/{friend.Id.AccountId.ToString()}").QueueFree();
	}

	public void Disconnect()
	{
		SteamManager.Manager.Disconnect();
	}

	public void AddLobbyPlayerElement(Friend friend)
	{
		LobbyPlayer player = lobbyPlayer.Instantiate<LobbyPlayer>();
		playerContainer.AddChild(player);
		player.Name = friend.Id.AccountId.ToString();

		Steamworks.Data.Image? rawSteamAvatar = friend.GetSmallAvatarAsync().Result;

		Texture2D avatar = new Texture2D();

		if(rawSteamAvatar.HasValue)
		{
			Steamworks.Data.Image steamImage = rawSteamAvatar.Value;
			Godot.Image godotImage = new Godot.Image();
			godotImage.SetData((int)steamImage.Width, (int)steamImage.Height,false, Godot.Image.Format.Rgba8, steamImage.Data);
			avatar = ImageTexture.CreateFromImage(godotImage);
		}
	
		player.SetLabels(friend.Name.ToString(),avatar);
	}

	public void ToggleReady() 
	{
		clientIsReady = !clientIsReady;

		Dictionary<string,string> packet = new Dictionary<string,string>()
		{
			{"DataType","ReadyMessage"},
			{"Sender",SteamManager.Manager.PlayerSteamID.AccountId.ToString()},
			{"Ready",clientIsReady.ToString()}
		};
		OnReadyMessageCallback(packet);
		SteamManager.SendData(packet);
	}

	private void OnPlayerJoinLobbyCallback(Friend friend)
	{
		AddLobbyPlayerElement(friend);
	}

	public void InviteFriend()
	{
		SteamManager.Manager.OpenFriendOverlayForInvite();
	}

	public void OnLobbyInitializedCallback(bool b) 
	{
		codeLabel.Text = "Code: " + SteamManager.currentLobby.Value.GetData("code");
	}

	private void OnReadyMessageCallback(Dictionary<string,string> packet) 
	{
		GetNode<LobbyPlayer>($"Players/{packet["Sender"]}").SetReady(bool.Parse(packet["Ready"]));
	}
}