using System.Globalization;

namespace azir_sempro.Models;

public class AdminMatchViewModel
{
    public int AntallTildelt { get; set; }
    public int AntallLost { get; set; }
    public List<MatchBehovViewModel> Behov { get; set; } = [];
}

public class AdminMatchArkivViewModel
{
    public string Filter { get; set; } = "alle";
    public int AntallTildelt { get; set; }
    public int AntallLost { get; set; }
    public List<MatchKoblingViewModel> Koblinger { get; set; } = [];
}

public class AdminMatchBehovViewModel
{
    public MatchMeldingViewModel Behov { get; set; } = new();
    public string BehovPunkterJson { get; set; } = "[]";
    public List<MatchKandidatViewModel> Kandidater { get; set; } = [];
}

public class MatchMeldingViewModel
{
    public int SubmissionId { get; set; }
    public string Tittel { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public string KategoriNavn { get; set; } = "";
    public string Adresse { get; set; } = "";
    public string? Prioritet { get; set; }
    public DateTime? Frist { get; set; }
    public string? Beskrivelse { get; set; }
    public DateTime Tidspunkt { get; set; }
    public string EierNavn { get; set; } = "";
    public string? EierEpost { get; set; }
    public string? EierTelefon { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }

    public string? LatTekst => Lat?.ToString("0.000000", CultureInfo.InvariantCulture);
    public string? LngTekst => Lng?.ToString("0.000000", CultureInfo.InvariantCulture);
    public string? LatKort => Lat?.ToString("0.000", CultureInfo.InvariantCulture);
    public string? LngKort => Lng?.ToString("0.000", CultureInfo.InvariantCulture);

    public string PrioritetTekst => Prioritet switch
    {
        "høy" => "Høy",
        "middels" => "Middels",
        "lav" => "Lav",
        _ => "Ikke satt"
    };

    public string PrioritetKlasse => Prioritet switch
    {
        "høy" => "rod",
        "middels" => "gul",
        _ => "gronn"
    };
}

public class MatchBehovViewModel
{
    public MatchMeldingViewModel Melding { get; set; } = new();
    public string? ForslagTittel { get; set; }
}

public class MatchKandidatViewModel
{
    public MatchMeldingViewModel Ressurs { get; set; } = new();
    public double? AvstandKm { get; set; }
    public string Treff { get; set; } = "";
}

public class MatchKoblingViewModel
{
    public int RequestId { get; set; }
    public MatchMeldingViewModel Behov { get; set; } = new();
    public MatchMeldingViewModel Ressurs { get; set; } = new();
    public bool ErLost { get; set; }
    public DateTime Tidspunkt { get; set; }
}
