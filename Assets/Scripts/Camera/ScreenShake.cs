using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private Player player;
    private Vector3 originalPos;

    private void Start()
    {
        originalPos = transform.localPosition;
    }
    public IEnumerator Shake(float magnitude, float duration)
    {
        if (magnitude == 0f || duration == 0f) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // getting random shakes 
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}
