using Mirror;
using UnityEngine;

public class ClearCounter : Furniture
{
    [SerializeField] Transform itemContainer;
    bool isFilled = false;

    [SyncVar (hook =nameof(Hook_SetItem)), HideInInspector] public GameObject droppedItem;

    public override void OnAction1(PlayerController player)
    {
        base.OnAction1(player);


    }

    public override void OnAction2(PlayerController player)
    {
        base.OnAction2(player);
    }

    public void Hook_SetItem(GameObject oldValue, GameObject newValue)
    {

    }
}