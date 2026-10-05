using System;
using PackedScores = System.Int32;
using SingleScore = byte;

// using  PlayerScores = (byte,byte,byte,byte);
// using System.Collections.Generic;


public class ScoreSytem
{
	const byte MAX_PLAYERS = sizeof(PackedScores) / sizeof(SingleScore);
	public static ScoreValues playerScores = new();
	public void Main()
	{
		for (uint i = 0; i < MAX_PLAYERS; i++)
		{
			playerScores[i] = (byte)(255-i);
		}
		
		Console.WriteLine(playerScores);
		Console.WriteLine(PackPlayerScores(playerScores));
		Console.WriteLine(PackPlayerScores(playerScores));
	}
	readonly PackedScores[] masks = [
		//MaxValue bitshifted by the byte size of SingleScore * the size of a byte in bits * index
		(SingleScore.MaxValue << sizeof(SingleScore) * 8 * 0),
		(SingleScore.MaxValue << sizeof(SingleScore) * 8 * 1),
		(SingleScore.MaxValue << sizeof(SingleScore) * 8 * 2),
		(SingleScore.MaxValue << sizeof(SingleScore) * 8 * 3)
	];
	PackedScores PackPlayerScores(ScoreValues scores)
	{
		PackedScores o = 0;
		for (byte i = 0; i < ScoreValues.Length; i++)
		{
			o += scores[i] << 8 * i;
		}
		return o;
	}

	ScoreValues UnpackPlayerScores(PackedScores packed)
	{
		ScoreValues o = new();
		for (byte i = 0; i < MAX_PLAYERS; ++i)
		{
			o[i] = (byte)((packed & masks[i]) >> sizeof(SingleScore) * i);
		}
		return o;
	}

	// [StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct ScoreValues
	{
		public const int Length = MAX_PLAYERS;
		public fixed SingleScore data[MAX_PLAYERS];
		public SingleScore this[uint i] 
		{
			get => data[i];
			set => data[i] = value;
		}
	}
}