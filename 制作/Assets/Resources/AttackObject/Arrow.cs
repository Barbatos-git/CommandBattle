using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPos;
    private float speed = 10f;

    public void Initialize(Vector3 archerPosition, Vector3 targetPosition)
    {
        startPos = archerPosition;
        targetPos = targetPosition;

        transform.position = startPos;

        Vector3 dir = (targetPos - startPos).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            OnHitTarget();
        }
    }

    private void OnHitTarget()
    {
        Destroy(gameObject);
    }
}
