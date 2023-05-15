using System.Collections.Generic;
using FishNet.Broadcast;

public struct CharacterAttributeUpdateBroadcast : IBroadcast
{
	public int templateID;
	public int baseValue;
	public int modifier;
	public uint applyTick;
}

public struct CharacterAttributeUpdateMultipleBroadcast : IBroadcast
{
	public List<CharacterAttributeUpdateBroadcast> attributes;
	public uint applyTick;
}