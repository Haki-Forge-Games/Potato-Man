using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Config/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [Header("Items NetworkObjects")]
    public List<GameObject> networkItemsPrefabs;
}