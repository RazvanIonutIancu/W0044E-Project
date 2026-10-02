using System;
using Godot;
public class LobbyVisibility
{
	public const string KEY_VIS = "visibility";
	public const string VALUE_VIS_PUB = nameof(LobbyVisibilityEnum.Public);
	public const LobbyVisibilityEnum DefaultValue = LobbyVisibilityEnum.Public;
	public LobbyVisibility()
	{
		innerValue = LobbyVisibilityEnum.Public;
	}
	/**
	<summary>maps to inpector: LobbyType.Items[uint idx].ID</summary>
	*/
	public enum LobbyVisibilityEnum
	{
		Public = 0,
		Hidden = 1,
		FriendsOnly = 2,
		Private = 3,
	}
	private LobbyVisibilityEnum innerValue;
	public LobbyVisibilityEnum GetValue() { return innerValue; }

	public void SetValue(LobbyVisibilityEnum value)
	{
		if (Enum.IsDefined<LobbyVisibilityEnum>(value))
			innerValue = value;
		else innerValue = DefaultValue;
	}
	public void SetValue(int value)
	{
		if (Enum.IsDefined<LobbyVisibilityEnum>((LobbyVisibilityEnum)value))
			innerValue = (LobbyVisibilityEnum)value;
		else innerValue = DefaultValue;
	}

	// casts default -> public
	public override string ToString()
	{
		return innerValue.ToString();
	}
	// public static string Sttringify
}