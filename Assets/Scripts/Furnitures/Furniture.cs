using Mirror;
using UnityEngine;

public abstract class Furniture : NetworkBehaviour
{

	[SerializeField] bool hasContainer;
	[SerializeField] bool isUsable;

    virtual public void OnAction1()
	{

	}

    virtual public void OnAction2()
    {
        if (!isUsable)
            return;
    }
}
