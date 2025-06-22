using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerPlayArea
{
    public int direction;
    public RectTransform rectTransform;
    public RectTransform cardContainer;
    public RectTransform disposedCardContainer;
    [HideInInspector] public List<Card> remainingCards;
}

public class GamePanelController : MonoBehaviour, IGameUI
{
    [SerializeField] private PlayerPlayArea _rightPlayerPlayArea;
    [SerializeField] private PlayerPlayArea _bottomPlayerPlayArea;
    [SerializeField] private PlayerPlayArea _leftPlayerPlayArea;
    [SerializeField] private PlayerPlayArea _topPlayerPlayArea;
    
    [Header("Game Info")]
    [SerializeField] private TextMeshProUGUI _stageTMP;
    [SerializeField] private TextMeshProUGUI _remainingLifeTMP;
    [SerializeField] private TextMeshProUGUI _remainingShurikensTMP;
    
    [Header("Stage Alarm")]
    [SerializeField] private RectTransform _stageAlarmRect;
    [SerializeField] private TextMeshProUGUI _stageAlarmTMP;
    
    [Header("Card")]
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private RectTransform _centerRect;
    
    private Dictionary<string, PlayerPlayArea> _playerPlayAreas = new Dictionary<string, PlayerPlayArea>();
    
    void OnEnable()
    {
    }
    
    public UniTask Show()
    {
        gameObject.SetActive(true);
        return UniTask.CompletedTask;
    }

    public UniTask Hide()
    {
        gameObject.SetActive(false);
        return UniTask.CompletedTask;
    }

    public void InitializeGame(GameInfo gameInfo)
    {
        GameManager.Instance.multiplayController.EventCardMove += UpdateOtherPlayerCardPositions;
        
        SetPlayerRects();

        _stageTMP.text = gameInfo.currentStage.ToString();
        _remainingLifeTMP.text = gameInfo.remainingLife.ToString();
        _remainingShurikensTMP.text = gameInfo.remainingShurikens.ToString();

        // Dotween으로 stage 1 띄우기
        _stageAlarmRect.DOAnchorPos(new Vector2(0, 100), 0.5f).OnComplete(() =>
        {
            GameManager.Instance.inGameController.StartStage();
        });
    }

    public void StartCardUI(int[] cardsNum) // UI상으로 카드 나눠주기
    {
        foreach (var playerId in _playerPlayAreas.Keys)
        {
            if (playerId == GameManager.Instance.userInfo.userId) // 유저일때 -> 모든 숫자 정보 공개
            {
                foreach (int cardNum in cardsNum)
                {
                    Card card = Instantiate(_cardPrefab, _bottomPlayerPlayArea.cardContainer).GetComponent<Card>();
                    card.SetCard(cardNum, _centerRect);
                    _bottomPlayerPlayArea.remainingCards.Add(card);
                }
            }
            else
            {
                for (int i = 0; i < cardsNum.Length; i++) // 내가 아닐 때 -> 상호작용x, 숫자정보x
                {
                    Card card = Instantiate(_cardPrefab, _playerPlayAreas[playerId].cardContainer).GetComponent<Card>();
                    _playerPlayAreas[playerId].remainingCards.Add(card);
                }
            }
        }
        _bottomPlayerPlayArea.remainingCards[0].ActivateDrag();
    }

    private void SetPlayerRects()
    {
        // 나 자신은 제외
        List<string> playerIdsExceptUser = GameManager.Instance.currentPlayingRoom.players
            .Where(id => id != GameManager.Instance.userInfo.userId).ToList();
        _bottomPlayerPlayArea.rectTransform.gameObject.SetActive(true);
        _bottomPlayerPlayArea.remainingCards = new List<Card>();
        _playerPlayAreas[GameManager.Instance.userInfo.userId] = _bottomPlayerPlayArea;
        
        switch (playerIdsExceptUser.Count)
        {
            case 1:
                _playerPlayAreas[playerIdsExceptUser[0]] = _topPlayerPlayArea;
                _topPlayerPlayArea.remainingCards = new List<Card>();
                _topPlayerPlayArea.rectTransform.gameObject.SetActive(true);
                break;
            case 2:
                _playerPlayAreas[playerIdsExceptUser[0]] = _leftPlayerPlayArea;
                _leftPlayerPlayArea.remainingCards = new List<Card>();
                _leftPlayerPlayArea.rectTransform.gameObject.SetActive(true);
                _playerPlayAreas[playerIdsExceptUser[1]] = _topPlayerPlayArea;
                _topPlayerPlayArea.remainingCards = new List<Card>();
                _topPlayerPlayArea.rectTransform.gameObject.SetActive(true);
                break;
            case 3:
                _playerPlayAreas[playerIdsExceptUser[0]] = _leftPlayerPlayArea;
                _leftPlayerPlayArea.remainingCards = new List<Card>();
                _leftPlayerPlayArea.rectTransform.gameObject.SetActive(true);
                _playerPlayAreas[playerIdsExceptUser[1]] = _topPlayerPlayArea;
                _topPlayerPlayArea.remainingCards = new List<Card>();
                _topPlayerPlayArea.rectTransform.gameObject.SetActive(true);
                _playerPlayAreas[playerIdsExceptUser[2]] = _rightPlayerPlayArea;
                _rightPlayerPlayArea.remainingCards = new List<Card>();
                _rightPlayerPlayArea.rectTransform.gameObject.SetActive(true);
                break;
        }
    }

    private void UpdateOtherPlayerCardPositions(CardMoveInfo cardMoveInfo)
    {
        string playerIdMoved = cardMoveInfo.playerId;
        if (!_playerPlayAreas.TryGetValue(playerIdMoved, out var playArea)) return;

        RectTransform cardRectTransform = _playerPlayAreas[playerIdMoved].remainingCards[0].GetComponent<RectTransform>();

        Vector2 newPos = new Vector2();
        switch (playArea.direction)
        {
            case 3:
                newPos = new Vector2(-cardMoveInfo.ratioToCenter * Screen.width + playArea.cardContainer.position.x, 
                    cardMoveInfo.ratioToCenterVertical * Screen.height + playArea.cardContainer.position.y);
                break;
            case 9:
                newPos = new Vector2(cardMoveInfo.ratioToCenter * Screen.width + playArea.cardContainer.position.x,
                    -cardMoveInfo.ratioToCenterVertical * Screen.height + playArea.cardContainer.position.y);
                break;
            case 12:
                newPos = new Vector2(-cardMoveInfo.ratioToCenterVertical * Screen.width + playArea.cardContainer.position.x,
                    -cardMoveInfo.ratioToCenter * Screen.height + playArea.cardContainer.position.y);
                break;
        }

        cardRectTransform.position = newPos;
    }
    
    public void PlayedCard(string playerId, int cardNum)
    {
        // 다음 카드 활성화
        if(GameManager.Instance.userInfo.userId == playerId)
        {
            PopLowestCard(_bottomPlayerPlayArea.remainingCards);
            if(_bottomPlayerPlayArea.remainingCards.Count > 0)
                _bottomPlayerPlayArea.remainingCards[0].ActivateDrag();
        }

        else // 다른 플레이어가 플레이 한 카드 보여주기
        {
            Card card = PopLowestCard(_playerPlayAreas[playerId].remainingCards);
            RectTransform playedCardTransform = card.GetComponent<RectTransform>();
            playedCardTransform.SetParent(_centerRect);
            playedCardTransform.anchorMin = new Vector2(0.5f, 0.5f);
            playedCardTransform.anchorMax = new Vector2(0.5f, 0.5f);
            playedCardTransform.anchoredPosition = Vector2.zero;
            playedCardTransform.GetComponent<Card>().SetNumber(cardNum);
        }
    }

    public void UpdateGameInfo(int currentStage = -1, int remainingLife = -1, int remainingShurikens = -1)
    {
        if(currentStage != -1)
            _stageTMP.text = currentStage.ToString();
        if(remainingLife != -1)
            _remainingLifeTMP.text = remainingLife.ToString();
        if(remainingShurikens != -1)
           _remainingShurikensTMP.text = remainingShurikens.ToString();
    }

    public void ThrowAwayCards(string playerId, int[] cardNums)
    {
        if (GameManager.Instance.userInfo.userId == playerId) // local 클라이언트는 정확히 숫자에 맞는 카드 제거하기
        {
            foreach (int _ in cardNums)
            {
                Card card = PopLowestCard(_bottomPlayerPlayArea.remainingCards);
                card.DeactivateDrag();
                card.transform.SetParent(_bottomPlayerPlayArea.disposedCardContainer);
            }
            if(_bottomPlayerPlayArea.remainingCards.Count > 0) _bottomPlayerPlayArea.remainingCards[0].ActivateDrag();
        }

        else // remote 클라이언트는 어차피 숫자 정보가 없으므로 제거하고 숫자 써주기
        {           
            PlayerPlayArea playerPlayArea = _playerPlayAreas[playerId];

            foreach (int cardNum in cardNums)
            {
                Card card = PopLowestCard(playerPlayArea.remainingCards);
                card.transform.SetParent(playerPlayArea.disposedCardContainer);
                card.SetNumber(cardNum);
            }
        }
    }

    public void ReadyNextStage()
    {
        foreach (PlayerPlayArea playerPlayArea in _playerPlayAreas.Values)
        {
            foreach (Transform child in playerPlayArea.disposedCardContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (Transform child in _centerRect.transform)
        {
            Destroy(child.gameObject);
        }
    }


    private Card PopLowestCard(List<Card> list)
    {
        Card card = list[0];
        list.RemoveAt(0);
        return card;
    }

    public void GameOver()
    {
        
    }
}
