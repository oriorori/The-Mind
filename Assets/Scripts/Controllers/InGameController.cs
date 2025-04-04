using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InGameController
{
    private GamePanelController _gamePanelController;

    Dictionary<string, List<int>> playerCards;

    public int playerCount;
    public int remainingLife;
    public int remainingShuriken;
    public int lastStage; // 마지막 stage
    public int currentStage;
    
    public InGameController(GamePanelController gamePanelController)
    {
        _gamePanelController = gamePanelController;

        InitGame();
        
        _gamePanelController.InitializeGame(playerCount);
    }

    public void InitGame()
    {
        playerCount = GameManager.Instance.currentPlayingRoom.playerCount;
        remainingLife = GameBaseData.startingLife[playerCount];
        remainingShuriken = GameBaseData.startingShuriken;
        lastStage = GameBaseData.lastStage[playerCount];
        currentStage = 0;
        
        playerCards = new Dictionary<string, List<int>>();
        foreach (string playerName in GameManager.Instance.currentPlayingRoom.players)
        {
            playerCards[playerName] = new List<int>();
        }
    }

    public void StartStage()
    {
        int cardPerPlayer = StageData.stages[currentStage].cardPerPlayer;
        
        List<int> randomNumbers = Enumerable.Range(1, 101)  // 0~99까지 숫자
            .OrderBy(_ => Guid.NewGuid())              // 무작위로 섞기
            .Take(cardPerPlayer * playerCount)                                    // 앞에서 4개 가져오기
            .ToList();

        int idx = 0;
        foreach (string playerName in playerCards.Keys)
        {
            playerCards[playerName] = randomNumbers.GetRange(idx * cardPerPlayer, cardPerPlayer);
            idx++;
        }
    }

    public void ClearStage()
    {
        // 전체 게임 클리어
        if (currentStage == lastStage)
        {
            ClearGame();
            return;
        }

        // 남은 생명, 수리검 업데이트
        if (StageData.stages[currentStage].getLife) remainingLife++;
        if (StageData.stages[currentStage].getShuriken) remainingShuriken++;
        
        currentStage++;
        StartStage();
    }

    public void ClearGame()
    {
        
    }

    public void GameOver()
    {
        
    }
}
