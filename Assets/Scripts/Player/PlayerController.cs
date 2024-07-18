using Mirror;
using Org.BouncyCastle.Utilities.IO.Pem;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
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
	bool isHoldingItem = false;
	bool isHoldingPlate = false;

	[SyncVar] bool isDashing = false;

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

    public bool IsDashing()
    {
        return isDashing;
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

		if (!onGround)
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
			OnIsDashingStateChanged(true);
			StartCoroutine(Dash());
			StartCoroutine(ReloadDash());
		}
	}

	IEnumerator Dash()
	{
		float elapsedTime = 0;
		float timeRatio = 1 / dashTime;
		float speedRatio;

        while (elapsedTime < dashTime)
        {
            elapsedTime += Time.deltaTime;
			speedRatio = walkSpeed + (smoothDashAnimCurve.Evaluate(elapsedTime * timeRatio) * (dashSpeed - walkSpeed));
            characterController.Move(transform.forward * speedRatio * Time.deltaTime);
			yield return null;
        }
        OnIsDashingStateChanged(false);
    }

    IEnumerator ReloadDash()
	{
		yield return new WaitForSeconds(dashReload);
		canDash = true;
	}

	void OnTakeDropItem(InputAction.CallbackContext context)
	{

	}

    void OnInteract(InputAction.CallbackContext context)
	{
		
	}

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
		bool isItem = (itemLayer.value & (1 << hit.gameObject.layer)) != 0;
		bool isPlayer = (playerLayer.value & (1 << hit.gameObject.layer)) != 0;

		if (!isItem && !isPlayer)
			return;

        if (isPlayer)
		{
			hit.transform.position -= (hit.normal * Time.deltaTime * walkSpeed);
		}

    }

    [Command] void OnIsDashingStateChanged(bool isDasing)
	{
		this.isDashing = isDasing;
	}
}
