using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Dapper.Nona.IntegrationTests.Entities
{
    public class LargeProduct:BaseEntity
    {
        
        public int IdArtikulli { get; set; }

        public override int ProductId
        {

            get => IdArtikulli;
            set => IdArtikulli = value;
        }


        public string Kodartikulli { get; set; }

        [MaxLength(1000)]
        public string Pershkrimartikulli { get; set; }

        public string Pershkrimiangartikulli { get; set; }

        
        [MaxLength(100)]
        public string Kodidoganorartikulli { get; set; }

        
        [MaxLength(100)]
        public string Vendodhjeartikulli { get; set; }

        public string Kodifikimi1Artikulli { get; set; }

        public string Kodifikimi2Artikulli { get; set; }

        
        [MaxLength(100)]
        public string Origjineartikulli { get; set; }

        public string Njesi1Artikulli { get; set; }

        public string Njesi2Artikulli { get; set; }

        
        public decimal Koeficentartikulli { get; set; }

        public string Idfurnitorikryesor { get; set; }

        
        public decimal Peshabrutoartikulli { get; set; }

        
        public decimal Peshanetoartikulli { get; set; }

        
        public bool Detajimartikulli { get; set; }

        
        public string Klasa { get; set; }

        public string Idskemakontabilitetiartikulli { get; set; }

        public string Idllogariinventari { get; set; }

        public string Idllogariblerje { get; set; }

        public string Idllogarishitje { get; set; }

        public string Idllogaritetrete { get; set; }

        public string Idllogarishpenzime { get; set; }

        
        public decimal Minimumartikulli { get; set; }

        
        public decimal Maximumartikulli { get; set; }

        public string Metodekostojeartikulli { get; set; }

        
        public int Llogaritjakmshartikulli { get; set; }

        
        public int Zevendesimautomatikartikulli { get; set; }

        
        public int Idperdoruesi { get; set; }

        public int Idndermarje { get; set; }

        public bool Kontrollgjendje { get; set; }

        public bool Kontrollcmimi { get; set; }

        public bool Kontrollgjendjeartikulli { get; set; }

        public int Idtvsh { get; set; }

        public string Idkonfig { get; set; }

        public bool Aktiv { get; set; }

        public string Idstatusdok { get; set; }

        public DateTime Dtkrijimi { get; set; }

        public DateTime Dtmodifikimi { get; set; }

        public bool Llojiart { get; set; }

        public string Idllogariamortizimi { get; set; }

        public decimal Sasinjesi { get; set; }

        public decimal Scrap { get; set; }

        public bool Prodhimmeporosi { get; set; }

        public string Idkategoridetajimi { get; set; }

        public string Idkategoridetajimi2 { get; set; }

        public bool Kontrollgjendjedetajim2 { get; set; }

        public string Idobjektivakosto { get; set; }

        public string Idllojgarancia { get; set; }

        public decimal Garancia { get; set; }

        public string Idmagazina { get; set; }

        public bool Irezervueshem { get; set; }

        public bool Pertransferim { get; set; }

        public bool Loan { get; set; }

        public bool Dhurate { get; set; }

        public string Aplikimdhurate { get; set; }

        public decimal Pike { get; set; }

        public decimal Vlere { get; set; }

        [MaxLength(20)]
        public string Kodvfone { get; set; }

        public bool Meserial { get; set; }

        public string Idllogaripakesim { get; set; }

        public bool Ishitshem { get; set; }

        public bool Mbetjeshitjshme { get; set; }

        public string Idartraportuesi { get; set; }

        public bool Perpeshore { get; set; }

        [MaxLength(100)]
        public string Pershkrimtefurnitori { get; set; }

        [MaxLength(500)]
        public string Siperfaqjam2 { get; set; }

        [MaxLength(500)]
        public string Nrkontrate { get; set; }

        [MaxLength(500)]
        public string Nrpasurie { get; set; }

        [MaxLength(500)]
        public string Zonakadastrale { get; set; }

        [MaxLength(500)]
        public string Shasia { get; set; }

        [MaxLength(500)]
        public string Marka { get; set; }

        [MaxLength(500)]
        public string Modeli { get; set; }

        [MaxLength(500)]
        public string Vitprodhimi { get; set; }

        [MaxLength(500)]
        public string Tedhenateknike { get; set; }

        public bool Mebarkodlogjik { get; set; }

        [MaxLength(100)]
        public string Skemabarkodit { get; set; }

        public string Kodifikimi3Artikulli { get; set; }

        public bool Aparatbazaar { get; set; }

        public string Kodoferte { get; set; }

        public bool Artikullivjeter { get; set; }

        public string Idkategoriseriali { get; set; }

        public bool Rezerverivleresimi { get; set; }

        public string Idllogarirezerve { get; set; }

        public string Idllogaripakesimrez { get; set; }

        public int KaraktereTac { get; set; }

        public bool Llogaritkomision { get; set; }

        public int LlogariKomisioni { get; set; }

        public int Stokumaxvfone { get; set; }

        public string Kodiibarit { get; set; }

        public bool Irimburshueshem { get; set; }
    }

}
