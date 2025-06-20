using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using SocketIOClient;

public class MultiplayController
{
    // socket 통신을 담당한다
    // 방에 참가한 시점 이후부터는 통신은 multiplayController가 담당한다.
    private SocketIOUnity _socket;

    private Dictionary<string, Action<SocketIOResponse>> socketEventHandlers;

    public Dictionary<EventType, Action<SocketIOResponse>> events;
    public event Action<string> EventJoinRoom;
    public event Action<GameInfo> EventStartGame;
    public event Action<SocketIOResponse> EventStartStage;
    public event Action<int[]> EventCardReceive;
    public event Action<CardMoveInfo> EventCardMove;
    public event Action<WrongCardPlayInfo> EventWrongCardPlay;
    public event Action<RightCardPlayInfo> EventRightCardPlay;
    public event Action<GameInfo> EventStageClear;
    public event Action EventGameOver;
    
    Queue<Action> _actionQueue = new Queue<Action>();
    bool _isProcessing = false;
    
    public void Initialize()
    {
        if (_socket != null && _socket.Connected) return;
        
        var uri = new Uri(Constants.GameServerURL);
        _socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
        });

        socketEventHandlers = new Dictionary<string, Action<SocketIOResponse>>() 
        {
            // socket에서 받은 response message와 클라이언트에서 실행할 event를 연결해주는 dictionary
            // 소켓통신에서 서버는 이벤트이름과 함께 데이터를 보낸다.
            // 이 dictionary의 key가 곧 이벤트 이름과 동일해야하며, value는 해당 이벤트를 받았을 때 실행할 Action인 것
            { "joinRoomCli", OnRoomJoined }, 
            { "suggestStartGameCli", OnStartGameSuggested },
            { "readyGameCli", OnGameReadied },
            { "startGameCli", OnGameStarted },
            { "refuseGameCli", OnGameRefused },
            { "cardMoveCli", OnCardMoved },
            { "playWrongCardCli", OnWrongCardPlayed },
            { "playRightCardCli", OnRightCardPlayed },
            { "suggestShurikenCli", OnShurikenSuggested },
            { "agreeShurikenCli", OnShurikenAgreed },
            { "useShurikenCli", OnShurikenUsed },
            { "refuseShurikenCli", OnShurikenRefused},
            { "startStageCli", OnStageStarted},
            { "receiveCardsCli", OnCardsReceived},
            { "gameOverCli", OnGameOvered},
            { "stageClearCli", OnStageCleared},
            { "gameClearCli", OnGameCleared},
        };

        // 소켓(서버)에서 메시지를 보내면 그에 맞는 Action을 실행하도록 이벤트를 연결
        foreach (var handler in socketEventHandlers)
        {
            /*
             아래 코드를 풀어서 쓰면
             void SomeHandler(SocketIOResponse response){
                EnqueueAction(()=>handler.Value(response)}
                
            _socket.OnUnityThread(handler.Key, SomeHandler);
             라고 볼 수 있음
             근데 이걸 줄여서 아래와 같이 쓴것임
             */
            _socket.OnUnityThread(handler.Key, (response) => EnqueueAction(() => handler.Value(response)));
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
    
    #region response처리

    private void OnRoomJoined(SocketIOResponse response) // 서버에 joingame을 보낼시 response로 joinroomcli가 오면 자동 실행
    {
        string playerName = response.GetValue<string>();
        GameManager.Instance.AddNewPlayer(playerName);
        EventJoinRoom?.Invoke(playerName);
    }

    private void OnGameStarted(SocketIOResponse response)
    {
        string gameInfoString = response.ToString();
        /*
         *  io.to(roomId).emit('startGameCli', {
                currentStage: roomInfos[roomId].currentStage,
                remainingLife: roomInfos[roomId].remainingLife,
                remainingShurikens: roomInfos[roomId].remainingShurikens
            });
            서버에서 위와 같이 보내면 json이 array형태로 오게 됨 이유는 모름...
            그래서 아래와 같이 처리
         */
        GameInfo[] gameInfos = JsonConvert.DeserializeObject<GameInfo[]>(gameInfoString);
        GameInfo gameInfo = gameInfos[0];
        
        // ready popup 비활성화
        // waitingroompanel 비활성화
        EventStartGame?.Invoke(gameInfo);
        GameManager.Instance.StartGame(gameInfo);
    }

    private void OnStartGameSuggested(SocketIOResponse response)
    {
        string firstSuggestId = response.GetValue<string>();
        WaitingReadyUIController waitingReadyUI = UIManager.Instance.GetUI<WaitingReadyUIController>(UI_TYPE.WaitingReady);
        
        waitingReadyUI.Initialize(GameManager.Instance.userInfo.userId == firstSuggestId);

    }

    private void OnGameReadied(SocketIOResponse response)
    {  
        // WaitingReadyUI에서 ready, refuse 제거
    }

    private void OnGameRefused(SocketIOResponse response)
    {
        // waitingReadyUIController 비활성화
    }

    private void OnStageStarted(SocketIOResponse response)
    {
        
    }

    private void OnCardMoved(SocketIOResponse response)
    {
        string cardMoveInfoString = response.ToString();
        CardMoveInfo[] cardMoveInfos = JsonConvert.DeserializeObject<CardMoveInfo[]>(cardMoveInfoString);
        CardMoveInfo cardMoveInfo = cardMoveInfos[0];
        EventCardMove?.Invoke(cardMoveInfo);
    }

    private void OnWrongCardPlayed(SocketIOResponse response)
    {
        string cardPlayInfoString = response.ToString();
        WrongCardPlayInfo[] cardPlayInfos = JsonConvert.DeserializeObject<WrongCardPlayInfo[]>(cardPlayInfoString);
        WrongCardPlayInfo cardPlayInfo = cardPlayInfos[0];
        EventWrongCardPlay?.Invoke(cardPlayInfo);
    }

    private void OnRightCardPlayed(SocketIOResponse response)
    {
        string cardPlayInfoString = response.ToString();
        RightCardPlayInfo[] cardPlayInfos = JsonConvert.DeserializeObject<RightCardPlayInfo[]>(cardPlayInfoString);
        RightCardPlayInfo cardPlayInfo = cardPlayInfos[0];
        EventRightCardPlay?.Invoke(cardPlayInfo);
    }

    private void OnShurikenSuggested(SocketIOResponse response)
    {
        
    }

    private void OnShurikenAgreed(SocketIOResponse response)
    {
        
    }

    private void OnShurikenUsed(SocketIOResponse response)
    {
        
    }

    private void OnShurikenRefused(SocketIOResponse response)
    {
        
    }

    private void OnCardsReceived(SocketIOResponse response)
    {
        int[] cards = response.GetValue<int[]>();
        EventCardReceive?.Invoke(cards);
    }
    
    private void OnGameCleared(SocketIOResponse response)
    {
        
    }

    private void OnStageCleared(SocketIOResponse response)
    {
        string gameInfoString = response.ToString();
        GameInfo[] gameInfos = JsonConvert.DeserializeObject<GameInfo[]>(gameInfoString);
        GameInfo gameInfo = gameInfos[0];
        
        Debug.Log($"{gameInfo.currentStage - 1} 스테이지 클리어!");
        EventStageClear?.Invoke(gameInfo);
    }

    private void OnGameOvered(SocketIOResponse response)
    {
        EventGameOver?.Invoke();
    }

    #endregion
    
    #region 소켓으로 송신
    public void SendJoinGame(string playerId, int roomId, int roomSize)
    {
        var data = new {
            playerId = playerId,
            roomId = roomId,
            roomSize = roomSize
        };
        _socket.Emit("joinGame", data);
    }
    
    public void SendSuggestStartGame(string playerId)
    {
        _socket.Emit("suggestStartGame");
    }

    public void SendReadyGame()
    {
        _socket.Emit("readyGame");
    }

    public void SendStartStage()
    {
        _socket.Emit("startStage");
    }

    public void SendRefuseGame()
    {
        _socket.Emit("refuseGame");
    }

    public void SendPlayCard(int cardNumber)
    {
        _socket.Emit("playCard", cardNumber);
    }

    public void SendCardMove(float ratioToCenter, float ratioToCenterVertical)
    {
        _socket.Emit("cardMove", ratioToCenter, ratioToCenterVertical);
    }

    public void SendRestartGame()
    {
        _socket.Emit("restartGame");
    }

    public void SendDestroyRoom()
    {
        _socket.Emit("destroyRoom");
    }
    #endregion
}
