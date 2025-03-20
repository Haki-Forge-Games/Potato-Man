using System;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    private UiManagerLobby uiManagerLobby;
    private Logger logger;

    private void Start()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            SignInAnonymouslyAsync();
        }

        Initialize();
        ValidateReferences();
    }

    private void Initialize()
    {
        uiManagerLobby = UiManagerLobby.Instance;
        logger = Logger.Instance;
    }

    private async void SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            if (uiManagerLobby != null)
            {
                uiManagerLobby.ShowFeedback("Sign in anonymously succeeded!", true);
                uiManagerLobby.ShowFeedback($"PlayerID: {AuthenticationService.Instance.PlayerId}", true);
            }

        }
        catch (Exception ex)
        {
            uiManagerLobby.ShowFeedback("Sign in failed!");
        }
    }

    private async void SignOut()
    {
        AuthenticationService.Instance.SignOut(true);
        Debug.Log("Sign out succeeded!");
    }

    private void ValidateReferences()
    {
        if (logger == null) return;

        if (uiManagerLobby == null) logger.LoggerError("[AuthManager] UiManagerLobby is not assigned.");
    }
}
