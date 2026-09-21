using Godot;
using System;
using System.Threading;

public partial class NetworkThreadManager : Node
{
	private Thread networkThread;
	private bool networkIsRunning = true;



	public async override void _EnterTree()
	{
		networkThread = new Thread(NetworkLoop);
		networkThread.IsBackground = true;
		networkThread.Start();
	}

	public override void _ExitTree()
	{
		networkIsRunning = false;
		if(networkThread != null && networkThread.IsAlive)
		{
			networkThread.Join(500);
		}
	}

	private void NetworkLoop()
	{
		while(networkIsRunning)
		{
			try
			{
				if(SteamManager.steamSocketManager != null)
				{
					SteamManager.steamSocketManager.Receive();
				}
				if(SteamManager.steamConnectionManager != null && SteamManager.steamConnectionManager.Connected)
				{
					SteamManager.steamConnectionManager.Receive();
				}
			}
			catch (System.Exception e)
			{
				GD.Print("Error receiving data: " + e.Message);
			}
			Thread.Sleep(1);
		}
	}
}
