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

	public static Dictionary<string,string> ParseData(nint data, int size)
	{
		Marshal.Copy(data, DataContainer.incomingData, 0, size);
		string str = System.Text.Encoding.UTF8.GetString(DataContainer.incomingData.AsSpan<byte>(0,size));
		GD.Print(str);
		return JsonConvert.DeserializeObject<Dictionary<string,string>>(str);
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
			case "ReadyMessage":
				OnReadyMessage.Invoke(packet);
				break;
			default:
				break;
		}
	}
}