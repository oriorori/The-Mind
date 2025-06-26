using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SocketIOClient;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class WaitingReadyUIController : MonoBehaviour, IGameUI
{
    [SerializeField] private Button _applyButton;
    [FormerlySerializedAs("_rejectButton")] [SerializeField] private Button _refuseButton;
    
    
    [SerializeField] private GameObject _playerIcon;
    [SerializeField] private GameObject _playerIconContainer;
    
    private List<GameObject> playerIconList;
    
    private int currentPlayerIndex;

    void Awake()
    {
        playerIconList = new List<GameObject>();
        
        _applyButton.onClick.AddListener(OnClickApplyButton);
        _refuseButton.onClick.AddListener(OnClickRefuseButton);

        GameManager.Instance.multiplayController.EventStartGame += ( (_) => { gameObject.SetActive(false); } );
        GameManager.Instance.multiplayController.EventReadyGame += OnReadiedGame;
        GameManager.Instance.multiplayController.EventRefuseGame += OnRefusePlayingGame;
    }
    
    public void Initialize(bool pressedStartButton)
    {
        int playerNum = GameManager.Instance.currentPlayingRoom.roomSize;
        for (int i = 0; i < playerNum; i++)
        {
            playerIconList.Add(Instantiate(_playerIcon, _playerIconContainer.transform));
        }

        playerIconList[currentPlayerIndex++].GetComponent<Image>().color = Color.green;

        if (pressedStartButton)
        {
            _applyButton.gameObject.SetActive(false);
            _refuseButton.gameObject.SetActive(false);
        }
        else
        {
            _applyButton.gameObject.SetActive(true);
            _refuseButton.gameObject.SetActive(true);
        }
    }

    private void OnClickApplyButton()
    {
        GameManager.Instance.multiplayController.SendReadyGame();
        _applyButton.gameObject.SetActive(false);
        _refuseButton.gameObject.SetActive(false);
    }

    private void OnClickRefuseButton()
    {
        GameManager.Instance.multiplayController.SendRefuseGame();
    }

    private void ChangePlayerIconColorGreen()
    {
        playerIconList[currentPlayerIndex++].GetComponent<Image>().color = Color.green;
    }

    private void OnReadiedGame()
    {
        ChangePlayerIconColorGreen();
    }

    private void OnRefusePlayingGame()
    {
        foreach (var icon in playerIconList)
        {
            icon.GetComponent<Image>().color = Color.white;
        }
        gameObject.SetActive(false);
    }

    public void OnDisable()
    {
        for (int i = playerIconList.Count - 1; i >= 0; i--)
        {
            Destroy(playerIconList[i]);
        }
        playerIconList.Clear();
        currentPlayerIndex = 0;
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
}
