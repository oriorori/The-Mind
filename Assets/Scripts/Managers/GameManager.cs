using System;
using System.Collections.Generic;
using SocketIOClient;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : SingletonDontDestroy<GameManager>
{
    public MultiplayController multiplayController;

    public Room currentPlayingRoom {get; private set;}

    private void Start()
    {
        multiplayController = new MultiplayController();
        multiplayController.Initialize();
    }

    public void SubscribeEvent(EventType eventType, Action<SocketIOResponse> action)
    {
        multiplayController.events[eventType] += action;
    }

    public void InitCurrentPlayingRoom(Room room)
    {
        currentPlayingRoom = room;
    }
    public void AddNewPlayer(string playerName)
    {
        currentPlayingRoom.players.Add(playerName);
    }
}
