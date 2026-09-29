using Godot;
using System;

public partial class RespawnScreen : Control
{
	public Timer respawnTimer;
	public Player player;
	Label respawnText;

	public override void _Ready()
	{
		respawnText = GetNode<CenterContainer>("CenterContainer").GetNode<Label>("Label");
	}

	public override void _PhysicsProcess(double delta)
	{
		if(respawnTimer == null || respawnTimer.IsStopped())
		{
			return;
		}

	
		Rotation = -player.Rotation;
		respawnText.Text = "Respawning in " + ((int)respawnTimer.TimeLeft + 1) + " seconds";

	}
}
