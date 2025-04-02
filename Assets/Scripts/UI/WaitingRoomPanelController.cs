using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaitingRoomPanelController : MonoBehaviour, IGameUI
{
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
