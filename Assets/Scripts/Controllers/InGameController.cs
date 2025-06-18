using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InGameController
{
    private GamePanelController _gamePanelController;
    
    public void InitGame(GameInfo gameInfo)
    {
        GameManager.Instance.multiplayController.EventCardReceive += OnCardReceived;
        _gamePanelController = UIManager.Instance.GetUI<GamePanelController>(UI_TYPE.Game);
        _gamePanelController.InitializeGame(gameInfo);
    }

    public void StartStage()
    {
        // stage 시작 신호
        GameManager.Instance.multiplayController.SendStartStage();
    }

    public void OnCardReceived(int[] cardsNum)
    {
        _gamePanelController.UpdateCardUI(cardsNum);
    }
    
    public void ClearStage()
    {
    }

    public void ClearGame()
    {
        
    }

    public void GameOver()
    {
        
    }
}
