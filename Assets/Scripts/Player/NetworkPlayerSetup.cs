using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerSetup : NetworkBehaviour
{

    [SerializeField] List<MonoBehaviour> componentsToDisable = new();

    void Start()
    {
        if (!isLocalPlayer)
            foreach (MonoBehaviour component in  componentsToDisable)
                component.enabled = false;
    }
}
