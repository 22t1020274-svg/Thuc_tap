using SV22T1020274.Application.Abstractions;
using SV22T1020274.Domain.DataDictionary;
using System.Threading.Tasks;

namespace SV22T1020274.Application
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến từ điển dữ liệu
    /// </summary>
    public static class DictionaryDataService
    {
        private static IDataDictionaryRepository<Province> provinceDB = null!;

        public static void Configure(IDataDictionaryRepository<Province> provinceRepository)
        {
            provinceDB = provinceRepository;
        }
        /// <summary>
        /// Lấy danh sách tỉnh thành
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Province>> ListProvincesAsync()
        {
            return await provinceDB.ListAsync();
        }
    }
}
