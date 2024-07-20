using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionHandler
{
    public NetworkIdentity GetNetworkIdentity();
    public void OnCollisionReaction(Vector3 direction, float strength, bool isImpulsion, NetworkIdentity savedTarget);
}
