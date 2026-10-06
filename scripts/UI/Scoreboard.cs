using Godot;
using System;
using System.Collections.Generic;

public partial class Scoreboard : Control
{

	//private Dictionary<string, string> players;

	[Export]
	VBoxContainer scoreHolder;
	[Export]
	int scorePerKill = 10;
	[Export]
	int minScorePerKill = 1;
	[Export]
	int maxScorePerKill = 20;

	Dictionary<string, int> playerScores = new Dictionary<string, int>();
	List<Label> scoreLabels = new List<Label>();
	List<Label> revengeLabels = new List<Label>();
	Player controlledPlayer;

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
			if(GameManager.GetPlayerObjectFromID(playerID.ToString()).isControlled)
			{
				controlledPlayer = GameManager.GetPlayerObjectFromID(playerID.ToString());
			}
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


	private void CreateUI()
	{
		foreach(PlayerState playerState in GameManager.Instance().playerList)
		{
			HBoxContainer container = new HBoxContainer();

			Label nameLabel = new Label();
			nameLabel.CustomMinimumSize = new Vector2(200f, 50f);
			nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
			nameLabel.VerticalAlignment = VerticalAlignment.Center;
			nameLabel.Text = playerState.GetName();

			Label scoreLabel = new Label();
			scoreLabel.CustomMinimumSize = new Vector2(200f, 50f);
			scoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
			scoreLabel.VerticalAlignment = VerticalAlignment.Center;
			scoreLabel.Text = playerScores[playerState.ToString()].ToString();
			scoreLabels.Add(scoreLabel);

			Label revengeLabel = new Label();
			revengeLabel.CustomMinimumSize = new Vector2(200f, 50f);
			revengeLabel.HorizontalAlignment = HorizontalAlignment.Center;
			revengeLabel.VerticalAlignment = VerticalAlignment.Center;
			if(playerState.ToString() != controlledPlayer.playerID)
			{
				revengeLabel.Text = controlledPlayer.playerRevengeScore[playerState.ToString()].ToString();
			}
			else
			{
				revengeLabel.Text = "-";
			}
			revengeLabels.Add(revengeLabel);
		}
	}

	private void UpdateUI()
	{
		for (int i = 0; i < GameManager.Instance().playerList.Count; i++)
		{
			scoreLabels[i].Text = playerScores[GameManager.Instance().playerList[i].ToString()].ToString();
			if(revengeLabels[i].Text != "-")
			{
				revengeLabels[i].Text = 
					controlledPlayer.playerRevengeScore[GameManager.Instance().playerList[i].ToString()].ToString();
			}
			else
			{
				continue;
			}
		}
	}

}
