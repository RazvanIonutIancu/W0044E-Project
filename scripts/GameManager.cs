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

	public static void AddPlayer(string id)
	{
        Instance().playerList.Add(new PlayerState(id));
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
			if(Instance().playerList[i].ToString() == id)
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
}
