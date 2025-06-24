using UnityEngine;
using UnityEngine.UI;

public enum WaitingStatus
{
    playing,
    waiting
}

public class PlayerWaitingStatusController : MonoBehaviour
{
    [SerializeField] private Image waitingStatusImage;

    public void ChangeStatus(WaitingStatus waitingStatus)
    {
        switch (waitingStatus)
        {
            case WaitingStatus.waiting:
                waitingStatusImage.color = Color.green;
                break;
            case WaitingStatus.playing:
                waitingStatusImage.color = Color.red;
                break;
        }
    }
}
