using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SaicaSplus.Models;
using System.Reflection.PortableExecutable;
using System.Data;

namespace SaicaSplus.Controllers
{
    public class DashboardController : Controller
    {
        private readonly string _connectionString;
        private readonly UserRepository _userRepository;

        public DashboardController(IConfiguration configuration, UserRepository userRepository)
        {
            _connectionString = configuration.GetConnectionString("SplusDb");
            _userRepository = userRepository; // UserRepository'yi al
        }

        public async Task<IActionResult> Index()
        {
            // Kullanıcı adını oturumdan al
            var username = HttpContext.Session.GetString("Username");

            if (username != null)
            {
                // Kullanıcı bilgilerini al
                var userDetails = _userRepository.GetUserDetails(username);

                ViewBag.FirstName = userDetails.FirstName;
                ViewBag.LastName = userDetails.LastName;
            }

            // Kullanıcı ve ekran kontrolü
            if (!await _userRepository.HasPermission(username, "dashboard"))
            {
                return RedirectToAction("index", "yetki_yok");
            }


            // Makine üretim miktarı sorgusu
            string productionQuery = @"
                SELECT 
                    dashbord_makina_uretim.lkmakina_ad,
                    SUM(dashbord_makina_uretim.miktar) AS Miktar
                FROM 
                    [splus].[dbo].[dashbord_makina_uretim] dashbord_makina_uretim
                INNER JOIN 
                    [uretim].[dbo].[lkmakina] lkmakina ON lkmakina.lkmakina_id = dashbord_makina_uretim.lkmakina_id
                INNER JOIN 
                    [uretim].[dbo].[ms_fabrika] ms_fabrika ON ms_fabrika.fabrika_id = lkmakina.fabrika_id
                WHERE 
                    dashbord_makina_uretim.lkmakina_ad = 'BHS'
                GROUP BY 
                    dashbord_makina_uretim.lkmakina_ad;";

            // Kağıt stok sorgusu
            string esk_stockQuery = @"
                DECLARE @bugun DATE, @buay DATE, @ay INT, @yil INT, @dun DATE
                SET @dun = DATEADD(dd, -1, GETDATE())
                SET @ay = DATEPART(mm, @dun)
                SET @yil = DATEPART(yy, @dun)

                SELECT 
                    SUM(COALESCE(CASE WHEN CAST(stok_ay.kalan AS INT) IS NULL THEN 0 ELSE CAST(stok_ay.kalan AS INT) END, 0)) AS [Stok]
                FROM uretim.dbo.stok_ay
                INNER JOIN uretim.dbo.stok_ana ON stok_ay.stok_ana_id = stok_ana.stok_ana_id
                INNER JOIN uretim.dbo.masraf_ana ON stok_ay.ambar_no = masraf_ana_id
                INNER JOIN uretim.dbo.cins ON stok_ana.cins_id = cins.cins_id
                WHERE stok_ay.ay = @ay AND stok_ay.yil = @yil AND stok_ana.kagitmi = 32048 AND masraf_ana.fabrika_id = 26;";

            // Eskişehir Kağıt Sarf Miktarı
            string esk_kagit_sarf = @"Declare @bugun date
declare @buay date
declare @ay date
Declare @gun int
Declare @dun date
Declare @dungun int

set @dun= dateadd(dd,-1,getdate())
set @dungun= datepart(dd,@dun)
set @ay= dateadd(dd,-@dungun,getdate())

SELECT
			sum(a.miktar) as [Sarf] 
FROM            
uretim.dbo.bobin with(nolock) , uretim.dbo.stok_hareket a with(nolock),uretim.dbo.stok_ana b with(nolock)

		inner join
			uretim.dbo.cins on b.cins_id = cins.cins_id 

WHERE bobin.fabrika_id = 26

and bobin.bobin_id = a.bobin_id
and a.stok_ana_id =b.stok_ana_id
and  a.hareket_tip  = 556  
and  convert(date,a.tarih) >= @ay
and  convert(date,a.tarih) <= @dun;";


            // Eskişehir BHS üretim miktarı
            string BHS_uretim = @"SELECT 
Sum(cast(dashbord_makina_uretim.miktar as int )) as [Miktar]

  FROM [splus].[dbo].[dashbord_makina_uretim] dashbord_makina_uretim
inner join [uretim].[dbo].[lkmakina] lkmakina on lkmakina.lkmakina_id=dashbord_makina_uretim.lkmakina_id
inner join [uretim].[dbo].[ms_fabrika] ms_fabrika on ms_fabrika.fabrika_id = lkmakina.fabrika_id
where dashbord_makina_uretim.lkmakina_ad ='BHS';";

            // Eskişehir'den Sakarya'ya Sevk Edilen'
            string esk_to_sak = @"DECLARE @bugun DATE
DECLARE @buay DATE
DECLARE @ay DATE
DECLARE @gun INT
DECLARE @dun DATE
DECLARE @dungun INT

SET @dun = DATEADD(dd, -1, GETDATE())
SET @dungun = DATEPART(dd, @dun)
SET @ay = DATEADD(dd, -@dungun, GETDATE())

SELECT
    sum(cast (urun.uretim_alan * irsaliye_detay.irsaliye_miktar as decimal(15,0))) as [m2],
    sum(cast (siparis_detay.satis_fiyat * irsaliye_detay.irsaliye_miktar as decimal(15,0))) as [Tutar]
FROM [uretim].[dbo].[irsaliye_detay]
INNER JOIN uretim.dbo.irsaliye ON irsaliye.irsaliye_id = irsaliye_detay.irsaliye_id
INNER JOIN uretim.dbo.siparis_detay ON siparis_detay.siparis_detay_id = irsaliye_detay.siparis_Detay_id
INNER JOIN uretim.dbo.urun ON irsaliye_detay.urun_id = urun.urun_id
INNER JOIN uretim.dbo.urun_sinif ON urun.urun_sinif_id = urun_sinif.urun_sinif_id
INNER JOIN uretim.dbo.ms_fabrika ON irsaliye.fabrika_id = ms_fabrika.fabrika_id
INNER JOIN uretim.dbo.firma ON irsaliye.firma_id = firma.firma_id
WHERE (irsaliye_detay.irsaliye_tarih BETWEEN @ay AND @dun)
  AND irsaliye_detay.siparis_Detay_id != 0
  AND (irsaliye_detay.irsaliye_tip BETWEEN '460' AND '463')
  AND irsaliye_detay.irsaliye_tip != '461'
  AND irsaliye_detay.irsaliye_tip != '462'
  AND irsaliye.fabrika_id = 26
  AND firma.firma_kodu LIKE 'SPSAM'";

            // Eskişehir Sipariş Miktarı
            string esk_siparis_miktarı = @"DECLARE @bugun DATE;
DECLARE @buay DATE;
DECLARE @ay DATE;
DECLARE @gun INT;
DECLARE @dun DATE;
DECLARE @dungun INT;
DECLARE @songun DATE;

SET @dun = DATEADD(dd, -1, GETDATE());
SET @dungun = DATEPART(dd, @dun);
SET @ay = DATEADD(dd, -@dungun, GETDATE());
SET @songun = DATEADD(dd, -1, DATEADD(mm, DATEDIFF(mm, 0, GETDATE()) + 1, 0));

-- İlk olarak m2 için veriler toplanıyor
;WITH m2_pivot AS (
    SELECT 
        CASE
            WHEN urun_sinif.kod = 40 THEN 'LEVHA'
            WHEN urun_sinif.kod = 06 THEN 'LEVHA'
            WHEN urun_sinif.kod = 93 THEN 'TİCARİ'
            ELSE 'KUTU'
        END AS 'Sınıf',
        SUM(CAST(urun.uretim_alan * siparis_detay.siparis_miktar AS REAL)) AS [m2]
    FROM [uretim].[dbo].[siparis_detay]
    INNER JOIN uretim.dbo.urun ON siparis_detay.urun_id = urun.urun_id
    INNER JOIN uretim.dbo.siparis ON siparis_detay.siparis_id = siparis.siparis_id
    INNER JOIN uretim.dbo.urun_sinif ON urun.urun_sinif_id = urun_sinif.urun_sinif_id
    INNER JOIN uretim.dbo.ms_fabrika ON siparis.fabrika_id = ms_fabrika.fabrika_id
    WHERE 
        (siparis_detay.termin_tarih BETWEEN @ay AND @songun)
        AND (siparis_detay.siparis_tip BETWEEN '150' AND '154')
        AND siparis_detay.siparis_tip != '151'
        AND siparis_detay.acma_kapama NOT IN ('140', '143', '144', '145')
        AND siparis.fabrika_id = 26
    GROUP BY
        CASE
            WHEN urun_sinif.kod = 40 THEN 'LEVHA'
            WHEN urun_sinif.kod = 06 THEN 'LEVHA'
            WHEN urun_sinif.kod = 93 THEN 'TİCARİ'
            ELSE 'KUTU'
        END
),
-- İkinci olarak Tutar için veriler toplanıyor
tutar_pivot AS (
    SELECT 
        CASE
            WHEN urun_sinif.kod = 40 THEN 'LEVHA'
            WHEN urun_sinif.kod = 06 THEN 'LEVHA'
            WHEN urun_sinif.kod = 93 THEN 'TİCARİ'
            ELSE 'KUTU'
        END AS 'Sınıf',
        SUM(CAST(siparis_detay.satis_fiyat * siparis_detay.siparis_miktar AS REAL)) AS [Tutar]
    FROM [uretim].[dbo].[siparis_detay]
    INNER JOIN uretim.dbo.urun ON siparis_detay.urun_id = urun.urun_id
    INNER JOIN uretim.dbo.siparis ON siparis_detay.siparis_id = siparis.siparis_id
    INNER JOIN uretim.dbo.urun_sinif ON urun.urun_sinif_id = urun_sinif.urun_sinif_id
    INNER JOIN uretim.dbo.ms_fabrika ON siparis.fabrika_id = ms_fabrika.fabrika_id
    WHERE 
        (siparis_detay.termin_tarih BETWEEN @ay AND @songun)
        AND (siparis_detay.siparis_tip BETWEEN '150' AND '154')
        AND siparis_detay.siparis_tip != '151'
        AND siparis_detay.acma_kapama NOT IN ('140', '143', '144', '145')
        AND siparis.fabrika_id = 26
    GROUP BY
        CASE
            WHEN urun_sinif.kod = 40 THEN 'LEVHA'
            WHEN urun_sinif.kod = 06 THEN 'LEVHA'
            WHEN urun_sinif.kod = 93 THEN 'TİCARİ'
            ELSE 'KUTU'
        END
)

-- İki veriyi INNER JOIN ile birleştiriyoruz ve her sınıf için tekil değerler alıyoruz
SELECT 
    cast(ISNULL(SUM(CASE WHEN t.Sınıf = 'KUTU' THEN t.Tutar ELSE 0 END), 0)as int) AS [BOX_TL],
    cast(ISNULL(SUM(CASE WHEN t.Sınıf = 'LEVHA' THEN t.Tutar ELSE 0 END), 0)as int) AS [SHEET_TL],
    cast(ISNULL(SUM(CASE WHEN t.Sınıf = 'TİCARİ' THEN t.Tutar ELSE 0 END), 0)as int) AS [Merch_TL],
    cast(ISNULL(SUM(CASE WHEN t.Sınıf IN ('KUTU', 'LEVHA', 'TİCARİ') THEN t.Tutar ELSE 0 END), 0)as int) AS [Total_TL],

    cast(ISNULL(SUM(CASE WHEN m.Sınıf = 'KUTU' THEN m.m2 ELSE 0 END), 0)as int) AS [BOX_m2],
    cast(ISNULL(SUM(CASE WHEN m.Sınıf = 'LEVHA' THEN m.m2 ELSE 0 END), 0)as int) AS [SHEET_m2],
    cast(ISNULL(SUM(CASE WHEN m.Sınıf = 'TİCARİ' THEN m.m2 ELSE 0 END), 0)as int) AS [Merch_m2],
    cast(ISNULL(SUM(CASE WHEN m.Sınıf IN ('KUTU', 'LEVHA', 'TİCARİ') THEN m.m2 ELSE 0 END), 0)as int) AS [Total_m2],
    cast(ISNULL(SUM(CASE WHEN t.Sınıf IN ('KUTU', 'LEVHA', 'TİCARİ') THEN t.Tutar ELSE 0 END), 0) / NULLIF(ISNULL(SUM(CASE WHEN m.Sınıf IN ('KUTU', 'LEVHA', 'TİCARİ') THEN m.m2 ELSE 0 END), 0), 0) AS decimal(15,2)) AS [TL/m2]
FROM tutar_pivot t
INNER JOIN m2_pivot m ON t.Sınıf = m.Sınıf;";

            // Eskişehir Sevk Miktarı
            string esk_sevk_miktarı = @"DECLARE @bugun DATE
DECLARE @buay DATE
DECLARE @ay DATE
DECLARE @gun INT
DECLARE @dun DATE
DECLARE @dungun INT

SET @dun = DATEADD(dd, -1, GETDATE())
SET @dungun = DATEPART(dd, @dun)
SET @ay = DATEADD(dd, -@dungun, GETDATE())

-- Hem m2 hem de Tutar verilerini birleştiren sorgu
SELECT 
    cast(ISNULL(m2.[LEVHA], 0) as int) AS [SHEET_m2], 
    cast(ISNULL(m2.[KUTU], 0) as int) AS [BOX_m2],
    cast(ISNULL(m2.[TİCARİ], 0) as int) AS [Ticari_m2],
    cast(ISNULL(m2.[KONSİNYE], 0) as int) AS [Konsinye_m2],
	cast(ISNULL(m2.[KONSİNYE], 0) + ISNULL(m2.[TİCARİ], 0) +ISNULL(m2.[KUTU], 0) + ISNULL(m2.[LEVHA], 0)as int) as [Toplam m2], 
    cast(ISNULL(tutar.[LEVHA], 0) as int) AS [SHEET_Tutar], 
    cast(ISNULL(tutar.[KUTU], 0) as int) AS [BOX_Tutar],
    cast(ISNULL(tutar.[TİCARİ], 0) as int) AS [Ticari_Tutar],
    cast(ISNULL(tutar.[KONSİNYE], 0) as int) AS [Konsinye_Tutar],
	cast(ISNULL(tutar.[LEVHA], 0) +  ISNULL(tutar.[KUTU], 0) + ISNULL(tutar.[TİCARİ], 0) + ISNULL(tutar.[KONSİNYE], 0) as int) as [Toplam Tutar],
	CAST((ISNULL(tutar.[LEVHA], 0) +  ISNULL(tutar.[KUTU], 0) + ISNULL(tutar.[TİCARİ], 0) + ISNULL(tutar.[KONSİNYE], 0)) / (ISNULL(m2.[KONSİNYE], 0) + ISNULL(m2.[TİCARİ], 0) +ISNULL(m2.[KUTU], 0) + ISNULL(m2.[LEVHA], 0)) as decimal(15,2)) as [TL/m2]
FROM (
    -- m2 hesaplaması için ilk alt sorgu
    SELECT 
        CASE
            WHEN irsaliye_detay.irsaliye_tip = 461 THEN 'KONSİNYE'
            WHEN urun_sinif.kod = 40 THEN 'LEVHA'
            WHEN urun_sinif.kod = 06 THEN 'LEVHA'
            WHEN urun_sinif.kod = 93 THEN 'TİCARİ'
            ELSE 'KUTU' 
        END AS 'Sınıf',
        CAST(urun.uretim_alan * irsaliye_detay.irsaliye_miktar AS REAL) AS [m2]
    FROM [uretim].[dbo].[irsaliye_detay]
    INNER JOIN uretim.dbo.irsaliye ON irsaliye.irsaliye_id = irsaliye_detay.irsaliye_id
    INNER JOIN uretim.dbo.siparis_detay ON siparis_detay.siparis_detay_id = irsaliye_detay.siparis_Detay_id
    INNER JOIN uretim.dbo.urun ON irsaliye_detay.urun_id = urun.urun_id
    INNER JOIN uretim.dbo.urun_sinif ON urun.urun_sinif_id = urun_sinif.urun_sinif_id
    INNER JOIN uretim.dbo.ms_fabrika ON irsaliye.fabrika_id = ms_fabrika.fabrika_id
    WHERE (irsaliye_detay.irsaliye_tarih BETWEEN @ay AND @dun) 
        AND irsaliye_detay.siparis_Detay_id != 0 
        AND (irsaliye_detay.irsaliye_tip BETWEEN '460' AND '463') 
        AND irsaliye_detay.irsaliye_tip != '462' 
        AND irsaliye.fabrika_id = 26
) AS sevkiyat_rapor_m2
PIVOT (
    SUM([m2]) FOR Sınıf IN ([KUTU], [LEVHA], [TİCARİ], [KONSİNYE])
) AS m2

FULL OUTER JOIN (
    -- Tutar hesaplaması için ikinci alt sorgu
    SELECT 
        CASE
            WHEN irsaliye_detay.irsaliye_tip = 461 THEN 'KONSİNYE'
            WHEN urun_sinif.kod = 40 THEN 'LEVHA'
            WHEN urun_sinif.kod = 06 THEN 'LEVHA'
            WHEN urun_sinif.kod = 93 THEN 'TİCARİ'
            ELSE 'KUTU' 
        END AS 'Sınıf',
        CAST(siparis_detay.satis_fiyat * irsaliye_detay.irsaliye_miktar AS REAL) AS [Tutar]
    FROM [uretim].[dbo].[irsaliye_detay]
    INNER JOIN uretim.dbo.irsaliye ON irsaliye.irsaliye_id = irsaliye_detay.irsaliye_id
    INNER JOIN uretim.dbo.siparis_detay ON siparis_detay.siparis_detay_id = irsaliye_detay.siparis_Detay_id
    INNER JOIN uretim.dbo.urun ON irsaliye_detay.urun_id = urun.urun_id
    INNER JOIN uretim.dbo.urun_sinif ON urun.urun_sinif_id = urun_sinif.urun_sinif_id
    INNER JOIN uretim.dbo.ms_fabrika ON irsaliye.fabrika_id = ms_fabrika.fabrika_id
    WHERE (irsaliye_detay.irsaliye_tarih BETWEEN @ay AND @dun) 
        AND irsaliye_detay.siparis_Detay_id != 0 
        AND (irsaliye_detay.irsaliye_tip BETWEEN '460' AND '463') 
        AND irsaliye_detay.irsaliye_tip != '462' 
        AND irsaliye.fabrika_id = 26
) AS sevkiyat_rapor_tutar
PIVOT (
    SUM([Tutar]) FOR Sınıf IN ([KUTU], [LEVHA], [TİCARİ], [KONSİNYE])
) AS tutar
ON 1 = 1;";



            // Sakarya Üretim Miktarı
            string sak_uretim = @"SELECT 

Sum(dashbord_makina_uretim.miktar) as [Miktar]

  FROM [Splus].[dbo].[dashbord_makina_uretim] dashbord_makina_uretim
inner join [uretim].[dbo].[lkmakina] lkmakina on lkmakina.lkmakina_id=dashbord_makina_uretim.lkmakina_id
inner join [uretim].[dbo].[ms_fabrika] ms_fabrika on ms_fabrika.fabrika_id = lkmakina.fabrika_id
where dashbord_makina_uretim.lkmakina_ad !='BHS' and dashbord_makina_uretim.lkmakina_ad !='FASON' and ms_fabrika.fabrika_id=54";


            // Sakarya Paper Stock
            string sak_paper_stock = @"Declare @bugun date
declare @buay date
declare @ay int
Declare @gun int
Declare @yil int
Declare @dun date

set @dun= dateadd(dd,-1,getdate())
set @ay= datepart(mm,@dun)
set @yil= datepart(yy,@dun)

SELECT 
	 sum(coalesce(case when cast (stok_ay.kalan as int) is NULL then 0 else cast (stok_ay.kalan as int) end,0)) as [Stok]
--COALESCE(Jan   ,0) Jan
--ISNULL (stok_ay.kalan,0) as [Kalan]

FROM            uretim.dbo.stok_ay INNER JOIN
                         uretim.dbo.stok_ana ON stok_ay.stok_ana_id = stok_ana.stok_ana_id INNER JOIN
						 uretim.dbo.masraf_ana on stok_ay.ambar_no = masraf_ana_id INNER JOIN
                         uretim.dbo.cins ON stok_ana.cins_id = cins.cins_id
						 where stok_ay.ay=@ay and stok_ay.yil=@yil and stok_ana.kagitmi=32048 and masraf_ana.fabrika_id=54";

            // Sakarya Kağıt Sarf
            string sak_kagit_sarf = @"Declare @bugun date
declare @buay date
declare @ay date
Declare @gun int
Declare @dun date
Declare @dungun int

set @dun= dateadd(dd,-1,getdate())
set @dungun= datepart(dd,@dun)
set @ay= dateadd(dd,-@dungun,getdate())

SELECT

			sum(cast(a.miktar as int)) as [Sarf] 
FROM            
uretim.dbo.bobin with(nolock) , uretim.dbo.stok_hareket a with(nolock),uretim.dbo.stok_ana b with(nolock)

		inner join
			uretim.dbo.cins on b.cins_id = cins.cins_id 

WHERE bobin.fabrika_id = 54

and bobin.bobin_id = a.bobin_id
and a.stok_ana_id =b.stok_ana_id
and  a.hareket_tip  = 556  --sarfiyat
and  convert(date,a.tarih) >= @ay
and  convert(date,a.tarih) <= @dun";

            // Sakarya Sipariş Miktarları
            string sak_siparis_miktar = @"DECLARE @bugun DATE
DECLARE @buay DATE
DECLARE @ay DATE
DECLARE @gun INT
DECLARE @dun DATE
DECLARE @dungun INT
DECLARE @songun DATE

SET @dun = DATEADD(dd, -1, GETDATE())
SET @dungun = DATEPART(dd, @dun)
SET @ay = DATEADD(dd, -@dungun, GETDATE())
SET @songun = DATEADD(dd, -1, DATEADD(mm, DATEDIFF(mm, 0, GETDATE()) + 1, 0))

-- m2 için PIVOT
;WITH PivotM2 AS (
    SELECT
        CASE
            WHEN urun_sinif.kod = 40 THEN 'LEVHA'
            WHEN urun_sinif.kod = 06 THEN 'LEVHA'
            WHEN urun_sinif.kod = 93 THEN 'TİCARİ'
        ELSE  
            'KUTU' 
        END AS Sınıf,
        CAST(urun.uretim_alan * siparis_detay.siparis_miktar AS REAL) AS m2
    FROM [uretim].[dbo].[siparis_detay]
    INNER JOIN uretim.dbo.urun ON siparis_detay.urun_id = urun.urun_id
    INNER JOIN uretim.dbo.siparis ON siparis_detay.siparis_id = siparis.siparis_id
    INNER JOIN uretim.dbo.urun_sinif ON urun.urun_sinif_id = urun_sinif.urun_sinif_id
    INNER JOIN uretim.dbo.ms_fabrika ON siparis.fabrika_id = ms_fabrika.fabrika_id
    WHERE 
        siparis_detay.termin_tarih BETWEEN @ay AND @songun
        AND siparis_detay.siparis_tip BETWEEN '150' AND '154'
        AND siparis_detay.siparis_tip != '151'
        AND siparis_detay.acma_kapama NOT IN ('140', '143', '144', '145')
        AND siparis.fabrika_id = 54
)
SELECT *
INTO #PivotM2
FROM PivotM2
PIVOT (
    SUM(m2) FOR Sınıf IN ([KUTU], [LEVHA], [TİCARİ])
) AS pivotsevkiyat_m2;

-- Tutar için PIVOT
;WITH PivotTutar AS (
    SELECT
        CASE
            WHEN urun_sinif.kod = 40 THEN 'LEVHA'
            WHEN urun_sinif.kod = 06 THEN 'LEVHA'
            WHEN urun_sinif.kod = 93 THEN 'TİCARİ'
        ELSE  
            'KUTU' 
        END AS Sınıf,
        CAST(siparis_detay.satis_fiyat * siparis_detay.siparis_miktar AS REAL) AS Tutar
    FROM [uretim].[dbo].[siparis_detay]
    INNER JOIN uretim.dbo.urun ON siparis_detay.urun_id = urun.urun_id
    INNER JOIN uretim.dbo.siparis ON siparis_detay.siparis_id = siparis.siparis_id
    INNER JOIN uretim.dbo.urun_sinif ON urun.urun_sinif_id = urun_sinif.urun_sinif_id
    INNER JOIN uretim.dbo.ms_fabrika ON siparis.fabrika_id = ms_fabrika.fabrika_id
    WHERE 
        siparis_detay.termin_tarih BETWEEN @ay AND @songun
        AND siparis_detay.siparis_tip BETWEEN '150' AND '154'
        AND siparis_detay.siparis_tip != '151'
        AND siparis_detay.acma_kapama NOT IN ('140', '143', '144', '145')
        AND siparis.fabrika_id = 54
)
SELECT *
INTO #PivotTutar
FROM PivotTutar
PIVOT (
    SUM(Tutar) FOR Sınıf IN ([KUTU], [LEVHA], [TİCARİ])
) AS pivotsevkiyat_tutar;

-- Sonuçları birleştiriyoruz
SELECT 
    cast(ISNULL(pivotsevkiyat_m2.[KUTU], 0) as int) AS [BOX_m2],
    cast(ISNULL(pivotsevkiyat_m2.[LEVHA], 0) as int) AS [SHEET_m2],
    cast(ISNULL(pivotsevkiyat_m2.[TİCARİ], 0) as int) AS [Ticari_m2],
	cast((ISNULL(pivotsevkiyat_m2.[KUTU], 0)) + (ISNULL(pivotsevkiyat_m2.[LEVHA], 0))+ (ISNULL(pivotsevkiyat_m2.[TİCARİ], 0)) as int) as [Toplam m2],
    cast(ISNULL(pivotsevkiyat_tutar.[KUTU], 0) as int) AS [BOX_Tutar],
    cast(ISNULL(pivotsevkiyat_tutar.[LEVHA], 0) as int) AS [SHEET_Tutar],
    cast(ISNULL(pivotsevkiyat_tutar.[TİCARİ], 0) as int) AS [Ticari_Tutar],
	cast((ISNULL(pivotsevkiyat_tutar.[KUTU], 0)) + (ISNULL(pivotsevkiyat_tutar.[LEVHA], 0)) + (ISNULL(pivotsevkiyat_tutar.[TİCARİ], 0)) as int) as [Toplam Tutar],
	cast(((ISNULL(pivotsevkiyat_tutar.[KUTU], 0)) + (ISNULL(pivotsevkiyat_tutar.[LEVHA], 0)) + (ISNULL(pivotsevkiyat_tutar.[TİCARİ], 0))) / ((ISNULL(pivotsevkiyat_m2.[KUTU], 0)) + (ISNULL(pivotsevkiyat_m2.[LEVHA], 0))+ (ISNULL(pivotsevkiyat_m2.[TİCARİ], 0))) as decimal(15,2)) as [TL/m2]


FROM #PivotM2 pivotsevkiyat_m2
JOIN #PivotTutar pivotsevkiyat_tutar
ON 1 = 1;

-- Temp tabloları temizle
DROP TABLE #PivotM2;
DROP TABLE #PivotTutar;";

            // Sakarya Sipariş Miktarları
            string sak_sevk_miktar = @"DECLARE @bugun DATE
DECLARE @buay DATE
DECLARE @ay DATE
DECLARE @gun INT
DECLARE @dun DATE
DECLARE @dungun INT

SET @dun = DATEADD(dd, -1, GETDATE())
SET @dungun = DATEPART(dd, @dun)
SET @ay = DATEADD(dd, -@dungun, GETDATE())

-- Hem m2 hem de Tutar verilerini birleştiren sorgu
SELECT 
    cast(ISNULL(m2.[LEVHA], 0) as int) AS [SHEET_m2], 
    cast(ISNULL(m2.[KUTU], 0) as int) AS [BOX_m2],
    cast(ISNULL(m2.[TİCARİ], 0) as int) AS [Ticari_m2],
    cast(ISNULL(m2.[KONSİNYE], 0) as int) AS [Konsinye_m2],
	cast(ISNULL(m2.[KONSİNYE], 0) + ISNULL(m2.[TİCARİ], 0) +ISNULL(m2.[KUTU], 0) + ISNULL(m2.[LEVHA], 0)as int) as [Toplam m2], 
    cast(ISNULL(tutar.[LEVHA], 0) as int) AS [SHEET_Tutar], 
    cast(ISNULL(tutar.[KUTU], 0) as int) AS [BOX_Tutar],
    cast(ISNULL(tutar.[TİCARİ], 0) as int) AS [Ticari_Tutar],
    cast(ISNULL(tutar.[KONSİNYE], 0) as int) AS [Konsinye_Tutar],
	cast(ISNULL(tutar.[LEVHA], 0) +  ISNULL(tutar.[KUTU], 0) + ISNULL(tutar.[TİCARİ], 0) + ISNULL(tutar.[KONSİNYE], 0) as int) as [Toplam Tutar],
	CAST((ISNULL(tutar.[LEVHA], 0) +  ISNULL(tutar.[KUTU], 0) + ISNULL(tutar.[TİCARİ], 0) + ISNULL(tutar.[KONSİNYE], 0)) / (ISNULL(m2.[KONSİNYE], 0) + ISNULL(m2.[TİCARİ], 0) +ISNULL(m2.[KUTU], 0) + ISNULL(m2.[LEVHA], 0)) as decimal(15,2)) as [TL/m2]
FROM (
    -- m2 hesaplaması için ilk alt sorgu
    SELECT 
        CASE
            WHEN irsaliye_detay.irsaliye_tip = 461 THEN 'KONSİNYE'
            WHEN urun_sinif.kod = 40 THEN 'LEVHA'
            WHEN urun_sinif.kod = 06 THEN 'LEVHA'
            WHEN urun_sinif.kod = 93 THEN 'TİCARİ'
            ELSE 'KUTU' 
        END AS 'Sınıf',
        CAST(urun.uretim_alan * irsaliye_detay.irsaliye_miktar AS REAL) AS [m2]
    FROM [uretim].[dbo].[irsaliye_detay]
    INNER JOIN uretim.dbo.irsaliye ON irsaliye.irsaliye_id = irsaliye_detay.irsaliye_id
    INNER JOIN uretim.dbo.siparis_detay ON siparis_detay.siparis_detay_id = irsaliye_detay.siparis_Detay_id
    INNER JOIN uretim.dbo.urun ON irsaliye_detay.urun_id = urun.urun_id
    INNER JOIN uretim.dbo.urun_sinif ON urun.urun_sinif_id = urun_sinif.urun_sinif_id
    INNER JOIN uretim.dbo.ms_fabrika ON irsaliye.fabrika_id = ms_fabrika.fabrika_id
    WHERE (irsaliye_detay.irsaliye_tarih BETWEEN @ay AND @dun) 
        AND irsaliye_detay.siparis_Detay_id != 0 
        AND (irsaliye_detay.irsaliye_tip BETWEEN '460' AND '463') 
        AND irsaliye_detay.irsaliye_tip != '462' 
        AND irsaliye.fabrika_id = 54
) AS sevkiyat_rapor_m2
PIVOT (
    SUM([m2]) FOR Sınıf IN ([KUTU], [LEVHA], [TİCARİ], [KONSİNYE])
) AS m2

FULL OUTER JOIN (
    -- Tutar hesaplaması için ikinci alt sorgu
    SELECT 
        CASE
            WHEN irsaliye_detay.irsaliye_tip = 461 THEN 'KONSİNYE'
            WHEN urun_sinif.kod = 40 THEN 'LEVHA'
            WHEN urun_sinif.kod = 06 THEN 'LEVHA'
            WHEN urun_sinif.kod = 93 THEN 'TİCARİ'
            ELSE 'KUTU' 
        END AS 'Sınıf',
        CAST(siparis_detay.satis_fiyat * irsaliye_detay.irsaliye_miktar AS REAL) AS [Tutar]
    FROM [uretim].[dbo].[irsaliye_detay]
    INNER JOIN uretim.dbo.irsaliye ON irsaliye.irsaliye_id = irsaliye_detay.irsaliye_id
    INNER JOIN uretim.dbo.siparis_detay ON siparis_detay.siparis_detay_id = irsaliye_detay.siparis_Detay_id
    INNER JOIN uretim.dbo.urun ON irsaliye_detay.urun_id = urun.urun_id
    INNER JOIN uretim.dbo.urun_sinif ON urun.urun_sinif_id = urun_sinif.urun_sinif_id
    INNER JOIN uretim.dbo.ms_fabrika ON irsaliye.fabrika_id = ms_fabrika.fabrika_id
    WHERE (irsaliye_detay.irsaliye_tarih BETWEEN @ay AND @dun) 
        AND irsaliye_detay.siparis_Detay_id != 0 
        AND (irsaliye_detay.irsaliye_tip BETWEEN '460' AND '463') 
        AND irsaliye_detay.irsaliye_tip != '462' 
        AND irsaliye.fabrika_id = 54
) AS sevkiyat_rapor_tutar
PIVOT (
    SUM([Tutar]) FOR Sınıf IN ([KUTU], [LEVHA], [TİCARİ], [KONSİNYE])
) AS tutar
ON 1 = 1;";


            var productionResult = await ExecuteQueryForMiktar(productionQuery);
            var esk_stockResult = await ExecuteQueryForStok(esk_stockQuery);
            var esk_sarfResult = await ExecuteQueryForESK_Sarf(esk_kagit_sarf);
            var bhsuretimResult = await ExecuteQueryForBHS_uretim(BHS_uretim);
            var esk_sak_sevkResult = await ExecuteQueryForESK_to_SAK(esk_to_sak);
            var esk_sipResult = await ExecuteQueryForTableData(esk_siparis_miktarı);
            var esk_sevkResult = await ExecuteQueryForSevkTableData(esk_sevk_miktarı);
            var sak_uretimResult = await ExecuteQueryForSakUretim(sak_uretim);
            var sak_kagitResult = await ExecuteQueryForSakPaper(sak_paper_stock);
            var sak_kagit_sarfResult = await ExecuteQueryForSakKagitSarf(sak_kagit_sarf);
            var sak_siparis_miktarResult = await ExecuteQueryForSakSiparisMiktar(sak_siparis_miktar);
            var sak_sevk_miktarResult = await ExecuteQueryForSakSevkMiktar(sak_sevk_miktar);


            DashboardViewModel model = new DashboardViewModel()
            {
                Makine_KG = productionResult,
                ESK_Kagit_stok = esk_stockResult,
                ESK_Sheet_m2 = esk_sipResult.SHEET_m2,
                ESK_Box_m2 = esk_sipResult.BOX_m2,
                ESK_Merch_m2 = esk_sipResult.Merch_m2,
                ESK_Total_m2 = esk_sipResult.Total_m2,
                ESK_Box_tl = esk_sipResult.box_tl,
                ESK_Sheet_tl = esk_sipResult.sheet_tl,
                ESK_Merch_tl = esk_sipResult.merch_tl,
                ESK_Total_tl = esk_sipResult.total_tl,
                ESK_tl_m2 = esk_sipResult.tl_m2,
                ESK_kagit_sarf = esk_sarfResult,
                BHS_KG = bhsuretimResult,
                ESK_SAK_SEVK = esk_sak_sevkResult.m2,
                ESK_SAK_SEVK_TL = esk_sak_sevkResult.Tutar,
                ESK_Sevk_Sheet_m2 = esk_sevkResult.Sevk_SHEET_m2,
                ESK_Sevk_Box_m2 = esk_sevkResult.Sevk_BOX_m2,
                ESK_Sevk_Merch_m2 = esk_sevkResult.Sevk_Merch_m2,
                ESK_Sevk_Konsinye_m2 = esk_sevkResult.Sevk_Konsinye_m2,
                ESK_Sevk_Total_m2 = esk_sevkResult.Sevk_Total_m2,
                ESK_Sevk_Box_tl = esk_sevkResult.Sevk_box_tl,
                ESK_Sevk_Sheet_tl = esk_sevkResult.Sevk_sheet_tl,
                ESK_Sevk_Merch_tl = esk_sevkResult.Sevk_merch_tl,
                ESK_Sevk_Konsinye_tl = esk_sevkResult.Sevk_Konsinye_tl,
                ESK_Sevk_Total_tl = esk_sevkResult.Sevk_total_tl,
                ESK_Sevk_tl_m2 = esk_sevkResult.Sevk_tl_m2,
                SAK_Uretim_m2 = sak_uretimResult,
                SAK_Kagit_kg = sak_kagitResult,
                SAK_Kagit_Sarf = sak_kagit_sarfResult,
                SAK_Sheet_m2 = sak_siparis_miktarResult.SAK_SHEET_m2,
                SAK_Box_m2 = sak_siparis_miktarResult.SAK_BOX_m2,
                SAK_Merch_m2 = sak_siparis_miktarResult.SAK_Merch_m2,
                SAK_Total_m2 = sak_siparis_miktarResult.SAK_Total_m2,
                SAK_box_tl = sak_siparis_miktarResult.SAK_box_tl,
                SAK_sheet_tl = sak_siparis_miktarResult.SAK_sheet_tl,
                SAK_merch_tl = sak_siparis_miktarResult.SAK_merch_tl,
                SAK_total_tl = sak_siparis_miktarResult.SAK_total_tl,
                SAK_tl_m2 = sak_siparis_miktarResult.SAK_tl_m2,
                SAK_Sevk_Sheet_m2 = sak_sevk_miktarResult.SAK_SHEET_m2,
                SAK_Sevk_Box_m2 = sak_sevk_miktarResult.SAK_BOX_m2,
                SAK_Sevk_Merch_m2 = sak_sevk_miktarResult.SAK_Merch_m2,
                SAK_Sevk_Konsinye_m2 = sak_sevk_miktarResult.SAK_Konsinye_m2,
                SAK_Sevk_Total_m2 = sak_sevk_miktarResult.SAK_Total_m2,
                SAK_Sevk_box_tl = sak_sevk_miktarResult.SAK_box_tl,
                SAK_Sevk_sheet_tl = sak_sevk_miktarResult.SAK_sheet_tl,
                SAK_Sevk_merch_tl = sak_sevk_miktarResult.SAK_merch_tl,
                SAK_Sevk_Konsinye_tl = sak_sevk_miktarResult.SAK_Konsinye_tl,
                SAK_Sevk_total_tl = sak_sevk_miktarResult.SAK_total_tl,
                SAK_Sevk_tl_m2 = sak_sevk_miktarResult.SAK_tl_m2


            };

            return View(model);
        }

        // BHS üretim miktarını çekmek için metot
        private async Task<int> ExecuteQueryForMiktar(string query)
        {
            int totalAmount = 0;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        totalAmount = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    }
                }
            }
            return totalAmount;
        }

        // Eskişehir Kağıt stok miktarını çekmek için metot
        private async Task<int> ExecuteQueryForStok(string query)
        {
            int totalStock = 0;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        totalStock = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    }
                }
            }
            return totalStock;
        }

        // Eskişehir Kağıt Sarf miktarını çekmek için metot
        private async Task<int> ExecuteQueryForESK_Sarf(string query)
        {
            int totalsarf = 0;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        // Değeri decimal olarak al
                        decimal result = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        totalsarf = (int)result; // Gerekirse int'e dönüştür
                    }
                }
            }
            return totalsarf;
        }


        // Eskişehir BHS Uretim miktarını çekmek için metot
        private async Task<int> ExecuteQueryForBHS_uretim(string query)
        {
            int BHS_uretim = 0;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        BHS_uretim = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    }
                }
            }
            return BHS_uretim;
        }

        // Eskişehir'den Sakarya'ya Sevk Miktarı
        private async Task<(int m2, int Tutar)> ExecuteQueryForESK_to_SAK(string query)
        {
            int m2 = 0;
            int Tutar = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        // Değeri decimal olarak al
                        decimal result = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        m2 = (int)result; // Gerekirse int'e dönüştür
                        Tutar = (int)result;
                    }
                }
            }
            return (m2, Tutar);
        }


        // Eskişehir Sipariş Miktarları
        private async Task<(int SHEET_m2, int BOX_m2, int Merch_m2, int Total_m2, int box_tl, int sheet_tl, int merch_tl, int total_tl, decimal tl_m2)> ExecuteQueryForTableData(string query)
        {
            int SHEET_m2 = 0;
            int BOX_m2 = 0;
            int Merch_m2 = 0;
            int Total_m2 = 0;
            int box_tl = 0;
            int sheet_tl = 0;
            int merch_tl = 0;
            int total_tl = 0;
            decimal tl_m2 = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        box_tl = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        sheet_tl = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        merch_tl = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                        total_tl = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                        BOX_m2 = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                        SHEET_m2 = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                        Merch_m2 = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                        Total_m2 = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
                        tl_m2 = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);

                    }
                }
            }
            return (SHEET_m2, BOX_m2, Merch_m2, Total_m2, box_tl, sheet_tl, merch_tl, total_tl, tl_m2);
        }

        private async Task<(int Sevk_SHEET_m2, int Sevk_BOX_m2, int Sevk_Merch_m2,int Sevk_Konsinye_m2, int Sevk_Total_m2, int Sevk_box_tl, int Sevk_sheet_tl, int Sevk_merch_tl, int Sevk_Konsinye_tl, int Sevk_total_tl, decimal Sevk_tl_m2)> ExecuteQueryForSevkTableData(string query)
        {
            int Sevk_SHEET_m2 = 0;
            int Sevk_BOX_m2 = 0;
            int Sevk_Merch_m2 = 0;
            int Sevk_Konsinye_m2 = 0;
            int Sevk_Total_m2 = 0;
            int Sevk_box_tl = 0;
            int Sevk_sheet_tl = 0;
            int Sevk_merch_tl = 0;
            int Sevk_Konsinye_tl = 0;
            int Sevk_total_tl = 0;
            decimal Sevk_tl_m2 = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        Sevk_SHEET_m2 = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        Sevk_BOX_m2 = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        Sevk_Merch_m2 = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                        Sevk_Konsinye_m2 = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                        Sevk_Total_m2 = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                        Sevk_sheet_tl = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                        Sevk_box_tl = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                        Sevk_merch_tl = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
                        Sevk_Konsinye_tl = reader.IsDBNull(8) ? 0 : reader.GetInt32(8);
                        Sevk_total_tl = reader.IsDBNull(9) ? 0 : reader.GetInt32(9);
                        Sevk_tl_m2 = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);

                    }
                }

            }
            return (Sevk_SHEET_m2, Sevk_BOX_m2, Sevk_Merch_m2, Sevk_Konsinye_m2, Sevk_Total_m2, Sevk_box_tl, Sevk_sheet_tl, Sevk_merch_tl, Sevk_Konsinye_tl, Sevk_total_tl, Sevk_tl_m2);

        }

               
        // Sakarya Üretim m2
        private async Task<int> ExecuteQueryForSakUretim(string query)
        {
            int sakarya_uretim_m2 = 0;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        sakarya_uretim_m2 = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    }
                }
            }
            return sakarya_uretim_m2;
        }


        

        // Sakarya Üretim m2
        private async Task<int> ExecuteQueryForSakPaper(string query)
        {
            int sakarya_paper_kg = 0;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        sakarya_paper_kg= reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    }
                }
            }
            return sakarya_paper_kg;
        }

        

        private async Task<int> ExecuteQueryForSakKagitSarf(string query)
        {
            int sakarya_kagit_sarf = 0;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        sakarya_kagit_sarf = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    }
                }
            }
            return sakarya_kagit_sarf;
        }

        

        // Sakarya Sipariş Miktarları
        private async Task<(int SAK_SHEET_m2, int SAK_BOX_m2, int SAK_Merch_m2, int SAK_Total_m2, int SAK_box_tl, int SAK_sheet_tl, int SAK_merch_tl, int SAK_total_tl, decimal SAK_tl_m2)> ExecuteQueryForSakSiparisMiktar(string query)
        {
            int SAK_SHEET_m2 = 0;
            int SAK_BOX_m2 = 0;
            int SAK_Merch_m2 = 0;
            int SAK_Total_m2 = 0;
            int SAK_box_tl = 0;
            int SAK_sheet_tl = 0;
            int SAK_merch_tl = 0;
            int SAK_total_tl = 0;
            decimal SAK_tl_m2 = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        SAK_BOX_m2 = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        SAK_SHEET_m2 = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        SAK_Merch_m2 = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                        SAK_Total_m2 = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                        SAK_box_tl = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                        SAK_sheet_tl = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                        SAK_merch_tl = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                        SAK_total_tl = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
                        SAK_tl_m2 = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);
                                                
                    }
                }
            }
            return (SAK_SHEET_m2, SAK_BOX_m2, SAK_Merch_m2, SAK_Total_m2, SAK_box_tl, SAK_sheet_tl, SAK_merch_tl, SAK_total_tl, SAK_tl_m2);
        }

        // Sakarya Sevk Miktarları
        private async Task<(int SAK_SHEET_m2, int SAK_BOX_m2, int SAK_Merch_m2, int SAK_Konsinye_m2, int SAK_Total_m2, int SAK_box_tl, int SAK_sheet_tl, int SAK_merch_tl, int SAK_Konsinye_tl, int SAK_total_tl, decimal SAK_tl_m2)> ExecuteQueryForSakSevkMiktar(string query)
        {
            int SAK_SHEET_m2 = 0;
            int SAK_BOX_m2 = 0;
            int SAK_Merch_m2 = 0;
            int SAK_Konsinye_m2 = 0;
            int SAK_Total_m2 = 0;
            int SAK_box_tl = 0;
            int SAK_sheet_tl = 0;
            int SAK_merch_tl = 0;
            int SAK_Konsinye_tl = 0;
            int SAK_total_tl = 0;
            decimal SAK_tl_m2 = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        SAK_SHEET_m2 = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        SAK_BOX_m2 = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        SAK_Merch_m2 = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                        SAK_Konsinye_m2 = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                        SAK_Total_m2 = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                        SAK_sheet_tl = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                        SAK_box_tl = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);                        
                        SAK_merch_tl = reader.IsDBNull(7) ? 0 : reader.GetInt32(7);
                        SAK_Konsinye_tl = reader.IsDBNull(8) ? 0 : reader.GetInt32(8);
                        SAK_total_tl = reader.IsDBNull(9) ? 0 : reader.GetInt32(9);
                        SAK_tl_m2 = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);

                    }
                }
            }
            return (SAK_SHEET_m2, SAK_BOX_m2, SAK_Merch_m2, SAK_Konsinye_m2, SAK_Total_m2, SAK_box_tl, SAK_sheet_tl, SAK_merch_tl, SAK_Konsinye_tl, SAK_total_tl, SAK_tl_m2);
        }


    }
}



