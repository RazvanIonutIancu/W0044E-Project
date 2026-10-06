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

	public override void _PhysicsProcess(double delta)
	{
		if(Input.IsActionPressed("showScoreboard"))
		{
			GlobalPosition = controlledPlayer.GlobalPosition;
			UpdateUI();
			Show();
		}
		if(Input.IsActionJustReleased("showScoreboard"))
		{
			Hide();
		}
	}

	public void Initialize()
	{
		foreach(PlayerState playerState in GameManager.Instance().playerList)
		{
			GD.Print(GameManager.Instance().playerList.Count);
			playerScores.Add(playerState.GetID(), 0);
			if(GameManager.GetPlayerObjectFromID(playerState.GetID()).isControlled)
			{
				controlledPlayer = GameManager.GetPlayerObjectFromID(playerState.GetID());
			}
		}
		CreateUI();
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

	}


	private void CreateUI()
	{
		foreach(PlayerState playerState in GameManager.Instance().playerList)
		{
			HBoxContainer container = new HBoxContainer();
			scoreHolder.AddChild(container);

			Label nameLabel = new Label();
			nameLabel.CustomMinimumSize = new Vector2(200f, 50f);
			nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
			nameLabel.VerticalAlignment = VerticalAlignment.Center;
			nameLabel.Text = playerState.GetName();
			container.AddChild(nameLabel);

			Label scoreLabel = new Label();
			scoreLabel.CustomMinimumSize = new Vector2(200f, 50f);
			scoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
			scoreLabel.VerticalAlignment = VerticalAlignment.Center;
			scoreLabel.Text = playerScores[playerState.GetID()].ToString();
			scoreLabels.Add(scoreLabel);
			container.AddChild(scoreLabel);

			Label revengeLabel = new Label();
			revengeLabel.CustomMinimumSize = new Vector2(200f, 50f);
			revengeLabel.HorizontalAlignment = HorizontalAlignment.Center;
			revengeLabel.VerticalAlignment = VerticalAlignment.Center;
			if(playerState.GetID() != controlledPlayer.playerID)
			{
				revengeLabel.Text = controlledPlayer.playerRevengeScore[playerState.GetID()].ToString();
			}
			else
			{
				revengeLabel.Text = "-";
			}
			revengeLabels.Add(revengeLabel);
			container.AddChild(revengeLabel);
		}
	}

	private void UpdateUI()
	{
		for (int i = 0; i < GameManager.Instance().playerList.Count; i++)
		{
			scoreLabels[i].Text = playerScores[GameManager.Instance().playerList[i].GetID()].ToString();
			if(revengeLabels[i].Text != "-")
			{
				revengeLabels[i].Text = 
					controlledPlayer.playerRevengeScore[GameManager.Instance().playerList[i].GetID()].ToString();
			}
			else
			{
				continue;
			}
		}
	}

}
