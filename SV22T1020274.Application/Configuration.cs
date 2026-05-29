using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020274.Application
{
    public static class Configuration
    {
        private static string _connectionString = "";
        /// <summary>
        /// Khởi tạo cấu hình Application (gọi từ Infrastructure composition root).
        /// (hàm này được gọi trước khi chạy ứng dụng)
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }
        /// <summary>
        /// Lấy chuỗi tham số kết nối đến CSDL
        /// </summary>
        public static string ConnectionString => _connectionString;
    }
}