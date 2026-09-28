using Godot;
using System;
using System.Collections.Generic;

public sealed class GameManager
{
    private static GameManager instance;

	public List<PlayerState> playerList = new List<PlayerState>();

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
        for (int i = 0; i < Instance().playerList.Count; i++)
		{
			if(Instance().playerList[i].ToString() == id)
			{
                Instance().playerList.RemoveAt(i);
                return;
			}
		}
    }
}
