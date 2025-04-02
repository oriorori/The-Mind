using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SocketIOClient;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomPanelController : MonoBehaviour, IGameUI
{
    [SerializeField] private Button _startGameButton;
    [SerializeField] private RectTransform _playerListRect;

    [SerializeField] private GameObject _playerObject;

    private List<string> playerNames;
    
    void Start()
    {
        playerNames = new List<string>();
    }

    public void AddNewPlayer(SocketIOResponse response)
    {
        string playerName = response.GetValue<string>();
        Debug.Log(playerName);
        playerNames.Add(playerName);
        
        GameObject playerObject = Instantiate(_playerObject, _playerListRect);
        TextMeshProUGUI playerNameText = playerObject.GetComponentInChildren<TextMeshProUGUI>();
        playerNameText.text = playerName;
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
