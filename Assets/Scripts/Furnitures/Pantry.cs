using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pantry : Furniture
{
    [SerializeField] ItemSO.ItemType givenItem;

    public override void OnAction1()
    {
        base.OnAction1();
    }

    public override void OnAction2()
    {
        base.OnAction2();
    }

    public ItemSO.ItemType GetGivenItem()
    {
        return givenItem;
    }
}
