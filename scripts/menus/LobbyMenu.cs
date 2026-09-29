using Godot;
using System;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

public partial class LobbyMenu : Control
{
	[Export] private PackedScene lobbyPlayer;
	[Export] private VBoxContainer playerContainer;

	[Export] private RichTextLabel codeLabel;

	[Export] public RichTextLabel chatLog;
	[Export] private LineEdit chatInput;

    [Export] private OptionButton levelSelect;
    [Export] private Button startButton;

    private bool clientIsReady = false;

    private int frameCounter = 0;
    private int frameCounterTarget = 30;

    //private Dictionary<string, string> initialStatePacket;

    public override void _EnterTree()
    {
		SteamCallbacks.OnPlayerLeftLobby += OnPlayerLeftLobbyCallback;
		SteamCallbacks.OnPlayerJoinLobby += OnPlayerJoinLobbyCallback;
		SteamManager.OnLobbyInitialized += OnLobbyInitializedCallback;
		DataParser.OnReadyMessage += OnReadyMessageCallback;
		DataParser.OnChatMessage += OnChatMessageCallback;
        DataParser.OnStartGame += OnStartGame;
        DataParser.OnInitialState += OnInitialState;
        DataParser.OnLevelSelected += OnLevelSelected;

        int _id = 0;
        foreach(PackedScene scene in SteamManager.Manager.sceneLoader.levels)
		{
            ILevelManager level = scene.Instantiate<ILevelManager>();
            levelSelect.AddItem(level.LevelName(), _id);
            ((Node)level).Free();
            _id++;
        }
        levelSelect.Selected = 0;
        LevelSelected(levelSelect.Selected);

        if(!SteamManager.Manager.IsHost)
		{
            levelSelect.Disabled = true;
            startButton.Visible = false;
        }
    }

    public override void _ExitTree()
    {
		SteamCallbacks.OnPlayerLeftLobby -= OnPlayerLeftLobbyCallback;
		SteamCallbacks.OnPlayerJoinLobby -= OnPlayerJoinLobbyCallback;
		SteamManager.OnLobbyInitialized -= OnLobbyInitializedCallback;
		DataParser.OnReadyMessage -= OnReadyMessageCallback;
		DataParser.OnChatMessage -= OnChatMessageCallback;
		DataParser.OnStartGame -= OnStartGame;
        DataParser.OnInitialState -= OnInitialState;
        DataParser.OnLevelSelected -= OnLevelSelected;
    }

	public override void _Process(double delta)
    {
		if(SteamManager.steamConnectionManager != null && SteamManager.steamConnectionManager.Connected)
		{
			frameCounter++;
			if(frameCounter >= frameCounterTarget) 
			{
 				Dictionary<string, string> packet = new ()
        		{
					{"DataType","PingInfo"},
					{"Sender",SteamManager.Manager.PlayerSteamID.AccountId.ToString()},
					{"Ping",SteamManager.steamConnectionManager.Connection.QuickStatus().Ping.ToString()}
        		};
        	    SteamManager.SendData(packet);
				foreach(Node node in playerContainer.GetChildren()) 
				{
					if(node is LobbyPlayer player)
					{
        	        	player.OnPingInfoCallback(packet);
					}
        	    }
        	    frameCounter = 0;
        	}
		}
    }

	private void OnInitialState(Dictionary<string,string> packet)
	{
        //initialStatePacket = packet;
        codeLabel.Text = "Code: " + packet["Code"];
        int levelSelectedIndex = int.Parse(packet["LevelIndex"]);
        levelSelect.Selected = levelSelectedIndex;
        LevelSelected(levelSelect.Selected);
		foreach(PlayerState player in GameManager.Instance().playerList)
		{
            bool _isReady = bool.Parse(packet[player.ToString()]);
            player.isReady = _isReady;
            GetNode<LobbyPlayer>($"Players/{player.ToString()}").SetReady(_isReady);
        }
    }

	private void OnPlayerLeftLobbyCallback(Friend friend)
	{
        GameManager.RemovePlayer(friend.Id.AccountId.ToString());
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

		Texture2D avatar = new ();

		if(rawSteamAvatar.HasValue)
		{
			Steamworks.Data.Image steamImage = rawSteamAvatar.Value;
			Godot.Image godotImage = new();
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
        GameManager.AddPlayer(friend.Id.AccountId.ToString());
        AddLobbyPlayerElement(friend);
        //OnLobbyInitializedCallback(true);
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
        string _id = packet["Sender"];
        bool _isReady = bool.Parse(packet["Ready"]);
        GameManager.GetPlayerState(_id).isReady = _isReady;
        GetNode<LobbyPlayer>($"Players/{_id}").SetReady(_isReady);
	}

	public void OnChatInputSubmitted(string text)
	{
		SendChatMessage();
	}

	public void OnSendPressed()
	{
		SendChatMessage();
	}

	public void OnStartGamePressed()
	{

        Dictionary<string, string> packet = new Dictionary<string, string>()
        {
			{"DataType","StartGame"},
			{"LevelID",levelSelect.Selected.ToString()}
        };
        SteamManager.SendData(packet);
        OnStartGame(packet);
    }

	private void OnStartGame(Dictionary<string,string> packet)
	{
        GameManager.Instance().currentLevel = SteamManager.Manager.sceneLoader.LoadLevel(int.Parse(packet["LevelID"]));

        foreach(PlayerState item in GameManager.Instance().playerList)
		{
            GameManager.Instance().currentLevel.SpawnPlayer(item);
        }

        Visible = false;
    }

	private void SendChatMessage()
	{
		string message = chatInput.Text.Trim();
		if(message == "") { return; }

		chatInput.Text = "";

		Dictionary<string,string> packet = new ()
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

	private void OnLevelSelected(Dictionary<string,string> packet)
	{
        levelSelect.Selected = int.Parse(packet["LevelID"]);
        LevelSelected(levelSelect.Selected);
    }

	private void LevelSelected(int idx)
	{
        GameManager.Instance().selectedLevelIndex = idx;
		if(SteamManager.Manager.IsHost)
		{
            Dictionary<string, string> packet = new()
            {
				{"DataType", "LevelSelected"},
				{"LevelID", levelSelect.Selected.ToString()}
            };
            SteamManager.SendData(packet);
        }
    }
}