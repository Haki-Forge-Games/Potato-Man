using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;


public class UiManagerLobby : MonoBehaviour
{
    [Header("Screens")]
    public GameObject createRoomScreen;
    public GameObject joinRoomScreen;
    public GameObject waitingScreen;


    [Header("Texts")]
    public TextMeshProUGUI feedbackText;


    [Header("Inputs")]
    public TMP_InputField lobbyNameInput;
    public TMP_InputField lobbyPasswordInput;
    public TMP_InputField joinLobbyNameInput;
    public TMP_InputField joinLobbyPasswordInput;

    private Logger logger;
    public static UiManagerLobby Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Initialize();
        ValidateReferences();
    }

    private void Initialize()
    {
        logger = Logger.Instance;
    }


    public void CloseCreateRoom()
    {
        if (createRoomScreen != null)
        {
            createRoomScreen.SetActive(false);
        }
    }

    public void HandleScreensToggles(GameObject screen = null)
    {
        if (screen == null) return;
        screen.SetActive(!screen.activeSelf);
    }


    public void ShowFeedback(string message, bool success = false)
    {
        if (feedbackText == null) return;

        feedbackText.text = message;
        feedbackText.color = success ? Color.green : Color.red;

    }

    private void ValidateReferences()
    {
        if (logger == null) return;

        if (waitingScreen == null) logger.LoggerError("[UiManagerLobby] waitingScreen is not assigned.");
        if (joinRoomScreen == null) logger.LoggerError("[UiManagerLobby] joinRoomScreen is not assigned.");
        if (createRoomScreen == null) logger.LoggerError("[UiManagerLobby] createRoomScreen is not assigned.");
        if (feedbackText == null) logger.LoggerError("[UiManagerLobby] feedbackText is not assigned.");
        if (joinLobbyNameInput == null) logger.LoggerError("[UiManagerLobby] joinLobbyNameInput is not assigned.");
        if (joinLobbyPasswordInput == null) logger.LoggerError("[UiManagerLobby] joinLobbyPasswordInput is not assigned.");
    }

}
