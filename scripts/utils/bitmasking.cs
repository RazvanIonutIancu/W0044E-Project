using System;
using PackedScores = System.Int32;
using ScoreMaskType = System.Int32;
using SingleScore = byte;

// using  PlayerScores = (byte,byte,byte,byte);


public class ScoreSytem
{
	const byte MAX_PLAYERS = sizeof(PackedScores) / sizeof(SingleScore);
	const int BITSHIFT_SIZE = sizeof(SingleScore) * 8;
	public static ScoreValues playerScores = new();
	/*
	00000000000000000000000011111111
	00000000000000001111111100000000
	00000000111111110000000000000000
	11111111000000000000000000000000
	*/
	static readonly ScoreMaskType[] masks = [
		//MaxValue bitshifted by the byte size of SingleScore * the size of a byte in bits * index
		(SingleScore.MaxValue << BITSHIFT_SIZE * 0),
		(SingleScore.MaxValue << BITSHIFT_SIZE * 1),
		(SingleScore.MaxValue << BITSHIFT_SIZE * 2),
		(SingleScore.MaxValue << BITSHIFT_SIZE * 3)
	];
	public static void Test()
	{
        Console.WriteLine("---mask");
		for (uint i = 0; i < MAX_PLAYERS; i++)
		{
			playerScores[i] = (byte)(255-i);
            Console.WriteLine($"{masks[i]}");
		}
		Console.WriteLine("...");
		
        Console.WriteLine($"{playerScores[0]},{playerScores[1]},{playerScores[2]},{playerScores[3]}");
        PackedScores packed = PackPlayerScores(playerScores);
		Console.WriteLine(packed);
        ScoreValues vals = UnpackPlayerScores(packed);
        PackedScores repacked = PackPlayerScores(vals);
        Console.WriteLine(repacked);
        Console.WriteLine($"{vals[0]},{vals[1]},{vals[2]},{vals[3]},");
            
        
	}
	public static PackedScores PackPlayerScores(ScoreValues scores)
	{
		PackedScores o = 0;
		for (byte i = 0; i < ScoreValues.Length; i++)
		{
            o += scores[i] << BITSHIFT_SIZE * i;
		}
		return o;
	}

	public static ScoreValues UnpackPlayerScores(PackedScores packed)
	{
		ScoreValues o = new();
		for (byte i = 0; i < MAX_PLAYERS; ++i)
		{
            o[i] = (byte)((packed & masks[i]) >> BITSHIFT_SIZE * i);
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