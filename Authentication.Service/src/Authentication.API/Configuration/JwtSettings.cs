namespace Authentication.API.Configuration
{
    /// <summary>
    /// Configuration JWT centralisée et réutilisable par les autres microservices
    /// </summary>
    public class JwtSettings
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Cle { get; set; } = string.Empty;
        public int DureeMinutes { get; set; } = 60;
        public int RefreshDureeJours { get; set; } = 7;
        public int ClockSkew { get; set; } = 5;

        /// <summary>
        /// Valide que la configuration est complète et valide
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Issuer) &&
                   !string.IsNullOrWhiteSpace(Audience) &&
                   !string.IsNullOrWhiteSpace(Cle) &&
                   Cle.Length >= 32 &&  // Minimum 32 caractères pour HMAC-SHA256
                   DureeMinutes > 0 &&
                   RefreshDureeJours > 0;
        }
    }
}
