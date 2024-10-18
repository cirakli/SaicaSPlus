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

        public int ESK_Sevk_Sheet_m2 { get; set; } = 0;  //Eskişehir Sheet m2

        public int ESK_Sevk_Box_m2 { get; set; } = 0;  //Eskişehir Box m2

        public int ESK_Sevk_Merch_m2 { get; set; } = 0;  //Eskişehir Merch m2

        public int ESK_Sevk_Konsinye_m2 { get; set; } = 0;  //Eskişehir Konsinye m2

        public int ESK_Sevk_Total_m2 { get; set; } = 0;  //Eskişehir Total m2

        public int ESK_Sevk_Box_tl { get; set; } = 0;  //Eskişehir Box TL

        public int ESK_Sevk_Sheet_tl { get; set; } = 0;  //Eskişehir Sheet TL

        public int ESK_Sevk_Merch_tl { get; set; } = 0;  //Eskişehir Merch TL

        public int ESK_Sevk_Konsinye_tl { get; set; } = 0;  //Eskişehir Konsinye TL

        public int ESK_Sevk_Total_tl { get; set; } = 0;  //Eskişehir Total TL

        public decimal ESK_Sevk_tl_m2 { get; set; } = 0;  //Eskişehir Sevk TL/m2

        public int SAK_Uretim_m2 { get; set; } = 0;  //Sakarya Uretim m2

        public int SAK_Kagit_kg { get; set; } = 0;  //Sakarya Kağıt Stok kg

        public int SAK_Kagit_Sarf { get; set; } = 0; //Sakarya Kağıt Stok sarf

        public int SAK_Sheet_m2 { get; set; } = 0; //Sakarya Sheet Sipariş Miktarı

        public int SAK_Box_m2 { get; set; } = 0; //Sakarya Box Sipariş Miktarı

        public int SAK_Merch_m2 { get; set; } = 0; //Sakarya Merch Sipariş Miktarı

        public int SAK_Total_m2 { get; set; } = 0; //Sakarya Toplam Sipariş Miktarı

        public int SAK_box_tl { get; set; } = 0; //Sakarya Box Sipariş TL

        public int SAK_sheet_tl { get; set; } = 0; //Sakarya Sheet Sipariş TL

        public int SAK_merch_tl { get; set; } = 0; //Sakarya Merch Sipariş TL

        public int SAK_total_tl { get; set; } = 0; //Sakarya Toplam Sipariş TL

        public decimal SAK_tl_m2 { get; set; } = 0; //Sakarya Sipariş TL/m2

        public int SAK_Sevk_Sheet_m2 { get; set; } = 0; //Sakarya Sevk Sheet m2

        public int SAK_Sevk_Box_m2 { get; set; } = 0; //Sakarya Sevk Box m2

        public int SAK_Sevk_Merch_m2 { get; set; } = 0; //Sakarya Sevk Merch m2

        public int SAK_Sevk_Konsinye_m2 { get; set; } = 0; //Sakarya Sevk Konsinye m2

        public int SAK_Sevk_Total_m2 { get; set; } = 0; //Sakarya Sevk Toplam m2

        public int SAK_Sevk_box_tl { get; set; } = 0; //Sakarya Sevk Box tl

        public int SAK_Sevk_sheet_tl { get; set; } = 0; //Sakarya Sevk Sheet tl

        public int SAK_Sevk_merch_tl { get; set; } = 0; //Sakarya Sevk Merch tl

        public int SAK_Sevk_Konsinye_tl { get; set; } = 0; //Sakarya Sevk Konsinye tl

        public int SAK_Sevk_total_tl { get; set; } = 0; //Sakarya Sevk Toplam tl

        public decimal SAK_Sevk_tl_m2 { get; set; } = 0; //Sakarya Sevk TL/m2


    }
}
