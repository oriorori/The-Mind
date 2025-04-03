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
    
    void Start()
    {

    }

    public void Initialize()
    {
        // 처음 방에 들어갈 때 실행
        // http 통신에서 받은 playerList를 이용해 init
        foreach (string playerName in GameManager.Instance.currentPlayingRoom.players)
        {
            GameObject playerObject = Instantiate(_playerObject, _playerListRect);
            TextMeshProUGUI playerNameText = playerObject.GetComponentInChildren<TextMeshProUGUI>();
            playerNameText.text = playerName;
        }
    }

    public void AddNewPlayer(SocketIOResponse response)
    {
        // 나는 참가해있고 다른 누군가가 추가로 참가할 때 호출
        // multiplaycontroller의 joinroom에 구독해놓고 socket통신에서 joinRoomCli response가 오면 실행된다
        GameObject playerObject = Instantiate(_playerObject, _playerListRect);
        TextMeshProUGUI playerNameText = playerObject.GetComponentInChildren<TextMeshProUGUI>();
        playerNameText.text = response.GetValue<string>();
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
