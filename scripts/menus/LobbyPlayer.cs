using Godot;
using Steamworks;
using System;
using System.Collections.Generic;

public partial class LobbyPlayer : Node
{
	[Export] private TextureRect avatar;
	[Export] private RichTextLabel name;
    [Export] private RichTextLabel readyText;
    [Export] private RichTextLabel pingText;

    private string id;

    private static readonly string prefix = "[font_size=11][i]";
    private static readonly string suffix = "[/i][font_size=11]";

    public override void _EnterTree()
    {
        DataParser.OnPingInfo += OnPingInfoCallback;
    }

	public override void _ExitTree()
    {
        DataParser.OnPingInfo -= OnPingInfoCallback;
    }

    public void SetLabels(string _name, Texture2D sprite, string _id)
	{
        id = _id;
        name.Text = _name;
		avatar.Texture = sprite;
        readyText.Text = prefix + "Not Ready" + suffix;
    }

	public void SetReady(bool isReady) 
	{
		readyText.Text = prefix + (isReady ? "Ready" : "Not Ready") + suffix;
		GD.Print(name.Text + " ready status changed to: " + isReady.ToString());
	}

	public void OnPingInfoCallback(Dictionary<string,string> packet) 
	{
		if(packet["Sender"] != id) { return; }
        pingText.Text = "[font_size=7]Ping: " + packet["Ping"] + "[font_size=7]";
    }
}