using Mirror;
using UnityEngine;

public class ClearCounter : Furniture
{
    [SerializeField] Transform itemContainer;
    [SerializeField] MeshRenderer fakeItemVisual;
    [SerializeField] MeshFilter fakeItemVisualFilter;
    bool isFilled = false;

    [SyncVar (hook =nameof(Hook_SetDroppedItem)), HideInInspector] public GameObject droppedItem;

    public override void OnAction1(PlayerController player)
    {
        base.OnAction1(player);

        if (!player.IsHoldingItem())
        {
            if (droppedItem == null)
                return;
            else
                PlayerTakeItem(player);
        }
        else if (player.IsHoldingItem() && !player.IsHoldingPlate())
        {
            if (droppedItem != null)
                return;
            else
                PlayerDropItem(player);
        }
        else if (player.IsHoldingItem() && player.IsHoldingPlate())
        {
            if (droppedItem == null)
                PlayerDropItem(player);
            else
            {
                if (!droppedItem.GetComponent<Item>().GetItemSO().isComestible)
                    return;

                // Plate Logic
            }
        }
    }

    public override void OnAction2(PlayerController player)
    {
        base.OnAction2(player);
    }

    void PlayerTakeItem(PlayerController player)
    {
        player.TakeDropItemFromClearCounter(droppedItem);
        RPC_SetDroppedItem(null);
    }

    void PlayerDropItem(PlayerController player)
    {
        RPC_SetDroppedItem(player.takenItem);
        player.TakeDropItemFromClearCounter(null);
    }

    [Command(requiresAuthority = false)] void RPC_SetDroppedItem(GameObject item)
    {
        droppedItem = item;
    }

    public void Hook_SetDroppedItem(GameObject oldValue, GameObject newValue)
    {
        if (newValue != null)
        {
            Item item = newValue.GetComponent<Item>();
            fakeItemVisual.sharedMaterial = item.GetItemSO().material;
            fakeItemVisualFilter.sharedMesh = item.GetItemSO().mesh;
        }

        fakeItemVisual.enabled = (newValue == null) ? false : true;
    }
}