using Steamworks;
using Godot;
using System;
using Steamworks.Data;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Collections.Generic;

public class DataParser
{
	public static List<SteamPacket> packetList = new List<SteamPacket>();

	public static Action<Dictionary<string,string>> OnReadyMessage;
	public static Action<Dictionary<string,string>> OnChatMessage;
    public static Action<Dictionary<string,string>> OnPingInfo;
    public static Action<Dictionary<string,string>> OnStartGame;
    public static Action<Dictionary<string,string>> OnPlayerMove;
    public static Action<Dictionary<string,string>> OnPlayerShoot;

    public static Action<Dictionary<string,string>> OnInitialState;
    public static Action<Dictionary<string, string>> OnLevelSelected;
    public static Dictionary<string,string> ParseData(nint data, int size)
	{
		Marshal.Copy(data, DataContainer.incomingData, 0, size);
		string str = System.Text.Encoding.UTF8.GetString(DataContainer.incomingData.AsSpan<byte>(0,size));
		GD.Print(str);
		return JsonConvert.DeserializeObject<Dictionary<string,string>>(str);
	}

	public static void ProcessData(nint data, int size, Connection? sender)
	{
		Dictionary<string,string> packet = ParseData(data, size);

		switch(packet["DataType"])
		{
			case "ReadyMessage":
			    SyncIncomingData(packet, sender);
				OnReadyMessage.Invoke(packet);
				break;
			case "ChatMessage":
                //message is relatyed so all cconnected clients/users see it terminology kinda crap maybe
                SyncIncomingData(packet, sender);
                OnChatMessage.Invoke(packet);
				break;
			case "PingInfo":
			    SyncIncomingData(packet, sender);
                OnPingInfo.Invoke(packet);
                break;
			case "StartGame":
                OnStartGame.Invoke(packet);
                break;
			case "MovePlayer":
                OnPlayerMove.Invoke(packet);
			    SyncIncomingData(packet, sender);
                break;
			case "Shoot":
				foreach(Player player in GameManager.Instance().playerNodeList)
				{
					if(player.playerID == packet["playerId"])
					{
						Dictionary<string, string> newPacket = player.CheckShot(packet);
						SyncIncomingData(newPacket, null); // ???
					}
				}
				break;
			case "ShootResults":
				OnPlayerShoot.Invoke(packet);
                SyncIncomingData(packet, null);
                break;
            case "InitialState":
                OnInitialState.Invoke(packet);
                break;
			case "LevelSelected":
                OnLevelSelected.Invoke(packet);
                break;
            default:
				break;
		}
	}

	private static void SyncIncomingData(Dictionary<string,string> packet, Connection? sender)
	{
		if(SteamManager.Manager.IsHost) 
		{
			SteamManager.Broadcast(JsonConvert.SerializeObject(packet), SendType.Reliable, sender);
		}
	}
}