using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followBoxSize = 1f;
    [SerializeField] private float followSpeed = 5f;

    private Vector3 offset;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow has no target set!");
            return;
        }

        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        Vector3 currentPos = transform.position;

        Vector2 delta = desiredPos - currentPos;

        if (Mathf.Abs(delta.x) > followBoxSize || Mathf.Abs(delta.y) > followBoxSize)
        {
            Vector3 move = Vector3.Lerp(currentPos, desiredPos, followSpeed * Time.deltaTime);
            transform.position = new Vector3(move.x, move.y, transform.position.z);
        }
    }
}
