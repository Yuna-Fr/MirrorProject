using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour, ICollisionHandler
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

    [Header("NET COLLISONS")]
    [SerializeField] float onlineCollisionBooster = 2.0f;
    [SerializeField] int allowedCallsWaitAnswer = 2;

    InputSystem inputs;
    NetworkIdentity networkIdentity;
    CharacterController characterController;
    GameObject targetedItem;
    GameObject targetedFurniture;
    MeshRenderer fakeItemVisual;
    MeshFilter fakeItemVisualFilter;
    Plate fakePlate;
    Dictionary<uint, int> onCollisionIds = new();
    Dictionary<uint, bool> onDashCollisionIds = new();
    Vector3 moveDirection;
    Vector2 stickVector;
    float dashThrust;
    bool wasLocalPlayer;
    bool canDash = true;
    bool isHoldingItem = false;
    bool isHoldingPlate = false;

    [SyncVar] bool isDashing = false;

    void Start()
    {
        networkIdentity = GetComponent<NetworkIdentity>();

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

    public NetworkIdentity GetNetworkIdentity()
    {
        return networkIdentity;
    }

    #region Movements

    public bool IsDashing()
    {
        return isDashing;
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

        if (!characterController.isGrounded)
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
            Rpc_OnIsDashingStateChanged(true);
            StartCoroutine(Dash());
            StartCoroutine(ReloadDash());
        }
    }

    IEnumerator Dash()
    {
        float elapsedTime = 0;
        float timeRatio = 1 / dashTime;

        while (elapsedTime < dashTime)
        {
            elapsedTime += Time.deltaTime;
            dashThrust = walkSpeed + (smoothDashAnimCurve.Evaluate(elapsedTime * timeRatio) * (dashSpeed - walkSpeed));
            characterController.Move(transform.forward * dashThrust * Time.deltaTime);
            yield return null;
        }
        Rpc_OnIsDashingStateChanged(false);
    }

    IEnumerator ReloadDash()
    {
        yield return new WaitForSeconds(dashReload);
        canDash = true;
    }

    [Command]
    void Rpc_OnIsDashingStateChanged(bool isDasing)
    {
        this.isDashing = isDasing;
    }

    #endregion

    #region Network collisions

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        bool isItem = (itemLayer.value & (1 << hit.gameObject.layer)) != 0;
        bool isPlayer = (playerLayer.value & (1 << hit.gameObject.layer)) != 0;

        if (!isItem && !isPlayer)
            return;

        ICollisionHandler collisionHandler = hit.gameObject.GetComponentInParent<ICollisionHandler>();

        float strength = isDashing ? dashThrust : walkSpeed * stickVector.magnitude * onlineCollisionBooster;

        float collisionAngle = isDashing ? Mathf.Acos(Vector3.Dot(-hit.normal, hit.moveDirection)) * Mathf.Rad2Deg : 0;

        uint id = collisionHandler.GetNetworkIdentity().netId;

        bool isImpulsion = isDashing && collisionAngle < 45.0f;

        if (isImpulsion)
        {
            if (!onDashCollisionIds.ContainsKey(id))
                onDashCollisionIds.Add(id, false);

            if (!onDashCollisionIds[id])
            {
                collisionHandler.OnCollisionReaction(-hit.normal, strength, isImpulsion, networkIdentity);
                onDashCollisionIds[id] = true;
            }

        }
        else
        {
            if (!onCollisionIds.ContainsKey(id))
                onCollisionIds.Add(id, 0);

            if (onCollisionIds[id] < allowedCallsWaitAnswer)
            {
                collisionHandler.OnCollisionReaction(-hit.normal, strength, isImpulsion, networkIdentity);
                onCollisionIds[id] ++;
            }

        }
    }

    public Dictionary<uint, int> GetOnCollisionIds()
    {
        return onCollisionIds;
    }

    public Dictionary<uint, bool> GetOnDashCollisionIds()
    {
        return onDashCollisionIds;
    }

    public void OnCollisionReaction(Vector3 direction, float strength, bool isImpulsion, NetworkIdentity savedTarget)
    {
        Cmd_OnCollisionReaction(direction, strength, isImpulsion, savedTarget);
    }

    IEnumerator OnDashCollide(Vector3 direction, float strength, NetworkIdentity savedTarget, uint id)
    {
        float impulsion;
        float elapsedTime = 0;
        float timeRatio = 1 / dashTime;

        while (elapsedTime < dashTime)
        {
            elapsedTime += Time.deltaTime;
            impulsion = smoothDashAnimCurve.Evaluate(elapsedTime * timeRatio) * strength;
            characterController.Move(direction * impulsion * Time.deltaTime);
            yield return null;
        }
        Cmd_CollisionHasBeenHandled(savedTarget, id, true);
    }

    [Command(requiresAuthority = false)]
    void Cmd_OnCollisionReaction(Vector3 direction, float strength, bool isImpulsion, NetworkIdentity savedTarget)
    {
        TargetRpc_OnCollisionReaction(direction, strength, isImpulsion, savedTarget);
    }

    [Command(requiresAuthority = false)]
    void Cmd_CollisionHasBeenHandled(NetworkIdentity savedTarget, uint id, bool wasImpulsion)
    {
        TargetRpc_CollisionHasBeenHandled(savedTarget.connectionToClient, savedTarget, id, wasImpulsion);
    }

    [TargetRpc]
    void TargetRpc_OnCollisionReaction(Vector3 direction, float strength, bool isImpulsion, NetworkIdentity savedTarget)
    {
        if (isImpulsion)
        {
            StartCoroutine(OnDashCollide(direction, strength, savedTarget, networkIdentity.netId));
            return;
        }

        characterController.Move(direction * strength * Time.deltaTime);
        Cmd_CollisionHasBeenHandled(savedTarget, networkIdentity.netId, isImpulsion);
    }

    [TargetRpc]
    void TargetRpc_CollisionHasBeenHandled(NetworkConnection connection, NetworkIdentity savedTarget, uint id, bool wasImpulsion)
    {
        if (wasImpulsion)
        {
            savedTarget.GetComponent<PlayerController>().GetOnDashCollisionIds()[id] = false;
            return;
        }

        savedTarget.GetComponent<PlayerController>().GetOnCollisionIds()[id]--;
    }

    #endregion

    #region Interactions

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

    public bool IsHoldingPlate()
    {
        return isHoldingPlate;
    }

    public Plate GetFakePlate()
    {
        return fakePlate;
    }

    void OnTakeDropItem(InputAction.CallbackContext context)
    {

    }

    void OnInteract(InputAction.CallbackContext context)
    {

    }

    #endregion
}
