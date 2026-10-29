using Godot;
using System;
using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using System.Globalization;

public class LobbySession
{

	public LobbyMenu lobbyUI;

    private bool clientIsReady = false;

	private int frameCounter = 0;
    private readonly int frameCounterTarget = 30;

	// how long a match lasts in secods
	private readonly float matchLength = 300f;
	private float matchTimeLeft = 0f;
	private bool matchRunning = false;
	private Scoreboard scoreboard;

	// DEBUG

	PackedScene scoreboardScene = GD.Load<PackedScene>("res://scenes/UI/Scoreboard.tscn");


	// DEBUG END

	public LobbySession(LobbyMenu ui = null)
	{
		lobbyUI = ui;
		SteamCallbacks.OnPlayerLeftLobby += OnPlayerLeftLobbyCallback;
		SteamCallbacks.OnPlayerJoinLobby += OnPlayerJoinLobbyCallback;
		SteamManager.OnLobbyInitialized += OnLobbyInitializedCallback;
		DataParser.OnReadyMessage += OnReadyMessageCallback;
		DataParser.OnChatMessage += OnChatMessageCallback;
        DataParser.OnStartGame += OnStartGame;
        DataParser.OnInitialState += OnInitialState;
        DataParser.OnLevelSelected += OnLevelSelected;
        DataParser.OnGameOver += OnGameOver;
	}

	~LobbySession()
	{

	}

	private void OnInitialState(Dictionary<string,string> packet)
	{
		lobbyUI?.OnInitialState(packet["Code"], int.Parse(packet["LevelIndex"]));
		foreach (PlayerState player in GameManager.Instance().playerList)
		{
			bool _isReady = bool.Parse(packet[player.GetID()]);
			player.isReady = _isReady;
			lobbyUI?.SetReadyLabel(player.GetID(), _isReady);
		}
	}

	private void OnPlayerJoinLobbyCallback(Friend friend)
	{
        GameManager.AddPlayer(friend.Id.AccountId.ToString(),friend.Name);
        lobbyUI?.AddLobbyPlayer(friend);
		lobbyUI?.SetStartButtonState(SteamManager.Manager.IsHost() && GameManager.IsEveryoneReady());
    }

	private void OnPlayerLeftLobbyCallback(Friend friend)
	{
		GameManager.RemovePlayer(friend.Id.AccountId.ToString());
		lobbyUI?.RemoveLobbyPlayer(friend.Id.AccountId.ToString());
	}

	public void Process()
	{
		if(SteamManager.steamConnectionManager != null && SteamManager.steamConnectionManager.Connected)
		{
			frameCounter++;
			if(frameCounter >= frameCounterTarget) 
			{
 				Dictionary<string, string> packet = new ()
        		{
					{"DataType","PingInfo"},
					{"Sender",SteamManager.Manager.GetPlayerSteamID().AccountId.ToString()},
					{"Ping",SteamManager.steamConnectionManager.Connection.QuickStatus().Ping.ToString()}
        		};
        	    SteamManager.SendData(packet);
				frameCounter = 0;
				if(lobbyUI == null) { return; }
				foreach(LobbyPlayer player in lobbyUI.GetLobbyPlayers()) 
				{
        	    	player.OnPingInfoCallback(packet);
        	    }
        	}
		}
	}

	public void Disconnect()
	{
		SteamCallbacks.OnPlayerLeftLobby -= OnPlayerLeftLobbyCallback;
		SteamCallbacks.OnPlayerJoinLobby -= OnPlayerJoinLobbyCallback;
		SteamManager.OnLobbyInitialized -= OnLobbyInitializedCallback;
		DataParser.OnReadyMessage -= OnReadyMessageCallback;
		DataParser.OnChatMessage -= OnChatMessageCallback;
		DataParser.OnStartGame -= OnStartGame;
        DataParser.OnInitialState -= OnInitialState;
        DataParser.OnLevelSelected -= OnLevelSelected;
        DataParser.OnGameOver -= OnGameOver;
		GameManager.Instance().Reset();

		// make sure it unfreezes for when next game gets started in saem session
		Player.gameOver = false;
		matchRunning = false;
		SteamManager.Manager.Disconnect();
	}

	public void ToggleReady()
	{
		clientIsReady = !clientIsReady;
        SendReadyPacket();
	}

	public void LevelSelected(int idx)
	{
        GameManager.Instance().selectedLevelIndex = idx;
		if(SteamManager.Manager.IsHost())
		{
            Dictionary<string, string> packet = new()
            {
				{"DataType", "LevelSelected"},
				{"LevelID", idx.ToString()}
            };
            SteamManager.SendData(packet);
        }
    }

	private void OnLevelSelected(Dictionary<string,string> packet)
	{
		int id = int.Parse(packet["LevelID"]);
		lobbyUI?.OnLevelSelected(id);
        LevelSelected(id);
    }

	private void SendReadyPacket()
	{
		Dictionary<string,string> packet = new Dictionary<string,string>()
		{
			{"DataType","ReadyMessage"},
			{"Sender",SteamManager.Manager.GetPlayerSteamID().AccountId.ToString()},
			{"Ready",clientIsReady.ToString()}
		};
		SteamManager.SendData(packet);
		OnReadyMessageCallback(packet);
	}

	private void OnReadyMessageCallback(Dictionary<string,string> packet) 
	{
        string _id = packet["Sender"];
        bool _isReady = bool.Parse(packet["Ready"]);
        GameManager.GetPlayerState(_id).isReady = _isReady;

		lobbyUI?.SetReadyLabel(_id, _isReady);
        lobbyUI?.SetStartButtonState(SteamManager.Manager.IsHost() && GameManager.IsEveryoneReady());
    }

	public void OnLobbyInitializedCallback(bool b) 
	{
		lobbyUI?.SetCodeLabel(SteamManager.currentLobby.Value.GetData("code"));
	}

	public void StartGame(int idx)
	{
		Dictionary<string, string> packet = new Dictionary<string, string>()
        {
			{"DataType","StartGame"},
			{"LevelID",idx.ToString()}
        };
        SteamManager.SendData(packet);
        OnStartGame(packet);
	}

	// run the same level again host only. StartGame already reloads everything fresh
	public void Restart()
	{
		if(!SteamManager.Manager.IsHost()) { return; }
		StartGame(GameManager.Instance().selectedLevelIndex);
	}

	private void OnStartGame(Dictionary<string,string> packet)
	{
        GameManager.Instance().currentLevel = SteamManager.Manager.GetSceneLoader().LoadLevel(int.Parse(packet["LevelID"]));

        foreach(PlayerState item in GameManager.Instance().playerList)
		{
            GameManager.Instance().currentLevel.SpawnPlayer(item);
        }

        foreach(Player player in GameManager.Instance().playerNodeList)
		{
            player.InitializeRevengeList();
        }

		SteamManager.Manager.GetSceneLoader().LoadPauseMenu(this);

		// DEBUG
		// clear out the old scoreboard first so restarts don't stack them
		if(scoreboard != null)
		{
			scoreboard.QueueFree();
		}
		scoreboard = scoreboardScene.Instantiate<Scoreboard>();
		lobbyUI?.GetParent<Control>().AddChild(scoreboard);
		scoreboard.session = this;
		scoreboard.Initialize();
		// DEBUG END

		// fresh match, let people move again and start the clock
		Player.gameOver = false;
		matchTimeLeft = matchLength;
		matchRunning = true;
	}

	// called every frame by the pause menu while a match is on everyone counts down for the on-screen timer, but only the host calls the game the other clients are just emulating i guess, or a rough approx.
	public void TickMatch(double delta)
	{
		if(!matchRunning) { return; }

		matchTimeLeft -= (float)delta;

		if(SteamManager.Manager.IsHost() && matchTimeLeft <= 0f)
		{
			matchRunning = false;
			SendGameOver();
		}
	}

	// used by the pause menu HUD to show the countdown
	public int GetMatchTimeLeft()
	{
		if(matchTimeLeft < 0f) { return 0; }
		return (int)matchTimeLeft;
	}
	// host works out the winner and tells everyone
	private void SendGameOver()
	{
		string winnerID = scoreboard.GetWinner();
		Dictionary<string, string> packet = new Dictionary<string, string>()
		{
			{"DataType","GameOver"},
			{"WinnerID",winnerID}
		};
		SteamManager.SendData(packet);
		OnGameOver(packet);
	}

	private void OnGameOver(Dictionary<string,string> packet)
	{
		// freeze everyone and show the final scores with the winner winner CHICKEN DINNER
		Player.gameOver = true;
		matchRunning = false;
		scoreboard.ShowResults(packet["WinnerID"]);
	}

	private void OnChatMessageCallback(Dictionary<string,string> packet)
	{
		lobbyUI?.AppendChatMessage(packet["SenderName"],packet["Message"]);
	}

	public void SendChatMessage(string msg)
	{
		Dictionary<string,string> packet = new ()
		{
			{"DataType","ChatMessage"},
			{"Sender",SteamManager.Manager.GetPlayerSteamID().AccountId.ToString()},
			{"SenderName",SteamManager.Manager.GetPlayerName()},
			{"Message",msg}
		};
		OnChatMessageCallback(packet);
		SteamManager.SendData(packet);
	}
}