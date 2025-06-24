using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShurikenVoteUIController : MonoBehaviour, IGameUI
{
    [SerializeField] private Button _applyButton;
    [SerializeField] private Button _refuseButton;
    [SerializeField] private TextMeshProUGUI _text;

    void Awake()
    {
        _applyButton.onClick.AddListener(OnClickApplyButton);
        _refuseButton.onClick.AddListener(OnClickRefuseButton);
        GameManager.Instance.multiplayController.EventUseShuriken += _ => { Hide(); };
    }

    public void Initialize(string firstSuggestedId)
    {
        if (GameManager.Instance.userInfo.userId == firstSuggestedId)
        {
            _applyButton.gameObject.SetActive(false);
            _refuseButton.gameObject.SetActive(false);
            _text.text = "다른 플레이어 기다리는중...";
        }
    }
    private void OnClickApplyButton()
    {
        GameManager.Instance.multiplayController.SendAgreeShuriken(GameManager.Instance.userInfo.userId);
        _applyButton.gameObject.SetActive(false);
        _refuseButton.gameObject.SetActive(false);
        _text.text = "다른 플레이어 기다리는중...";
    }

    private void OnClickRefuseButton()
    {
        GameManager.Instance.multiplayController.SendRefuseShuriken(GameManager.Instance.userInfo.userId);
        _applyButton.gameObject.SetActive(false);
        _refuseButton.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        _applyButton.gameObject.SetActive(true);
        _refuseButton.gameObject.SetActive(true);
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
