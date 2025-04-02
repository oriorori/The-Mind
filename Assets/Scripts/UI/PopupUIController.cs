using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupUIController : MonoBehaviour, IGameUI
{
    [SerializeField] private TextMeshProUGUI popupText;
    
    [SerializeField] private Button closeButton;

    void Start()
    {
        closeButton.onClick.AddListener(OnClickClose);
    }

    public void SetText(string text)
    {
        popupText.text = text;
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

    private void OnClickClose()
    {
        Hide();
    }
}
