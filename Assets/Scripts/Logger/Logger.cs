using UnityEngine;

public class Logger : MonoBehaviour
{
    public static Logger Instance { get; private set; }
    [SerializeField] private bool isDevelopment = true;

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

    public void LoggerMessage(string message)
    {
        if (isDevelopment)
        {
            Debug.Log($"[INFO] {message}");
        }
    }

    public void LoggerWarning(string warningMessage)
    {
        if (isDevelopment)
        {
            Debug.LogWarning($"[WARNING] {warningMessage}");
        }
    }

    public void LoggerError(string errorMessage)
    {
        if (isDevelopment)
        {
            Debug.LogError($"[ERROR] {errorMessage}");
        }
    }
}
