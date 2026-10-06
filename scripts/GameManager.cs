using Godot;
using System;
using System.Collections.Generic;

public sealed class GameManager
{
    private static GameManager instance;

	public List<PlayerState> playerList = new List<PlayerState>();
	public List<Player> playerNodeList = new List<Player>();

    public int selectedLevelIndex = 0;

    public ILevelManager currentLevel = null;

    public static GameManager Instance()
	{
		if(instance == null)
		{
            instance = new GameManager();
        }
        return instance;
    }

	private GameManager()
	{
		
	}

	public static void AddPlayer(string id, string name)
	{
        Instance().playerList.Add(new PlayerState(id, name));
    }

	public static void RemovePlayer(string id)
	{
        PlayerState player = GetPlayerState(id);
		if(player == null) { return; }
    	Instance().playerList.Remove(player);
    }

	public static PlayerState GetPlayerState(string id)
	{
        for (int i = 0; i < Instance().playerList.Count; i++)
		{
			if(Instance().playerList[i].GetID() == id)
			{
                return Instance().playerList[i];
            }
		}
        return null;
    }

	public static bool IsEveryoneReady()
	{
		foreach(PlayerState player in Instance().playerList)
		{
			if(!player.isReady) { return false; }
		}
        return true;
    }


	/// <summary>
	/// 
	/// </summary>
	/// <param name="playerID"></param>
	/// <returns>NULL if it doesn't find the player</returns>
	public static Player GetPlayerObjectFromID(string playerID)
	{

		foreach(Player player in Instance().playerNodeList)
		{
			if(playerID == player.playerID)
			{
				return player;
			}
		}


		return null;
	}

	public static int GetRevengeScore(string shootingPlayer, string playerShot)
	{
		return GetPlayerObjectFromID(shootingPlayer).playerRevengeScore[playerShot];
	}

	public static void ChangeRevengeScore(string shootingPlayer, string playerShot)
	{
		GetPlayerObjectFromID(shootingPlayer).playerRevengeScore[playerShot] -= 1;
		GetPlayerObjectFromID(playerShot).playerRevengeScore[shootingPlayer] += 1;
	}

	public static string GetPlayerName(string playerID)
	{
		foreach(PlayerState player in Instance().playerList)
		{
			if(player.ToString() == playerID)
			{
				return player.GetName();
			}
		}
		return "";
	}

}
