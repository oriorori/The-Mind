using System;
using SocketIOClient;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : SingletonDontDestroy<GameManager>
{
    public MultiplayController multiplayController;

    private void Start()
    {
        multiplayController = new MultiplayController();
        multiplayController.Initialize();
    }

    public void SubscribeEvent(EventType eventType, Action<SocketIOResponse> action)
    {
        multiplayController.events[eventType] += action;
    }
}
