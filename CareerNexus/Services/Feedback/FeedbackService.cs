using CareerNexus.Common;
using CareerNexus.Models.Feedback;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CareerNexus.Services.Feedback
{
    public class FeedbackService:IFeedbackService
    {
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(ILogger<FeedbackService> logger)
        {
            _logger = logger;
        }

        public async Task<long> SubmitFeedbackAsync(long userId, string message, string feedbackType)
        {
            long insertedId = 0;
            try
            {
                string getUserQuery = "SELECT Fullname, Email FROM Users WHERE Id = @UserId";
                var getUserCmd = new SqlCommand();
                getUserCmd.CommandText = getUserQuery;
                getUserCmd.CommandType = CommandType.Text;
                getUserCmd.Parameters.AddWithValue("@UserId", userId);

                var userDt = DBEngine.GetDataTable(getUserCmd, Databaseoperations.Select, getUserQuery);
                if (userDt.Rows.Count == 0)
                {
                    _logger.LogWarning("SubmitFeedback: User {UserId} not found", userId);
                    return 0;
                }

                string userName = userDt.Rows[0]["Fullname"]?.ToString() ?? "Unknown";
                string userEmail = userDt.Rows[0]["Email"]?.ToString() ?? "";

                string feedbackTypeNorm = (feedbackType ?? "suggestion").ToLowerInvariant();
                if (feedbackTypeNorm != "error" && feedbackTypeNorm != "suggestion")
                    feedbackTypeNorm = "suggestion";

                string insertQuery = @"INSERT INTO Feedback (UserId, UserName, UserEmail, Message, FeedbackType, SubmittedAt)
VALUES (@UserId, @UserName, @UserEmail, @Message, @FeedbackType, GETUTCDATE());
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                var cmd = new SqlCommand();
                cmd.CommandText = insertQuery;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@UserName", userName);
                cmd.Parameters.AddWithValue("@UserEmail", userEmail);
                cmd.Parameters.AddWithValue("@Message", message ?? "");
                cmd.Parameters.AddWithValue("@FeedbackType", feedbackTypeNorm);

                insertedId = DBEngine.ExecuteScalar(cmd, Databaseoperations.Insert, insertQuery);
                _logger.LogInformation("Feedback submitted by UserId {UserId}, FeedbackId {FeedbackId}", userId, insertedId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SubmitFeedback failed for UserId {UserId}", userId);
            }

            return await Task.FromResult(insertedId);
        }

        public async Task<List<FeedbackItemModel>> GetAllFeedbackAsync()
        {
            var list = new List<FeedbackItemModel>();
            try
            {
                string query = "SELECT Id, UserName, UserEmail, Message, FeedbackType, SubmittedAt FROM Feedback ORDER BY SubmittedAt DESC";
                var cmd = new SqlCommand();
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                var dt = DBEngine.GetDataTable(cmd, Databaseoperations.Select, query);
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new FeedbackItemModel
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Username = row["UserName"]?.ToString() ?? "",
                        Email = row["UserEmail"]?.ToString() ?? "",
                        Message = row["Message"]?.ToString() ?? "",
                        FeedbackType = row["FeedbackType"]?.ToString() ?? "suggestion",
                        SubmittedAt = row["SubmittedAt"] != DBNull.Value ? Convert.ToDateTime(row["SubmittedAt"]) : DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllFeedback failed");
            }

            return await Task.FromResult(list);
        }

        public async Task<List<FeedbackItemModel>> GetFeedbackByUserIdAsync(long userId)
        {
            var list = new List<FeedbackItemModel>();

            try
            {
                string query = @"
            SELECT Id, UserName, UserEmail, Message, FeedbackType, SubmittedAt
            FROM Feedback
            WHERE UserId = @UserId
            ORDER BY SubmittedAt DESC";

                var cmd = new SqlCommand();
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@UserId", userId);   // ✅ safe

                var dt = DBEngine.GetDataTable(cmd, Databaseoperations.Select, query);

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new FeedbackItemModel
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Username = row["UserName"]?.ToString() ?? "",
                        Email = row["UserEmail"]?.ToString() ?? "",
                        Message = row["Message"]?.ToString() ?? "",
                        FeedbackType = row["FeedbackType"]?.ToString() ?? "suggestion",
                        SubmittedAt = row["SubmittedAt"] != DBNull.Value
                                        ? Convert.ToDateTime(row["SubmittedAt"])
                                        : DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFeedbackByUserId failed");
            }

            return await Task.FromResult(list);
        }

    }
}
