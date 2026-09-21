using Steamworks;
using System;
using Godot;
using Steamworks.Data;
using System.Collections.Generic;

public class SteamConnectionManager : ConnectionManager
{
	public override void OnConnected(ConnectionInfo info)
	{
		base.OnConnected(info);
		GD.Print("On connected");
	}

	public override void OnConnecting(ConnectionInfo info)
	{
		base.OnConnecting(info);
		GD.Print("On connecting");
	}

	public override void OnDisconnected(ConnectionInfo info)
	{
		base.OnDisconnected(info);
		GD.Print("On disconnection");
	}

	public override void OnMessage(nint data, int size, long messageNum, long recvTime, int channel)
	{
		base.OnMessage(data, size, messageNum, recvTime, channel);
		DataParser.packetList.Add(new SteamPacket(data, size));
	}
}
