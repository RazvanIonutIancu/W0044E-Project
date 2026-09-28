using Godot;
using System;

public partial class TestLevel1 : Node2D, ILevelManager
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void SpawnPlayer(PlayerState state)
    {
        // SpawnPlayer
    }

    public string Name() { return "Test Level 1"; }
}
