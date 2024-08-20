using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PreGameHUD : NetworkBehaviour
{
    [SerializeField] private TMP_Text lobbyCodeText;

    private NetworkManager networkManager;

    private NetworkVariable<FixedString32Bytes> lobbyCode = new NetworkVariable<FixedString32Bytes>("");
    public NetworkVariable<int> numPlayersReady = new NetworkVariable<int>();

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

    [SerializeField] private GameObject readyUpButton;
    [SerializeField] private GameObject unreadyButton;
    [SerializeField] private GameObject hostReadyUpButton;
    [SerializeField] private GameObject hostUnreadyButton;
    [SerializeField] private TMP_Text WaitingForHostText;
    [SerializeField] private TMP_Text playersOutOfPlayersText;

    const string waitingString = "Waiting for Host...";
    

    public override void OnNetworkSpawn()
    {
        networkManager = FindAnyObjectByType<NetworkManager>();

        if (IsClient)
        {
            if (hostHUD) hostHUD.SetActive(false);
            if(clientHUD) clientHUD.SetActive(true);
            if (WaitingForHostText) WaitingForHostText.text = string.Empty;
        }

        lobbyCode.OnValueChanged += HandleLobbyCodeChanged;
        HandleLobbyCodeChanged(string.Empty, lobbyCode.Value);

        numPlayersReady.OnValueChanged += HandlePlayersReadyChanged;
        HandlePlayersReadyChanged(0, numPlayersReady.Value);
        UpdateReadyText();

        networkManager.OnClientDisconnectCallback += HandleClientLeft;
        networkManager.OnClientConnectedCallback += HandleClientJoined;

        if (!IsHost) return;

        if (hostHUD) hostHUD.SetActive(true);
        if (clientHUD) clientHUD.SetActive(false);

        lobbyCode.Value = HostSingleton.Instance.GameManager.JoinCode;
    }

    private void HandleClientJoined(ulong obj)
    {
        if (IsServer)
        {
            StartCoroutine(RefreshPlayerNumText(false));

            if(playersOutOfPlayersText) playersOutOfPlayersText.text = $"{numPlayersReady.Value}/{NetworkManager.Singleton.ConnectedClients.Count}";
        }
    }

    private void HandleClientLeft(ulong obj)
    {
        if(IsServer)
        {
            StartCoroutine(RefreshPlayerNumText(true));
            if(playersOutOfPlayersText) playersOutOfPlayersText.text = $"{numPlayersReady.Value}/{NetworkManager.Singleton.ConnectedClients.Count}";
        }
        
    }

    IEnumerator RefreshPlayerNumText(bool minus)
    {
        yield return new WaitForSeconds(0.2f);
        if(minus)
        {
            numPlayersReady.Value -= 1;
        }
    }

    public override void OnNetworkDespawn()
    {
        networkManager.OnClientDisconnectCallback -= HandleClientLeft;
        networkManager.OnClientConnectedCallback -= HandleClientJoined;

        if (IsClient)
        {
            lobbyCode.OnValueChanged -= HandleLobbyCodeChanged;
            numPlayersReady.OnValueChanged -= HandlePlayersReadyChanged;
        }
    }

    private void UpdateReadyText()
    {
        if (playersOutOfPlayersText == null) return;
        playersOutOfPlayersText.text = $"{numPlayersReady.Value}/{NetworkManager.Singleton.ConnectedClients.Count}";
    }

    private void HandlePlayersReadyChanged(int previousValue, int newValue)
    {
        if(IsServer)
        {
            numPlayersReady.Value = newValue;
            UpdateReadyText();
        }
        if(IsClient && !IsHost)
        {
            UpdateReadyText();
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
        if (numPlayersReady.Value != NetworkManager.Singleton.ConnectedClients.Count) return;

        await HostSingleton.Instance.GameManager.ChangeSceneAsync(MapSceneName);
    }

    public void LeaveGame()
    {
        if(NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.GameManager.Shutdown();
        }

        ClientSingleton.Instance.GameManager.Disconnect();

        UpdateReadyText();
    }

    public void ReadyUp()
    {
        if(IsHost)
        {
            UpdateReadyPlayersServerRpc(true);
            hostReadyUpButton.SetActive(false);
            hostUnreadyButton.SetActive(true);
            WaitingForHostText.text = waitingString;
        }
        else
        {
            UpdateReadyPlayersServerRpc(true);
            readyUpButton.SetActive(false);
            unreadyButton.SetActive(true);
            WaitingForHostText.text = waitingString;
        }
    }

    public void Unready()
    {
        if(IsHost)
        {
            UpdateReadyPlayersServerRpc(false);
            hostUnreadyButton.SetActive(false);
            hostReadyUpButton.SetActive(true);
            WaitingForHostText.text = string.Empty;
        }
        else
        {
            UpdateReadyPlayersServerRpc(false);
            unreadyButton.SetActive(false);
            readyUpButton.SetActive(true);
            WaitingForHostText.text = string.Empty;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateReadyPlayersServerRpc(bool increase)
    {
        if(increase)
        {
            numPlayersReady.Value += 1;
        }
        else
        {
            numPlayersReady.Value -= 1;
        }
    }

    private void HandleLobbyCodeChanged(FixedString32Bytes oldCode, FixedString32Bytes newCode)
    {
        lobbyCodeText.text = newCode.ToString();
    }
}
