using Mirror;
using System;
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
    Vector3 moveDirection;
    Vector2 stickVector;
    bool wasLocalPlayer;
	bool onGround = true;
    bool canDash = true;
	bool isDashing = false;
	bool isHoldingItem = false;

	[SyncVar(hook = nameof(Hook_TakeDropItem))] GameObject takenItem;

	void Start()
	{
		fakeItemVisual = fakeItem.GetComponent<MeshRenderer>();
        fakeItemVisualFilter = fakeItem.GetComponent<MeshFilter>();

        if (!isLocalPlayer)
			return;

		wasLocalPlayer = true;
        characterController = GetComponent<CharacterController>();

		inputs = new InputSystem();
		inputs.InGame.Enable();
		inputs.InGame.Dash.performed += OnDash;
		inputs.InGame.Takedrop.performed += TakeDropItem;
	}

	void OnDestroy()
	{
		if (!wasLocalPlayer)
			return;

        inputs.InGame.Interact.performed -= OnDash;
		inputs.InGame.Takedrop.performed -= TakeDropItem;
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

	void TakeDropItem(InputAction.CallbackContext context)
	{
		if (!isHoldingItem)
			TakeItem();
		else
			DropItem();
	}

	void TakeItem()
	{
		if (targetedFurniture == null && targetedItem == null)
			return;

		if (targetedItem != null)
		{
            isHoldingItem = true;
			RPC_TakeDropItem(targetedItem);
			if(targetedFurniture == null)
            return;
		}
	}

	void DropItem()
	{
        if (targetedFurniture == null)
		{
            isHoldingItem = false;
            RPC_TakeDropItem(null);
        }
		else
		{
			Furniture furniture = targetedFurniture.GetComponent<Furniture>();
			if (furniture && furniture.IsAvailable())
				furniture.GetItemContainer();
		}
	}

    [Command] void RPC_TakeDropItem(GameObject item)
	{
        if (item == null)
            takenItem.GetComponent<NetworkTransformUnreliable>().RpcTeleport(fakeItem.transform.position, fakeItem.transform.rotation);

        if (item != null) item.GetComponent<Item>().isTaken = true;
        else if (takenItem != null) takenItem.GetComponent<Item>().isTaken = false;

        takenItem = item;
    }

	void Hook_TakeDropItem(GameObject oldValue, GameObject newValue)
	{
        if (newValue != null)
		{
			Item item = newValue.GetComponent<Item>();
			fakeItemVisual.sharedMaterial = item.GetItemSO().material;
			fakeItemVisualFilter.sharedMesh = item.GetItemSO().mesh;
		}

        fakeItemVisual.enabled = (newValue == null) ? false : true;
	}

}
