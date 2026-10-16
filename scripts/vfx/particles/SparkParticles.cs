using Godot;
using System;

public partial class SparkParticles : GpuParticles2D
{
	public override void _Ready()
	{
		Emitting = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		if(!Emitting)
		{
			QueueFree();
		}
	}
}
