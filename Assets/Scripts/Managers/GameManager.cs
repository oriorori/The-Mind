using System;
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

    public void SubscribeEvent(EventType eventType, Action action)
    {
        multiplayController.events[eventType] += action;
    }
}
