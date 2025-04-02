using UnityEngine;

public struct CreateRoomData
{
    public string playerName;
    public int roomID;
    public int maxPlayers;
}

public struct JoinRoomData
{
    public string playerName;
    public int roomID;
}

public struct DestroyRoomData
{
    public int roomID;
}