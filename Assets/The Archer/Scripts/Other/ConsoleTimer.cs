using UnityEngine;
namespace OctoberStudio
{
    public class ConsoleTimer : MonoBehaviour
    {
        private float elapsedTime = 0f;
        private bool isRunning = true;

        // Chuyển biến này thành true (từ script này hoặc script khác) để dừng bộ đếm
        public bool stopFlag = false;

        private void Update()
        {
            if (!isRunning) return;

            // Kiểm tra nếu cờ dừng đã được bật
            if (stopFlag)
            {
                isRunning = false;
                // Hiển thị tổng thời gian với 2 chữ số thập phân
                Debug.Log("Tổng thời gian chơi khi kết thúc: " + elapsedTime.ToString("F2") + " giây");
                return;
            }

            // Tăng thời gian theo mỗi frame
            elapsedTime += Time.deltaTime;
            // Debug.Log("Thời gian chơi: " + elapsedTime.ToString("F2") + " giây");
        }
    }
}