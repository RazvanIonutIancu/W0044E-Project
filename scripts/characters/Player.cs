using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
	[Export]
	public float speed = 300.0f * 60f;
	[Export]
	public RayCast2D aimingRayCast;
	[Export]
	public Timer respawnTimer;
	[Export]
	public Timer respawnCameraTimer;
	[Export]
	public Timer reloadTimer;

	[Export] private Camera2D camera;
	[Export] private PointLight2D visionLight;


	public string playerID = "0A";
	private bool isControlled = false;
	public bool isAlive = true;


	Dictionary<string, string> playerMovementDictionary = new Dictionary<string, string>();




	// DEBUG

	PackedScene sparkPackedScene = GD.Load<PackedScene>("res://scenes/particles/SparkParticles.tscn");
	PackedScene clonePackedScene = GD.Load<PackedScene>("res://scenes/characters/Clone.tscn");
	PackedScene fogPackedScene = GD.Load<PackedScene>("res://scenes/UI/FogOfWar.tscn");

	[Export]
	RespawnScreen respawnScreen;

	// DEBUG END


	public void Initialize()
	{
		isControlled = playerID == SteamManager.Manager.PlayerSteamID.AccountId.ToString();
		if (isControlled)
		{
			camera.MakeCurrent();

			//fog for us only, follows this player + camera
			FogOfWar fog = fogPackedScene.Instantiate<FogOfWar>();
			fog.player = this;
			fog.camera = camera;
			AddSibling(fog);
		}
		else
		{
			//other players shouldnt provide fog of war for each other
			visionLight.Enabled = false;
		}


		// DEBUG

		respawnScreen.player = this;
		respawnScreen.respawnTimer = respawnTimer;

		// DEBUG END
		playerMovementDictionary.Add("DataType", "MovePlayer");
		playerMovementDictionary.Add("playerID", playerID);
		playerMovementDictionary.Add("posX", Position.X.ToString());
		playerMovementDictionary.Add("posY", Position.Y.ToString());
		playerMovementDictionary.Add("rotation", Rotation.ToString());
	}

	public override void _EnterTree()
	{
		DataParser.OnPlayerMove += MovePlayer;
	}

	public override void _ExitTree()
	{
		DataParser.OnPlayerMove -= MovePlayer;
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
		Vector2 velocity = Input.GetVector("left", "right", "up", "down");

		// if(Input.IsActionPressed("up"))
		// {
		// 	velocity += Vector2.Up;
		// }
		// if(Input.IsActionPressed("down"))
		// {
		// 	velocity += Vector2.Down;
		// }
		// if(Input.IsActionPressed("left"))
		// {
		// 	velocity += Vector2.Left;
		// }
		// if(Input.IsActionPressed("right"))
		// {
		// 	velocity += Vector2.Right;
		// }


		if(Input.IsActionJustPressed("shoot"))
		{
			Shoot();
		}

		if(Input.IsActionJustPressed("spawnClone"))
		{
			KillPlayer();
		}


		Velocity = velocity.Normalized() * speed * (float)delta;
		MoveAndSlide();

		playerMovementDictionary["posX"] = GlobalPosition.X.ToString();
		playerMovementDictionary["posY"] = GlobalPosition.Y.ToString();
		playerMovementDictionary["rotation"] = Rotation.ToString();

		SteamManager.SendData(playerMovementDictionary, Steamworks.Data.SendType.NoDelay);
	}


	private void MovePlayer(Dictionary<string, string> packet)
	{
		if(isControlled)
		{
			return;
		}

		if(playerID != packet["playerID"])
		{
			return;
		}

		float newPosX = float.Parse(packet["posX"]);
		float newPosY = float.Parse(packet["posY"]);
		float newRotation = float.Parse(packet["rotation"]);

		Vector2 newGlobalPosition = new Vector2(newPosX, newPosY);

		GlobalPosition = newGlobalPosition;
		Rotation = newRotation;


	}





	private void PlayerAim()
	{
		LookAt(GetGlobalMousePosition());
	}






	// DEBUG

	private void Shoot()
	{
		if(!reloadTimer.IsStopped())
		{
			return;
		}

		Vector2 targetPosition = aimingRayCast.GetCollisionPoint();

		GpuParticles2D sparkParticles = sparkPackedScene.Instantiate<GpuParticles2D>();
		sparkParticles.GlobalPosition = targetPosition;
		
		if(aimingRayCast.IsColliding())
		{
			AddSibling(sparkParticles);
		}
		
		reloadTimer.Start();

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



	private void RespawnPlayer()
	{
		respawnScreen.Hide();
		isAlive = true;
		GetNode<Polygon2D>("Mesh").Visible = true;
		GetNode<CollisionPolygon2D>("CollisionShape").Visible = true;
	}

	public void MoveRespawnPosition()
	{
		GlobalPosition = GameManager.Instance().currentLevel.GetSpawnPoint();
	}




	// DEBUG END





}
