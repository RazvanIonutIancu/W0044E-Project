using Steamworks;
using Godot;
using System;
using Steamworks.Data;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Collections.Generic;

public class DataParser
{
	public static event Action<Dictionary<string,string>> OnUpdateNode;
	public static event Action<Dictionary<string,string>> OnChatMessage;
	public static event Action<Dictionary<string,string>> OnReadyMessage;
	public static event Action<Dictionary<string,string>> OnGameStartMessage;
	public static event Action<Dictionary<string,string>> OnPlayerUpdate;

	public static List<SteamPacket> packetList = new List<SteamPacket>();

	public static Dictionary<string,string> ParseData(nint data, int size)
	{
		Marshal.Copy(data, DataContainer.incomingData, 0, size);
		return JsonConvert.DeserializeObject<Dictionary<string,string>>(System.Text.Encoding.Default.GetString(DataContainer.incomingData.AsSpan<byte>(0,size)));
	}

	public static void ProcessAllData()
	{
		while(packetList.Count > 0)
		{
			SteamPacket packet = packetList[0];
			ProcessData(packet.data, packet.size);
			packetList.Remove(packet);
		}
	}

	public static void ProcessData(nint data, int size)
	{
		Dictionary<string,string> packet = ParseData(data, size);

		switch(packet["DataType"])
		{
			case "ChatMessage":
				OnChatMessage.Invoke(packet);
				break;
			case "Ready":
				OnReadyMessage.Invoke(packet);
				break;
			case "StartGame":
				OnGameStartMessage.Invoke(packet);
				break;
			case "UpdatePlayer":
				OnPlayerUpdate.Invoke(packet);
				break;
			case "UpdateNode":
				OnUpdateNode.Invoke(packet);
				break;
			default:
				break;
		}
	}
}