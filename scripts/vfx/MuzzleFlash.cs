using Godot;
using System;

public partial class MuzzleFlash : Sprite2D
{

	Timer flashTimer = new Timer();

	public override void _Ready()
	{
		flashTimer.OneShot = true;
		flashTimer.Autostart = false;
		flashTimer.WaitTime = 0.1f;
		AddChild(flashTimer);
	}

	public override void _PhysicsProcess(double delta)
	{
		if(!Visible)
		{
			return;
		}

		if(flashTimer.IsStopped())
		{
			Visible = false;
		}
	}

	public void ShowMuzzleFlash()
	{
		Visible = true;
		flashTimer.Start();
	}
}
