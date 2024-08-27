using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] InputAction pause;
    public static bool GameIsPaused = false;

    [SerializeField] private GameObject lobbyCodeObject;
    [SerializeField] private GameObject leaveGameObject;

    ZPlayer zPlayer;

    // Start is called before the first frame update
    void Start()
    {
        zPlayer = FindAnyObjectByType<ZPlayer>();
    }

    private void OnEnable()
    {
        pause.Enable();
    }

    private void OnDisable()
    {
        pause.Disable();
    }

    private void Pause()
    {
        var wasPressed = pause.triggered && pause.ReadValue<float>() > 0;

        if (wasPressed)
        {
            if (GameIsPaused)
            {
                RemovePauseScreen();
            }
            else
            {
                ShowPauseScreen();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Pause();
    }

    private void ShowPauseScreen()
    {
        leaveGameObject?.SetActive(true);
        lobbyCodeObject?.SetActive(true);
        GameIsPaused = true;
        zPlayer.paused = true;
        Debug.Log("Pause");
    }
    private void RemovePauseScreen()
    {
        leaveGameObject?.SetActive(false);
        lobbyCodeObject?.SetActive(false);
        GameIsPaused = false;
        zPlayer.paused = false;
        Debug.Log("Resume");
    }
}
