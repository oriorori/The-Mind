using UnityEngine;

public struct CreateRoomData
{
    public string playerId;
    public int roomId;
    public int maxPlayerNumber;
}

public struct JoinRoomData
{
    public string playerId;
    public int roomId;
}

public struct DestroyRoomData
{
    public int roomId;
}