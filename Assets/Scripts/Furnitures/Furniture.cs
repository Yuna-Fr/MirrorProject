using Mirror;
using UnityEngine;

public abstract class Furniture : NetworkBehaviour
{

	[SerializeField] bool hasContainer;
	[SerializeField] bool isUsable;

    virtual public void OnAction1(PlayerController player)
	{

	}

    virtual public void OnAction2(PlayerController player)
    {
        if (!isUsable)
            return;
    }
}
