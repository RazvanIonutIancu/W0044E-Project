using Godot;
using System;

public partial class TestLevel1 : Node2D, ILevelManager
{

    [Export] private PackedScene playerPrefab;

    [Export] private Node2D spawnPointContainer;
    [Export] private Node2D playerContainer;

    private int spawnpointIndex = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public Player SpawnPlayer(PlayerState state)
    {
        // SpawnPlayer
        Player player = playerPrefab.Instantiate<Player>();
        playerContainer.AddChild(player);
        player.GlobalPosition = GetSpawnPoint();
        player.playerID = state.ToString();
        return player;
    }

	public Vector2 GetSpawnPoint()
	{
        Vector2 pos = ((Node2D)spawnPointContainer.GetChildren()[spawnpointIndex]).GlobalPosition;
        spawnpointIndex++;
		if(spawnpointIndex >= spawnPointContainer.GetChildCount()) { spawnpointIndex = 0; }
        return pos;
    }

    public string LevelName() { return "Test Level 1"; }
}
