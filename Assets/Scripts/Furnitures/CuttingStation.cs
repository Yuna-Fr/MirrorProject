using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class CuttingStation : Furniture
{
    [SerializeField] Transform itemContainer;
    [SerializeField] MeshRenderer fakeItemVisual;
    [SerializeField] MeshFilter fakeItemVisualFilter;
    [SerializeField] List<ItemSO.ItemType> cutablesItems;

    
}
