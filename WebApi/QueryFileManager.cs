﻿using Newtonsoft.Json;
using static WebApi.Controllers.ReportController;

namespace WebApi
{
    public class QueryFileManager
    {


        public static List<UserStatisticsRequestInfo> LoadPendingUserStatisticsRequests()
        {
            string filePath = "userStatisticsRequests.json";

            List<UserStatisticsRequestInfo> existingRequests;

            try
            {
                fileAccessSemaphore.Wait();
                string json = System.IO.File.ReadAllText(filePath);
                fileAccessSemaphore.Release();
                existingRequests = JsonConvert.DeserializeObject<List<UserStatisticsRequestInfo>>(json) ?? new List<UserStatisticsRequestInfo>();
                return existingRequests;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading user statistics requests file: {ex.Message}");
                existingRequests = new List<UserStatisticsRequestInfo>();
                return existingRequests;
            }
        }

        public static void SaveUserStatisticsRequest(UserStatisticsRequestInfo requestInfo)
        {
            string filePath = "userStatisticsRequests.json";

            List<UserStatisticsRequestInfo> existingRequests = LoadPendingUserStatisticsRequests();

            existingRequests.Add(requestInfo);

            try
            {
                string updatedJson = JsonConvert.SerializeObject(existingRequests);
                fileAccessSemaphore.Wait();
                System.IO.File.WriteAllText(filePath, updatedJson);
                fileAccessSemaphore.Release();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing user statistics requests file: {ex.Message}");
            }
        }

        public static void SavePendingRequests(List<UserStatisticsRequestInfo> pendingRequests)
        {
            string filePath = "userStatisticsRequests.json";

            try
            {
                var updatedJson = JsonConvert.SerializeObject(pendingRequests);
                fileAccessSemaphore.Wait();
                System.IO.File.WriteAllText(filePath, updatedJson);
                fileAccessSemaphore.Release();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing user statistics requests file: {ex.Message}");
            }
        }

        private static readonly SemaphoreSlim fileAccessSemaphore = new SemaphoreSlim(1, 1);

        public static async Task RemoveProcessedRequestAsync(Guid queryId)
        {
            await fileAccessSemaphore.WaitAsync();

            try
            {
                List<UserStatisticsRequestInfo> unfinishedRequests = LoadPendingUserStatisticsRequests();
                unfinishedRequests.RemoveAll(r => r.QueryId == queryId);
                SavePendingRequests(unfinishedRequests);
            }
            finally
            {
                fileAccessSemaphore.Release();
            }
        }
    }
}
