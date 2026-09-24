using Godot;
using System;
using System.Runtime.InteropServices;
using System.Reflection;
using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Buffers;
using System.Text;

public partial class SteamManager : Node3D
{

	const int MAX_MEMBERS = 4;
	const int MAX_LOBBY_QUERY_RESULTS = 10;
	const string GAME_KEY = "nyckel";
	const string GAME_VALUE = "varde";

	public static SteamManager Manager;
	public SteamId PlayerSteamID;
	public string PlayerName;

	private static uint gameAppId = 480;
	private bool connectedToSteam;
	public static Lobby? currentLobby;
	private List<Lobby> availableLobbies = new List<Lobby>();

	public static SteamConnectionManager steamConnectionManager;
	public static SteamSocketManager steamSocketManager;

	public static event Action<List<Lobby>> OnLobbyRefreshCompleted;
	public static event Action<bool> OnLobbyInitialized;

	[Export] private SceneLoader sceneLoader;

	private Random random = new Random();

	public bool IsInLobby()
	{
		return currentLobby != null;
	}

	public bool IsHost = false;

	public SteamManager()
	{
		string path = ProjectSettings.GlobalizePath("res://libsteam_api.so");
		if (OperatingSystem.IsLinux())
		{
			NativeLibrary.Load(path);
		}

		if (Manager == null)
		{
			Manager = this;
			try
			{
				SteamClient.Init(gameAppId, true);
				SteamNetworkingUtils.FakeRecvPacketLoss = 0;
				SteamNetworkingUtils.FakeRecvPacketLag = 0;
				SteamNetworkingUtils.FakeSendPacketLag = 0;
				SteamNetworkingUtils.FakeSendPacketLoss = 0;

				if (!SteamClient.IsValid)
				{
					GD.Print("Something went wrong. Steam client is not valid!");
					throw new Exception();
				}
				PlayerName = SteamClient.Name;
				PlayerSteamID = SteamClient.SteamId;
				connectedToSteam = true;
				SteamNetworkingUtils.InitRelayNetworkAccess();
				GD.Print("Steam is connected! Player name: " + PlayerName);
			}
			catch (System.Exception e)
			{
				connectedToSteam = false;
				GD.Print("Error connecting to steam: " + e.Message);
			}
		}
	}

	public override void _Ready()
	{
		sceneLoader.LoadMainMenu();
	}

	public override void _Process(double delta)
	{
		SteamClient.RunCallbacks();
		try
		{
			if(steamSocketManager != null)
			{
                steamSocketManager.Receive();
            }
			if(steamConnectionManager != null && steamConnectionManager.Connected)
			{
                steamConnectionManager.Receive();
            }
		}
		catch(Exception e)
		{
            GD.Print("Error receiving message: " + e.Message + e.StackTrace);
        }
	}

	public void Disconnect()
	{
		sceneLoader.LoadMainMenu();
		if(currentLobby.HasValue)
		{
			currentLobby.Value.Leave();
		}
		currentLobby = null;
		if (steamConnectionManager != null && steamConnectionManager.Connected)
		{
			steamConnectionManager.Close();
		}
		if (steamSocketManager != null)
		{
			steamSocketManager.Close();
		}
		IsHost = false;
	}

	public async Task<bool> CreateLobby()
	{
		if(SteamNetworkingUtils.Status != SteamNetworkingAvailability.Current) 
		{ 
			GD.Print("Try again later. Steam networking availability is pending");
			return false; 
		}
		try
		{
			GD.Print("Creating lobby");
			Lobby? createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(MAX_MEMBERS);

			if(!createLobbyOutput.HasValue)
			{
				GD.Print("Lobby created but didn't instance correctly!");
				throw new Exception();
			}

			Lobby lobby;
			
			lobby = createLobbyOutput.Value;
			lobby.SetPublic();
			lobby.SetJoinable(true);
			lobby.SetData("ownerNameDataString", PlayerName);
			lobby.SetData(GAME_KEY, GAME_VALUE);

			string chars = "ABCDEFGHIJKLMNPQRSTUVWXYZ0123456789";			
			string code = "";

			while(true) 
			{
				code = "";
				for(int i = 0; i < 4; i++) 
				{
					int r = random.Next(0,chars.Length - 1);
					code += chars[r];
					GD.Print("Code " + code);
				}
				Lobby[] lobbies = 
					await SteamMatchmaking.LobbyList.
					WithKeyValue(GAME_KEY,GAME_VALUE).
					WithKeyValue("code",code).RequestAsync();
				if(lobbies == null) { break; }
			}
			GD.Print("Code " + code);

			lobby.SetData("code",code);

			currentLobby = lobby;
			OnLobbyInitialized.Invoke(true);
			GD.Print("Lobby created!");

			return true;
		}
		catch (System.Exception e)
		{
			GD.Print("Failed to create lobby " + e.Message + e.StackTrace);
			return false;
		}
	}

	public void OpenFriendOverlayForInvite()
	{
		if (!currentLobby.HasValue) { return; }
		SteamFriends.OpenGameInviteOverlay(currentLobby.Value.Id);
	}

	public async Task<bool> GetMultiplayerLobbies()
	{
		try
		{
			Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithKeyValue(GAME_KEY,GAME_VALUE).WithMaxResults(MAX_LOBBY_QUERY_RESULTS).RequestAsync();
			if (lobbies != null)
			{
				foreach(var item in lobbies)
				{
					availableLobbies.Add(item);
				}
			}

			OnLobbyRefreshCompleted.Invoke(availableLobbies);
			return true;
		}
		catch (System.Exception e)
		{
			GD.Print("Error fetching lobbies! " + e.Message);
			return false;
		}
	}

	public async Task<bool> TryJoinViaCode(string code)
	{
		if(SteamNetworkingUtils.Status != SteamNetworkingAvailability.Current) 
		{ 
			GD.Print("Try again later. Steam networking availability is pending");
			return false; 
		}

		Lobby[] lobbies = 
			await SteamMatchmaking.LobbyList.
			WithKeyValue(GAME_KEY,GAME_VALUE).
			WithKeyValue("code",code).RequestAsync();
		if(lobbies == null) 
		{ 
			GD.Print("Found no lobby with code: " + code);
			return false; 
		}
		await lobbies[0].Join();
		return true;
	}

	public override void _Notification(int what)
	{
		base._Notification(what);
		if (what == NotificationWMCloseRequest)
		{
			Disconnect();
			SteamClient.Shutdown();
			GetTree().Quit();
		}
	}

	public void CreateSteamSocketServer()
	{
		steamSocketManager = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);
		steamConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(PlayerSteamID,0);
		IsHost = true;

		GD.Print("Socket server created!");
	}

	public void JoinSteamSocketServer(SteamId host)
	{
		if (!IsHost)
		{
			GD.Print("Joining socket server...");
			steamConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(host, 0);
		}
	}

	public void Broadcast(string packetStr, SendType sendType = SendType.Reliable, Steamworks.Data.Connection? skip = null)
	{
		int byteCount = Encoding.UTF8.GetByteCount(packetStr);
		if(byteCount > DataContainer.outgoingData.Length)
		{
			GD.Print("String is too large for the outgoing data buffer");
			return;
		}
		Encoding.UTF8.GetBytes(packetStr, 0, packetStr.Length, DataContainer.outgoingData, 0);
		unsafe
		{
			fixed(byte* ptr = DataContainer.outgoingData)
			{
				foreach(var item in steamSocketManager.Connected.Skip(1).ToArray())
				{
					if(skip.HasValue) 
					{
						if(item == skip.Value) { continue; }
					}
					item.SendMessage((nint)ptr, byteCount, sendType);
				}
			}
		}
	}

	public static void SendData(Dictionary<string,string> packet, SendType sendType = SendType.Reliable)
	{
		string str = JsonConvert.SerializeObject(packet);
		int byteCount = Encoding.UTF8.GetByteCount(str);
		if(byteCount > DataContainer.outgoingData.Length)
		{
			GD.Print("String is too large for the outgoing data buffer");
			return;
		}
		Encoding.UTF8.GetBytes(str, 0, str.Length, DataContainer.outgoingData, 0);
        unsafe
        {
            fixed (byte* ptr = DataContainer.outgoingData)
            {
				if (Manager.IsHost)
				{
					foreach(var item in steamSocketManager.Connected.Skip(1).ToArray())
					{
						item.SendMessage((nint)ptr, byteCount, sendType);
					}
				} 
				else
				{
					steamConnectionManager.Connection.SendMessage((nint)ptr, byteCount, sendType);
				}
            }
        }
	}
}
