using Microsoft.Data.SqlClient;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("SplusDb");
    }

    public List<string> GetUserDomains()
    {
        var userDomains = new List<string>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT s_user_domain FROM s_user where aktif=12", connection);
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
}
