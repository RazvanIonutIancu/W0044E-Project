using Godot;
using Godot.NativeInterop;
using Steamworks.Data;
using System;

public class PlayerState(string _id, string _name)
{
    private readonly string id = _id;
	private readonly string name = _name;
	public bool isReady = false;

    public string GetID()
	{
        return id;
    }

	public string GetName()
	{
		return name;
	}
}