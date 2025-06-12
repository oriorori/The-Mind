using System.Collections.Generic;
using UnityEngine;

public struct SignupData
{
    public string userId;
    public string nickname;
    public string password;
    public string passwordConfirmation;
}

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

[System.Serializable]
public class Room
{
    public int id;
    public List<string> players;
    public int maxPlayerNumber;
    public int playerCount;
}

public struct RoomResponse
{
    public Room room;
}


public struct DestroyRoomData
{
    public int roomId;
}