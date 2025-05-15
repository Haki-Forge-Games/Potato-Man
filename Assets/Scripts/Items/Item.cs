using System.Collections;
using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float walkEffectHeight = 0.3f;
    [SerializeField] private float movementSpeed = 1f;

    [Header("Offsets")]
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    public Vector3 scaleOffset;

    [Header("References")]
    public GameObject teammateViewItemPrefab;

    // states 
    public bool isPickedUp { get; private set; } = false;
    public Player owner { get; private set; }

    // records
    private float originalHoldPositionY;
    private Coroutine walkEffectCoroutine;

    private void Update()
    {
        if (owner == null)
        {
            if (walkEffectCoroutine != null)
            {
                StopCoroutine(walkEffectCoroutine);
                walkEffectCoroutine = null;
            }
            return;
        }

        var movement = owner.GetComponent<Movement>();
        if (movement == null) return;
        bool shouldWalk = isPickedUp && movement.IsWalking;

        if (shouldWalk && walkEffectCoroutine == null)
        {
            originalHoldPositionY = owner.holdPosition.localPosition.y;
            walkEffectCoroutine = StartCoroutine(WalkEffect());
        }
        else if (!shouldWalk && walkEffectCoroutine != null)
        {
            StopCoroutine(walkEffectCoroutine);
            walkEffectCoroutine = null;

            Vector3 pos = owner.holdPosition.localPosition;
            pos.y = originalHoldPositionY;
            owner.holdPosition.localPosition = pos;
        }
    }

    // <summary>
    // Sets the item to be picked up or not.
    // </summary>
    public void SetPickedUpState(bool state = false)
    {
        this.isPickedUp = state;
    }

    // <summary>
    // Sets the owner of the item.
    // </summary>
    public void SetOwner(Player player = null)
    {
        this.owner = player;
    }

    // <summary>
    // Makes the item go up and down when the player is walking to make a walking effect.
    // </summary>
    private IEnumerator WalkEffect()
    {
        Transform holdPosition = owner.holdPosition;
        var movement = owner.GetComponent<Movement>();
        if (holdPosition == null || movement == null)
        {
            walkEffectCoroutine = null;
            yield break;
        }

        float baseY = holdPosition.localPosition.y;

        while (isPickedUp && movement.IsWalking)
        {
            float offset = Mathf.PingPong(Time.time * movementSpeed, walkEffectHeight);
            holdPosition.localPosition = new Vector3(
                holdPosition.localPosition.x,
                baseY + offset,
                holdPosition.localPosition.z
            );
            yield return null;
        }
    }
}
