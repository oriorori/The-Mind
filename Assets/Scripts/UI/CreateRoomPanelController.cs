using System;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPanelController : MonoBehaviour, IGameUI
{
    // text
    [SerializeField] private TextMeshProUGUI _playerNameTMP;
    [SerializeField] private TextMeshProUGUI _roomNumberTMP;
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
        UIManager.Instance.ShowUI<WaitingRoomPanelController>(UI_TYPE.WaitingRoom, () => Hide());
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
}
