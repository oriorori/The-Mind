using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InGameController
{
    private GamePanelController _gamePanelController;
    
    public void InitGame(GameInfo gameInfo)
    {
        GameManager.Instance.multiplayController.EventWrongCardPlay += OnWrongCardPlayed;
        GameManager.Instance.multiplayController.EventRightCardPlay += OnRightCardPlayed;
        GameManager.Instance.multiplayController.EventCardReceive += OnCardReceived;
        GameManager.Instance.multiplayController.EventStageClear += OnStageCleared;
        GameManager.Instance.multiplayController.EventGameOver += OnGameOver;
        _gamePanelController = UIManager.Instance.GetUI<GamePanelController>(UI_TYPE.Game);
        _gamePanelController.InitializeGame(gameInfo);
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
                _gamePanelController.ThrowAwayCards(playerId, cardPlayInfo.lowerNumbers[playerId]);
            }
        }
        
        _gamePanelController.UpdateGameInfo(remainingLife: cardPlayInfo.ramainingLife);
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

    public void OnGameOver()
    {
        _gamePanelController.GameOver();
    }
}
