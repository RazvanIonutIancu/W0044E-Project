using Godot;
using System;
using System.Collections.Generic;

public partial class Scoreboard : Control
{

	//private Dictionary<string, string> players;


	[Export]
	int scorePerKill = 10;
	[Export]
	int minScorePerKill = 1;
	[Export]
	int maxScorePerKill = 20;

	Dictionary<string, int> playerScores = new Dictionary<string, int>();

	public override void _EnterTree()
	{
		DataParser.OnShootResults += UpdateScores;
	}

	public override void _ExitTree()
	{
		DataParser.OnShootResults -= UpdateScores;
	}



	public void Initialize()
	{
		foreach(PlayerState playerID in GameManager.Instance().playerList)
		{
			playerScores.Add(playerID.ToString(), 0);
		}
	}

	private void UpdateScores(Dictionary<string, string> packet)
	{
		if(packet["targetHit"] != Player.PossibleHits.Player.ToString())
		{
			return;
		}

		AddScore(packet["playerID"], packet["playerHit"]);
		GameManager.ChangeRevengeScore(packet["playerID"], packet["playerHit"]);
	}



	public void AddScore(string shootingPlayer, string playerShot)
	{
		int scoreToAdd = scorePerKill + GameManager.GetRevengeScore(shootingPlayer, playerShot);

		if(scoreToAdd < 1)
		{
			scoreToAdd = 1;
		}
		else if(scoreToAdd > maxScorePerKill)
		{
			scoreToAdd = maxScorePerKill;
		}

		playerScores[shootingPlayer] += scoreToAdd;


		UpdateUI();
	}


	private void UpdateUI()
	{
	}

}
