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

    public List<string> GetUserDomains(string username)
    {

        var userDomains = new List<string>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            var command = new SqlCommand("SELECT s_user_domain FROM s_user where aktif=1 and s_user_domain = '" + username + "' ", connection);
            command.Parameters.AddWithValue("@Username", username); // Kullanıcı adını parametre olarak ekle

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    userDomains.Add(reader.GetString(0));
                }
            }

            connection.Close();
        }

        return userDomains;
    }

    public async Task<bool> HasPermission(string username, string ekranAd)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false; // Kullanıcı adı geçersizse çık
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Kullanıcı ID'sini alma
            var userIdCommand = new SqlCommand("SELECT s_user_id FROM s_user WHERE s_user_domain = '" + username + "'", connection);
            userIdCommand.Parameters.AddWithValue("@Username", username);

            var result = await userIdCommand.ExecuteScalarAsync();
            if (result == null)
            {
                return false; // Kullanıcı adı bulunamadı
            }

            var userId = Convert.ToInt32(result);

            // Ekran ID'sini alma
            var ekranIdCommand = new SqlCommand("SELECT ekran_id FROM s_ekran WHERE ekran_ad = '" + ekranAd + "'", connection);
            ekranIdCommand.Parameters.AddWithValue("@EkranAd", ekranAd);

            result = await ekranIdCommand.ExecuteScalarAsync();
            if (result == null)
            {
                return false; // Ekran bulunamadı
            }

            var ekranId = Convert.ToInt32(result);

            // Yetki kontrolü
            var permissionCommand = new SqlCommand("SELECT COUNT(*) FROM s_yetki WHERE s_user_id = '" + userId + "' AND ekran_id = '" + ekranId + "' AND izin = 1", connection);
            permissionCommand.Parameters.AddWithValue("@UserId", userId);
            permissionCommand.Parameters.AddWithValue("@EkranId", ekranId);

            var count = (int)await permissionCommand.ExecuteScalarAsync();
            return count > 0;
        }
    }


    public (string FirstName, string LastName) GetUserDetails(string username)
    {
        string firstName = string.Empty;
        string lastName = string.Empty;

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            var command = new SqlCommand("SELECT s_user_ad, s_user_soyad FROM s_user WHERE s_user_domain = @Username", connection);
            command.Parameters.AddWithValue("@Username", username);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    firstName = reader["s_user_ad"].ToString();
                    lastName = reader["s_user_soyad"].ToString();
                }
            }

            connection.Close();
        }

        return (firstName, lastName);
    }


}
