using CareerNexus.Common;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace CareerNexus.Services.CareerRecommendation
{
    public class CareerRecommendationService:ICareerRecommendationService
    {
        public async Task<List<string>> RecommendCareersAsync(List<string> skills)
        {
            if (skills == null || skills.Count == 0) return new List<string>();

            // build LIKE clauses safely
            var query = new StringBuilder("SELECT Name, RequiredSkills FROM Careers WHERE ");
            var parameters = new List<SqlParameter>();
            for (int i = 0; i < skills.Count; i++)
            {
                if (i > 0) query.Append(" OR ");
                query.Append("RequiredSkills LIKE @p" + i);
                parameters.Add(new SqlParameter("@p" + i, "%" + skills[i] + "%"));
            }

            //using var conn = new SqlConnection(_connectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query.ToString();
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddRange(parameters.ToArray());
            //await conn.OpenAsync();
            using var reader = await DBEngine.ExecuteReaderAsync(cmd, Databaseoperations.Select, query.ToString());

            var recommendations = new List<string>();
            while (await reader.ReadAsync())
            {
                recommendations.Add(reader["Name"].ToString());
            }
            return recommendations;
        }

    }
}
