using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pantry : Furniture
{
    [SerializeField] ItemSO.ItemType givenItem;
    [SerializeField] GameObject itemPrefab;

    public override void OnAction1(PlayerController player)
    {
        base.OnAction1(player);

        if (player.IsHoldingItem())
            return;

        RPC_SpawnAndGiveItem(player);
        player.SetIsHoldingItem(true);
    }

    public override void OnAction2(PlayerController player)
    {
        base.OnAction2(player);
    }

    [Command(requiresAuthority = false)] void RPC_SpawnAndGiveItem(PlayerController player)
    {
        GameObject item = Instantiate(itemPrefab, new Vector3(0.0f, -5.0f, -0.0f), new Quaternion());
        Item itemComp = item.GetComponent<Item>();
        itemComp.itemType = givenItem;
        itemComp.isTaken = true;
        NetworkServer.Spawn(item);
        Debug.Log("prout", player.gameObject);
        player.TakeItemFromPantry(item);
    }
}
