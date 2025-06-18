using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public struct SignUpData
{
    public string userId;
    public string nickname;
    public string password;
    public string passwordConfirmation;
}

public struct SignInData
{
    public string userId;
    public string password;
}

public struct SignInResult
{
    public UserInfo userInfo;
}

public struct CreateRoomData
{
    public string playerId;
    public int roomId;
    public int roomSize;
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
    public int roomSize;
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

[System.Serializable]
public class GameInfo
{
    public int roomSize;
    public int currentStage;
    public int remainingLife;
    public int remainingShurikens;
}