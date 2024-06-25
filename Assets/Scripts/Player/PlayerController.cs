using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	public class PlayerController : MonoBehaviour
	{
		[Header("REFERENCES")]
		[SerializeField] CharacterController controller;

		[Header("SETTINGS")]
		[SerializeField] float speed = 10f;
		[SerializeField] float stickThreshold = 0.05f;

		InputSystem inputs;
		Vector2 moveInput;
		Vector3 moveDirection;

		void Awake()
		{
			inputs = new InputSystem();

			inputs.InGame.Enable();
			inputs.InGame.Interact.performed += OnInteract;
		}

		void OnDestroy()
		{
			inputs.InGame.Interact.performed -= OnInteract;
			inputs.InGame.Disable();
		}

		void FixedUpdate()
		{
			moveInput = inputs.InGame.Move.ReadValue<Vector2>();
			
			if (Mathf.Abs(moveInput.x) < stickThreshold && Mathf.Abs(moveInput.y) < stickThreshold) //To avoid joystick drift
				moveDirection = Vector3.zero;
			else
				moveDirection = new(moveInput.x, 0.0f, moveInput.y);

			controller.Move(moveDirection.normalized * (speed * Time.deltaTime));
		}

		void OnInteract(InputAction.CallbackContext context)
		{
			Debug.Log("Interact !");
		}
	}
}