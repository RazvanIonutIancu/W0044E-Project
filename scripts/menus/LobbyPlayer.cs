using Godot;
using System;

public partial class LobbyPlayer : Node
{
	[Export] private TextureRect avatar;
	[Export] private RichTextLabel name;

	public void SetLabels(string _name, Texture2D sprite)
	{
		name.Text = _name;
		avatar.Texture = sprite;
	}
}