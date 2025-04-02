using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using SocketIOClient;

public enum EventType
{
    JoinRoom,
    SuggestStartGame,
    ReadyGame,
    StartGame
}

public class MultiplayController
{
    private SocketIOUnity _socket;

    private Dictionary<string, Action<SocketIOResponse>> socketEventHandlers;

    public Dictionary<EventType, Action> events;
    private event Action OnJoinRoom;
    private event Action OnSuggestStartGame;
    private event Action OnReadyGame;
    private event Action OnStartGame;
    
    Queue<Action> _actionQueue = new Queue<Action>();
    bool _isProcessing = false;
    
    public void Initialize(){
        var uri = new Uri(Constants.GameServerURL);
        _socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
        });

        socketEventHandlers = new Dictionary<string, Action<SocketIOResponse>>()
        {
            { "joinRoomCli", JoinRoom }, 
            { "suggestStartGameCli", SuggestStartGame },
            { "readyGameCli", ReadyGame },
            { "startGameCli", StartGame },
            { "refuseGameCli", RefuseGame },
            { "playCardCli", PlayCard },
            { "suggestShurikenCli", SuggestShuriken },
            { "agreeShurikenCli", AgreeShuriken },
            { "useShurikenCli", UseShuriken },
            { "refuseShurikenCli", RefuseShuriken}
        };

        events = new Dictionary<EventType, Action>()
        {
            { EventType.JoinRoom, OnJoinRoom },
            { EventType.SuggestStartGame, OnSuggestStartGame },
            { EventType.ReadyGame, OnReadyGame },
            { EventType.StartGame, OnStartGame }
        };

        
        foreach (var handler in socketEventHandlers)
        {
            _socket.OnUnityThread(handler.Key, response => EnqueueAction(() => handler.Value(response)));
        }
        
        _socket.Connect();
    }
    
    void EnqueueAction(Action action)
    {
        lock (_actionQueue)
        {
            _actionQueue.Enqueue(action);
        }

        ProcessQueue().Forget();
    }
    
    async UniTask ProcessQueue()
    {
        if (_isProcessing) return; // 이미 처리 중이면 대기
        _isProcessing = true;

        while (_actionQueue.Count > 0)
        {
            Action action;
            lock (_actionQueue)
            {
                action = _actionQueue.Dequeue();
            }
            
            action.Invoke();
            await UniTask.Delay(1); // 잠깐 딜레이를 줘서 자연스럽게 처리
        }

        _isProcessing = false;
    }

    private void JoinRoom(SocketIOResponse response) // 서버에 joingame을 보낼시 response로 joinroomcli가 오면 자동 실행
    {
        events[EventType.JoinRoom]?.Invoke();
    }

    private void SuggestStartGame(SocketIOResponse response)
    {
        
    }

    private void ReadyGame(SocketIOResponse response)
    {
        
    }

    private void StartGame(SocketIOResponse response)
    {
        
    }

    private void RefuseGame(SocketIOResponse response)
    {
        
    }

    private void PlayCard(SocketIOResponse response)
    {
        
    }

    private void SuggestShuriken(SocketIOResponse response)
    {
        
    }

    private void AgreeShuriken(SocketIOResponse response)
    {
        
    }

    private void UseShuriken(SocketIOResponse response)
    {
        
    }

    private void RefuseShuriken(SocketIOResponse response)
    {
        
    }

    public void JoinGame(string playerId, int roomId, int playerNumber)
    {
        _socket.Emit("joinGame", playerId, roomId, playerNumber);
    }
}
