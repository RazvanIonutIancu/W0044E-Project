using Godot;
using System;

public interface ILevelManager
{
    public Player SpawnPlayer(PlayerState state);
    public string LevelName();

    public Vector2 GetSpawnPoint();
}