using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
	[Header("MOVEMENTS")]
	[SerializeField] float speed = 5.0f;
	[SerializeField] float fallSpeed = 5.0f;
	[SerializeField] float stickThreshold = 0.05f;
	[SerializeField] float dashLength = 2.5f;
	[SerializeField] float dashReload = 0.3f;
	[SerializeField] float dashSpeed = 50.0f;
	[SerializeField] float rotationSpeed = 15.0f;

	[Header("INTERACTIONS")]
	[SerializeField] GameObject fakeItem;

	InputSystem inputs;
	CharacterController characterController;
	GameObject targetedItem;
	GameObject targetedFurniture;
	MeshRenderer fakeItemVisual;
	MeshFilter fakeItemVisualFilter;
	Plate fakePlate;
	Vector3 moveDirection;
	Vector2 stickVector;
	bool wasLocalPlayer;
	bool onGround = true;
	bool canDash = true;
	bool isDashing = false;
	bool isHoldingItem = false;
	bool isHoldingPlate = false;

	[SyncVar(hook = nameof(Hook_TakeDropItem)), HideInInspector] public GameObject takenItem;

	void Start()
	{
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

		if (!isDashing)
			Move();
	}

	public void SetNewTargetedItem(GameObject targetedItem)
	{
		this.targetedItem = targetedItem;
	}

	public void SetNewTargetedFurnitures(GameObject targetedFurniture)
	{
		this.targetedFurniture = targetedFurniture;
	}

	public void SetGroundedState(bool isGrounded)
	{
		onGround = isGrounded;
	}

	public bool IsHoldingItem()
	{
		return isHoldingItem;
	}

	public bool IsHoldingPlate()
	{
		return isHoldingPlate;
	}

	public void SetIsHoldingItem(bool isHoldingItem)
	{
		this.isHoldingItem = isHoldingItem;
	}

	public void SetIsHoldingPlate(bool isHoldingPlate)
	{
		this.isHoldingPlate = isHoldingPlate;
	}

	public void TakeDropItemFromClearCounter(GameObject droppedItem)
	{
		if (droppedItem == null)
			isHoldingItem = isHoldingPlate = false;
		else
		{
			isHoldingItem = true;
			isHoldingPlate = (droppedItem.GetComponent<Item>().GetItemSO().itemType == ItemSO.ItemType.Plate);
		}

		RPC_TakeDropItem(droppedItem, true);
	}

	public void TakeItemFromPantry(GameObject pantryItem)
	{
		takenItem = pantryItem;
	}

	public GameObject DropItemOnDeliveryTable()
	{
		GameObject item = takenItem;
		RemoveTakeItem();
		return item;
	}

	public Plate GetFakePlate()
	{
		return fakePlate;
	}

	#region Movements
	void Move()
	{
		stickVector = inputs.InGame.Move.ReadValue<Vector2>();

		if (Mathf.Abs(stickVector.x) < stickThreshold && Mathf.Abs(stickVector.y) < stickThreshold)
			moveDirection = Vector3.zero;
		else
			moveDirection = new(stickVector.x, 0.0f, stickVector.y);

		characterController.Move(moveDirection * speed * Time.deltaTime);

		if (moveDirection != Vector3.zero)
			Rotate();

		if (!onGround)
			ApplyGravity();
	}

	void Rotate()
	{
		transform.forward = Vector3.Slerp(transform.forward, moveDirection.normalized, rotationSpeed * Time.deltaTime);
	}

	void ApplyGravity()
	{
		characterController.Move(transform.up * (-1.0f) * fallSpeed * Time.deltaTime);
	}

	void OnDash(InputAction.CallbackContext context)
	{
		if (canDash)
		{
			canDash = false;
			isDashing = true;
			Vector3 destination = transform.position + (transform.forward * dashLength);
			StartCoroutine(Dash(destination));
			StartCoroutine(ReloadDash());
		}
	}

	IEnumerator Dash(Vector3 destination)
	{
		while ((transform.position - destination).magnitude >= 0.1f)
		{
			transform.position = Vector3.Lerp(transform.position, destination, dashSpeed * Time.deltaTime);
			yield return new WaitForSeconds(Time.deltaTime);
		}

		isDashing = false;
		yield return null;
	}

	IEnumerator ReloadDash()
	{
		yield return new WaitForSeconds(dashReload);
		canDash = true;
	}
	#endregion

	#region ItemGestion
	void OnTakeDropItem(InputAction.CallbackContext context)
	{
		if (!isHoldingItem)
			TakeItem();
		else if (isHoldingItem && !isHoldingPlate)
			DropItem();
		else if (isHoldingItem && isHoldingPlate)
			TakeDropWithPlate();
	}

	void OnInteract(InputAction.CallbackContext context)
	{
		if (!isHoldingItem && targetedFurniture != null)
			targetedFurniture.GetComponent<Furniture>().OnAction2(this);
	}

	void TakeItem()
	{
		if (targetedFurniture == null && targetedItem == null)
			return;

		if (targetedItem != null)
		{
			isHoldingItem = true;
			isHoldingPlate = (targetedItem.GetComponent<Item>().GetItemSO().itemType == ItemSO.ItemType.Plate);
			RPC_TakeDropItem(targetedItem, false);
			if (targetedFurniture == null)
				return;
		}

		if (targetedFurniture != null)
			targetedFurniture.GetComponent<Furniture>().OnAction1(this);
	}

	void DropItem()
	{
		if (targetedFurniture == null)
		{
			isHoldingItem = false;
			isHoldingPlate = false;
			RPC_TakeDropItem(null, false);
		}
		else
			targetedFurniture.GetComponent<Furniture>().OnAction1(this);
	}

	void RemoveTakeItem()
	{
		takenItem = null;
	}

	void TakeDropWithPlate()
	{
		if (targetedItem != null && targetedItem.GetComponent<Item>().GetItemSO().isComestible)
		{
			// Plate Logic
			return;
		}
		else
			DropItem();
	}

	[Command]
	void RPC_TakeDropItem(GameObject item, bool isFromFurniture)
	{
		if (item == null && !isFromFurniture)
			takenItem.GetComponent<NetworkTransformUnreliable>().RpcTeleport(fakeItem.transform.position, fakeItem.transform.rotation);

		if (item != null) item.GetComponent<Item>().isTaken = true;
		else if (takenItem != null && !isFromFurniture) takenItem.GetComponent<Item>().isTaken = false;

		takenItem = item;
	}

	void Hook_TakeDropItem(GameObject oldValue, GameObject newValue)
	{
		if (newValue != null)
		{
			Item item = newValue.GetComponent<Item>();
			fakeItemVisual.sharedMaterial = item.GetItemSO().material;
			fakeItemVisualFilter.sharedMesh = item.GetItemSO().mesh;

			if (item.itemType == ItemSO.ItemType.Plate)
				fakePlate.SetItemsVisuals(item.plateScript.GetItemsList());
		}

		if (oldValue != null && oldValue.GetComponent<Item>().itemType == ItemSO.ItemType.Plate)
			fakePlate.ResetVisuals();

		fakeItemVisual.enabled = (newValue == null) ? false : true;
	}
	#endregion
}
