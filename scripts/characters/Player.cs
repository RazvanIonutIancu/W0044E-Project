using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public float speed = 300.0f;

	public override void _PhysicsProcess(double delta)
	{

		PlayerAim();
		MoveCharacter(delta);

	}


	private void MoveCharacter(double delta)
	{
		Vector2 velocity = Vector2.Zero;

		if(Input.IsActionPressed("up"))
		{
			velocity += Vector2.Up;
		}
		if(Input.IsActionPressed("down"))
		{
			velocity += Vector2.Down;
		}
		if(Input.IsActionPressed("left"))
		{
			velocity += Vector2.Left;
		}
		if(Input.IsActionPressed("right"))
		{
			velocity += Vector2.Right;
		}

		Velocity = velocity.Normalized() * speed;
		MoveAndSlide();
	}


	private void PlayerAim()
	{
		LookAt(GetGlobalMousePosition());
	}


}
