using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{

	public class PlayerController : MonoBehaviour
	{
		[SerializeField] InputSystem inputs;
		[SerializeField] CharacterController controller;
		[SerializeField] float moveSpeed = 10f;

		private void OnEnable()
		{
			inputs = new InputSystem();

			inputs.InGame.Move.Enable();
			inputs.InGame.Interact.Enable();
		}

		private void OnDisable()
		{
			inputs.InGame.Move.Disable();
			inputs.InGame.Interact.Disable();
		}

		void OnMove(InputAction.CallbackContext context)
		{
			Vector3 moveDirection = new(inputs.InGame.Move.ReadValue<Vector2>().x, 0.0f, inputs.InGame.Move.ReadValue<Vector2>().y);
			controller.Move(moveDirection.normalized * (moveSpeed * Time.deltaTime) + new Vector3(0.0f, 0f, 0f) * Time.deltaTime);
		}
	}
}