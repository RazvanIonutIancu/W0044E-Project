using Godot;
using System;
using System.Collections.Generic;

public partial class Scoreboard : Control
{

	//private Dictionary<string, string> players;


	[Export]
	int scorePerKill = 10;

	Dictionary<string, int> playerScores = new Dictionary<string, int>();



	public void Initialize()
	{
		foreach(PlayerState playerID in GameManager.Instance().playerList)
		{
			playerScores.Add(playerID.ToString(), 0);
		}
	}



	public void AddScore(string shootingPlayer, string playerShot)
	{
		int scoreToAdd = scorePerKill + GameManager.GetRevengeScore(shootingPlayer, playerShot);

		if(scoreToAdd < 1)
		{
			scoreToAdd = 1;
		}

		playerScores[shootingPlayer] += scoreToAdd;


		UpdateUI();
	}


	private void UpdateUI()
	{
	}

}
