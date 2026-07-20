using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Notifications.Application.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Notifications.Infrastructure.ExternalServices;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task EnvoyerEmailCandidatureAccepteeAsync(string emailDestinataire, string nomDestinataire)
    {
        var sujet = "Votre candidature de stage a été acceptée";
        var corps = $@"
<html><body style='font-family:Arial,sans-serif;'>
<h2 style='color:#28a745;'>Félicitations, {nomDestinataire} !</h2>
<p>Votre candidature de stage a été <strong>acceptée</strong> par un encadrant.</p>
<p>Veuillez maintenant soumettre les documents suivants via la plateforme :</p>
<ul>
  <li>Curriculum Vitae (CV)</li>
  <li>Convention de stage</li>
  <li>Attestation d'assurance</li>
  <li>Carte d'identité (CIN)</li>
  <li>Demande manuscrite</li>
</ul>
<p>
  Connectez-vous et rendez-vous dans la section <strong>Documents</strong> pour déposer vos fichiers.
</p>
<hr/>
<small>Plateforme de Gestion des Stagiaires</small>
</body></html>";

        await EnvoyerAsync(emailDestinataire, sujet, corps);
    }

    public async Task EnvoyerEmailDemandeCorrectionsAsync(
        string emailDestinataire, string nomDestinataire, string typeDocument, string commentaire)
    {
        var sujet = $"Correction demandée pour votre document : {typeDocument}";
        var corps = $@"
<html><body style='font-family:Arial,sans-serif;'>
<h2 style='color:#ffc107;'>Correction requise</h2>
<p>Bonjour {nomDestinataire},</p>
<p>Une correction est demandée pour votre document <strong>{typeDocument}</strong>.</p>
<p><strong>Commentaire de l'encadrant :</strong></p>
<blockquote style='border-left:4px solid #ffc107;padding-left:12px;color:#555;'>{commentaire}</blockquote>
<p>Connectez-vous et rendez-vous dans la section <strong>Documents</strong> pour soumettre la correction.</p>
<hr/>
<small>Plateforme de Gestion des Stagiaires</small>
</body></html>";

        await EnvoyerAsync(emailDestinataire, sujet, corps);
    }

    private async Task EnvoyerAsync(string to, string sujet, string corps)
    {
        var host = _config["Smtp:Host"] ?? "smtp.gmail.com";
        var port = int.Parse(_config["Smtp:Port"] ?? "587");
        var user = _config["Smtp:Username"] ?? throw new InvalidOperationException("Smtp:Username manquant");
        var pass = _config["Smtp:Password"] ?? throw new InvalidOperationException("Smtp:Password manquant");
        var from = _config["Smtp:From"] ?? user;

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true
        };

        var message = new MailMessage(from, to, sujet, corps)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(message);
        _logger.LogInformation("Email envoyé à {To} : {Sujet}", to, sujet);
    }
}
