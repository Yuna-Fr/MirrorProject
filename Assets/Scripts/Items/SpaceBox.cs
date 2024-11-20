using Mirror;
using UnityEngine;

public class SpaceBox : NetworkBehaviour, ICollisionHandler
{
	[Header("NET COLLISIONS")]
	[SerializeField] float dashCollisionNerf = 0.1f;
	[SerializeField] float rigidbodyMass = 0.5f;

    Rigidbody rigidBody;
	NetworkIdentity networkIdentdity;

	private void Start()
	{
		networkIdentdity = GetComponent<NetworkIdentity>();

		if (isServer)
		{
            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            rigidBody.mass = rigidbodyMass;
        }
	}

    public NetworkIdentity GetNetworkIdentity()
	{
		return networkIdentdity;
	}

    #region Network collisions

    public void OnCollisionReaction(Vector3 direction, float strength, bool isImpulsion, NetworkIdentity savedTarget)
	{
		Cmd_OnCollisionReaction(direction, strength, isImpulsion, savedTarget);
    }

	[Command(requiresAuthority = false)]
	void Cmd_OnCollisionReaction(Vector3 direction, float strength, bool isImpulsion, NetworkIdentity savedTarget)
	{
        ForceMode forceMode = isImpulsion ? ForceMode.Impulse : ForceMode.Force;

        direction = (direction.y < 0) ? new Vector3(direction.x, 0, direction.z) : direction;

		strength = isImpulsion ? strength * dashCollisionNerf : strength;

		rigidBody.AddForce(direction * strength * (isImpulsion ? 1.0f : 2.0f), forceMode);

		TargetRpc_CollisionHasBeenHandled(savedTarget.connectionToClient, savedTarget, networkIdentdity.netId, isImpulsion);
    }

	[TargetRpc]
	void TargetRpc_CollisionHasBeenHandled(NetworkConnection networkConnection, NetworkIdentity savedTarget, uint id, bool wasImpulsion)
	{
		if (wasImpulsion)
		{
            savedTarget.GetComponent<PlayerController>().GetOnDashCollisionIds()[id] = false;
            return;
		}

        savedTarget.GetComponent<PlayerController>().GetOnCollisionIds()[id]--;
    }

    #endregion
}
