using Cysharp.Threading.Tasks;
using UnityEngine;

public class GamePanelController : MonoBehaviour, IGameUI
{
    private InGameController _inGameController;

    [SerializeField] private GameObject _bottomPlayer; // 무조건 자신
    [SerializeField] private GameObject _topPlayer;
    [SerializeField] private GameObject _leftPlayer;
    [SerializeField] private GameObject _rightPlayer;

    void Awake()
    {
        _inGameController = new InGameController(this);
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

    public void InitializeGame(int playerCount)
    {
        switch (playerCount)
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
    }
}
