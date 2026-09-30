using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace OctoberStudio
{
    public class ConsoleTimer : MonoBehaviour
    {
        // private static ConsoleTimer instance;
        // private float elapsedTime = 0f;
        // private bool isRunning = true;

        // // Chuyển biến này thành true (từ script này hoặc script khác) để dừng bộ đếm
        // public bool stopFlag = false;

        // private void Update()
        // {
        //     if (!isRunning) return;

        //     // Kiểm tra nếu cờ dừng đã được bật
        //     if (stopFlag)
        //     {
        //         isRunning = false;
        //         // Hiển thị tổng thời gian với 2 chữ số thập phân
        //         Debug.Log("Tổng thời gian chơi khi kết thúc: " + elapsedTime.ToString("F2") + " giây");
        //         return;
        //     }

        //     // Tăng thời gian theo mỗi frame
        //     elapsedTime += Time.deltaTime;
        //     // Debug.Log("Thời gian chơi: " + elapsedTime.ToString("F2") + " giây");
        // }
        // ====================================================
        public static ConsoleTimer Instance { get; private set; }

        // Quản lý cờ dừng (stopFlag) cho từng luồng đếm
        private Dictionary<string, bool> stopFlags = new Dictionary<string, bool>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Bắt đầu một luồng đếm thời gian riêng biệt
        /// </summary>
        public void StartTimer(string timerId)
        {
            stopFlags[timerId] = false;
            StartCoroutine(CountRoutine(timerId));
        }

        /// <summary>
        /// Bật flag dừng luồng đếm tương ứng và in kết quả
        /// </summary>
        public void StopTimer(string timerId)
        {
            if (stopFlags.ContainsKey(timerId))
            {
                stopFlags[timerId] = true;
            }
        }

        private IEnumerator CountRoutine(string timerId)
        {
            float elapsedTime = 0f;

            // Chạy liên tục mỗi frame cho đến khi cờ dừng bật
            while (!stopFlags[timerId])
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            Debug.Log($"[Timer: {timerId}] Tổng thời gian: {elapsedTime:F2} giây");
        }
    }
}