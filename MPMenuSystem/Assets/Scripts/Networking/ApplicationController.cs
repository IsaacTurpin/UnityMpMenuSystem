using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton clientPrefab;

    [SerializeField] private HostSingleton hostPrefab;

    [SerializeField] private NetworkObject playerPrefab;

    private ApplicationData appData;

    private const string GameSceneName = "Game";

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);

        await LaunchInMode();
    }

    private async Task LaunchInMode()
    {
        HostSingleton hostSingleton = Instantiate(hostPrefab);
        hostSingleton.CreateHost(playerPrefab);

        ClientSingleton clientSingleton = Instantiate(clientPrefab);
        bool authenticated = await clientSingleton.CreateClient();

        if (authenticated)
        {
            clientSingleton.GameManager.GoToMenu();
        }
    }
}
