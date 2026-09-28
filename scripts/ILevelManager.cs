using Godot;
using System;

public interface ILevelManager
{
    public void SpawnPlayer(PlayerState state);
    public string Name();
}