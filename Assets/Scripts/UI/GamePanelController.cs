using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GamePanelController : MonoBehaviour, IGameUI
{
    [SerializeField] private GameObject _bottomPlayer; // 무조건 자신
    [SerializeField] private GameObject _topPlayer;
    [SerializeField] private GameObject _leftPlayer;
    [SerializeField] private GameObject _rightPlayer;
    
    [Header("Game Info")]
    [SerializeField] private TextMeshProUGUI _stageTMP;
    [SerializeField] private TextMeshProUGUI _remainingLifeTMP;
    [SerializeField] private TextMeshProUGUI _remainingShurikensTMP;
    
    [Header("Stage Alarm")]
    [SerializeField] private RectTransform _stageAlarmRect;
    [SerializeField] private TextMeshProUGUI _stageAlarmTMP;
    
    [Header("Card")]
    [SerializeField] private GameObject _cardPrefab;
    
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
        switch (gameInfo.roomSize)
        {
            case 4:
                _bottomPlayer.SetActive(true);
                _topPlayer.SetActive(true);
                _leftPlayer.SetActive(true);
                _rightPlayer.SetActive(true);
                break;
            case 3:
                _bottomPlayer.SetActive(true);
                _topPlayer.SetActive(true);
                _leftPlayer.SetActive(true);
                break;
            case 2:
                _bottomPlayer.SetActive(true);
                _topPlayer.SetActive(true);
                break;
        }

        _stageTMP.text = gameInfo.currentStage.ToString();
        _remainingLifeTMP.text = gameInfo.remainingLife.ToString();
        _remainingShurikensTMP.text = gameInfo.remainingShurikens.ToString();

        // Dotween으로 stage 1 띄우기
        _stageAlarmRect.DOAnchorPos(new Vector2(0, 100), 0.5f).OnComplete(() =>
        {
            GameManager.Instance.inGameController.StartStage();
        });
    }

    public void UpdateCardUI(int[] cardsNum)
    {
        
    }
}
