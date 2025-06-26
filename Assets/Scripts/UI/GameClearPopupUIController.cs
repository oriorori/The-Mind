using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameClearPopupUIController : MonoBehaviour, IGameUI
{
    [SerializeField] private Button _nextButton;
    
    void OnEnable()
    {
        _nextButton.onClick.AddListener(OnClickNextButton);
    }

    private void OnClickNextButton()
    {
        UIManager.Instance.GetUI<GamePanelController>(UI_TYPE.Game).Hide();
        UIManager.Instance.GetUI<WaitingRoomPanelController>(UI_TYPE.WaitingRoom).Show();
        GameManager.Instance.multiplayController.SendBackToRoom();
        Hide();
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
