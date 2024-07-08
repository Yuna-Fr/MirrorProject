using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class CuttingStation : Furniture
{
    [SerializeField] Transform itemContainer;
    [SerializeField] MeshRenderer fakeItemVisual;
    [SerializeField] MeshFilter fakeItemVisualFilter;
    [SerializeField] List<ItemSO.ItemType> cutablesItems;

    [SyncVar(hook = nameof(Hook_SetDroppedItem)), HideInInspector] public GameObject droppedItem;
    [SyncVar(hook = nameof(Hook_SetFakeCutItem))] ItemSO.ItemType hookForFakeCutItem;

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
                if (IsItemCutable(player.takenItem.GetComponent<Item>().itemType))
                    PlayerDropItem(player);
        }
        else if (player.IsHoldingItem() && player.IsHoldingPlate())
        {
            if (droppedItem == null)
                PlayerDropItem(player);

            else if (player.GetFakePlate().TryAddItemOnPlate(droppedItem.GetComponent<Item>()))
            {
                player.GetFakePlate().AddItemInPlate(droppedItem.GetComponent<Item>(), player.takenItem.GetComponent<Item>());
                RPC_SetDroppedItem(null);
            }
        }

    }

    public override void OnAction2(PlayerController player)
    {
        if (droppedItem == null)
            return;

        if (IsItemCutable(droppedItem.GetComponent<Item>().itemType))
        {
            RPC_CutItem();
        }
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

    bool IsItemCutable(ItemSO.ItemType itemType)
    {
        bool result = false;

        foreach (ItemSO.ItemType cutable in cutablesItems)
            if (cutable == itemType)
            {
                result = true;
                break;
            }

        return result;
    }

    [Command(requiresAuthority = false)]
    void RPC_SetDroppedItem(GameObject item)
    {
        droppedItem = item;
    }

    [Command(requiresAuthority = false)]
    void RPC_CutItem()
    {
        Item item = droppedItem.GetComponent<Item>();
        item.itemType = hookForFakeCutItem = item.GetItemSO().nextItemType;
    }

    void Hook_SetDroppedItem(GameObject oldValue, GameObject newValue)
    {
        if (newValue != null)
        {
            Item item = newValue.GetComponent<Item>();
            fakeItemVisual.sharedMaterial = item.GetItemSO().material;
            fakeItemVisualFilter.sharedMesh = item.GetItemSO().mesh;
        }

        fakeItemVisual.enabled = (newValue == null) ? false : true;
    }

    void Hook_SetFakeCutItem(ItemSO.ItemType oldValue, ItemSO.ItemType newValue)
    {
        ItemSO itemSO = Resources.Load<ItemSO>($"Items/{newValue}");
        fakeItemVisual.sharedMaterial = itemSO.material;
        fakeItemVisualFilter.sharedMesh = itemSO.mesh;
    }
}
