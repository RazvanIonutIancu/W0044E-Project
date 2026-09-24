using Steamworks;
using Godot;
using System;
using Steamworks.Data;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Collections.Generic;

public class SteamSocketManager : SocketManager
{
	public override void OnConnected(Connection connection, ConnectionInfo info)
    {
        base.OnConnected(connection, info);
		// Send inital info

		GD.Print("Player has connected");
    }

    public override void OnConnecting(Connection connection, ConnectionInfo info)
    {
        base.OnConnecting(connection, info);
		GD.Print("New player connecting");
    }

    public override void OnDisconnected(Connection connection, ConnectionInfo info)
    {
        base.OnDisconnected(connection, info);
		GD.Print("Player has disconnected");
    }

    public override void OnMessage(Connection connection, NetIdentity identity, nint data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);

        DataParser.ProcessData(data, size, connection);
    }
}