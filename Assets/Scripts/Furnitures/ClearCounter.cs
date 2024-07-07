using Mirror;
using UnityEngine;

public class ClearCounter : Furniture
{
    [SerializeField] Transform itemContainer;

    [SyncVar (hook =nameof(Hook_SetItem)), HideInInspector] public GameObject droppedItem;

    public override void OnAction1()
    {
        base.OnAction1();
    }

    public override void OnAction2()
    {
        base.OnAction2();
    }

    public void Hook_SetItem(GameObject oldValue, GameObject newValue)
    {

    }
}