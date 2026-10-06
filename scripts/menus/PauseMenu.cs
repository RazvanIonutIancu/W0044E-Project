using Godot;
using System;

public partial class PauseMenu : Control
{
	public LobbySession session;
	public StringName PauseAction = "pause";
	[Export] private BaseButton pauseButton;
	[Export] private Control Menuroot;

	public override void _EnterTree()
	{
		SetPaused(false);
	}
	public override void _Process(double delta)
	{
	}
	public override void _Input(InputEvent @event)
	{
		// base._Input(@event);
		if (@event.IsActionPressed(PauseAction))
		{
			SetPaused(!Menuroot.Visible);
		}
	}
	public void SetPaused(bool isPaused)
	{
		Menuroot.Visible = isPaused;
		pauseButton.Visible = !isPaused;
	}

	public void Unstuck()
	{
		// kill player
	}
	void QuitToLobby()
	{

	}
	void QuitGameToMainMenu()
	{
		session.Disconnect();
	}
}
