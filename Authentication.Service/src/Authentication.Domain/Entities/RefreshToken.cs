namespace Authentication.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime DateExpiration { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        public bool Revoque { get; set; } = false;
        public string? AdresseIp { get; set; }
    }
}
