using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

public class UserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly string _connectionString;

    public UserRepository(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("SplusDb");
    }

    public List<string> GetUserDomains()
    {
        var userDomains = new List<string>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            var command = new SqlCommand("SELECT s_user_domain FROM s_user where aktif=1", connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    userDomains.Add(reader.GetString(0));
                }
            }
        }

        return userDomains;
    }

    public bool HasPermission(string username, string ekranAd)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // İlk olarak kullanıcının ID'sini almak için sorgu
            var userId = (int?)null;
            var userIdCommand = new SqlCommand("SELECT s_user_id FROM s_user WHERE s_user_domain = '" + username + "'", connection);
            userIdCommand.Parameters.AddWithValue("@Username", username);

            var result = userIdCommand.ExecuteScalar();
            if (result != null)
            {
                userId = Convert.ToInt32(result);
            }

            if (userId == null) return false;

            // İkinci olarak ekran ID'sini almak için sorgu
            var ekranId = (int?)null;
            var ekranIdCommand = new SqlCommand("SELECT ekran_id FROM s_ekran WHERE ekran_ad = '" + ekranAd + "'", connection);
            ekranIdCommand.Parameters.AddWithValue("@EkranAd", ekranAd);

            result = ekranIdCommand.ExecuteScalar();
            if (result != null)
            {
                ekranId = Convert.ToInt32(result);
            }

            if (ekranId == null) return false;

            // Son olarak yetki kontrolü
            var hasPermission = false;
            var permissionCommand = new SqlCommand("SELECT COUNT(*) FROM s_yetki WHERE  s_user_id = '" + userId + "' AND ekran_id = '" + ekranId + "' AND izin = 1", connection);
            permissionCommand.Parameters.AddWithValue("@UserId", userId);
            permissionCommand.Parameters.AddWithValue("@EkranId", ekranId);

            var count = (int)permissionCommand.ExecuteScalar();
            hasPermission = count > 0;

            return hasPermission;
        }
    }


}
