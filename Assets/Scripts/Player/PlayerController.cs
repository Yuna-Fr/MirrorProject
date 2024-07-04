using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
	[Header("MOVEMENTS")]
	[SerializeField] float speed = 5.0f;
	[SerializeField] float stickThreshold = 0.05f;
    [SerializeField] float dashLength = 2.5f;
	[SerializeField] float dashReload = 0.3f;
	[SerializeField] float dashSpeed = 50.0f;
    [SerializeField] float rotationSpeed = 15.0f;

	[Header("INTERACTIONS")]
	[SerializeField] Transform holdPosition;

    InputSystem inputs;
    CharacterController characterController;
    GameObject targetedItem;
	GameObject targetedFurniture;
    Vector3 moveDirection;
    Vector2 stickVector;
    bool wasLocalPlayer;
    bool canDash = true;
	bool isDashing = false;
	bool isHoldingItem = false;

	void Start()
	{
        if (!isLocalPlayer)
		{
				
        }

		wasLocalPlayer = true;
        characterController = GetComponent<CharacterController>();

		inputs = new InputSystem();
		inputs.InGame.Enable();
		inputs.InGame.Dash.performed += OnDash;
	}

	void OnDestroy()
	{
		if (!wasLocalPlayer)
			return;

        inputs.InGame.Interact.performed -= OnDash;
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
    }

	void Rotate()
	{
		transform.forward = Vector3.Slerp(transform.forward, moveDirection.normalized, rotationSpeed * Time.deltaTime);
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
}
