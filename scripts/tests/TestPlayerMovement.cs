using GdUnit4;
using static GdUnit4.Assertions;
using Godot;
using System;
using System.Threading.Tasks;


[TestSuite]
public partial class TestPlayerMovement : Node2D
{
	
	//PackedScene playerScene = GD.Load<PackedScene>("res://scenes/characters/Player.tscn");
	//Player player;
	Vector2 playerCurrentPosition;
	Vector2 playerLastPosition;


	[TestCase]
	[RequireGodotRuntime]
	public async Task PlayerCanMove()
	{
		ISceneRunner runner = ISceneRunner.Load("res://scenes/characters/DummyPlayer.tscn");
		DummyPlayer player = runner.Scene() as DummyPlayer;


		playerCurrentPosition = player.GlobalPosition;

		playerLastPosition = playerCurrentPosition;

		runner.SimulateKeyPress(Key.W);
		await runner.AwaitInputProcessed();

		await runner.AwaitInputProcessed();
		await runner.AwaitIdleFrame();
		await runner.SimulateFrames(30);

		runner.SimulateKeyRelease(Key.W);
		await runner.AwaitInputProcessed();

		playerCurrentPosition = player.GlobalPosition;

		AssertBool(playerCurrentPosition == playerLastPosition).IsFalse();


		
		
		playerLastPosition = playerCurrentPosition;

		runner.SimulateKeyPress(Key.A);
		await runner.AwaitInputProcessed();

		await runner.AwaitInputProcessed();
		await runner.AwaitIdleFrame();
		await runner.SimulateFrames(30);

		runner.SimulateKeyRelease(Key.A);
		await runner.AwaitInputProcessed();

		playerCurrentPosition = player.GlobalPosition;

		AssertBool(playerCurrentPosition == playerLastPosition).IsFalse();


		
		
		playerLastPosition = playerCurrentPosition;

		runner.SimulateKeyPress(Key.S);
		await runner.AwaitInputProcessed();

		await runner.AwaitInputProcessed();
		await runner.AwaitIdleFrame();
		await runner.SimulateFrames(30);

		runner.SimulateKeyRelease(Key.S);
		await runner.AwaitInputProcessed();

		playerCurrentPosition = player.GlobalPosition;

		AssertBool(playerCurrentPosition == playerLastPosition).IsFalse();


		
		
		playerLastPosition = playerCurrentPosition;

		runner.SimulateKeyPress(Key.D);
		await runner.AwaitInputProcessed();

		await runner.AwaitInputProcessed();
		await runner.AwaitIdleFrame();
		await runner.SimulateFrames(30);

		runner.SimulateKeyRelease(Key.D);
		await runner.AwaitInputProcessed();

		playerCurrentPosition = player.GlobalPosition;

		AssertBool(playerCurrentPosition == playerLastPosition).IsFalse();












	}









}