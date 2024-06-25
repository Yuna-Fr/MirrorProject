using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	public class PlayerController : MonoBehaviour
	{
		[Header("SETTINGS")]
		[SerializeField] float speed = 10.0f;
		[SerializeField] float stickThreshold = 0.05f;
		[SerializeField] float dashLength = 3.0f;
		[SerializeField] float dashReload = 0.5f;

		InputSystem inputs;
        CharacterController characterController;
        Vector2 stickVector;
		Vector3 moveDirection;
		bool canDash = true;
		bool isDashing = false;

		void Awake()
		{
			characterController = GetComponent<CharacterController>();

			inputs = new InputSystem();
			inputs.InGame.Enable();
			inputs.InGame.Dash.performed += OnDash;
		}

		void OnDestroy()
		{
			inputs.InGame.Interact.performed -= OnDash;
			inputs.InGame.Disable();
		}

		void Update()
		{
            if (!isDashing)
            {
                stickVector = inputs.InGame.Move.ReadValue<Vector2>();

                if (Mathf.Abs(stickVector.x) < stickThreshold && Mathf.Abs(stickVector.y) < stickThreshold) //To avoid joystick drift
                    moveDirection = Vector3.zero;
                else
                    moveDirection = new(stickVector.x, 0.0f, stickVector.y);

                characterController.Move(moveDirection * speed * Time.deltaTime);
            }
		}

		void OnDash(InputAction.CallbackContext context)
		{
			if (canDash)
			{
				canDash = false;
				isDashing = true;
				Vector3 destination = transform.position + (moveDirection.normalized * dashLength);
				StartCoroutine(Dash(destination));
				StartCoroutine(ReloadDash());
			}
		}

		IEnumerator Dash(Vector3 destination)
		{
            while ((transform.position - destination).magnitude >= 0.001f)
			{
                transform.position = Vector3.Lerp(transform.position, destination, 0.1f);
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
}