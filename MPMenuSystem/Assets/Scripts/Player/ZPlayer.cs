using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using Unity.Collections;
using System;
using UnityEngine.SceneManagement;
using StarterAssets;

public class ZPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Texture2D crosshair;
    private StarterAssetsInputs starterAssetsInputs;
    //[field: SerializeField] public Health Health {  get; private set; }

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    //public NetworkVariable<int> TeamIndex = new NetworkVariable<int>();

    public static event Action<ZPlayer> OnPlayerSpawned;
    public static event Action<ZPlayer> OnPlayerDespawned;

    public bool completed = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = null;
            if (IsHost)
            {
                userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }
            //else
           // {
            //    userData = ServerSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            //}

            PlayerName.Value = userData.userName;
           // TeamIndex.Value = userData.teamIndex;

            OnPlayerSpawned?.Invoke(this);
        }

        if (IsOwner)
        {
            virtualCamera.Priority = ownerPriority;

            starterAssetsInputs = GetComponent<StarterAssetsInputs>();

            if (crosshair != null && SceneManager.GetActiveScene().name == "Menu")
            {
                Cursor.SetCursor(crosshair, new Vector2(crosshair.width / 2, crosshair.height / 2), CursorMode.Auto);
            }
            
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }

    private void Update()
    {
        if(IsOwner)
        {
            if(SceneManager.GetActiveScene().name != "PreGame")
            {
                //starterAssetsInputs?.SetCursorState(false);
                if (!completed)//not already set
                {
                    //starterAssetsInputs?.SetCursorState(true);
                    HideCursor();
                    Debug.Log("not paused not Pregame");
                    completed = true;
                }
            }
        }
    }

    private void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
