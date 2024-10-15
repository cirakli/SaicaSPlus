using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SaicaSplus.Services // Namespace projenizin ismine göre ayarlanmalı
{
    public class DashboardService
    {
        private readonly string _connectionString;

        public DashboardService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SplusDb");
        }

        public decimal GetTotalMiktar()
        {
            decimal totalMiktar = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT SUM(dashbord_makina_uretim.miktar) AS [Miktar]
                    FROM [bobin_durum].[dbo].[dashbord_makina_uretim] dashbord_makina_uretim
                    INNER JOIN [uretim].[dbo].[lkmakina] lkmakina ON lkmakina.lkmakina_id = dashbord_makina_uretim.lkmakina_id
                    INNER JOIN [uretim].[dbo].[ms_fabrika] ms_fabrika ON ms_fabrika.fabrika_id = lkmakina.fabrika_id
                    WHERE dashbord_makina_uretim.lkmakina_ad != 'BHS' 
                    AND dashbord_makina_uretim.lkmakina_ad != 'FASON' 
                    AND ms_fabrika.fabrika_id = 26";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    var result = command.ExecuteScalar();
                    totalMiktar = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }

            return totalMiktar;
        }
    }
}
