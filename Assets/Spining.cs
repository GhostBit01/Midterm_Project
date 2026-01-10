using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spining : MonoBehaviour
{
    [Header("Rotation Settings (การหมุน)")]
    // ความเร็วในการหมุน (องศาต่อวินาที)
    public float rotationSpeed = 50f;

    [Header("Floating Settings (การส่ายขึ้นลง)")]
    // ความเร็วในการส่ายขึ้นลง (ยิ่งมากยิ่งส่ายเร็ว)
    public float floatSpeed = 1f;

    // ระยะความสูงที่ส่าย (ยิ่งมากยิ่งขึ้นลงสูง)
    public float floatHeight = 0.5f;

    // เก็บตำแหน่งเริ่มต้น
    private Vector3 startPos;

    void Start()
    {
        // จำตำแหน่งเริ่มต้นของวัตถุไว้
        startPos = transform.position;
    }

    void Update()
    {
        // 1. ส่วนของการหมุน (Rotate)
        // หมุนรอบแกน Y (Vector3.up)
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 2. ส่วนของการส่ายขึ้นลง (Float/Bob)
        // คำนวณตำแหน่ง Y ใหม่โดยใช้ Sin Wave
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        // อัปเดตตำแหน่งของวัตถุ (รักษาค่า X และ Z เดิมไว้ เปลี่ยนแค่ Y)
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
