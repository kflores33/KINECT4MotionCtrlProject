using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum CarDesiredDirection
{
    Straight,
    Left,
    Right
}

public enum CarCommandType
{
    None,
    Stop,
    Straight,
    Left,
    Right
}

/// <summary>
/// 只给“会被玩家指挥”的 AI 车用：
/// - 出生点由 VehicleSpawner 决定
/// - laneIndex = 0/1/2/3（比如 L1/L2/R2/R1）
/// - 车自己知道这一条车道的 Stop 和 Straight，以及全局 Left/Right waypoint
/// - 玩家用 WASD 发指令：A=Left, W=Straight, D=Right, S=Stop
/// </summary>
public class NpcCarController : MonoBehaviour
{
    public static readonly List<NpcCarController> AllCars = new List<NpcCarController>();

    [Header("Lane & Speed")]
    [Tooltip("L1/L2/R2/R1 四条车道，用 0,1,2,3 表示（Spawner 会填）")]
    public int laneIndex = 0;

    public float moveSpeed = 6f;

    [Tooltip("车离开路口之后，向前开出多远就销毁")]
    public float afterExitTravelDistance = 30f;

    [Header("Model Orientation")]
    [Tooltip("修正模型朝向（度），比如模型默认朝 +X，逻辑“前进”是 +Z，可填 90/-90 等")]
    public float visualYawOffset = 0f;

    [Tooltip("只旋转模型，不动碰撞体的话，把模型根节点拖进来；留空就转整个物体")]
    public Transform visualRoot;

    [Header("Waypoints（场景里的 Cube，Spawner 会填）")]
    [Tooltip("本车道的停止线 waypoint（4 个停止线里的其中一个，对应 laneIndex）")]
    public Transform stopPoint;

    [Tooltip("本车道直行的出口 waypoint（4 个直行出口里的其中一个）")]
    public Transform straightPoint;

    [Tooltip("所有车共用的左转出口 waypoint（一个 Cube）")]
    public Transform leftTurnPoint;

    [Tooltip("所有车共用的右转出口 waypoint（一个 Cube）")]
    public Transform rightTurnPoint;

    [Header("Direction UI（头顶提示，告诉玩家这辆车“想去哪里”）")]
    public CarDesiredDirection desiredDirection = CarDesiredDirection.Straight;
    public TextMeshPro directionLabel;   // 注意：这里用 TextMeshPro（3D 文本），不是 TextMeshProUGUI

    [Header("Debug")]
    public bool drawDebugLines = false;

    private Rigidbody rb;

    private enum Phase
    {
        BeforeStop,         // 出生点 → 停止线（不停）
        GoingToExit,        // 停止线 → 出口 waypoint（左/直/右）
        AfterExit           // 出口之后继续往前走一段，然后销毁
    }

    private Phase phase = Phase.BeforeStop;

    private Transform exitTarget;         // 出口 waypoint（left/straight/right 里的一个）
    private Vector3 afterExitDir;         // 离开出口之后继续前进的方向
    private Vector3 exitPointPosition;    // 出口位置（用于计算走了多远）

    private CarCommandType currentCommand = CarCommandType.None;
    private bool isStopped = false;       // 玩家按 S 停止
    private bool hasScored = false;       // 是否已经给过一次“正确指挥 +1 分”
    private bool isCrashed = false;       // 是否已经发生过撞车（防止重复处理）

    private void OnEnable()
    {
        AllCars.Add(this);
    }

    private void OnDisable()
    {
        AllCars.Remove(this);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        RefreshDirectionLabel();
    }

    private void FixedUpdate()
    {
        if (isStopped || isCrashed) return;  // 停车或撞车后不再移动

        switch (phase)
        {
            case Phase.BeforeStop:
                MoveTowardsStop();
                break;
            case Phase.GoingToExit:
                MoveTowardsExit();
                break;
            case Phase.AfterExit:
                MoveAfterExit();
                break;
        }
    }

    // ========== 阶段 1：从出生点 → 停止线（不停车） ==========

    private void MoveTowardsStop()
    {
        if (stopPoint == null)
        {
            MoveStraight(transform.forward);
            return;
        }

        Vector3 toStop = stopPoint.position - transform.position;
        float dist = toStop.magnitude;

        if (dist < 0.2f)
        {
            phase = Phase.GoingToExit;

            // 决定出口 waypoint（左/直/右），但不停车
            exitTarget = ChooseExitWaypoint();
            if (exitTarget == null)
            {
                phase = Phase.AfterExit;
                afterExitDir = transform.forward.normalized;
                exitPointPosition = transform.position;
            }
            return;
        }

        Vector3 dir = toStop.normalized;
        MoveStraight(dir);
    }

    // ========== 阶段 2：停止线 → 出口 waypoint ==========

    private void MoveTowardsExit()
    {
        if (exitTarget == null)
        {
            phase = Phase.AfterExit;
            afterExitDir = transform.forward.normalized;
            exitPointPosition = transform.position;
            return;
        }

        Vector3 toExit = exitTarget.position - transform.position;
        float dist = toExit.magnitude;

        if (dist < 0.2f)
        {
            phase = Phase.AfterExit;
            exitPointPosition = exitTarget.position;

            Vector3 baseDir = (exitTarget.position - (stopPoint != null ? stopPoint.position : transform.position));
            if (baseDir.sqrMagnitude < 0.0001f)
                baseDir = transform.forward;

            afterExitDir = baseDir.normalized;
            return;
        }

        Vector3 dir = toExit.normalized;
        MoveStraight(dir);
    }

    // ========== 阶段 3：出口之后继续往前走一段，然后销毁 ==========

    private void MoveAfterExit()
    {
        if (afterExitDir.sqrMagnitude < 0.0001f)
            afterExitDir = transform.forward.normalized;

        MoveStraight(afterExitDir);

        float travelled = Vector3.Distance(exitPointPosition, transform.position);
        if (travelled >= afterExitTravelDistance)
        {
            Destroy(gameObject);
        }
    }

    // ========== 通用移动 + 旋转 ==========

    private void MoveStraight(Vector3 dir)
    {
        dir.Normalize();
        Vector3 newPos = transform.position + dir * moveSpeed * Time.fixedDeltaTime;

        if (rb != null)
        {
            rb.MovePosition(newPos);
        }
        else
        {
            transform.position = newPos;
        }

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            targetRot *= Quaternion.Euler(0f, visualYawOffset, 0f);

            if (visualRoot != null)
                visualRoot.rotation = targetRot;
            else
                transform.rotation = targetRot;
        }
    }

    // ========== 出口 waypoint 选择逻辑 ==========

    private Transform ChooseExitWaypoint()
    {
        switch (currentCommand)
        {
            case CarCommandType.Left:
                return leftTurnPoint != null ? leftTurnPoint : straightPoint;
            case CarCommandType.Right:
                return rightTurnPoint != null ? rightTurnPoint : straightPoint;
            case CarCommandType.Straight:
            case CarCommandType.None:
            default:
                return straightPoint;
        }
    }

    // ========== 玩家指令 ==========

    public void ApplyCommand(CarCommandType command)
    {
        currentCommand = command;

        if (command == CarCommandType.Stop)
        {
            isStopped = true;
            if (rb != null) rb.linearVelocity = Vector3.zero;
            return;
        }
        else
        {
            isStopped = false;
        }

        if (!hasScored && command != CarCommandType.None)
        {
            bool correct = false;

            if (command == CarCommandType.Straight && desiredDirection == CarDesiredDirection.Straight)
                correct = true;
            else if (command == CarCommandType.Left && desiredDirection == CarDesiredDirection.Left)
                correct = true;
            else if (command == CarCommandType.Right && desiredDirection == CarDesiredDirection.Right)
                correct = true;

            if (correct && ScoreManager.Instance != null)
            {
                hasScored = true;
                ScoreManager.Instance.OnCarGuidedSuccessfully(this);
            }
        }

        if (phase == Phase.GoingToExit)
        {
            exitTarget = ChooseExitWaypoint();
        }
    }

    // ========== 碰撞逻辑：两辆 AICar 撞上 → 全停 → 一起销毁 ==========

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("AICar")) return;
        if (isCrashed) return;

        // 防止同一对车被处理两次：只让 instanceID 更小的那一辆负责
        if (GetInstanceID() > collision.gameObject.GetInstanceID())
            return;

        isCrashed = true;

        GameObject other = collision.gameObject;

        // 停止自己
        StopCarCompletely(this);

        // 停止对方（无论是 NpcCarController 还是 HorizontalTrafficCar）
        StopOtherCarCompletely(other);

        // 扣一次分
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnCarCrash();
        }

        // 一段时间后一起销毁
        StartCoroutine(DestroyPairAfterDelay(other, 2f));
    }

    private void StopCarCompletely(NpcCarController car)
    {
        if (car == null) return;

        car.isStopped = true;
        car.moveSpeed = 0f;
        car.phase = car.phase; // 保持原状态，但不会再走逻辑

        if (car.rb != null)
        {
            car.rb.linearVelocity = Vector3.zero;
            car.rb.angularVelocity = Vector3.zero;
            car.rb.isKinematic = true;            // 不再被物理推动
        }
    }

    private void StopOtherCarCompletely(GameObject other)
    {
        if (other == null) return;

        // 如果对方也是 NpcCarController
        NpcCarController npc = other.GetComponent<NpcCarController>();
        if (npc != null)
        {
            npc.isCrashed = true;
            StopCarCompletely(npc);
        }

        // 如果对方是横向路人车
        HorizontalTrafficCar horiz = other.GetComponent<HorizontalTrafficCar>();
        if (horiz != null)
        {
            Rigidbody rb2 = other.GetComponent<Rigidbody>();
            if (rb2 != null)
            {
                rb2.linearVelocity = Vector3.zero;
                rb2.angularVelocity = Vector3.zero;
                rb2.isKinematic = true;
            }

            horiz.enabled = false;   // 停止脚本移动
        }
    }

    private IEnumerator DestroyPairAfterDelay(GameObject other, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (other != null)
            Destroy(other);

        Destroy(gameObject);
    }

    // ========== UI 显示 ==========

    private void RefreshDirectionLabel()
    {
        if (directionLabel == null) return;

        switch (desiredDirection)
        {
            case CarDesiredDirection.Left:
                directionLabel.text = "Left";
                break;
            case CarDesiredDirection.Right:
                directionLabel.text = "Right";
                break;
            case CarDesiredDirection.Straight:
            default:
                directionLabel.text = "Straight";
                break;
        }
    }

    // ========== Debug Gizmos ==========

    private void OnDrawGizmosSelected()
    {
        if (!drawDebugLines) return;

        Gizmos.color = Color.yellow;
        if (stopPoint != null)
            Gizmos.DrawLine(transform.position, stopPoint.position);

        Gizmos.color = Color.green;
        if (straightPoint != null && stopPoint != null)
            Gizmos.DrawLine(stopPoint.position, straightPoint.position);

        Gizmos.color = Color.red;
        if (leftTurnPoint != null && stopPoint != null)
            Gizmos.DrawLine(stopPoint.position, leftTurnPoint.position);

        Gizmos.color = Color.blue;
        if (rightTurnPoint != null && stopPoint != null)
            Gizmos.DrawLine(stopPoint.position, rightTurnPoint.position);
    }
}
