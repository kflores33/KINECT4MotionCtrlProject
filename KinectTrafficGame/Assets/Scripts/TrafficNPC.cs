using UnityEngine;

/// <summary>
/// 横向路人车：
/// - 刷出来后一直往 targetPoint 开
/// - 到达 targetPoint 附近后自动销毁
/// - 可配置速度区间和旋转修正
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class HorizontalTrafficCar : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("车要开往的目标点（场景里的一个 Cube / 空物体）")]
    public Transform targetPoint;

    [Tooltip("最小速度")]
    public float minSpeed = 5f;

    [Tooltip("最大速度")]
    public float maxSpeed = 8f;

    [Tooltip("当距离目标小于这个值时，认为到达并销毁车辆")]
    public float arriveDistance = 0.3f;

    [Header("Visual Orientation")]
    [Tooltip("模型朝向修正角度（度）。例如模型默认朝 +X，但你逻辑上想以 +Z 为前方，就可以填 90 或 -90。")]
    public float visualYawOffset = 0f;

    [Tooltip("只想旋转车身模型、不旋转碰撞体的话，把模型根节点拖进来。留空则旋整个物体。")]
    public Transform visualRoot;

    [Header("Safety")]
    [Tooltip("最大生存时间（秒），防止目标丢失时车永远不删）")]
    public float maxLifeTime = 30f;

    private Rigidbody rb;
    private float moveSpeed;
    private float lifeTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;        // 一般横向路人车不需要受重力
        rb.constraints = RigidbodyConstraints.FreezeRotation; // 防止碰撞乱转
    }

    private void Start()
    {
        // 刷新时随机一个速度
        moveSpeed = Random.Range(minSpeed, maxSpeed);
    }

    private void FixedUpdate()
    {
        lifeTimer += Time.fixedDeltaTime;
        if (lifeTimer >= maxLifeTime)
        {
            Destroy(gameObject);
            return;
        }

        // 如果没有目标，就沿着自身 forward 一直开
        if (targetPoint == null)
        {
            MoveInDirection(transform.forward);
            return;
        }

        Vector3 toTarget = targetPoint.position - transform.position;
        float dist = toTarget.magnitude;

        // 接近目标后销毁
        if (dist <= arriveDistance)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = toTarget.normalized;
        MoveInDirection(dir);
    }

    private void MoveInDirection(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;

        Vector3 newPos = transform.position + dir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        // 旋转视觉朝向前进方向 + 修正角度
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        targetRot *= Quaternion.Euler(0f, visualYawOffset, 0f);

        if (visualRoot != null)
        {
            visualRoot.rotation = targetRot;
        }
        else
        {
            transform.rotation = targetRot;
        }
    }
}
