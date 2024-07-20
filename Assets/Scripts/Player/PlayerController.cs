using Mirror;
using Org.BouncyCastle.Utilities.IO.Pem;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour, ICollisionHandler
{
	[SerializeField] bool ToDeleteTest = false;

	[Header("MOVEMENTS")]
    [SerializeField] float stickThreshold = 0.05f;
    [SerializeField] float walkSpeed = 6.0f;
	[SerializeField] float fallSpeed = 7.0f;
    [SerializeField] float dashSpeed = 15.0f;
    [SerializeField] float rotationSpeed = 13.0f;
    [SerializeField] float dashTime = 0.25f;
	[SerializeField] float dashReload = 0.4f;
	[SerializeField] AnimationCurve smoothDashAnimCurve;

	[Header("INTERACTIONS")]
	[SerializeField] GameObject fakeItem;
	[SerializeField] LayerMask playerLayer;
	[SerializeField] LayerMask itemLayer;

    InputSystem inputs;
    NetworkIdentity networkIdentity;
	CharacterController characterController;
	GameObject targetedItem;
	GameObject targetedFurniture;
	MeshRenderer fakeItemVisual;
	MeshFilter fakeItemVisualFilter;
	Plate fakePlate;
    Dictionary<uint, bool> onCollisionIds = new();
	Vector3 moveDirection;
	Vector2 stickVector;
	float dashThrust;
	bool wasLocalPlayer;
	bool canDash = true;
	bool isHoldingItem = false;
	bool isHoldingPlate = false;

	[SyncVar] bool isDashing = false;

    void Start()
	{
        networkIdentity = GetComponent<NetworkIdentity>();

		fakeItemVisual = fakeItem.GetComponent<MeshRenderer>();
		fakeItemVisualFilter = fakeItem.GetComponent<MeshFilter>();
		fakePlate = fakeItem.GetComponent<Plate>();

		if (!isLocalPlayer)
			return;

		wasLocalPlayer = true;
		characterController = GetComponent<CharacterController>();

		inputs = new InputSystem();
		inputs.InGame.Enable();
		inputs.InGame.Dash.performed += OnDash;
		inputs.InGame.Takedrop.performed += OnTakeDropItem;
		inputs.InGame.Interact.performed += OnInteract;
    }

	void OnDestroy()
	{
		if (!wasLocalPlayer)
			return;

		inputs.InGame.Interact.performed -= OnDash;
		inputs.InGame.Takedrop.performed -= OnTakeDropItem;
        inputs.InGame.Interact.performed -= OnInteract;
        inputs.InGame.Disable();
	}

	void Update()
	{
		if (!isLocalPlayer)
			return;

		Move();
    }

    public NetworkIdentity GetNetworkIdentity() 
    { 
        return networkIdentity; 
    }

    #region Movements

    public bool IsDashing()
    {
        return isDashing;
    }

    void Move()
    {
        stickVector = inputs.InGame.Move.ReadValue<Vector2>();

        if (Mathf.Abs(stickVector.x) < stickThreshold && Mathf.Abs(stickVector.y) < stickThreshold)
            moveDirection = Vector3.zero;
        else
            moveDirection = new(stickVector.x, 0.0f, stickVector.y);

        if (!isDashing)
            characterController.Move(moveDirection * walkSpeed * Time.deltaTime);

        if (moveDirection != Vector3.zero)
            Rotate();

        if (!characterController.isGrounded)
            ApplyGravity();
    }

    void Rotate()
    {
        transform.forward = Vector3.Slerp(transform.forward, moveDirection.normalized, rotationSpeed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        characterController.Move(-transform.up * fallSpeed * Time.deltaTime);
    }

    void OnDash(InputAction.CallbackContext context)
    {
        if (canDash)
        {
            canDash = false;
            Rpc_OnIsDashingStateChanged(true);
            StartCoroutine(Dash());
            StartCoroutine(ReloadDash());
        }
    }

    IEnumerator Dash()
    {
        float elapsedTime = 0;
        float timeRatio = 1 / dashTime;

        while (elapsedTime < dashTime)
        {
            elapsedTime += Time.deltaTime;
            dashThrust = walkSpeed + (smoothDashAnimCurve.Evaluate(elapsedTime * timeRatio) * (dashSpeed - walkSpeed));
            characterController.Move(transform.forward * dashThrust * Time.deltaTime);
            yield return null;
        }
        Rpc_OnIsDashingStateChanged(false);
    }

    IEnumerator ReloadDash()
    {
        yield return new WaitForSeconds(dashReload);
        canDash = true;
    }

    [Command]
    void Rpc_OnIsDashingStateChanged(bool isDasing)
    {
        this.isDashing = isDasing;
    }

    #endregion

    #region Network Collisions

    public Dictionary<uint, bool> GetOnCollisionIds()
    {
        return onCollisionIds;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        bool isItem = (itemLayer.value & (1 << hit.gameObject.layer)) != 0;
        bool isPlayer = (playerLayer.value & (1 << hit.gameObject.layer)) != 0;

        if (!isItem && !isPlayer)
            return;

        ICollisionHandler collisionHandler = hit.gameObject.GetComponent<ICollisionHandler>();

        float strength = isDashing ? dashThrust : walkSpeed * stickVector.magnitude;

        uint id = collisionHandler.GetNetworkIdentity().netId;

        if (!onCollisionIds.ContainsKey(id) && isPlayer)
            onCollisionIds.Add(id, false);

        if (!isPlayer || !onCollisionIds[id])
            collisionHandler.OnCollisionReaction(-hit.normal, strength, isDashing, networkIdentity);

        if (isPlayer)
            onCollisionIds[id] = true;
    }

    public void OnCollisionReaction(Vector3 direction, float strength, bool isImpulsion, NetworkIdentity savedTarget)
    {
        Cmd_OnCollisionReaction(direction, strength, isImpulsion, savedTarget);
    }

    [Command(requiresAuthority = false)]
    void Cmd_OnCollisionReaction(Vector3 direction, float strength, bool isImpulsion, NetworkIdentity savedTarget)
    {
        TargetRpc_OnCollisionReaction(direction, strength, isImpulsion, savedTarget);
    }

    [Command(requiresAuthority = false)]
    void Cmd_CollisionHasBeenHandled(NetworkIdentity savedTarget, uint id)
    {
        TargetRpc_CollisionHasBeenHandled(savedTarget.connectionToClient, savedTarget, id);
    }

    [TargetRpc]
    void TargetRpc_OnCollisionReaction(Vector3 direction, float strength, bool isImpulsion, NetworkIdentity savedTarget)
    {
        if (isImpulsion)
            return;

        characterController.Move(direction * strength * Time.deltaTime);
        Cmd_CollisionHasBeenHandled(savedTarget, networkIdentity.netId);
    }

    [TargetRpc]
    void TargetRpc_CollisionHasBeenHandled(NetworkConnection connection, NetworkIdentity savedTarget, uint id)
    {
        savedTarget.GetComponent<PlayerController>().GetOnCollisionIds()[id] = false;
    }

    #endregion

    #region Interactions

    public void SetNewTargetedItem(GameObject targetedItem)
	{
		this.targetedItem = targetedItem;
	}

	public void SetNewTargetedFurnitures(GameObject targetedFurniture)
	{
		this.targetedFurniture = targetedFurniture;
	}

    public bool IsHoldingItem()
	{
		return isHoldingItem;
	}

	public bool IsHoldingPlate()
	{
		return isHoldingPlate;
	}

	public Plate GetFakePlate()
	{
		return fakePlate;
	}

	void OnTakeDropItem(InputAction.CallbackContext context)
	{

	}

    void OnInteract(InputAction.CallbackContext context)
	{
		
	}

    #endregion
}
