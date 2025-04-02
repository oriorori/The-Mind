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
        _joinRoomButton.onClick.AddListener(OnClickCreateRoom);
        _backButton.onClick.AddListener(OnClickBack);
    }

    private void OnClickCreateRoom()
    {
        UIManager.Instance.ShowUI<WaitingRoomPanelController>(UI_TYPE.WaitingRoom, () => Hide());
    }
    
    private void OnClickBack()
    {
        UIManager.Instance.ShowUI<MainMenuPanelController>(UI_TYPE.MainMenu, () => Hide());
    }
}
