using System;
using UnityEngine;
using Unity.Services.Relay;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;

public class RelayManager : MonoBehaviour
{

    public static RelayManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public async Task<(string, Allocation)> CreateAllocation(int maxPlayers)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return (joinCode, allocation);
        }
        catch (Exception ex)
        {
            return ("", null);
        }
    }

    public async Task<JoinAllocation> JoinRelayAllocation(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}