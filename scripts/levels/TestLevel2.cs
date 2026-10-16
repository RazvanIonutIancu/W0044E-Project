using Godot;
using System;

public partial class TestLevel2 : Node2D, ILevelManager
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public Vector2 GetSpawnPoint()
	{
        return Vector2.Zero;
    }

	public Player SpawnPlayer(PlayerState state)
	{
        return null;
    }

	public string LevelName() { return "Test Level 2"; }
}
