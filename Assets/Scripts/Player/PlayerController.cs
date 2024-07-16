using Mirror;
using Org.BouncyCastle.Utilities.IO.Pem;
using System.Collections;
using Unity.VisualScripting;
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
	[SerializeField] AnimationCurve smoothAnimCurve;

	[Header("INTERACTIONS")]
	[SerializeField] GameObject fakeItem;

	InputSystem inputs;
	Rigidbody rigidBody;
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

	void Start()
	{
		fakeItemVisual = fakeItem.GetComponent<MeshRenderer>();
		fakeItemVisualFilter = fakeItem.GetComponent<MeshFilter>();
		fakePlate = fakeItem.GetComponent<Plate>();

		if (!isLocalPlayer)
			return;
        

        wasLocalPlayer = true;
		rigidBody = GetComponent<Rigidbody>();

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

	void FixedUpdate()
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

	public Plate GetFakePlate()
	{
		return fakePlate;
	}

	
	void Move()
	{
		stickVector = inputs.InGame.Move.ReadValue<Vector2>();
		bool isMoving = !(Mathf.Abs(stickVector.x) < stickThreshold && Mathf.Abs(stickVector.y) < stickThreshold);


        if (isMoving)
		{
            moveDirection = new(stickVector.x, 0.0f, stickVector.y);

			rigidBody.AddForce(moveDirection * 20, ForceMode.Force);

			Vector3 planSpeed = new Vector3 (rigidBody.velocity.x, 0, rigidBody.velocity.z);

			if (planSpeed.magnitude > 4)
			{
				planSpeed = planSpeed.normalized * 4;
				rigidBody.velocity = new Vector3(planSpeed.x, rigidBody.velocity.y, planSpeed.z);
			}
        }
		else
		{
            moveDirection = Vector3.zero;

			rigidBody.velocity = new Vector3(rigidBody.velocity.x * 0.7f, rigidBody.velocity.y, rigidBody.velocity.z * 0.7f);
        }

        if (moveDirection != Vector3.zero)
			Rotate();

	}

	void Rotate()
	{
		transform.forward = Vector3.Slerp(transform.forward, moveDirection.normalized, rotationSpeed * Time.deltaTime);
	}

	void OnDash(InputAction.CallbackContext context)
	{
		if (!isDashing && canDash)
		{
            isDashing = true;
			canDash = false;
            rigidBody.AddForce(transform.forward*10, ForceMode.Impulse);
			StartCoroutine(ReloadDash());
			StartCoroutine(ReloadIsDashing());
        }
	}

	IEnumerator Dash(Vector3 destination)
	{
        int steps = Mathf.RoundToInt(0.3f / Time.deltaTime);

        for (int i = 0; i < steps; i++)
        {
            float timeRatio = (float)i / steps;
            float dashFactor = smoothAnimCurve.Evaluate(timeRatio);
            transform.position = Vector3.Lerp(transform.position, destination, dashFactor);
            yield return new WaitForSeconds(Time.deltaTime);
        }

        /*float elapsedTime = 0.0f;

		while ((transform.position - destination).magnitude >= 0.05f)
		{

			elapsedTime += Time.deltaTime;
			float timeRatio = elapsedTime / 2f;
			float dashFactor = smoothAnimCurve.Evaluate(timeRatio);

            //transform.position = Vector3.Lerp(transform.position, destination, dashSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, destination, dashFactor);
            yield return new WaitForSeconds(Time.deltaTime);
		}*/

        isDashing = false;
		yield return null;
	}

	IEnumerator ReloadDash()
	{
		yield return new WaitForSeconds(dashReload);
		canDash = true;
	}

    IEnumerator ReloadIsDashing()
    {
        yield return new WaitForSeconds(0.1f);
        isDashing = false;
    }

    void OnTakeDropItem(InputAction.CallbackContext context)
	{

	}

    void OnInteract(InputAction.CallbackContext context)
	{

	}
}
