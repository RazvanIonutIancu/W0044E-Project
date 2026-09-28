using Godot;
using Steamworks.Data;
using System;

public class PlayerState(string _id)
{
    private readonly string id = _id;

	public override string ToString()
	{
        return id;
    }
}


