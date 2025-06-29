using System;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPanelController : MonoBehaviour, IGameUI
{
    // text
    // [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private TMP_InputField _roomNumberInput;
    [SerializeField] private TextMeshProUGUI _playerNumberTMP;
    
    // button
    [SerializeField] private Button _playerNumberLeftButton;
    [SerializeField] private Button _playerNumberRightButton;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _backButton;

    // playerNum
    private int _playerNum;
    private int _roomNumber;
    
    void Start()
    {
        _playerNumberLeftButton.onClick.AddListener(OnClickNumberLeft);
        _playerNumberRightButton.onClick.AddListener(OnClickNumberRight);
        _createRoomButton.onClick.AddListener(OnClickCreateRoom);
        _backButton.onClick.AddListener(OnClickBack);

        _playerNum = Int32.Parse(_playerNumberTMP.text);
    }
    
    public UniTask Show()
    {
        gameObject.SetActive(true);
        return UniTask.CompletedTask;
    }

    public UniTask Hide()
    {
        _roomNumberInput.text = "";
        gameObject.SetActive(false);
        return UniTask.CompletedTask;
    }

    private void OnClickCreateRoom()
    {
        _roomNumber = Int32.Parse(_roomNumberInput.text);
        CreateRoomData roomData = new CreateRoomData()
        {
            playerId = GameManager.Instance.userInfo.userId,
            nickname = GameManager.Instance.userInfo.nickname,
            roomId = _roomNumber,
            roomSize = _playerNum
        };

        StartCoroutine(NetworkManager.Instance.CreateRoom(roomData, OnSuccessCreateRoom));
    }

    private void OnClickNumberLeft()
    {
        _playerNum = Mathf.Max(2, _playerNum-1);
        _playerNumberTMP.text = _playerNum.ToString();
    }

    private void OnClickNumberRight()
    {
        _playerNum = Mathf.Min(4, _playerNum + 1);
        _playerNumberTMP.text = _playerNum.ToString();
    }

    private void OnClickBack()
    {
        UIManager.Instance.ShowUI<MainMenuPanelController>(UI_TYPE.MainMenu, () => Hide()).Forget();
    }

    private async void OnSuccessCreateRoom(Room roomData)
    {
        WaitingRoomPanelController waitingRoomPanelController = await UIManager.Instance.ShowUI<WaitingRoomPanelController>(UI_TYPE.WaitingRoom);
        GameManager.Instance.multiplayController.EventJoinRoom += waitingRoomPanelController.AddNewPlayer;
        
        waitingRoomPanelController.GetIntoRoom(roomData.players, roomData.nicknames);
        GameManager.Instance.multiplayController.SendJoinGame(
            GameManager.Instance.userInfo.userId, GameManager.Instance.userInfo.nickname,_roomNumber, _playerNum);
        Hide().Forget();
    }
}
