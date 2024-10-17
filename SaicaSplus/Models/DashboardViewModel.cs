namespace SaicaSplus.Models
{
    public class DashboardViewModel
    {
        public int Makine_KG { get; set; } = 0; // BHS Üretimi
        public int ESK_Kagit_stok { get; set; } = 0; // ESK Kağıt Stok

        public decimal ESK_kagit_sarf { get; set; } = 0; // ESK Kağıt Sarf

        public decimal BHS_KG { get; set; } = 0; // ESK Kağıt Sarf

        public int ESK_SAK_SEVK { get; set; } = 0; // ESK'den SAK'a sevkiyat miktarı

        public int ESK_SAK_SEVK_TL { get; set; } = 0; // ESK'den SAK'a sevkiyat TL

        // ESK Açık Sipariş miktarları
        public int ESK_Sheet_m2 { get; set; } = 0; // Levha
        public int ESK_Box_m2 { get; set; } = 0; // Kutu
        public int ESK_Merch_m2 { get; set; } = 0; // Ticari
        public int ESK_Total_m2 { get; set; } = 0; // Eskişehir Sipariş Toplamları

        public int ESK_Box_tl { get; set; } = 0; //Eskişehir Box TL

        public int ESK_Sheet_tl { get; set; } = 0;  //Eskişehir Sheet TL

        public int ESK_Merch_tl {  get; set; } = 0; //Eskişehir Merch TL

        public int ESK_Total_tl { get; set; } = 0;  //Eskişehir Total TL

        public decimal ESK_tl_m2 { get; set; } = 0; //Eskişehir TL/m2

    }
}
