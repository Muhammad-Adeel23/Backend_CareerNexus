using CareerNexus.Common;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace CareerNexus.Services.Admin
{
    public class OverviewStatsModel
    {
        public int TotalUsers { get; set; }
        public int AssessmentsCompleted { get; set; }
        public int ResumesUploaded { get; set; }
        public int CareerMatches { get; set; }
    }

    public class UserListModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string Status { get; set; } = "Inactive";
        public string? Joined { get; set; }
        public bool AssessmentCompleted { get; set; }
        public bool ResumeUploaded { get; set; }
    }

    public class UserDetailModel
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? RoleName { get; set; }
        public bool AssessmentCompleted { get; set; }
        public bool ResumeUploaded { get; set; }
    }

    public class AssessmentListModel
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? PersonalityType { get; set; }
        public int Score { get; set; }
        public string? CompletedDate { get; set; }
    }

    public class AssessmentDetailModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? PersonalityType { get; set; }
        public int TotalScore { get; set; }
        public string? Answers { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Description { get; set; }
    }

    public class ResumeListModel
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? FileName { get; set; }
        public string? FileURL { get; set; }
        public string Status { get; set; } = "Pending";
        public string? UploadDate { get; set; }
        public List<string> Skills { get; set; } = new();
    }

    public class CareerModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<string> RequiredSkills { get; set; } = new();
        public List<string> PersonalityMatchs { get; set; } = new();
    }

    public class SkillModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public List<string> LinkedCareers { get; set; } = new();
    }

    public class TopCareerModel
    {
        public string? CareerName { get; set; }
        public int MatchCount { get; set; }
    }
    public class AdminService:IAdminService
    {
        private readonly ILogger<AdminService> _logger;
        public AdminService(ILogger<AdminService> logger)
        {
            _logger = logger;
        }
        public async Task<OverviewStatsModel> GetOverviewStatsAsync()
        {
            try
            {
                var stats = new OverviewStatsModel();

                // Total Users
                string query = "SELECT COUNT(*) FROM Users";
                SqlCommand cmd = new SqlCommand(query);
                stats.TotalUsers = (int)DBEngine.ExecuteScalar(cmd, Databaseoperations.Select, query);

                // Assessments Completed
                query = "SELECT COUNT(DISTINCT UserId) FROM Assesments WHERE UserId IS NOT NULL";
                cmd = new SqlCommand(query);
                stats.AssessmentsCompleted = (int)DBEngine.ExecuteScalar(cmd, Databaseoperations.Select, query);

                // Resumes Uploaded
                query = "SELECT COUNT(DISTINCT UserId) FROM Resumes WHERE UserId IS NOT NULL";
                cmd = new SqlCommand(query);
                stats.ResumesUploaded = (int)DBEngine.ExecuteScalar(cmd, Databaseoperations.Select, query);

                // Career Matches (count of assessments with personality types)
                query = "SELECT COUNT(*) FROM Assesments WHERE PersonalityType IS NOT NULL AND PersonalityType != ''";
                cmd = new SqlCommand(query);
                stats.CareerMatches = (int)DBEngine.ExecuteScalar(cmd, Databaseoperations.Select, query);

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overview stats");
                throw;
            }
        }

        public async Task<List<UserListModel>> GetUsersAsync()
        {
            try
            {
                string query = @"
                    SELECT 
                        U.Id,
                        U.FullName as Name,
                        U.Email,
                        CASE WHEN U.IsActive = 1 THEN 'Active' ELSE 'Inactive' END as Status,
                        CONVERT(varchar, U.CreatedOn, 23) as Joined,
                        CASE WHEN EXISTS(SELECT 1 FROM Assesments A WHERE A.UserId = U.Id) THEN 1 ELSE 0 END as AssessmentCompleted,
                        CASE WHEN EXISTS(SELECT 1 FROM Resumes R WHERE R.UserId = U.Id) THEN 1 ELSE 0 END as ResumeUploaded
                    FROM Users U
                    ORDER BY U.CreatedOn DESC";

                SqlCommand cmd = new SqlCommand(query);
                DataTable dt = DBEngine.GetDataTable(cmd, Databaseoperations.Select, query);

                var users = new List<UserListModel>();
                foreach (DataRow row in dt.Rows)
                {
                    users.Add(new UserListModel
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = row["Name"]?.ToString(),
                        Email = row["Email"]?.ToString(),
                        Status = row["Status"]?.ToString() ?? "Inactive",
                        Joined = row["Joined"]?.ToString(),
                        AssessmentCompleted = Convert.ToBoolean(row["AssessmentCompleted"]),
                        ResumeUploaded = Convert.ToBoolean(row["ResumeUploaded"])
                    });
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                throw;
            }
        }

        public async Task<UserDetailModel?> GetUserAsync(int userId)
        {
            try
            {
                string query = @"
                    SELECT 
                        U.Id,
                        U.UserName,
                        U.Email,
                        U.FullName,
                        U.IsActive,
                        U.CreatedOn,
                        R.RoleName,
                        CASE WHEN EXISTS(SELECT 1 FROM Assesments A WHERE A.UserId = U.Id) THEN 1 ELSE 0 END as AssessmentCompleted,
                        CASE WHEN EXISTS(SELECT 1 FROM Resumes R WHERE R.UserId = U.Id) THEN 1 ELSE 0 END as ResumeUploaded
                    FROM Users U
                    LEFT JOIN UserRoles UR ON UR.UserId = U.Id
                    LEFT JOIN Roles R ON UR.RoleId = R.Id
                    WHERE U.Id = @UserId";

                SqlCommand cmd = new SqlCommand(query);
                cmd.Parameters.AddWithValue("@UserId", userId);
                DataTable dt = DBEngine.GetDataTable(cmd, Databaseoperations.Select, query);

                if (dt.Rows.Count == 0)
                    return null;

                var row = dt.Rows[0];
                return new UserDetailModel
                {
                    Id = Convert.ToInt32(row["Id"]),
                    UserName = row["UserName"]?.ToString(),
                    Email = row["Email"]?.ToString(),
                    FullName = row["FullName"]?.ToString(),
                    IsActive = Convert.ToBoolean(row["IsActive"]),
                    CreatedOn = row["CreatedOn"] != DBNull.Value ? Convert.ToDateTime(row["CreatedOn"]) : null,
                    RoleName = row["RoleName"]?.ToString(),
                    AssessmentCompleted = Convert.ToBoolean(row["AssessmentCompleted"]),
                    ResumeUploaded = Convert.ToBoolean(row["ResumeUploaded"])
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
        {
            try
            {
                string query = "UPDATE Users SET IsActive = @IsActive, UpdatedOn = GETDATE() WHERE Id = @UserId";
                SqlCommand cmd = new SqlCommand(query);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@IsActive", isActive);

                return DBEngine.ExecuteNonQuery(cmd, Databaseoperations.Update, query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                // Delete related records first
                string query = "DELETE FROM UserRoles WHERE UserId = @UserId";
                SqlCommand cmd = new SqlCommand(query);
                cmd.Parameters.AddWithValue("@UserId", userId);
                DBEngine.ExecuteNonQuery(cmd, Databaseoperations.Delete, query);

                query = "DELETE FROM Users WHERE Id = @UserId";
                cmd = new SqlCommand(query);
                cmd.Parameters.AddWithValue("@UserId", userId);

                return DBEngine.ExecuteNonQuery(cmd, Databaseoperations.Delete, query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<AssessmentListModel>> GetAssessmentsAsync()
        {
            try
            {
                string query = @"
                    SELECT 
                        A.Id,
                        ISNULL(U.FullName, 'Guest') as UserName,
                        A.PersonalityType,
                        A.TotalScore as Score,
                        CONVERT(varchar, A.CompletedAt, 23) as CompletedDate
                    FROM Assesments A
                    LEFT JOIN Users U ON A.UserId = U.Id
                    ORDER BY A.CompletedAt DESC";

                SqlCommand cmd = new SqlCommand(query);
                DataTable dt = DBEngine.GetDataTable(cmd, Databaseoperations.Select, query);

                var assessments = new List<AssessmentListModel>();
                foreach (DataRow row in dt.Rows)
                {
                    assessments.Add(new AssessmentListModel
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        UserName = row["UserName"]?.ToString(),
                        PersonalityType = row["PersonalityType"]?.ToString(),
                        Score = row["Score"] != DBNull.Value ? Convert.ToInt32(row["Score"]) : 0,
                        CompletedDate = row["CompletedDate"]?.ToString()
                    });
                }

                return assessments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessments");
                throw;
            }
        }

        public async Task<AssessmentDetailModel?> GetAssessmentAsync(int assessmentId)
        {
            try
            {
                string query = @"
                    SELECT 
                        A.Id,
                        A.UserId,
                        ISNULL(U.FullName, 'Guest') as UserName,
                        A.PersonalityType,
                        A.TotalScore,
                        A.Answer,
                        A.CompletedAt,
                        A.Description
                    FROM Assesments A
                    LEFT JOIN Users U ON A.UserId = U.Id
                    WHERE A.Id = @AssessmentId";

                SqlCommand cmd = new SqlCommand(query);
                cmd.Parameters.AddWithValue("@AssessmentId", assessmentId);
                DataTable dt = DBEngine.GetDataTable(cmd, Databaseoperations.Select, query);

                if (dt.Rows.Count == 0)
                    return null;

                var row = dt.Rows[0];
                return new AssessmentDetailModel
                {
                    Id = Convert.ToInt32(row["Id"]),
                    UserId = row["UserId"] != DBNull.Value ? Convert.ToInt32(row["UserId"]) : null,
                    UserName = row["UserName"]?.ToString(),
                    PersonalityType = row["PersonalityType"]?.ToString(),
                    TotalScore = row["TotalScore"] != DBNull.Value ? Convert.ToInt32(row["TotalScore"]) : 0,
                    Answers = row["Answer"]?.ToString(),
                    CompletedAt = row["CompletedAt"] != DBNull.Value ? Convert.ToDateTime(row["CompletedAt"]) : null,
                    Description = row["Description"]?.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assessment {AssessmentId}", assessmentId);
                throw;
            }
        }

        public async Task<List<ResumeListModel>> GetResumesAsync()
        {
            try
            {
                string query = @"
                    SELECT 
                        R.Id,
                        ISNULL(U.FullName, 'Guest') as UserName,
                        SUBSTRING(R.FileURL, CHARINDEX('\', REVERSE(R.FileURL)) + 1, LEN(R.FileURL)) as FileName,
                        R.FileURL,
                        CASE WHEN R.Analysis IS NOT NULL AND R.Analysis != '' THEN 'Analyzed' ELSE 'Pending' END as Status,
                        CONVERT(varchar, R.UploadedAt, 23) as UploadDate,
                        R.ParsedSkills
                    FROM Resumes R
                    LEFT JOIN Users U ON R.UserId = U.Id
                    ORDER BY R.UploadedAt DESC";

                SqlCommand cmd = new SqlCommand(query);
                DataTable dt = DBEngine.GetDataTable(cmd, Databaseoperations.Select, query);

                var resumes = new List<ResumeListModel>();
                foreach (DataRow row in dt.Rows)
                {
                    var skills = new List<string>();
                    var parsedSkills = row["ParsedSkills"]?.ToString();
                    if (!string.IsNullOrEmpty(parsedSkills))
                    {
                        skills = parsedSkills.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                    }

                    resumes.Add(new ResumeListModel
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        UserName = row["UserName"]?.ToString(),
                        FileName = row["FileName"]?.ToString(),
                        FileURL = row["FileURL"]?.ToString(),
                        Status = row["Status"]?.ToString() ?? "Pending",
                        UploadDate = row["UploadDate"]?.ToString(),
                        Skills = skills
                    });
                }

                return resumes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resumes");
                throw;
            }
        }

        public async Task<List<CareerModel>> GetCareersAsync()
        {
            try
            {
                string query = "SELECT Id, Name, Description, RequiredSkills, PersonalityMatchs FROM Careers ORDER BY Name";
                SqlCommand cmd = new SqlCommand(query);
                DataTable dt = DBEngine.GetDataTable(cmd, Databaseoperations.Select, query);

                var careers = new List<CareerModel>();
                foreach (DataRow row in dt.Rows)
                {
                    var requiredSkills = new List<string>();
                    var personalityMatchs = new List<string>();

                    var skillsStr = row["RequiredSkills"]?.ToString();
                    if (!string.IsNullOrEmpty(skillsStr))
                    {
                        try
                        {
                            requiredSkills = JsonConvert.DeserializeObject<List<string>>(skillsStr) ?? new List<string>();
                        }
                        catch
                        {
                            // If not JSON, treat as comma-separated
                            requiredSkills = skillsStr.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                        }
                    }

                    var traitsStr = row["PersonalityMatchs"]?.ToString();
                    if (!string.IsNullOrEmpty(traitsStr))
                    {
                        try
                        {
                            personalityMatchs = JsonConvert.DeserializeObject<List<string>>(traitsStr) ?? new List<string>();
                        }
                        catch
                        {
                            personalityMatchs = traitsStr.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                        }
                    }

                    careers.Add(new CareerModel
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = row["Name"]?.ToString(),
                        Description = row["Description"]?.ToString(),
                        RequiredSkills = requiredSkills,
                        PersonalityMatchs = personalityMatchs
                    });
                }

                return careers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting careers");
                throw;
            }
        }

        public async Task<CareerModel?> CreateCareerAsync(CareerModel career)
        {
            try
            {
                string query = @"
                    INSERT INTO Careers (Name, Description, RequiredSkills, PersonalityMatchs)
                    VALUES (@Name, @Description, @RequiredSkills, @PersonalityMatchs);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                SqlCommand cmd = new SqlCommand(query);
                cmd.Parameters.AddWithValue("@Name", career.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", career.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@RequiredSkills", JsonConvert.SerializeObject(career.RequiredSkills ?? new List<string>()));
                cmd.Parameters.AddWithValue("@PersonalityMatchs", JsonConvert.SerializeObject(career.PersonalityMatchs ?? new List<string>()));

                var id = DBEngine.ExecuteScalar(cmd, Databaseoperations.Insert, query);
                career.Id = (int)id;
                return career;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating career");
                throw;
            }
        }

        public async Task<bool> UpdateCareerAsync(int id, CareerModel career)
        {
            try
            {
                string query = @"
                    UPDATE Careers 
                    SET Name = @Name, 
                        Description = @Description, 
                        RequiredSkills = @RequiredSkills, 
                        PersonalityMatchs = @PersonalityMatchs
                    WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Name", career.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", career.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@RequiredSkills", JsonConvert.SerializeObject(career.RequiredSkills ?? new List<string>()));
                cmd.Parameters.AddWithValue("@PersonalityMatchs", JsonConvert.SerializeObject(career.PersonalityMatchs ?? new List<string>()));

                return DBEngine.ExecuteNonQuery(cmd, Databaseoperations.Update, query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating career {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCareerAsync(int id)
        {
            try
            {
                string query = "DELETE FROM Careers WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query);
                cmd.Parameters.AddWithValue("@Id", id);

                return DBEngine.ExecuteNonQuery(cmd, Databaseoperations.Delete, query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting career {Id}", id);
                throw;
            }
        }

        public async Task<List<SkillModel>> GetSkillsAsync()
        {
            try
            {
                string query = @"
                    SELECT 
                        S.Id,
                        S.Name,
                        S.Category,
                        S.Description
                    FROM Skills S
                    ORDER BY S.Name";

                SqlCommand cmd = new SqlCommand(query);
                DataTable dt = DBEngine.GetDataTable(cmd, Databaseoperations.Select, query);

                var skills = new List<SkillModel>();
                foreach (DataRow row in dt.Rows)
                {
                    var skillId = Convert.ToInt32(row["Id"]);
                    var skillName = row["Name"]?.ToString();

                    // Get linked careers by checking which careers reference this skill
                    var linkedCareers = new List<string>();
                    if (!string.IsNullOrEmpty(skillName))
                    {
                        string careerQuery = @"
                            SELECT DISTINCT C.Name
                            FROM Careers C
                            WHERE C.RequiredSkills LIKE '%' + @SkillName + '%'
                            OR C.RequiredSkills LIKE '%' + @SkillNameLower + '%'";
                        SqlCommand careerCmd = new SqlCommand(careerQuery);
                        careerCmd.Parameters.AddWithValue("@SkillName", skillName);
                        careerCmd.Parameters.AddWithValue("@SkillNameLower", skillName.ToLower());
                        DataTable careerDt = DBEngine.GetDataTable(careerCmd, Databaseoperations.Select, careerQuery);
                        foreach (DataRow careerRow in careerDt.Rows)
                        {
                            var careerName = careerRow["Name"]?.ToString();
                            if (!string.IsNullOrEmpty(careerName))
                                linkedCareers.Add(careerName);
                        }
                    }

                    skills.Add(new SkillModel
                    {
                        Id = skillId,
                        Name = skillName,
                        Category = row["Category"]?.ToString(),
                        Description = row["Description"]?.ToString(),
                        LinkedCareers = linkedCareers
                    });
                }

                return skills;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting skills");
                throw;
            }
        }

        public async Task<SkillModel?> CreateSkillAsync(SkillModel skill)
        {
            try
            {
                string query = @"
                    INSERT INTO Skills (Name, Category, Description)
                    VALUES (@Name, @Category, @Description);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                SqlCommand cmd = new SqlCommand(query);
                cmd.Parameters.AddWithValue("@Name", skill.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Category", skill.Category ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", skill.Description ?? (object)DBNull.Value);

                var id = DBEngine.ExecuteScalar(cmd, Databaseoperations.Insert, query);
                skill.Id = (int)id;
                return skill;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating skill");
                throw;
            }
        }

        public async Task<bool> UpdateSkillAsync(int id, SkillModel skill)
        {
            try
            {
                string query = @"
                    UPDATE Skills 
                    SET Name = @Name, 
                        Category = @Category, 
                        Description = @Description
                    WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Name", skill.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Category", skill.Category ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", skill.Description ?? (object)DBNull.Value);

                return DBEngine.ExecuteNonQuery(cmd, Databaseoperations.Update, query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating skill {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteSkillAsync(int id)
        {
            try
            {
                // Delete from UserSkills first
                string query = "DELETE FROM UserSkills WHERE SkillId = @Id";
                SqlCommand cmd = new SqlCommand(query);
                cmd.Parameters.AddWithValue("@Id", id);
                DBEngine.ExecuteNonQuery(cmd, Databaseoperations.Delete, query);

                query = "DELETE FROM Skills WHERE Id = @Id";
                cmd = new SqlCommand(query);
                cmd.Parameters.AddWithValue("@Id", id);

                return DBEngine.ExecuteNonQuery(cmd, Databaseoperations.Delete, query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting skill {Id}", id);
                throw;
            }
        }

        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            try
            {
                string query = "SELECT [Key], [Value] FROM Setting WHERE IsActive = 1";
                SqlCommand cmd = new SqlCommand(query);
                DataTable dt = DBEngine.GetDataTable(cmd, Databaseoperations.Select, query);

                var settings = new Dictionary<string, string>();
                foreach (DataRow row in dt.Rows)
                {
                    var key = row["Key"]?.ToString();
                    var value = row["Value"]?.ToString();
                    if (!string.IsNullOrEmpty(key))
                    {
                        settings[key] = value ?? "";
                    }
                }

                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting settings");
                throw;
            }
        }

        public async Task<bool> UpdateSettingsAsync(Dictionary<string, string> settings)
        {
            try
            {
                foreach (var setting in settings)
                {
                    string query = @"
                        IF EXISTS (SELECT 1 FROM Setting WHERE [Key] = @Key)
                            UPDATE Setting SET [Value] = @Value, UpdatedOn = GETDATE() WHERE [Key] = @Key
                        ELSE
                            INSERT INTO Setting ([Key], [Value], IsActive, CreatedOn)
                            VALUES (@Key, @Value, 1, GETDATE())";

                    SqlCommand cmd = new SqlCommand(query);
                    cmd.Parameters.AddWithValue("@Key", setting.Key);
                    cmd.Parameters.AddWithValue("@Value", setting.Value);
                    DBEngine.ExecuteNonQuery(cmd, Databaseoperations.Update, query);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settings");
                throw;
            }
        }

        public async Task<List<TopCareerModel>> GetTopCareersAsync()
        {
            try
            {
                string query = @"
                    SELECT TOP 10
                        A.PersonalityType as CareerName,
                        COUNT(*) as MatchCount
                    FROM Assesments A
                    WHERE A.PersonalityType IS NOT NULL AND A.PersonalityType != ''
                    GROUP BY A.PersonalityType
                    ORDER BY MatchCount DESC";

                SqlCommand cmd = new SqlCommand(query);
                DataTable dt = DBEngine.GetDataTable(cmd, Databaseoperations.Select, query);

                var topCareers = new List<TopCareerModel>();
                foreach (DataRow row in dt.Rows)
                {
                    topCareers.Add(new TopCareerModel
                    {
                        CareerName = row["CareerName"]?.ToString(),
                        MatchCount = Convert.ToInt32(row["MatchCount"])
                    });
                }

                return topCareers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top careers");
                throw;
            }
        }

    }
}
