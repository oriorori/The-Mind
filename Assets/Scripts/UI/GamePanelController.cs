using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
    [SerializeField] private TextMeshProUGUI _stageAlarmTMP;
    
    [Header("Card")]
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private RectTransform _centerRect;

    [Header("Shuriken")] 
    [SerializeField] private Button _suggestShurikenButton;

    [SerializeField] private Image _loseHPEffect;
    
    private Dictionary<string, PlayerPlayArea> _playerPlayAreas = new Dictionary<string, PlayerPlayArea>();
    
    private float _curveHeight = 80f; // 포물선 높이
    private float _curvature = 1.2f;
    private float _spacing = 50f;            // 카드 간 간격
    private float _curveAngle = 5f;    // 카드가 펼쳐지는 각도

    void Awake()
    {
        _suggestShurikenButton.onClick.AddListener(OnClickSuggestShurikenButton);
        GameManager.Instance.multiplayController.EventSuggestShurikenUse += OnSuggestShuriken;
        GameManager.Instance.multiplayController.EventUseShuriken += OnUseShuriken;
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
        _stageAlarmTMP.text = $"스테이지 {gameInfo.currentStage}";
        _remainingLifeTMP.text = gameInfo.remainingLife.ToString();
        _remainingShurikensTMP.text = gameInfo.remainingShurikens.ToString();

        GameManager.Instance.inGameController.StartStage();
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
                ArrangeCard(playerId);
            }
        }
        
        Sequence seq = DOTween.Sequence();
        seq.Append(_stageAlarmTMP.DOFade(1f, 1f));  // Fade In
        seq.Append(_stageAlarmTMP.DOFade(0f, 1f));  // Fade Out
        seq.OnComplete(() => { _bottomPlayerPlayArea.remainingCards[0].ActivateDrag(); });
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
        if (currentStage != -1)
        {
            _stageTMP.text = currentStage.ToString();
            _stageAlarmTMP.text = $"스테이지 {currentStage}";
        }
        if(remainingLife != -1)
            _remainingLifeTMP.text = remainingLife.ToString();
        if(remainingShurikens != -1)
           _remainingShurikensTMP.text = remainingShurikens.ToString();
    }

    public void LoseHPEffect()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_loseHPEffect.DOFade(0.5f, 0.1f));
        sequence.Append(_loseHPEffect.DOFade(0f, 0.1f));
    }

    public void DiscardCards(string playerId, int[] cardNums)
    {
        if (GameManager.Instance.userInfo.userId == playerId) // local 클라이언트는 정확히 숫자에 맞는 카드 제거하기
        {
            foreach (int _ in cardNums)
            {
                Card card = PopLowestCard(_bottomPlayerPlayArea.remainingCards);
                card.DeactivateDrag();
                card.transform.rotation = Quaternion.identity;
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
                card.transform.rotation = Quaternion.identity;
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

    private void OnClickSuggestShurikenButton()
    {
        GameManager.Instance.multiplayController.SendSuggestShuriken(GameManager.Instance.userInfo.userId);
    }

    private void OnSuggestShuriken(string firstSuggetId)
    {
        ShurikenVoteUIController shurikenVoteUIController = UIManager.Instance.GetUI<ShurikenVoteUIController>(UI_TYPE.ShurikenVoteUI);
        shurikenVoteUIController.Show();
        shurikenVoteUIController.Initialize(firstSuggetId);
    }

    private void OnUseShuriken(ShurikenUseInfo shurikenUseInfo)
    {
        _remainingShurikensTMP.text = shurikenUseInfo.remainingShurikens.ToString();

        foreach (string playerId in shurikenUseInfo.lowestNumbers.Keys)
        {
            if (shurikenUseInfo.lowestNumbers[playerId] < 1) return;
            
            int[] discardedNums = new int[1]{shurikenUseInfo.lowestNumbers[playerId]};
            DiscardCards(playerId, discardedNums);
        }
    }

    private void ArrangeCard(string playerId)
    {
        // 실제 카드를 들 때 처럼 포물선 형태로 카드를 배열
        int count = _playerPlayAreas[playerId].remainingCards.Count;
        float centerIndex = (count - 1) / 2f;

        switch (_playerPlayAreas[playerId].direction)
        {
            case 12:
                for (int i = 0; i < count; i++)
                {
                    RectTransform card = _playerPlayAreas[playerId].remainingCards[i].GetComponent<RectTransform>();

                    // 포물선 형태로 카드 배열
                    float x = (i - centerIndex) * _spacing;
                    float y = Mathf.Pow(i - centerIndex, 2) * _curvature - _curveHeight;
                    card.anchoredPosition = new Vector2(x, y);

                    // 카드에 회전 더하기
                    float angle = (i - centerIndex) * _curveAngle;
                    card.localRotation = Quaternion.Euler(0, 0, angle);
                }
                break;
            case 9:
                for (int i = 0; i < count; i++)
                {
                    RectTransform card = _playerPlayAreas[playerId].remainingCards[i].GetComponent<RectTransform>();
    
                    float y = (i - centerIndex) * _spacing;
                    float x = Mathf.Pow(i - centerIndex, 2) * _curvature + _curveHeight;
                    card.anchoredPosition = new Vector2(x, y);
    
                    float angle = (i - centerIndex) * _curveAngle - 90f;
                    card.transform.localRotation = Quaternion.Euler(0, 0, angle);
                }
                break;
            case 3:
                for (int i = 0; i < count; i++)
                {
                    RectTransform card = _playerPlayAreas[playerId].remainingCards[i].GetComponent<RectTransform>();
    
                    float y = (i - centerIndex) * _spacing;
                    float x = Mathf.Pow(i - centerIndex, 2) * _curvature - _curveHeight;
                    card.anchoredPosition = new Vector2(x, y);
    
                    float angle = (i - centerIndex) * -_curveAngle + 90f;
                    card.transform.localRotation = Quaternion.Euler(0, 0, angle);
                }
                break;
        }
    }

    private void OnDisable()
    {
        // List 및 dictionary 초기화
        foreach (Transform child in _centerRect)
            Destroy(child.gameObject);
        
        foreach (PlayerPlayArea playerPlayArea in _playerPlayAreas.Values)
        {
            foreach(Transform child in playerPlayArea.disposedCardContainer)
                Destroy(child.gameObject);
            foreach (Card card in playerPlayArea.remainingCards)
                Destroy(card.gameObject);
            playerPlayArea.remainingCards.Clear();
        }

        _playerPlayAreas.Clear();
    }
}
