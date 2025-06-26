using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupUIController : MonoBehaviour, IGameUI
{
    [SerializeField] private TextMeshProUGUI popupText;
    
    [SerializeField] private Button closeButton;
    private Action EventClosingPopupUI;

    void Start()
    {
        closeButton.onClick.AddListener(OnClickClose);
    }

    public void SetText(string text)
    {
        popupText.text = text;
    }

    public void SetCloseAction(Action closeAction)
    {
        EventClosingPopupUI = closeAction;
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
        EventClosingPopupUI?.Invoke();
        popupText.text = "";
        Hide();
        EventClosingPopupUI = null;
    }
}
