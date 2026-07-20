namespace Notifications.Application.Interfaces;

public interface IEmailService
{
    Task EnvoyerEmailCandidatureAccepteeAsync(string emailDestinataire, string nomDestinataire);
    Task EnvoyerEmailDemandeCorrectionsAsync(string emailDestinataire, string nomDestinataire, string typeDocument, string commentaire);
}
