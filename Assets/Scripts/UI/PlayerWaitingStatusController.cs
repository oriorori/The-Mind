using UnityEngine;
using UnityEngine.UI;

public enum WaitingStatus
{
    playing,
    waiting
}

public class PlayerWaitingStatusController : MonoBehaviour
{
    [SerializeField] private GameObject _loadingIconPrefab;

    public void ChangeStatus(WaitingStatus waitingStatus)
    {
        switch (waitingStatus)
        {
            case WaitingStatus.waiting:
                _loadingIconPrefab.SetActive(false);
                break;
            case WaitingStatus.playing:
                _loadingIconPrefab.SetActive(true);
                break;
        }
    }
}
