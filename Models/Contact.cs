public class Contact
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? Poste { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public string? TelephoneCountry { get; set; }
    public bool IsActive { get; set; }
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
}