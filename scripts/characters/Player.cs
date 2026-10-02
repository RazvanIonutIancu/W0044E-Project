using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D, IShootable
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

	/// <summary>
	/// Changed by the host
	/// </summary>
	public Vector2 respawnLocation = new Vector2();


	Dictionary<string, string> playerMovementDictionary = new Dictionary<string, string>();
	Dictionary<string, string> playerShoot = new Dictionary<string, string>();
	Dictionary<string, string> playerShootResults = new Dictionary<string, string>();

	public enum PossibleHits
	{
		NoHit,
		RigidBody,
		Player
	}



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

			GetNode<Polygon2D>("Mesh").Color = Colors.Black;
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

		playerShoot.Add("DataType", "Shoot");
		playerShoot.Add("playerID", playerID);

		playerShootResults.Add("DataType", "ShootResults");
		playerShootResults.Add("playerID", playerID);
		// if hitting something, for particle uses. NoHit has no particles
		playerShootResults.Add("targetHit", PossibleHits.NoHit.ToString());
		playerShootResults.Add("hitPosX", "0.0");
		playerShootResults.Add("hitPosY", "0.0");
		// if it hits a player send playerID
		playerShootResults.Add("playerHit", "0A");
	}

	public override void _EnterTree()
	{
		GameManager.Instance().playerNodeList.Add(this);
		DataParser.OnPlayerMove += MovePlayer;
		DataParser.OnShootResults += ShootResults;
	}

	public override void _ExitTree()
	{
		GameManager.Instance().playerNodeList.Remove(this);
		DataParser.OnPlayerMove -= MovePlayer;
		DataParser.OnShootResults -= ShootResults;
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

	/// <summary>
	/// This one is moved by the client
	/// </summary>
	/// <param name="delta"></param>
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
			TryShoot();
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

	/// <summary>
	/// This one is moved by the host
	/// </summary>
	/// <param name="packet"></param>
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





	/// <summary>
	/// Sends request to shoot
	/// </summary>
	private void TryShoot()
	{
		

		if(!reloadTimer.IsStopped())
		{
			return;
		}

		if(!SteamManager.Manager.IsHost)
		{
			SteamManager.SendData(playerShoot, Steamworks.Data.SendType.Reliable);
		}
		else
		{
			GD.Print("Host is trying to shoot");
			Dictionary<string, string> shotResults = CheckShot(playerShoot);
			SteamManager.SendData(shotResults, Steamworks.Data.SendType.Reliable);
			DataParser.OnShootResults.Invoke(shotResults);
			//ShootResults(shotResults);
		}

		


		reloadTimer.Start();
	}


	// DEBUG

	public Dictionary<string, string> CheckShot(Dictionary<string, string> packet)
	{
		GD.Print("Checking shot...");
		Vector2 targetPosition;

		if(aimingRayCast.IsColliding())
		{
			if(aimingRayCast.GetCollider() is IShootable obj)
			{
				if(obj is Player player)
				{
					playerShootResults["targetHit"] = PossibleHits.Player.ToString();
					playerShootResults["playerHit"] = player.playerID;
				}
				else
				{
					playerShootResults["targetHit"] = PossibleHits.RigidBody.ToString();
				}
			}
			else
			{
				playerShootResults["targetHit"] = PossibleHits.RigidBody.ToString();
			}

			targetPosition = aimingRayCast.GetCollisionPoint();

			playerShootResults["hitPosX"] = targetPosition.X.ToString();
			playerShootResults["hitPosY"] = targetPosition.Y.ToString();
			
		}
		else
		{
			playerShootResults["targetHit"] = PossibleHits.NoHit.ToString();
		}


		GD.Print("Shot Checked!");
		return playerShootResults;
	}


	private void ShootResults(Dictionary<string, string> packet)
	{

		if(packet["playerID"] == playerID)
		{
			GetNode<MuzzleFlash>("MuzzleFlash").ShowMuzzleFlash();
		}

		if(packet["playerHit"] == playerID)
		{
			GD.Print("Hit a player!");
			KillPlayer();
		}

		if(isControlled)
		{
			if(packet["targetHit"] == PossibleHits.NoHit.ToString())
			{

				GD.Print("No target hit!");
				return;
			}

			GpuParticles2D sparkParticles = sparkPackedScene.Instantiate<GpuParticles2D>();
			Vector2 targetPosition = new Vector2(float.Parse(packet["hitPosX"]), float.Parse(packet["hitPosY"]));
			sparkParticles.GlobalPosition = targetPosition;
			AddSibling(sparkParticles);

			if(packet["targetHit"] == PossibleHits.RigidBody.ToString())
			{
				GD.Print("Hit a wall!");
			}
		}
	}


	private void KillPlayer()
	{
		isAlive = false;
		GetNode<Polygon2D>("Mesh").Visible = false;
		GetNode<CollisionPolygon2D>("CollisionShape").Visible = false;
		SpawnClone();
		respawnTimer.Start();
		respawnCameraTimer.Start();
		if(isControlled)
		{
			respawnScreen.Show();
		}
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
		if(isControlled)
		{
			respawnScreen.Hide();
		}
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
