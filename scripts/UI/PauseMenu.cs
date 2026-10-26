using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
	public LobbySession session;
	public StringName PauseAction = "pause";
	[Export] private BaseButton pauseButton;
	[Export] private Control Menuroot;
	[Export] private Label timerLabel;

	public override void _EnterTree()
	{
		SetPaused(false);
	}

	public override void _Ready()
	{
		// the HUD covers the whole screen and would otherwise eat clicks meant
		// for the game-over scoreboard sitting underneath it
		GetNode<Control>("HUD").MouseFilter = Control.MouseFilterEnum.Ignore;
	}

	public override void _Process(double delta)
	{
		session.TickMatch(delta);

		// show the time left as minutes:seconds
		int timeLeft = session.GetMatchTimeLeft();
		timerLabel.Text = (timeLeft / 60) + ":" + (timeLeft % 60).ToString("00");
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
		// NOT IMPLEMENTED
		// kill player
	}
	void QuitToLobby()
	{
		// NOT IMPLEMENTED
		// EndGame()
		// SceneLoader.LoadLobbyMenuOrSomething()
	}
	void QuitGameToMainMenu()
	{
		session.Disconnect();
	}
}
