using UnityEngine;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;

public class SceneCommunicator : MonoBehaviour
{
    public bool isHost { get; set; } = false;
    public Lobby lobby { get; set; }
    public Allocation allocation { get; set; }
    public JoinAllocation joinAllocation { get; set; }

    public static SceneCommunicator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

}
