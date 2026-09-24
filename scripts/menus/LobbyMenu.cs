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

	[Export] public RichTextLabel chatLog;
	[Export] private LineEdit chatInput;

	private bool clientIsReady = false;

    private int frameCounter = 0;
    private int frameCounterTarget = 30;

    public override void _EnterTree()
    {
		SteamCallbacks.OnPlayerLeftLobby += OnPlayerLeftLobbyCallback;
		SteamCallbacks.OnPlayerJoinLobby += OnPlayerJoinLobbyCallback;
		SteamManager.OnLobbyInitialized += OnLobbyInitializedCallback;
		DataParser.OnReadyMessage += OnReadyMessageCallback;
		DataParser.OnChatMessage += OnChatMessageCallback;
    }

    public override void _ExitTree()
    {
		SteamCallbacks.OnPlayerLeftLobby -= OnPlayerLeftLobbyCallback;
		SteamCallbacks.OnPlayerJoinLobby -= OnPlayerJoinLobbyCallback;
		SteamManager.OnLobbyInitialized -= OnLobbyInitializedCallback;
		DataParser.OnReadyMessage -= OnReadyMessageCallback;
		DataParser.OnChatMessage -= OnChatMessageCallback;
    }

	public override void _Process(double delta)
    {
		if(SteamManager.steamConnectionManager != null && SteamManager.steamConnectionManager.Connected)
		{
			frameCounter++;
			if(frameCounter >= frameCounterTarget) 
			{
 				Dictionary<string, string> packet = new Dictionary<string, string>()
        		{
					{"DataType","PingInfo"},
					{"Sender",SteamManager.Manager.PlayerSteamID.AccountId.ToString()},
					{"Ping",SteamManager.steamConnectionManager.Connection.QuickStatus().Ping.ToString()}
        		};
        	    SteamManager.SendData(packet);
				foreach(Node node in playerContainer.GetChildren()) 
				{
					if(node is LobbyPlayer)
					{
        	        	((LobbyPlayer)node).OnPingInfoCallback(packet);
					}
        	    }
        	    frameCounter = 0;
        	}
		}
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
	
		player.SetLabels(friend.Name.ToString(),avatar,friend.Id.AccountId.ToString());
	}

	public void ToggleReady() 
	{
		clientIsReady = !clientIsReady;
        SendReadyPacket();
	}

	private void SendReadyPacket()
	{
		Dictionary<string,string> packet = new Dictionary<string,string>()
		{
			{"DataType","ReadyMessage"},
			{"Sender",SteamManager.Manager.PlayerSteamID.AccountId.ToString()},
			{"Ready",clientIsReady.ToString()}
		};
		SteamManager.SendData(packet);
		OnReadyMessageCallback(packet);
	}

	private void OnPlayerJoinLobbyCallback(Friend friend)
	{
		AddLobbyPlayerElement(friend);
        OnLobbyInitializedCallback(true);
        //SendReadyPacket();
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

	public void OnChatInputSubmitted(string text)
	{
		SendChatMessage();
	}

	public void OnSendPressed()
	{
		SendChatMessage();
	}

	private void SendChatMessage()
	{
		string message = chatInput.Text.Trim();
		if(message == "") { return; }

		chatInput.Text = "";

		Dictionary<string,string> packet = new Dictionary<string,string>()
		{
			{"DataType","ChatMessage"},
			{"Sender",SteamManager.Manager.PlayerSteamID.AccountId.ToString()},
			{"SenderName",SteamManager.Manager.PlayerName},
			{"Message",message}
		};
		OnChatMessageCallback(packet);
		SteamManager.SendData(packet);
	}

	private void OnChatMessageCallback(Dictionary<string,string> packet)
	{ //this is where the messages are handled in tersm of customising. a gd print is put in so we can see in the terminal who is sending what, this is is just me testing sendname packet and message, iuf you see this i forgot to delete so DELETE lol 
		GD.Print(packet["SenderName"] + " sent a message to the lobby, the message was:" + packet["Message"] + ". great success very niceee" );
		chatLog.AppendText("[color=green]" + DateTime.Now.ToString("HH:mm") + "[/color]" + "[b]" + "[color=orange]" + packet["SenderName"] + "[/color][/b]:" + packet["Message"] + "\n");
	}

}