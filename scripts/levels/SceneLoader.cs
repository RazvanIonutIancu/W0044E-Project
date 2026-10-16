using Godot;
using System;

public partial class SceneLoader : Node
{
	[Export] private PackedScene mainMenu;
	[Export] private PackedScene lobbyMenu;
	[Export] private PackedScene pauseMenu;

	[Export] private Control menuSceneContainer;

    [Export] private Node2D levelContainer;

    [Export] public PackedScene[] levels;

    private void ClearMenuContainer()
	{
		Godot.Collections.Array<Node> children = menuSceneContainer.GetChildren();
		foreach(Node node in children)
		{
			node.QueueFree();
		}
	}

	private void ClearLevelContainer()
	{
		Godot.Collections.Array<Node> children = levelContainer.GetChildren();
		foreach(Node node in children)
		{
			node.QueueFree();
		}
	}

	public void LoadMainMenu()
	{
		ClearMenuContainer();
		ClearLevelContainer();

		MainMenu menu = mainMenu.Instantiate<MainMenu>();
		menuSceneContainer.AddChild(menu);
	}

	public void LoadLobbyMenu() 
	{
		ClearMenuContainer();

		LobbyMenu menu = lobbyMenu.Instantiate<LobbyMenu>();
		menuSceneContainer.AddChild(menu);
	}

	public ILevelManager LoadLevel(int id)
	{
        ClearLevelContainer();

        ILevelManager level = (ILevelManager)levels[id].Instantiate<Node2D>();
        levelContainer.AddChild((Node)level);
        return level;
    }

	public void LoadPauseMenu(LobbySession session)
	{
		ClearMenuContainer();

		PauseMenu menu = pauseMenu.Instantiate<PauseMenu>();
		menuSceneContainer.AddChild(menu);
		menu.session = session;
	}
}
