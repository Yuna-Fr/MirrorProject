using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pantry : Furniture
{
    [SerializeField] ItemSO.ItemType givenItem;

    public override void OnAction1(PlayerController player)
    {
        base.OnAction1(player);
    }

    public override void OnAction2(PlayerController player)
    {
        base.OnAction2(player);
    }

    public ItemSO.ItemType GetGivenItem()
    {
        return givenItem;
    }
}
