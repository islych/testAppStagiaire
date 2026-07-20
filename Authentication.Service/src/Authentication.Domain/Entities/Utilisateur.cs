namespace Authentication.Domain.Entities
{
    public class Utilisateur
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MotDePasseHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        public int? DepartementId { get; set; }  // Uniquement pour les Encadrants
        public bool Statut { get; set; } = true;
        public DateTime DateCreation { get; set; } = DateTime.Now;
    }
}