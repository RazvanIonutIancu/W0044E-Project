using Godot;
using System;

public partial class SteamPacket : Node
{
	public nint data; //test commit
	public int size;

	public SteamPacket(nint _data, int _size)
	{
		data = _data;
		size = _size;
	}
}