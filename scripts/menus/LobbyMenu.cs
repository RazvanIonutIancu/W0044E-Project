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

	private LobbySession session;

	public override void _EnterTree()
    {
		session = new LobbySession(this);
		int _id = 0;
        foreach(PackedScene scene in SteamManager.Manager.sceneLoader.levels)
		{
            ILevelManager level = scene.Instantiate<ILevelManager>();
            levelSelect.AddItem(level.LevelName(), _id);
            ((Node)level).Free();
            _id++;
        }
        levelSelect.Selected = 0;
        session.LevelSelected(levelSelect.Selected);

		startButton.Visible = false;

        if(!SteamManager.Manager.IsHost)
		{
            levelSelect.Disabled = true;
        }
    }

	public override void _Process(double delta)
    {
		session.Process();
	}

	public void OnInitialState(string code, int levelID)
	{
        codeLabel.Text = "Code: " + code;
        int levelSelectedIndex = levelID;
        levelSelect.Selected = levelSelectedIndex;
        session.LevelSelected(levelSelect.Selected);
    }

	public void SetReadyLabel(string playerID, bool _isReady)
	{
        GetNode<LobbyPlayer>($"Players/{playerID}").SetReady(_isReady);
	}

	public void RemoveLobbyPlayer(string playerID)
	{
        GetNode<LobbyPlayer>($"Players/{playerID}").QueueFree();
	}

	public void Disconnect()
	{
		session = null;
		SteamManager.Manager.Disconnect();
	}

	public void AddLobbyPlayer(Friend friend)
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
		session.ToggleReady();
	}

	public void SetStartButtonState(bool visible)
	{
		startButton.Visible = visible;
	}

	public void InviteFriend()
	{
		SteamManager.Manager.OpenFriendOverlayForInvite();
	}

	public void SetCodeLabel(string code)
	{
		codeLabel.Text = "Code: " + code;
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
		session.StartGame(levelSelect.Selected);
    }

	private void SendChatMessage()
	{
		string message = chatInput.Text.Trim();
		if(message == "") { return; }
		chatInput.Text = "";
		session.SendChatMessage(message);
	}

	public void AppendChatMessage(string senderName, string msg)
	{
		chatLog.AppendText("[color=green]" + DateTime.Now.ToString("HH:mm") + " [/color]" + "[b]" + "[color=orange]" + senderName + ":[/color][/b] " + msg + "\n");
	}

	public void OnLevelSelected(int id)
	{
        levelSelect.Selected = id;
    }

	public LobbyPlayer[] GetLobbyPlayers()
	{
		LobbyPlayer[] arr = new LobbyPlayer[playerContainer.GetChildCount()];
		int idx = 0;
		foreach(Node node in playerContainer.GetChildren())
		{
			arr[idx] = (LobbyPlayer)node;
			idx++;
		}
		return arr;
	}
}