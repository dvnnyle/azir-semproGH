using System.Text.Json.Serialization;

namespace azir_sempro.Models;

// Svar-formen fra Kartverket/Geonorge sitt adresse-API (ws.geonorge.no/adresser/v1/punktsok)
public class GeonorgeAdresseRespons
{
    [JsonPropertyName("adresser")]
    public List<GeonorgeAdresse> Adresser { get; set; } = [];
}

public class GeonorgeAdresse
{
    [JsonPropertyName("adressetekst")]
    public string Adressetekst { get; set; } = "";

    [JsonPropertyName("postnummer")]
    public string Postnummer { get; set; } = "";

    [JsonPropertyName("poststed")]
    public string Poststed { get; set; } = "";
}
