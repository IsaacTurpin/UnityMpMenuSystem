using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PreGameHUD : NetworkBehaviour
{
    [SerializeField] private TMP_Text lobbyCodeText;

    private NetworkVariable<FixedString32Bytes> lobbyCode = new NetworkVariable<FixedString32Bytes>("");

    private string MapSceneName = string.Empty;
    private const string Map1Name = "Map1";
    private const string Map2Name = "Map2";
    private const string Map3Name = "Map3";
    private const string Map4Name = "Map4";
    [SerializeField] private Toggle map1Toggle;
    [SerializeField] private Toggle map2Toggle;
    [SerializeField] private Toggle map3Toggle;
    [SerializeField] private Toggle map4Toggle;

    [SerializeField] GameObject hostHUD;
    [SerializeField] GameObject clientHUD;

    public override void OnNetworkSpawn()
    {
        if(IsClient)
        {
            lobbyCode.OnValueChanged += HandleLobbyCodeChanged;
            HandleLobbyCodeChanged(string.Empty, lobbyCode.Value);
            if(hostHUD) hostHUD.SetActive(false);
            if(clientHUD) clientHUD.SetActive(true);
        }

        if (!IsHost) return;

        if (hostHUD) hostHUD.SetActive(true);
        if (clientHUD) clientHUD.SetActive(false);

        lobbyCode.Value = HostSingleton.Instance.GameManager.JoinCode;
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            lobbyCode.OnValueChanged -= HandleLobbyCodeChanged;
        }
    }

    public void GetMapSceneName()
    {
        if(map1Toggle.isOn)
        {
            MapSceneName = Map1Name;
        }
        if (map2Toggle.isOn)
        {
            MapSceneName = Map2Name;
        }
        if (map3Toggle.isOn)
        {
            MapSceneName = Map3Name;
        }
        if (map4Toggle.isOn)
        {
            MapSceneName = Map4Name;
        }
        if(!map1Toggle.isOn && !map2Toggle.isOn && !map3Toggle.isOn && !map4Toggle.isOn)
        {
            MapSceneName = string.Empty;
        }
    }

    public async void StartGame()
    {
        if (MapSceneName == string.Empty) return;
        await HostSingleton.Instance.GameManager.ChangeSceneAsync(MapSceneName);
    }

    public void LeaveGame()
    {
        if(NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.GameManager.Shutdown();
        }

        ClientSingleton.Instance.GameManager.Disconnect();
    }

    private void HandleLobbyCodeChanged(FixedString32Bytes oldCode, FixedString32Bytes newCode)
    {
        lobbyCodeText.text = newCode.ToString();
    }
}
