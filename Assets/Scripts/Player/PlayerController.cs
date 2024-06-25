using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	public class PlayerController : MonoBehaviour
	{
		[Header("REFERENCES")]
		[SerializeField] CharacterController controller;

		[Header("SETTINGS")]
		[SerializeField] float moveSpeed = 10f;

		InputSystem inputs;
		Vector3 moveDirection;

		void OnEnable()
		{
			inputs = new InputSystem();

			inputs.InGame.Move.Enable();

			inputs.InGame.Interact.Enable();
			inputs.InGame.Interact.performed += OnInteract;
		}

		void OnDisable()
		{
			inputs.InGame.Move.Disable();

			inputs.InGame.Interact.performed += OnInteract;
			inputs.InGame.Interact.Disable();
		}

		void FixedUpdate()
		{
			moveDirection = new(inputs.InGame.Move.ReadValue<Vector2>().x, 0.0f, inputs.InGame.Move.ReadValue<Vector2>().y);
			controller.Move(moveDirection.normalized * (moveSpeed * Time.deltaTime) + new Vector3(0.0f, 0f, 0f) * Time.deltaTime);
		}

		void OnInteract(InputAction.CallbackContext context)
		{
			Debug.Log("Interact !");
		}
	}
}