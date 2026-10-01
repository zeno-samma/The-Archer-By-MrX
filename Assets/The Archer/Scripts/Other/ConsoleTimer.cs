using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace OctoberStudio
{
    public class ConsoleTimer : MonoBehaviour
    {
        public static ConsoleTimer Instance { get; private set; }

        private Dictionary<string, bool> stopFlags = new Dictionary<string, bool>();

        // Lưu trữ giá trị thời gian (giây) theo ID để tra cứu sau
        private Dictionary<string, float> timerResults = new Dictionary<string, float>();

        private bool hasReportedThisRun = false; // Cờ chặn tổng hợp nhiều lần
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Bắt đầu đếm thời gian cho một ID
        /// </summary>
        public void StartTimer(string timerId)
        {
            stopFlags[timerId] = false;
            timerResults[timerId] = 0f; // Reset về 0 khi bắt đầu phiên mới
            StartCoroutine(CountRoutine(timerId));
        }

        /// <summary>
        /// Dừng đếm và trả về ngay kết quả thời gian
        /// </summary>
        public float StopTimer(string timerId)
        {
            // Debug.Log(timerId);
            if (stopFlags.ContainsKey(timerId))
            {
                stopFlags[timerId] = true;
            }
            return GetElapsedTime(timerId);
        }

        /// <summary>
        /// Lấy kết quả thời gian của một ID (kể cả khi đang chạy hoặc đã dừng)
        /// </summary>
        public float GetElapsedTime(string timerId)
        {
            if (timerResults.TryGetValue(timerId, out float time))
            {
                return time;
            }
            return 0f;
        }

        private IEnumerator CountRoutine(string timerId)
        {
            float elapsedTime = 0f;
            while (!stopFlags[timerId])// Chạy cho đến khi stopFlags[timerId] được đặt thành true
            {
                elapsedTime += Time.deltaTime;
                timerResults[timerId] = elapsedTime; // Cập nhật biến lưu trữ theo realtime
                yield return null;
            }
        }
        /// <summary>
        /// Trả về bản sao của toàn bộ Dictionary kết quả để duyệt bằng code
        /// </summary>
        public Dictionary<string, float> GetAllResults()
        {
            return new Dictionary<string, float>(timerResults);
        }

        /// <summary>
        /// In ra Console toàn bộ danh sách thời gian đã ghi nhận từ trước đến nay
        /// </summary>
        public void LogAllResults()
        {
            Debug.Log("================ DANH SÁCH THỜI GIAN CÁC PHÒNG ================");
            foreach (var item in timerResults)
            {
                Debug.Log($"- {item.Key}: {item.Value:F2} giây");
            }
            Debug.Log("===============================================================");
        }
        /// <summary>
        /// Gọi hàm này mỗi khi bắt đầu một Lượt Chơi mới (Game mới / Room 1)
        /// </summary>
        public void ResetSession()
        {
            hasReportedThisRun = false;
            // timerResults.Clear();
        }
        /// <summary>
        /// Tổng hợp kết quả phiên chơi (chỉ chạy duy nhất 1 lần cho sự kiện xảy ra trước)
        /// </summary>
        public void ReportSessionResult(string message)
        {
            // Nếu đã xuất báo cáo trước đó trong lượt chơi này -> Chặn ngay
            if (hasReportedThisRun)
            {
                Debug.Log("Đã có thông báo tổng hợp rồi, bỏ qua sự kiện: " + message);
                return;
            }

            // Đánh dấu đã báo cáo
            hasReportedThisRun = true;

            // 1. In lý do kết thúc
            switch (message)
            {
                case "Die":
                    Debug.Log($"===> KẾT THÚC: Người chơi chết tại phòng {StageController.Room.RoomId}");
                    break;
                case "FinishAllRoom":
                    Debug.Log("===> KẾT THÚC: Người chơi đã phá đảo toàn bộ phòng!");
                    break;
                case "BackToMenu":
                    Debug.Log("===> KẾT THÚC: Người chơi chủ động thoát về Menu");
                    break;
                default:
                    Debug.Log("===> KẾT THÚC: " + message);
                    break;
            }
            // 2. In toàn bộ danh sách thời gian các phòng
            LogAllResults();
        }
    }



    // ==========================
    // // 1. Bắt đầu phiên đếm
    // ConsoleTimer.Instance.StartTimer("Wave_1");

    // // 2. Dừng khi kết thúc phiên và lấy giá trị ra ngay
    // float finalTime = ConsoleTimer.Instance.StopTimer("Wave_1");
    // Debug.Log("Thời gian hoàn thành Wave 1: " + finalTime.ToString("F2") + "s");

    // // // 3. Hoặc đọc lại biến lưu trữ ở bất kỳ đâu/bất kỳ lúc nào về sau
    // float recordedTime = ConsoleTimer.Instance.GetElapsedTime("Wave_1");

    // Cách 1: In nhanh toàn bộ danh sách ra Console
    // Gọi dòng này ở cuối màn chơi hoặc khi Game Over
    // ConsoleTimer.Instance.LogAllResults();
    // Cách 2: Lấy dữ liệu dạng danh sách để xử lý bằng code (ví dụ vẽ UI, gửi dữ liệu Analytic)
    //     var allRoomTimes = ConsoleTimer.Instance.GetAllResults();

    // foreach (KeyValuePair<string, float> entry in allRoomTimes)
    // {
    //     string roomId = entry.Key;     // Ví dụ: "RoomId_1"
    //     float timeSpent = entry.Value; // Ví dụ: 15.42f

    //     Debug.Log($"[Thống kê] {roomId} hoàn thành trong {timeSpent:F2}s");
    // }
}