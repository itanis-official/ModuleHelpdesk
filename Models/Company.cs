public class Company
{
    public int Id { get; set; }
    public string RaisonSociale { get; set; } = string.Empty;
    public string? Secteur { get; set; }
    public string? EmailPrincipal { get; set; }
    public string? TelephonePrincipal { get; set; }
    public string? Adresse { get; set; }
    public string? CodePostal { get; set; }
    public string? Ville { get; set; }
    public string? Pays { get; set; }
    public string? MatriculeFiscal { get; set; }
    public string? Statut { get; set; }
    public decimal? MaxHeuresTraitementTicket { get; set; }   // SLA ticket
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
}