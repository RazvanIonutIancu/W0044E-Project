using Godot;
using System;

public partial class SteamPacket : Node
{
	public nint data;
	public int size;

	public SteamPacket(nint _data, int _size)
	{
		data = _data;
		size = _size;
	}
}