using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	public class PlayerController : NetworkBehaviour
    {
		[Header("SETTINGS")]
		[SerializeField] float speed = 5.0f;
		[SerializeField] float stickThreshold = 0.05f;
        [SerializeField] float dashLength = 2.5f;
		[SerializeField] float dashReload = 0.3f;
		[SerializeField] float dashSpeed = 50.0f;
        [SerializeField] float rotationSpeed = 15.0f;

        InputSystem inputs;
        CharacterController characterController;
        Vector2 stickVector;
		Vector3 moveDirection;
		Quaternion moveRotation;
		bool canDash = true;
		bool isDashing = false;

		void Start()
		{
            if (!isLocalPlayer)
                return;

            characterController = GetComponent<CharacterController>();

			inputs = new InputSystem();
			inputs.InGame.Enable();
			inputs.InGame.Dash.performed += OnDash;
		}

		void OnDestroy()
		{
            if (!isLocalPlayer)
                return;

            inputs.InGame.Interact.performed -= OnDash;
			inputs.InGame.Disable();
		}

		void Update()
		{
            if (!isDashing)
            {
                stickVector = inputs.InGame.Move.ReadValue<Vector2>();

                if (Mathf.Abs(stickVector.x) < stickThreshold && Mathf.Abs(stickVector.y) < stickThreshold)
                    moveDirection = Vector3.zero;
                else
                    moveDirection = new(stickVector.x, 0.0f, stickVector.y);

                characterController.Move(moveDirection * speed * Time.deltaTime);

				if (moveDirection != Vector3.zero)
				{
                    moveRotation = Quaternion.LookRotation(moveDirection.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, moveRotation, rotationSpeed * Time.deltaTime);
                }
            }
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
				transform.position = Vector3.Lerp(transform.position, destination, dashSpeed * Time.deltaTime) ;
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