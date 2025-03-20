using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemsDatabase", menuName = "config/ItemsDatabase")]


public class ItemsDatabase : ScriptableObject
{
    public List<GameObject> items;

    public GameObject GetItemByName(string name)
    {
        return items.Find(item => item.name == name);
    }
}