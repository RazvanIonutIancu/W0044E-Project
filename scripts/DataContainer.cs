using Godot;
using System;

public static class DataContainer
{
	public static byte[] incomingData = new byte[65536];
	public static byte[] outgoingData = new byte[65536];
}

public enum NetEventTypeEnum : byte {
	ReadyMessage,
	ChatMessage,
}
public struct NetEventData {
	public NetEventTypeEnum dataType;
}

public struct NetEventChatMessage() {
	public readonly NetEventTypeEnum dataType = NetEventTypeEnum.ChatMessage;
	public string Message;
}

public struct NetEventReady() {
	public readonly NetEventTypeEnum dataType = NetEventTypeEnum.ReadyMessage;
	public bool Ready;
}