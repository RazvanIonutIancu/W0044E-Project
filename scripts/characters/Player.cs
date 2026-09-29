using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public float speed = 300.0f;
	[Export]
	public RayCast2D aimingRayCast;
	[Export]
	public Timer respawnTimer;
	[Export]
	public Timer respawnCameraTimer;

	[Export] private Camera2D camera;


	public string playerID = "0A";
	private bool isControlled = false;
	public bool isAlive = true;


	// DEBUG

	PackedScene sparkPackedScene = GD.Load<PackedScene>("res://scenes/particles/SparkParticles.tscn");
	PackedScene clonePackedScene = GD.Load<PackedScene>("res://scenes/characters/Clone.tscn");

	[Export]
	RespawnScreen respawnScreen;

	// DEBUG END


	public void Initialize()
	{
		isControlled = playerID == SteamManager.Manager.PlayerSteamID.AccountId.ToString();
		if (isControlled) { camera.MakeCurrent(); }


		// DEBUG

		respawnScreen.player = this;
		respawnScreen.respawnTimer = respawnTimer;

		// DEBUG END
	}


	public override void _PhysicsProcess(double delta)
	{

		if(!isControlled)
		{
			// This is a remote player
			return;
		}

		if(!isAlive)
		{
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

		if(Input.IsActionJustPressed("spawnClone"))
		{
			KillPlayer();
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


	private void KillPlayer()
	{
		isAlive = false;
		GetNode<Polygon2D>("Mesh").Visible = false;
		GetNode<CollisionPolygon2D>("CollisionShape").Visible = false;
		SpawnClone();
		respawnTimer.Start();
		respawnCameraTimer.Start();
		respawnScreen.Show();
	}

	private void SpawnClone()
	{
		Vector2 targetPosition = GlobalPosition;

		StaticBody2D clone = clonePackedScene.Instantiate<StaticBody2D>();
		clone.GlobalPosition = targetPosition;
		clone.Rotation = Rotation;
		AddSibling(clone);
	}



	private void RepawnPlayer()
	{
		respawnScreen.Hide();
		isAlive = true;
		GetNode<Polygon2D>("Mesh").Visible = true;
		GetNode<CollisionPolygon2D>("CollisionShape").Visible = true;
	}

	public void MoveRespawn()
	{
		GlobalPosition = GameManager.instance.currentLevel.GetSpawnPoint();
	}




	// DEBUG END


}
