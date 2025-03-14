using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    public Vector3 scaleOffset;

    public bool isPickedUp { get; set; } = false;

    [Header("After spawn prefab")]
    public GameObject afterSpawnPrefab;
}
