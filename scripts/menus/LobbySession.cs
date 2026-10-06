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
	}

	~LobbySession()
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
		lobbyUI?.SetStartButtonState(SteamManager.Manager.IsHost && GameManager.IsEveryoneReady());
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
					{"Sender",SteamManager.Manager.PlayerSteamID.AccountId.ToString()},
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
		if(SteamManager.Manager.IsHost)
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
			{"Sender",SteamManager.Manager.PlayerSteamID.AccountId.ToString()},
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
        lobbyUI?.SetStartButtonState(SteamManager.Manager.IsHost && GameManager.IsEveryoneReady());
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

	private void OnStartGame(Dictionary<string,string> packet)
	{
        GameManager.Instance().currentLevel = SteamManager.Manager.sceneLoader.LoadLevel(int.Parse(packet["LevelID"]));

        foreach(PlayerState item in GameManager.Instance().playerList)
		{
            GameManager.Instance().currentLevel.SpawnPlayer(item);
        }

		SteamManager.Manager.sceneLoader.LoadPauseMenu(this);
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
			{"Sender",SteamManager.Manager.PlayerSteamID.AccountId.ToString()},
			{"SenderName",SteamManager.Manager.PlayerName},
			{"Message",msg}
		};
		OnChatMessageCallback(packet);
		SteamManager.SendData(packet);
	}
}