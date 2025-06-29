using System;
using System.Collections.Generic;
using SocketIOClient;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : SingletonDontDestroy<GameManager>
{
    public MultiplayController multiplayController;
    public InGameController inGameController;

    public Room currentPlayingRoom {get; private set;}

    public UserInfo userInfo;
    
    private void Start()
    {
        multiplayController = new MultiplayController();
        inGameController = new InGameController();
    }

    public void InitCurrentPlayingRoom(Room room)
    {
        currentPlayingRoom = room;
    }
    public void AddNewPlayer(string playerName, string nickname)
    {
        currentPlayingRoom.players.Add(playerName);
        currentPlayingRoom.nicknames.Add(nickname);
    }

    public void StartGame(GameInfo gameInfo)
    {
        inGameController.InitGame(gameInfo);
    }

    void OnDestroy()
    {
        multiplayController.DisconnectSocket();
    }
}
