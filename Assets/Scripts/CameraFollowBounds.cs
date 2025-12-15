using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowBounds : Singleton<CameraFollowBounds>
{
    [Header("Target")]
    public Transform target;

    [Header("Bounds Settings")]
    public Collider2D mapBounds;
    public Transform edgeParent;

    [Header("Camera Settings")]
    public float smooth = 5f;

    private Camera cam;
    public float halfHeight;
    public float halfWidth;
    public float minX, maxX, minY, maxY;

    [Header("Edges")]
    public EdgeCollider2D[] edges;

    void Start()
    {
        cam = GetComponent<Camera>();
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
        GetCameraBound();
    }

    public void SetEdgeParent(Transform parent)
    {
        edgeParent = parent;
        GetCameraBound();
        SnapToTarget();
    }

    public void SnapToTarget()
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(target.position.x, target.position.y + 3, transform.position.z);

        float clampX = Mathf.Clamp(targetPos.x, minX + halfWidth, maxX - halfWidth);
        float clampY = Mathf.Clamp(targetPos.y, minY + halfHeight, maxY - halfHeight);

        transform.position = new Vector3(clampX, clampY, transform.position.z);
    }

    private void GetCameraBound()
    {
        if (edgeParent != null)
        {
            edges = edgeParent.GetComponentsInChildren<EdgeCollider2D>();

            if (edges.Length > 0)
            {
                Vector3 topEdge = edges[0].transform.position;
                Vector3 bottomEdge = edges[1].transform.position;
                Vector3 leftEdge = edges[2].transform.position;
                Vector3 rightEdge = edges[3].transform.position;
                
                minY = bottomEdge.y;
                maxY = topEdge.y;
                minX = leftEdge.x;
                maxX = rightEdge.x;
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Chưa gán Map Bounds hoặc Edge Parent cho CameraFollowBounds!");
        }
    }

    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(target.position.x, target.position.y + 3, transform.position.z);

        float clampX = Mathf.Clamp(targetPos.x, minX + halfWidth, maxX - halfWidth);
        float clampY = Mathf.Clamp(targetPos.y, minY + halfHeight, maxY - halfHeight);

        Vector3 smoothPos = Vector3.Lerp(transform.position, new Vector3(clampX, clampY, transform.position.z), smooth * Time.deltaTime);
        transform.position = smoothPos;
    }

    public void SetTarget(Transform transform)
    {
        target = transform;
    }
}