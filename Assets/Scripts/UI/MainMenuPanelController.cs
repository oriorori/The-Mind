using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanelController : MonoBehaviour, IGameUI
{
    [SerializeField] private Button _randomMatching;
    [SerializeField] private Button _createRoom;
    [SerializeField] private Button _joinRoom;

    void Start()
    {
        _createRoom.onClick.AddListener(OnClickCreateRoom);
        _joinRoom.onClick.AddListener(OnClickJoinRoom);
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
        UIManager.Instance.ShowUI<CreateRoomPanelController>(UI_TYPE.CreateRoom, () => Hide());
    }

    private void OnClickJoinRoom()
    {
        UIManager.Instance.ShowUI<JoinRoomPanelController>(UI_TYPE.JoinRoom, () => Hide());
    }
}
