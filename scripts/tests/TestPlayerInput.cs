using GdUnit4;
using static GdUnit4.Assertions;
using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;


[TestSuite]
public partial class TestPlayerInput : Node2D
{
	[Before]
	public void Setup()
	{
		// Setup suite-level shared resources, expensive setup
		SteamManager.Manager = new DummyNetworkManager(); // Keep this line, see if the regular player works
	}

	[After]
	public void Cleanup()
	{
		// Cleanup suite-level shared resources, expensive setup
		SteamManager.Manager = null;
	}

	//PackedScene playerScene = GD.Load<PackedScene>("res://scenes/characters/Player.tscn");
	//Player player;
	Vector2 playerCurrentPosition;
	Vector2 playerLastPosition;

	enum Comparison : byte { GreaterThan, LessThan, }

	readonly struct KeyExpect(Vector2.Axis axis, Comparison comp)
	{
		// public readonly Key key = key;
		public readonly Vector2.Axis axis = axis;
		public readonly Comparison Comp = comp;
	}
	static readonly Dictionary<Key, KeyExpect> MovementKeyMap = new()
	{
		[Key.W] = new(Vector2.Axis.Y, Comparison.LessThan),
		[Key.A] = new(Vector2.Axis.X, Comparison.LessThan),
		[Key.S] = new(Vector2.Axis.Y, Comparison.GreaterThan),
		[Key.D] = new(Vector2.Axis.X, Comparison.GreaterThan),
	};
	readonly static Key[][] MoveInputKeyCombos = [
		// cardinals
		[Key.W], [Key.A], [Key.S], [Key.D],

		// diagonals
		[Key.W, Key.A],
		[Key.S, Key.D],
		[Key.A, Key.S],
		[Key.D, Key.W],
	];

	// tests caridnal and diagonals with the power of loops and selective lazyness
	[TestCase]
	[RequireGodotRuntime]
	public async Task PlayerCanSchmove()
	{

		ISceneRunner runner = ISceneRunner.Load("res://scenes/characters/Player.tscn", true);

		Player player = runner.Scene() as Player;
		player.isControlled = true;



		foreach (Key[] keys in MoveInputKeyCombos)
		{
			playerCurrentPosition = player.GlobalPosition;

			playerLastPosition = playerCurrentPosition;

			List<KeyExpect> expects = [];
			foreach (Key key in keys)
			{
				expects.Add(MovementKeyMap[key]);

				runner.SimulateKeyPress(key);
			}
			await runner.AwaitInputProcessed();

			await runner.AwaitInputProcessed();
			await runner.AwaitIdleFrame();
			await runner.SimulateFrames(30);

			foreach (Key key in keys) { runner.SimulateKeyRelease(key); }
			await runner.AwaitInputProcessed();

			playerCurrentPosition = player.GlobalPosition;


			AssertPositions(playerCurrentPosition, playerLastPosition, expects);
		}

	}

	// +-- x
	// |
	// y
	void AssertPositions(Vector2 currentPos, Vector2 lastPos, List<KeyExpect> expects)
	{
		foreach (KeyExpect expect in expects)
		{
			switch (expect.Comp)
			{
				case Comparison.GreaterThan:
					AssertBool(currentPos[(int)expect.axis] > lastPos[(int)expect.axis]).IsTrue();
					break;
				case Comparison.LessThan:
					AssertBool(currentPos[(int)expect.axis] < lastPos[(int)expect.axis]).IsTrue();
					break;
					// case Comparison.Equals:
					// 	res = res && (lh[(int)expect.axis] == rh[(int)expect.axis]) ;
					// 	break;
			}
		}
		if (expects.Count == 1)
		{
			// TODO: Maybe use range check instead of == due to potential floating point precicion errors
			// Eg. abs(playerCurrentPosition.X - playerLastPosition.X) > EPSLION 
			// where EPSILON is a positive number smaller than 0.1
			// +1 %2 maps X=>Y and Y=>X since Vector2.Axis = { X=0, Y=1 }
			AssertBool(currentPos[((int)expects[0].axis + 1) % 2] == lastPos[((int)expects[0].axis + 1) % 2]).IsTrue();
		}
	}


	[TestCase]
	[RequireGodotRuntime]
	public async Task PlayerCanMove()
	{
		// SteamManager.Manager = new DummyNetworkManager(); // Keep this line, see if the regular player works

		ISceneRunner runner = ISceneRunner.Load("res://scenes/characters/Player.tscn", true);

		Player player = runner.Scene() as Player;
		player.isControlled = true;

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

		// +-- x
		// |
		// y

		// TODO: Maybe use range check instead of == due to potential floating point precicion errors
		// Eg. abs(playerCurrentPosition.X - playerLastPosition.X) > EPSLION 
		// where EPSILON is a positive number smaller than 0.1
		AssertBool(playerCurrentPosition.Y < playerLastPosition.Y).IsTrue();
		AssertBool(playerCurrentPosition.X == playerLastPosition.X).IsTrue();




		playerLastPosition = playerCurrentPosition;

		runner.SimulateKeyPress(Key.A);
		await runner.AwaitInputProcessed();

		await runner.AwaitInputProcessed();
		await runner.AwaitIdleFrame();
		await runner.SimulateFrames(30);

		runner.SimulateKeyRelease(Key.A);
		await runner.AwaitInputProcessed();

		playerCurrentPosition = player.GlobalPosition;

		AssertBool(playerCurrentPosition.X < playerLastPosition.X).IsTrue();
		AssertBool(playerCurrentPosition.Y == playerLastPosition.Y).IsTrue();




		playerLastPosition = playerCurrentPosition;

		runner.SimulateKeyPress(Key.S);
		await runner.AwaitInputProcessed();

		await runner.AwaitInputProcessed();
		await runner.AwaitIdleFrame();
		await runner.SimulateFrames(30);

		runner.SimulateKeyRelease(Key.S);
		await runner.AwaitInputProcessed();

		playerCurrentPosition = player.GlobalPosition;

		AssertBool(playerCurrentPosition.Y > playerLastPosition.Y).IsTrue();
		AssertBool(playerCurrentPosition.X == playerLastPosition.X).IsTrue();




		playerLastPosition = playerCurrentPosition;

		runner.SimulateKeyPress(Key.D);
		await runner.AwaitInputProcessed();

		await runner.AwaitInputProcessed();
		await runner.AwaitIdleFrame();
		await runner.SimulateFrames(30);

		runner.SimulateKeyRelease(Key.D);
		await runner.AwaitInputProcessed();

		playerCurrentPosition = player.GlobalPosition;

		AssertBool(playerCurrentPosition.X > playerLastPosition.X).IsTrue();
		AssertBool(playerCurrentPosition.Y == playerLastPosition.Y).IsTrue();


	}


}