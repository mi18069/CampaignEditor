using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities
{
    public class Schema
    {
        
        [Column("id")]
        public int id { get; set; }
        [Column("chid")]
        public int chid { get; set; }
        [Column("naziv")]
        public string name { get; set; }
        [Column("pozicija")]
        public string position { get; set; }
        [Column("vremeod")]
        public string stime { get; set; }
        [Column("vremedo")]
        public string? etime { get; set; }
        [Column("vremerbl")]
        public string? blocktime { get; set; }
        [Column("dani")]
        public string days { get; set; }
        [Column("tipologija")]
        public string type { get; set; }
        [Column("specijal")]
        public bool special { get; set; }
        [Column("datumod")]
        public DateOnly sdate { get; set; }
        [Column("datumdo")]
        public DateOnly? edate { get; set; }
        [Column("progkoef")]
        public float progcoef { get; set; }
        [Column("datumkreiranja")]
        public DateOnly created { get; set; }
        [Column("datumizmene")]
        public DateOnly? modified { get; set; }
    }
}
