using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    [Header("ITEM DETECTION")]
    [SerializeField] Transform detectionBoxCenter;
    [SerializeField] Vector3 halfExtentsBox = new Vector3(0.65f,0.75f,0.65f);
    [SerializeField] LayerMask itemLayer;
    [SerializeField] int detectionBufferSize = 10;

    [Header("FURNITURE DETECTION")]
    [SerializeField] LayerMask furnitureLayer;
    [SerializeField] float furnitureRayLength = 1.3f;

    [Header("GROUND DETECTION")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundRayLength = 0.3f;

    [Header("GIZMOS")]
    [SerializeField] bool drawGizmos = false;
    [SerializeField] bool drawIfSelected = false;
    [SerializeField] Color noItemHitColor = Color.green;
    [SerializeField] Color onItemHitColor = Color.red;
    [SerializeField] Color noFurnitureHitColor = Color.green;
    [SerializeField] Color onFurnitureHitColor = Color.red;

    PlayerController playerController;
    Collider[] detectionBuffer;
    RaycastHit furnitureHitInfo;
    RaycastHit groundHitInfo;
    Color itemGizmosColor;
    Color furnitureGizmosColor;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        detectionBuffer = new Collider[detectionBufferSize];
    }

    void Update()
    {
        if (!playerController.IsHoldingItem() || (playerController.IsHoldingItem() && playerController.IsHoldingPlate()))
            DetectItem();

        DetectFurniture();
        DetectGround();
    }

    void DetectFurniture()
    {
        if (Physics.Raycast(transform.position, transform.forward, out furnitureHitInfo, furnitureRayLength, furnitureLayer, QueryTriggerInteraction.Ignore))
        {
            furnitureGizmosColor = onFurnitureHitColor;
            playerController.SetNewTargetedFurnitures(furnitureHitInfo.transform.gameObject);
        }
        else
        {
            furnitureGizmosColor = noFurnitureHitColor;
            playerController.SetNewTargetedFurnitures(null);
        }
    }

    void DetectItem()
    {
        int hits = Physics.OverlapBoxNonAlloc(detectionBoxCenter.position, halfExtentsBox, detectionBuffer, transform.rotation, itemLayer, QueryTriggerInteraction.Ignore);

        if (hits > 0)
        {
            itemGizmosColor = onItemHitColor;
            playerController.SetNewTargetedItem(GetDetectedItem(hits));
        }
        else
        {
            itemGizmosColor = noItemHitColor;
            playerController.SetNewTargetedItem(null);
        }
    }

    GameObject GetDetectedItem(int hits)
    {
        float dist;
        float smallestDist = 10.0f;
        int index = 0;

        for (int i = 0; i < hits; i++)
        {
            dist = (detectionBuffer[i].transform.position - transform.position).magnitude;

            if ( dist < smallestDist)
            {
                smallestDist = dist;
                index = i;
            }
        }

        return detectionBuffer[index].gameObject.GetComponentInParent<Item>().gameObject;
    }

    void DetectGround()
    {
        if (Physics.Raycast(transform.position, transform.up * (-1.0f), out groundHitInfo, groundRayLength, groundLayer, QueryTriggerInteraction.Ignore))
        {
            if (groundHitInfo.distance > 0.1f)
                playerController.SetGroundedState(false);
            else
                playerController.SetGroundedState(true);
        }
        else
            playerController.SetGroundedState(false);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || drawIfSelected)
            return;

        Matrix4x4 defaultMatrix = Gizmos.matrix;

        if (!Application.isPlaying)
            Gizmos.color = noItemHitColor;
        else
            Gizmos.color = itemGizmosColor;

        Gizmos.matrix = detectionBoxCenter.localToWorldMatrix;

        if (!Application.isPlaying || (Application.isPlaying && (!playerController.IsHoldingItem()) || (playerController.IsHoldingItem() && playerController.IsHoldingPlate())))
            Gizmos.DrawWireCube(Vector3.zero, halfExtentsBox * 2);

        Gizmos.matrix = defaultMatrix;

        if (!Application.isPlaying)
            Gizmos.color = noFurnitureHitColor;
        else
            Gizmos.color = furnitureGizmosColor;

        Gizmos.DrawLine(transform.position, transform.position + transform.forward * furnitureRayLength);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos || !drawIfSelected)
            return;

        Matrix4x4 defaultMatrix = Gizmos.matrix;

        if (!Application.isPlaying)
            Gizmos.color = noItemHitColor;
        else
            Gizmos.color = itemGizmosColor;

        Gizmos.matrix = detectionBoxCenter.localToWorldMatrix;

        if (!Application.isPlaying || (Application.isPlaying && (!playerController.IsHoldingItem() || (playerController.IsHoldingItem() && playerController.IsHoldingPlate()))))
            Gizmos.DrawWireCube(Vector3.zero, halfExtentsBox*2);

        Gizmos.matrix = defaultMatrix;

        if (!Application.isPlaying)
            Gizmos.color = noFurnitureHitColor;
        else
            Gizmos.color = furnitureGizmosColor;

        Gizmos.DrawLine(transform.position, transform.position + transform.forward * furnitureRayLength);
    }

}
