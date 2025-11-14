using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;   // 新 Input System

public class PlayerTrafficController : MonoBehaviour
{
    [Header("Lane Settings")]
    [Tooltip("四个车道位置：顺序建议 L1, L2, R2, R1")]
    public Transform[] lanePositions;

    [Tooltip("玩家切换车道时的平滑移动速度")]
    public float laneChangeSpeed = 10f;

    [Header("Officer Visuals (可选)")]
    [Tooltip("四个警察小人的 SpriteRenderer，当前车道实心，其他车道半透明")]
    public SpriteRenderer[] officerRenderers;

    // 当前车道索引：0 = L1, 1 = L2, 2 = R2, 3 = R1
    private int currentLaneIndex = 0;
    private Vector3 targetPosition;

    public int CurrentLaneIndex => currentLaneIndex;

    private void Start()
    {
        // 初始化：把玩家放到当前车道
        if (lanePositions != null && lanePositions.Length > 0)
        {
            currentLaneIndex = Mathf.Clamp(currentLaneIndex, 0, lanePositions.Length - 1);
            targetPosition = lanePositions[currentLaneIndex].position;
            transform.position = targetPosition;
        }

        UpdateOfficerVisuals();
    }

    private void Update()
    {
        HandleLaneInput();      // 左右箭头换车道
        MoveToLane();           // 插值移动到车道位置
        HandleCommandInput();   // WASD 发指令
    }

    // =============================
    //        车道切换逻辑
    // =============================

    private void HandleLaneInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return; // 没键盘设备就不处理

        // 左右方向键
        if (kb.leftArrowKey.wasPressedThisFrame)
        {
            ChangeLane(-1);
        }
        else if (kb.rightArrowKey.wasPressedThisFrame)
        {
            ChangeLane(1);
        }
    }

    private void ChangeLane(int delta)
    {
        if (lanePositions == null || lanePositions.Length == 0)
            return;

        int newIndex = Mathf.Clamp(currentLaneIndex + delta, 0, lanePositions.Length - 1);
        if (newIndex != currentLaneIndex)
        {
            currentLaneIndex = newIndex;
            targetPosition = lanePositions[currentLaneIndex].position;
            UpdateOfficerVisuals();
        }
    }

    private void MoveToLane()
    {
        if (lanePositions == null || lanePositions.Length == 0)
            return;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            laneChangeSpeed * Time.deltaTime
        );
    }

    private void UpdateOfficerVisuals()
    {
        if (officerRenderers == null || officerRenderers.Length == 0)
            return;

        for (int i = 0; i < officerRenderers.Length; i++)
        {
            if (officerRenderers[i] == null) continue;

            Color c = officerRenderers[i].color;
            c.a = (i == currentLaneIndex) ? 1f : 0.25f; // 当前车道实心，其余虚
            officerRenderers[i].color = c;
        }
    }

    // =============================
    //        发指令逻辑 (WASD)
    // =============================

    private void HandleCommandInput()
    {
        if (NpcCarController.AllCars.Count == 0)
            return;

        var kb = Keyboard.current;
        if (kb == null) return;

        // A = 左转
        if (kb.aKey.wasPressedThisFrame)
        {
            SendCommandToCurrentLane(CarCommandType.Right);
        }
        // W = 直行
        else if (kb.wKey.wasPressedThisFrame)
        {
            SendCommandToCurrentLane(CarCommandType.Straight);
        }
        // D = 右转
        else if (kb.dKey.wasPressedThisFrame)
        {
            SendCommandToCurrentLane(CarCommandType.Left);
        }
        // S = 停止
        else if (kb.sKey.wasPressedThisFrame)
        {
            SendCommandToCurrentLane(CarCommandType.Stop);
        }
    }

    private void SendCommandToCurrentLane(CarCommandType command)
    {
        NpcCarController target = FindNearestCarInLane(currentLaneIndex);
        if (target != null)
        {
            target.ApplyCommand(command);
        }
        else
        {
            Debug.Log("当前车道没有可控制的车，LaneIndex = " + currentLaneIndex);
        }
    }

    // 找到当前车道「离玩家最近、尚未开过玩家」的一辆车
    private NpcCarController FindNearestCarInLane(int laneIndex)
    {
        NpcCarController nearest = null;
        float bestDist = Mathf.Infinity;

        Vector3 playerPos = transform.position;

        foreach (NpcCarController car in NpcCarController.AllCars)
        {
            if (car == null) continue;
            if (car.laneIndex != laneIndex) continue;

            // 用 3D 距离（或者你想只算 XZ 平面也可以）
            float dist = Vector3.Distance(playerPos, car.transform.position);

            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = car;
            }
        }

        return nearest;
    }

}
