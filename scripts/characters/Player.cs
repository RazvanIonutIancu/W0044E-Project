using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public float speed = 300.0f;
	[Export]
	public RayCast2D aimingRayCast;

    [Export] private Camera2D camera;


    public string playerID = "0A";
	public bool isAlive = true;

    private bool isControlled = false;


    // DEBUG

    PackedScene sparkPackedScene = GD.Load<PackedScene>("res://scenes/particles/SparkParticles.tscn");

    // DEBUG END


    public void Initialize()
	{
        isControlled = playerID == SteamManager.Manager.PlayerSteamID.AccountId.ToString();
        if (isControlled) { camera.MakeCurrent(); }
    }


	public override void _PhysicsProcess(double delta)
	{
		if(!isControlled)
		{
			// This is a remote player
            return;
        }
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


		if(Input.IsActionJustPressed("shoot"))
		{
			Shoot();
		}


		Velocity = velocity.Normalized() * speed;
		MoveAndSlide();
	}


	private void PlayerAim()
	{
		LookAt(GetGlobalMousePosition());
	}






	// DEBUG

	private void Shoot()
	{


		Vector2 targetPosition = aimingRayCast.GetCollisionPoint();

		GpuParticles2D sparkParticles = sparkPackedScene.Instantiate<GpuParticles2D>();
		sparkParticles.GlobalPosition = targetPosition;
		AddSibling(sparkParticles);


	}




}
