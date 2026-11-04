namespace azir_sempro.Models;

public class ResourceDraft
{
    public int ResourceDraftId { get; set; }
    public string IdentityUserId { get; set; } = "";
    public string DataJson { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
