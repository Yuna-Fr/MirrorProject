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
		characterController.Move(-transform.up * fallSpeed * Time.deltaTime);
	}

	void OnDash(InputAction.CallbackContext context)
	{
		if (canDash)
		{
			canDash = false;
			isDashing = true;
			Vector3 destination = transform.position + (transform.forward * dashLength);
            RaycastHit hitInfo;

			if (Physics.CapsuleCast(transform.position, transform.position + transform.up*characterController.height, characterController.radius, transform.forward, out hitInfo, dashLength))
			{
				Vector3 normal = hitInfo.normal;
				Vector3 point = hitInfo.point;

				Vector3 surfaceVec = Vector3.Cross(normal, Vector3.up).normalized;

				if ((surfaceVec.x < 0 && (Mathf.Abs(surfaceVec.x) > Mathf.Abs(surfaceVec.z))) || (surfaceVec.z < 0 && (Mathf.Abs(surfaceVec.z) > Mathf.Abs(surfaceVec.x))))
					surfaceVec *= -1.0f;

				Vector3 remainingForce = destination - point;

				Vector3 projection = Vector3.Project(remainingForce, surfaceVec);

				Vector3 radiusOffset = normal * characterController.radius;

                Vector3 finalPoint = point + projection + radiusOffset;

				if (Physics.Raycast(point + radiusOffset, projection, out hitInfo, projection.magnitude + characterController.radius))
				{
					finalPoint = hitInfo.point + (hitInfo.normal * characterController.radius);
				}

                destination = new Vector3(finalPoint.x, transform.position.y, finalPoint.z);
			}

			StartCoroutine(Dash(destination));
			StartCoroutine(ReloadDash());
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

	void OnTakeDropItem(InputAction.CallbackContext context)
	{

	}

    void OnInteract(InputAction.CallbackContext context)
	{

	}
}
