using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SocketIOClient;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomPanelController : MonoBehaviour, IGameUI
{
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _leaveRoomButton;
    [SerializeField] private RectTransform _playerListRect;

    [SerializeField] private GameObject _playerObject;

    private Dictionary<string, PlayerWaitingStatusController> _playerStatusDict;

    void Awake()
    {
        _startGameButton.onClick.AddListener(OnClickStartGameButton);
        _leaveRoomButton.onClick.AddListener(OnClickLeaveRoomButton);
        _playerStatusDict = new Dictionary<string, PlayerWaitingStatusController>();
        GameManager.Instance.multiplayController.EventStartGame += (_) => { OnGameStarted(); };
        GameManager.Instance.multiplayController.EventLeaveRoom += OnRoomLeft;
        GameManager.Instance.multiplayController.EventBackToRoom += BackToRoom;
    }
    
    public void GetIntoRoom(List<string> players)
    {
        // 처음 방에 들어갈 때 실행
        // http 통신에서 받은 playerList를 이용해 init
        foreach (string playerName in players)
        {
            AddWaitingPlayerStatus(playerName);
        }
    }

    public void BackToRoom(string playerName)
    {
        _playerStatusDict[playerName].ChangeStatus(WaitingStatus.waiting);
    }

    public void AddNewPlayer(string playerName)
    {
        // 나는 참가해있고 다른 누군가가 추가로 참가할 때 호출
        // multiplaycontroller의 joinroom에 구독해놓고 socket통신에서 joinRoomCli response가 오면 실행된다
        AddWaitingPlayerStatus(playerName);
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

    private void OnClickStartGameButton()
    {
        Room currentRoom = GameManager.Instance.currentPlayingRoom;
        if (currentRoom.players.Count < currentRoom.roomSize)
        {
            PopupUIController popupUI = UIManager.Instance.GetUI<PopupUIController>(UI_TYPE.Popup);
            popupUI.SetText("인원이 다 차지 않았습니다.");
        }
        else
        {
            GameManager.Instance.multiplayController.SendSuggestStartGame();
        }
    }

    private void OnClickLeaveRoomButton()
    {
        
    }

    private void OnGameStarted()
    {
        foreach (PlayerWaitingStatusController playerStatus in _playerStatusDict.Values)
        {
            playerStatus.ChangeStatus(WaitingStatus.playing);
        }
        Hide();
    }

    private void OnRoomLeft(string playerId)
    {
        if (playerId == GameManager.Instance.userInfo.userId) // 내가 나간 경우
        {
            UIManager.Instance.GetUI<MainMenuPanelController>(UI_TYPE.MainMenu).Show();
            GameManager.Instance.InitCurrentPlayingRoom(null);
            gameObject.SetActive(false);
        }
        else // 방의 다른 사람이 나간 경우
        {
            GameManager.Instance.currentPlayingRoom.players.Remove(playerId);
            Destroy(_playerStatusDict[playerId].gameObject);
            _playerStatusDict.Remove(playerId);
        }
    }

    private void AddWaitingPlayerStatus(string playerName)
    {
        GameObject playerObject = Instantiate(_playerObject, _playerListRect);
        PlayerWaitingStatusController playerStatusController = playerObject.GetComponent<PlayerWaitingStatusController>();
        playerStatusController.ChangeStatus(WaitingStatus.waiting);
        _playerStatusDict[playerName] = playerStatusController;
        TextMeshProUGUI playerNameText = playerObject.GetComponentInChildren<TextMeshProUGUI>();
        playerNameText.text = playerName;
    }
    
}
