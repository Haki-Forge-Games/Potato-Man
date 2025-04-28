using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Offsets")]
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    public Vector3 scaleOffset;

    // states 
    public bool isPickedUp { get; private set; }
    public Player owner { get; private set; }

    // <Summary> 
    // Sets the isPickedUp state of the object
    // </Summary>
    public void SetPickedUpState(bool state = false)
    {
        this.isPickedUp = state;
    }

    // <Summary> 
    // Sets the owner of the object
    // </Summary>
    public void SetOwner(Player player = null)
    {
        this.owner = player;
    }
}
