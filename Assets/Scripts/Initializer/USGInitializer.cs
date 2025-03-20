using System;
using UnityEngine;
using Unity.Services.Core;

public class USGInitializer : MonoBehaviour
{
    private async void Awake()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            Debug.Log("Unity Services has already been initialized.");
            return;
        }

        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services has been initialized.");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
