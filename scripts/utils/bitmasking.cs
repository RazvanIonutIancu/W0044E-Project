using System;
using PackedScores = System.Int32;
using ScoreMaskType = System.Int32;
using SingleScore = byte;

// using  PlayerScores = (byte,byte,byte,byte);


public class ScoreSytem
{
	const byte MAX_PLAYERS = sizeof(PackedScores) / sizeof(SingleScore);
	public static ScoreValues playerScores = new();
	static readonly ScoreMaskType[] masks = [
		//MaxValue bitshifted by the byte size of SingleScore * the size of a byte in bits * index
		(SingleScore.MaxValue << sizeof(SingleScore) * 8 * 0),
		(SingleScore.MaxValue << sizeof(SingleScore) * 8 * 1),
		(SingleScore.MaxValue << sizeof(SingleScore) * 8 * 2),
		(SingleScore.MaxValue << sizeof(SingleScore) * 8 * 3)
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
            o += scores[i] << sizeof(SingleScore) * 8 * i;
		}
		return o;
	}

	public static ScoreValues UnpackPlayerScores(PackedScores packed)
	{
		ScoreValues o = new();
		for (byte i = 0; i < MAX_PLAYERS; ++i)
		{
            o[i] = (byte)((packed & masks[i]) >> sizeof(SingleScore) * 8 * i);
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