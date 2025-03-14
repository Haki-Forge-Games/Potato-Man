using UnityEngine;
using Unity.Netcode;

public class NetworkPickAnsDrop : NetworkBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private ItemsDatabase itemsDatabase;
    [SerializeField] private PickUpAndDrop pickAndDrop;

    // <Symmary>
    // <para>Handle pick up item over the network</para>
    // </Symmary>

    [ServerRpc]
    public void PickUpServerRpc(NetworkObjectReference itemRef, ServerRpcParams serverRpcParams = default)
    {
        if (!itemRef.TryGet(out NetworkObject itemNtwObject)) return;

        Item itemComponent = itemNtwObject.transform.GetComponent<Item>();
        if (itemComponent == null || itemComponent.afterSpawnPrefab == null) return;

        // remove network obj from world
        itemNtwObject.Despawn();

        // rpc for spawning a local item in player hand 
        SpawnItemClientRpc(itemComponent.afterSpawnPrefab.name);

        // changes the isPickedUp state of the item 
        ChangePickUpStatusClientRpc(true, CreateClientRpcParams(serverRpcParams.Receive.SenderClientId));
    }

    // <Symmary>
    // <para>Handle held item drop over the network</para>
    // </Symmary>

    [ServerRpc]
    public void DropItemServerRpc(NetworkObjectReference playerRef, ServerRpcParams serverRpcParams = default)
    {
        if (!playerRef.TryGet(out NetworkObject playerNtwObject)) return;

        Player playerScript = playerNtwObject.GetComponent<Player>();
        if (playerScript == null || playerScript.holdPosition.childCount == 0) return;

        GameObject handItem = playerScript.holdPosition.GetChild(0).gameObject;
        Item itemComponent = handItem.GetComponent<Item>();
        if (itemComponent == null || itemComponent.afterSpawnPrefab == null) return;

        GameObject worldInstance = Instantiate(itemComponent.afterSpawnPrefab, playerScript.holdPosition.position, playerScript.holdPosition.rotation);
        worldInstance.GetComponent<NetworkObject>()?.Spawn();

        DestroyItemClientRpc();
        ChangePickUpStatusClientRpc(false, CreateClientRpcParams(serverRpcParams.Receive.SenderClientId));
    }

    // <Symmary>
    // <para>For spawning a local item in player hand and to show up that in all clients</para>
    // </Symmary>

    [ClientRpc]
    private void SpawnItemClientRpc(string itemname)
    {
        GameObject prefab = itemsDatabase.GetItemByName(itemname);
        if (prefab == null) return;

        GameObject instance = Instantiate(prefab, player.holdPosition);

        if (instance == null) return;

        instance.transform.SetParent(player.holdPosition);
        Item itemComponent = instance.GetComponent<Item>();

        if (itemComponent == null) return;
        SetupItemTransform(instance, itemComponent);
        itemComponent.isPickedUp = true;
    }

    // <Symmary>
    // <para>Changes the isPickedUp state</para>
    // </Symmary>

    [ClientRpc]
    private void ChangePickUpStatusClientRpc(bool status, ClientRpcParams clientRpcParams = default)
    {
        if (pickAndDrop == null) return;
        pickAndDrop.isPickedUp = status;
    }


    // <Symmary>
    // <para>Destroy the item in the hand</para>
    // </Symmary>

    [ClientRpc]
    private void DestroyItemClientRpc()
    {
        if (player.holdPosition.childCount > 0)
        {
            Destroy(player.holdPosition.GetChild(0).gameObject);
        }
    }

    // <Symmary>
    // <para>Gives the sender client id</para>
    // </Symmary>
    private ClientRpcParams CreateClientRpcParams(ulong clientId)
    {
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
        };
    }

    // <Symmary>
    // <para>Sets teh local positions of the held item</para>
    // </Symmary>
    private void SetupItemTransform(GameObject instance, Item itemComponent)
    {
        instance.transform.localPosition = itemComponent.positionOffset;
        instance.transform.localRotation = Quaternion.Euler(itemComponent.rotationOffset);
        instance.transform.localScale = itemComponent.scaleOffset;
    }
}