using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : Singleton<NetworkManager>
{
    public IEnumerator CreateRoom(CreateRoomData createRoomData, Action success)
    {
        string jsonString = JsonConvert.SerializeObject(createRoomData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);
        
        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/createRoom", 
                   UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error");
                Debug.Log(www.responseCode);

                if (www.responseCode == 409)
                {
                    PopupUIController popupUI = UIManager.Instance.GetUI<PopupUIController>(UI_TYPE.Popup);
                    popupUI.SetText("이미 존재하는 방 번호입니다");
                }
            }
            else
            {
                var result = www.downloadHandler.text;
                
                success?.Invoke();
                
                Debug.Log(result);
            }
        }
    }

    public IEnumerator JoinRoom(JoinRoomData joinRoomData)
    {
        string jsonString = JsonConvert.SerializeObject(joinRoomData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/joinRoom",
                   UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error");

                if (www.responseCode == 404)
                {
                    Debug.Log("방이 존재하지 않음");
                }
                else if (www.responseCode == 403)
                {
                    Debug.Log("방이 가득 참");
                }
                else if (www.responseCode == 409)
                {
                    Debug.Log("이미 방에 참가함");
                }
            }

            else
            {
                var resultString = www.downloadHandler.text;
                
                Debug.Log(resultString);
            }
        }
    }

    public IEnumerator LeaveRoom(DestroyRoomData destroyRoomData)
    {
        string jsonString = JsonConvert.SerializeObject(destroyRoomData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www =
               new UnityWebRequest(Constants.ServerURL + "/users/destroyRoom",
                   UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            
            yield return www.SendWebRequest();

            
            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error");

                if (www.responseCode == 404)
                {
                    Debug.Log("방이 존재하지 않음");
                }
            }

            else
            {
                var resultString = www.downloadHandler.text;
                
                Debug.Log(resultString);
            }
        }
    }
}
