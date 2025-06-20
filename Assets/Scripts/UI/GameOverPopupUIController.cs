using Cysharp.Threading.Tasks;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPopupUIController : MonoBehaviour, IGameUI
{
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _exitButton;


    void OnEnable()
    {
        _restartButton.onClick.AddListener(OnClickRestartButton);
        _exitButton.onClick.AddListener(OnClickExitButton);
    }

    private void OnClickRestartButton()
    {
        GameManager.Instance.multiplayController.SendRestartGame();
        Hide();
    }

    private void OnClickExitButton()
    {
        GameManager.Instance.multiplayController.SendDestroyRoom();
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
