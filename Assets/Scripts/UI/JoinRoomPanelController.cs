using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class JoinRoomPanelController : MonoBehaviour, IGameUI
{
    // text
    [SerializeField] private TMP_InputField _roomNumberInput;
    [SerializeField] private Button _joinRoomButton;
    [SerializeField] private Button _backButton;

    private int _roomNumber;
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
    
    void Start()
    {
        _joinRoomButton.onClick.AddListener(OnClickJoinRoom);
        _backButton.onClick.AddListener(OnClickBack);
    }

    private void OnClickJoinRoom()
    {
        _roomNumber = int.Parse(_roomNumberInput.text);
        JoinRoomData joinRoomData = new JoinRoomData()
        {
            playerId = GameManager.Instance.userInfo.userId,
            nickname = GameManager.Instance.userInfo.nickname,
            roomId = _roomNumber
        };
        StartCoroutine(NetworkManager.Instance.JoinRoom(joinRoomData, OnSuccessJoinRoom));
    }
    
    private void OnClickBack()
    {
        UIManager.Instance.ShowUI<MainMenuPanelController>(UI_TYPE.MainMenu, () => Hide()).Forget();
    }

    private async void OnSuccessJoinRoom(Room roomData)
    {
        WaitingRoomPanelController waitingRoomPanelController = await UIManager.Instance.ShowUI<WaitingRoomPanelController>(UI_TYPE.WaitingRoom, () => Hide());
        GameManager.Instance.multiplayController.EventJoinRoom += waitingRoomPanelController.AddNewPlayer;
        waitingRoomPanelController.GetIntoRoom(roomData.players, roomData.nicknames);
        GameManager.Instance.multiplayController.SendJoinGame(
            GameManager.Instance.userInfo.userId, GameManager.Instance.userInfo.nickname, _roomNumber, roomData.roomSize);
    }
}
