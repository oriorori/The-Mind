using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class InGameController
{
    private const long MaxReasonableRttMs = 3000;   // 이상치 필터 (원하면 조정)
    private const double RttEmaAlpha = 0.2;         // EMA(지수평균) 반영 비율 (선택)
    private double _rttEma = -1;                    // RTT EMA 저장(선택)
    
    private GamePanelController _gamePanelController;
    private bool _playing;
    private int syncSeq;
    private Dictionary<int, long> clientTimeDict = new Dictionary<int, long>(); 
    
    public InGameController() // 이벤트 구독
    {
        GameManager.Instance.multiplayController.EventWrongCardPlay += OnWrongCardPlayed;
        GameManager.Instance.multiplayController.EventRightCardPlay += OnRightCardPlayed;
        GameManager.Instance.multiplayController.EventCardReceive += OnCardReceived;
        GameManager.Instance.multiplayController.EventStageClear += OnStageCleared;
        GameManager.Instance.multiplayController.EventGameOver += OnGameOver;
        GameManager.Instance.multiplayController.EventPongReceived += OnPongReceived;
    }
    
    public void InitGame(GameInfo gameInfo) // GamePanelController 띄우기
    {
        _gamePanelController = UIManager.Instance.GetUI<GamePanelController>(UI_TYPE.Game);
        _gamePanelController.Show();
        _gamePanelController.InitializeGame(gameInfo);
        _playing = true;
        syncSeq = 0;
        UpdatePing().Forget();
    }

    public void StartStage()
    {
        // stage 시작 신호
        GameManager.Instance.multiplayController.SendStartStage();
    }

    private void OnCardReceived(int[] cardsNum)
    {
        _gamePanelController.StartCardUI(cardsNum);
    }

    private void OnWrongCardPlayed(WrongCardPlayInfo cardPlayInfo)
    {
        _gamePanelController.PlayedCard(cardPlayInfo.playedPlayer, cardPlayInfo.playedCardNumber);
        
        foreach (string playerId in cardPlayInfo.lowerNumbers.Keys)
        {
            if (cardPlayInfo.lowerNumbers[playerId].Length > 0)
            {
                _gamePanelController.DiscardCards(playerId, cardPlayInfo.lowerNumbers[playerId]);
            }
        }
        _gamePanelController.LoseHPEffect();
        _gamePanelController.UpdateGameInfo(remainingLife: cardPlayInfo.remainingLife);
    }

    private void OnRightCardPlayed(RightCardPlayInfo cardPlayInfo)
    {
        _gamePanelController.PlayedCard(cardPlayInfo.playedPlayer, cardPlayInfo.playedCardNumber);
    }
    
    public void OnStageCleared(GameInfo gameInfo)
    {
        // 1스테이지 클리어 효과
        // 수리검, 생명, UI 업데이트
        _gamePanelController.UpdateGameInfo(gameInfo.currentStage, gameInfo.remainingLife, gameInfo.remainingShurikens);
        
        // 다음 스테이지 시작
        _gamePanelController.ReadyNextStage();
        StartStage();
    }

    public void ClearGame()
    {
        
    }
    
    private async UniTask UpdatePing()
    {
        while (_playing)
        {
            long clientSentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            clientTimeDict.Add(syncSeq, clientSentTime);
            GameManager.Instance.multiplayController.SendPingSync(clientSentTime, syncSeq);
            // Debug.Log($"Send {syncSeq}th ping request: {clientSentTime}");
            syncSeq++;

            await UniTask.WaitForSeconds(0.5f);
        }
    }

    private void OnPongReceived(int sequence, long serverTime)
    {
        long estimatedOffset = 0;
        long rtt = 0;
        if (!clientTimeDict.TryGetValue(sequence, out long t0))
        {
            // 매칭되는 ping이 없으면 계산 불가
            Debug.LogWarning($"[Ping] Unknown seq: {sequence}");
            return;
        }
        
        // 현재 클라이언트 시각(t1)
        long t1 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // RTT = (pong 수신 시각) - (ping 전송 시각)
        rtt = t1 - t0;
        if (rtt < 0) rtt = 0; // 시계가 뒤로 갔거나 오류 방지
        if (rtt > MaxReasonableRttMs)
        {
            // 너무 큰 값은 이상치로 보고 클램프/무시 (원하는 정책으로 조정)
            rtt = MaxReasonableRttMs;
        }

        // 편도 지연 ≈ RTT/2
        long oneWay = rtt / 2;

        // offset ≈ 서버시각(서버가 pong 보낸 순간) - (클라가 ping 보낸 시각 + 편도지연)
        // = 서로 같은 "실제 순간"을 각 시계로 본 값의 차이
        estimatedOffset = serverTime - (t0 + oneWay);

        // (선택) RTT EMA 갱신 — 튀는 값 완화
        if (_rttEma < 0) _rttEma = rtt;
        else _rttEma = _rttEma * (1.0 - RttEmaAlpha) + rtt * RttEmaAlpha;

        // 사용 끝난 seq는 정리
        clientTimeDict.Remove(sequence);
        // Debug.Log("Send sync result request");
        GameManager.Instance.multiplayController.SendSyncResult(estimatedOffset, rtt);
    }

    public void OnGameOver()
    {
        UIManager.Instance.GetUI<GameOverPopupUIController>(UI_TYPE.GameOverPopup).Show();
        _gamePanelController.GameOver(); // Ingame 화면 상호작용 안되도록 막기
        _playing = false;
    }
}
