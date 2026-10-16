using GdUnit4;
using static GdUnit4.Assertions;
using Godot;
using System;
using System.Threading.Tasks;






[TestSuite]
public partial class TestScripts : Node2D
{
	

	PackedScene playerScene = GD.Load<PackedScene>("res://scenes/characters/Player.tscn");

	Player player;
	bool isDead = false;

	[TestCase]
	[RequireGodotRuntime]
	public async Task TestDeath()
	{
		player = playerScene.Instantiate<Player>();

		AddChild(player);
		
		player.KillPlayer();

		await Task.Delay(1000);


		if(!player.isAlive && !player.GetNode<Polygon2D>("Mesh").Visible && player.GetNode<CollisionPolygon2D>("CollisionShape").Disabled && !player.GetNode<CollisionPolygon2D>("CollisionShape").Visible)
		{
			isDead = true;
		}
		AssertBool(isDead).IsTrue();

	}

	
	[TestCase]
	[RequireGodotRuntime]
	public async Task TestRespawn()
	{
		if(isDead)
		{
			player.RespawnPlayer();
		}

		if(player.isAlive && player.GetNode<Polygon2D>("Mesh").Visible && !player.GetNode<CollisionPolygon2D>("CollisionShape").Disabled && player.GetNode<CollisionPolygon2D>("CollisionShape").Visible)
		{
			isDead = false;
		}

		AssertBool(isDead).IsFalse();

	}







}



