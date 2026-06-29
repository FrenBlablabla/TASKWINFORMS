using System.Text.Json.Serialization;

namespace IsvuWinForms
{
    public class StudentDto
    {
        [JsonPropertyName("jmbag")]
        public string Jmbag { get; set; } = "";

        [JsonPropertyName("oib")]
        public string? Oib { get; set; }

        [JsonPropertyName("ime")]
        public string? Ime { get; set; }

        [JsonPropertyName("prezime")]
        public string? Prezime { get; set; }

        [JsonPropertyName("datumRodenja")]
        public DateTime? DatumRodenja { get; set; }

        [JsonPropertyName("datumUpisa")]
        public DateTime? DatumUpisa { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("studijskiProgramId")]
        public int? StudijskiProgramId { get; set; }
    }
}