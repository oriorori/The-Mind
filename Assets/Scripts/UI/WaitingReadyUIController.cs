using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class WaitingReadyUIController : MonoBehaviour
{
    [SerializeField] private Button _applyButton;
    [SerializeField] private Button _rejectButton;
    
    
    [SerializeField] private GameObject _playerIcon;
    [SerializeField] private GameObject _playerIconContainer;
    
    private List<GameObject> playerIconList;
    
    private int currentPlayerIndex;

    void Awake()
    {
        _applyButton.onClick.AddListener(OnClickApplyButton);
        _rejectButton.onClick.AddListener(OnClickRejectButton);
    }
    
    public void Initialize(bool pressedStartButton)
    {
        int playerNum = GameManager.Instance.currentPlayingRoom.maxPlayerNumber;
        for (int i = 0; i < playerNum; i++)
        {
            playerIconList.Add(Instantiate(_playerIcon, _playerIconContainer.transform));
        }

        playerIconList[currentPlayerIndex++].GetComponent<Image>().color = Color.green;

        if (pressedStartButton)
        {
            _applyButton.gameObject.SetActive(false);
            _rejectButton.gameObject.SetActive(false);
        }
        else
        {
            _applyButton.gameObject.SetActive(true);
            _rejectButton.gameObject.SetActive(true);
        }
    }

    private void OnClickApplyButton()
    {
        GameManager.Instance.multiplayController.ReadyGame();
        _applyButton.gameObject.SetActive(false);
        _rejectButton.gameObject.SetActive(false);
    }

    private void OnClickRejectButton()
    {
        GameManager.Instance.multiplayController.RejectGame();
    }

    public void ChangePlayerIconColorGreen()
    {
        playerIconList[currentPlayerIndex++].GetComponent<Image>().color = Color.green;
    }

    public void RejectStartingGame()
    {
        playerIconList[currentPlayerIndex++].GetComponent<Image>().color = Color.red;
    }

    public void OnDisable()
    {
        for (int i = playerIconList.Count - 1; i >= 0; i--)
        {
            Destroy(playerIconList[i]);
        }
        playerIconList.Clear();
        currentPlayerIndex = 0;
    }
}
