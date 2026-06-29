namespace IsvuWinForms
{
    public class UpisPredmetaDto
    {
        public int Id { get; set; }
        public string StudentJmbag { get; set; } = null!;
        public string PredmetSifra { get; set; } = null!;
        public int AkademskaGodinaId { get; set; }
        public string? Status { get; set; }
        public DateTime? DatumUpisa { get; set; }
        public int BrojUpisa { get; set; }
    }

    public class PrijavaIspitaDto
    {
        public int Id { get; set; }
        public int UpisPredmetaId { get; set; }
        public int IspitniRokId { get; set; }
        public DateTime? DatumPrijave { get; set; }
        public int RedniBrojIzlaska { get; set; }
        public string? Status { get; set; }
    }
}