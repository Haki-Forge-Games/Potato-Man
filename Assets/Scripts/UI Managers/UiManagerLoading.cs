using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UiManagerLoading : MonoBehaviour
{
    [Header("Screens")]
    public GameObject loadingScreen;

    [Header("Texts")]
    public TextMeshProUGUI loadingText;

    private GameManager gameManager;
    private Logger logger;

    private void Start()
    {
        Initialize();
        ActiveSceneLoading(2);
        ValidateReferences();
    }

    private void Initialize()
    {
        gameManager = GameManager.Instance;
        logger = Logger.Instance;
    }

    private void ActiveSceneLoading(int index)
    {
        if (gameManager == null || index == null) return;
        StartCoroutine(gameManager.LoadSceneAsync(index, loadingText));
    }

    private void ValidateReferences()
    {
        if (logger == null) return;

        if (gameManager == null) logger.LoggerError("[UiManagerLoading] gameManager is not assigned.");
        if (loadingScreen == null) logger.LoggerError("[UiManagerLoading] waitingScreen is not assigned.");
        if (loadingText == null) logger.LoggerError("[UiManagerLoading] loadingText is not assigned.");
    }
}
