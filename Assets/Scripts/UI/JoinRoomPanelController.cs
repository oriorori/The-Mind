using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class JoinRoomPanelController : MonoBehaviour, IGameUI
{
    // text
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private TMP_InputField _roomNumberInput;
    [SerializeField] private Button _joinRoomButton;
    [SerializeField] private Button _backButton;
    
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
    
    
    void Start()
    {
        _joinRoomButton.onClick.AddListener(OnClickJoinRoom);
        _backButton.onClick.AddListener(OnClickBack);
    }

    private void OnClickJoinRoom()
    {
        JoinRoomData joinRoomData = new JoinRoomData()
        {
            playerId = _playerNameInput.text,
            roomId = Int32.Parse(_roomNumberInput.text)
        };
        StartCoroutine(NetworkManager.Instance.JoinRoom(joinRoomData, OnSuccessJoinRoom));
    }
    
    private void OnClickBack()
    {
        UIManager.Instance.ShowUI<MainMenuPanelController>(UI_TYPE.MainMenu, () => Hide());
    }

    private async void OnSuccessJoinRoom(Room roomData)
    {
        WaitingRoomPanelController waitingRoomPanelController = await UIManager.Instance.ShowUI<WaitingRoomPanelController>(UI_TYPE.WaitingRoom, () => Hide());
        waitingRoomPanelController.Initialize();
        GameManager.Instance.SubscribeEvent(EventType.JoinRoom, waitingRoomPanelController.AddNewPlayer);
        GameManager.Instance.multiplayController.JoinGame(_playerNameInput.text, Int32.Parse(_roomNumberInput.text), roomData.maxPlayerNumber);
    }
}
