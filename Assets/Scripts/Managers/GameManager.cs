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
        // multiplayController.Initialize();
    }

    public void InitCurrentPlayingRoom(Room room)
    {
        currentPlayingRoom = room;
    }
    public void AddNewPlayer(string playerName)
    {
        currentPlayingRoom.players.Add(playerName);
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
