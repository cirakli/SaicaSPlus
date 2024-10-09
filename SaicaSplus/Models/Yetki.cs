namespace SaicaSplus.Models
{
    public class Yetki
    {
        public int Id { get; set; }
        public int SUserId { get; set; } // s_user_id
        public int UserId { get; set; } // s_user_id
        public int EkranId { get; set; } // s_ekran_id
        public int IslemId { get; set; } // s_islem_id

        public bool Izin { get; set; }
    }
}
