using System;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPanelController : MonoBehaviour, IGameUI
{
    // text
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private TMP_InputField _roomNumberInput;
    [SerializeField] private TextMeshProUGUI _playerNumberTMP;
    
    // button
    [SerializeField] private Button _playerNumberLeftButton;
    [SerializeField] private Button _playerNumberRightButton;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _backButton;

    // playerNum
    private int playerNum;
    
    void Start()
    {
        _playerNumberLeftButton.onClick.AddListener(OnClickNumberLeft);
        _playerNumberRightButton.onClick.AddListener(OnClickNumberRight);
        _createRoomButton.onClick.AddListener(OnClickCreateRoom);
        _backButton.onClick.AddListener(OnClickBack);

        playerNum = Int32.Parse(_playerNumberTMP.text);
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

    private void OnClickCreateRoom()
    {
        CreateRoomData roomData = new CreateRoomData()
        {
            playerId = _playerNameInput.text,
            roomId = Int32.Parse(_roomNumberInput.text),
            maxPlayerNumber = playerNum
        };

        StartCoroutine(NetworkManager.Instance.CreateRoom(roomData, OnSuccessCreateRoom));
    }

    private void OnClickNumberLeft()
    {
        playerNum = Mathf.Max(2, playerNum-1);
        _playerNumberTMP.text = playerNum.ToString();
    }

    private void OnClickNumberRight()
    {
        playerNum = Mathf.Min(4, playerNum + 1);
        _playerNumberTMP.text = playerNum.ToString();
    }

    private void OnClickBack()
    {
        UIManager.Instance.ShowUI<MainMenuPanelController>(UI_TYPE.MainMenu, () => Hide());
    }

    private async void OnSuccessCreateRoom()
    {
        WaitingRoomPanelController waitingRoomPanelController = await UIManager.Instance.ShowUI<WaitingRoomPanelController>(UI_TYPE.WaitingRoom, () => Hide());
        GameManager.Instance.SubscribeEvent(EventType.JoinRoom, waitingRoomPanelController.AddNewPlayer);
        GameManager.Instance.multiplayController.JoinGame(_playerNameInput.text, Int32.Parse(_roomNumberInput.text), playerNum);
    }
}
