using Documents.Application.Commands;
using Documents.Application.DTOs;
using Documents.Application.Interfaces;
using Documents.Application.Validators;
using Documents.Domain.Entities;
using Documents.Domain.Enums;
using Documents.Domain.Exceptions;
using Documents.Domain.Interfaces;

namespace Documents.Application.Services;

/// <summary>
/// Service métier pour la gestion des documents administratifs
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly ICandidaturesServiceClient _candidaturesClient;

    public DocumentService(
        IDocumentRepository repository,
        INotificationService notificationService,
        ICandidaturesServiceClient candidaturesClient)
    {
        _repository = repository;
        _notificationService = notificationService;
        _candidaturesClient = candidaturesClient;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UPLOAD
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Upload un nouveau document et le place en statut EnAttente.
    /// Lève une ApplicationException si le stagiaire n'a aucune candidature acceptée.
    /// </summary>
    public async Task<DocumentDto> UploadDocumentAsync(
        int stagiaireId,
        Guid? candidatureId,
        TypeDocument type,
        string nomFichier,
        string nomFichierStockage,
        string cheminFichier,
        string extension,
        long tailleFichierOctets,
        string contentType,
        string jwtToken)
    {
        // ── Règle métier : vérifier que le stagiaire a une candidature acceptée ──
        var aUneAcceptee = await _candidaturesClient
            .StagiaireACandidatureAccepteeAsync(stagiaireId, jwtToken);

        if (!aUneAcceptee)
            throw new ApplicationException(
                "Vous ne pouvez pas déposer de documents tant que votre candidature " +
                "n'a pas été acceptée par un encadrant.");

        // ── Validation du fichier ────────────────────────────────────────────
        var command = new UploadDocumentCommand(
            stagiaireId, candidatureId, type,
            nomFichier, nomFichierStockage, cheminFichier,
            extension, tailleFichierOctets, contentType);

        var validationResult = new UploadDocumentValidator().Validate(command);
        if (!validationResult.IsValid)
            throw new ApplicationException(string.Join(", ", validationResult.Errors));

        // ── Créer l'entité ───────────────────────────────────────────────────
        var document = new Document
        {
            Id = Guid.NewGuid(),
            StagiaireId = stagiaireId,
            CandidatureId = candidatureId,
            Type = type,
            NomFichier = nomFichier,
            NomFichierStockage = nomFichierStockage,
            CheminFichier = cheminFichier,
            Extension = extension,
            TailleFichierOctets = tailleFichierOctets,
            ContentType = contentType,
            Statut = DocumentStatut.EnAttente,
            EstVersionCourante = true,
            Version = 1,
            DateDepot = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(document);

        // ── Notifier l'encadrant ─────────────────────────────────────────────
        await _notificationService.NotifyNouveauDocumentAsync(created.Id, stagiaireId);

        return MapToDto(created);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SOUMETTRE CORRECTIONS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Soumet une nouvelle version d'un document en DemandeModification.
    /// La correction va DIRECTEMENT au Centre, sans repasser par l'encadrant.
    /// </summary>
    public async Task<DocumentDto> SoumettreCorrectionsAsync(
        Guid documentOriginalId,
        int stagiaireId,
        TypeDocument type,
        string nomFichier,
        string nomFichierStockage,
        string cheminFichier,
        string extension,
        long tailleFichierOctets,
        string contentType)
    {
        var original = await _repository.GetByIdAsync(documentOriginalId)
            ?? throw new DocumentNotFoundException(documentOriginalId);

        if (original.Statut != DocumentStatut.DemandeModification)
            throw new ApplicationException(
                "Seul un document avec une demande de modification peut faire l'objet d'une correction.");

        if (original.StagiaireId != stagiaireId)
            throw new ApplicationException(
                "Vous n'êtes pas autorisé à soumettre une correction pour ce document.");

        var command = new UploadDocumentCommand(
            stagiaireId, original.CandidatureId, type,
            nomFichier, nomFichierStockage, cheminFichier,
            extension, tailleFichierOctets, contentType);

        var validationResult = new UploadDocumentValidator().Validate(command);
        if (!validationResult.IsValid)
            throw new ApplicationException(string.Join(", ", validationResult.Errors));

        // Marquer l'ancienne version comme non courante
        original.EstVersionCourante = false;
        original.DateMiseAJour = DateTime.UtcNow;
        await _repository.UpdateAsync(original);

        // Nouvelle version → va directement au Centre
        var correction = new Document
        {
            Id = Guid.NewGuid(),
            StagiaireId = stagiaireId,
            CandidatureId = original.CandidatureId,
            Type = type,
            NomFichier = nomFichier,
            NomFichierStockage = nomFichierStockage,
            CheminFichier = cheminFichier,
            Extension = extension,
            TailleFichierOctets = tailleFichierOctets,
            ContentType = contentType,
            Statut = DocumentStatut.EnCorrectionSoumise,
            DestinataireActuel = "Centre", // ← Directement au Centre, pas à l'encadrant
            EstVersionCourante = true,
            DocumentPrecedentId = documentOriginalId,
            Version = original.Version + 1,
            DateDepot = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(correction);

        // Notifier le Centre (pas l'encadrant)
        await _notificationService.NotifyCorrectionSoumiseCentreAsync(created.Id, stagiaireId);

        return MapToDto(created);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TRANSMETTRE AU CENTRE
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// L'encadrant transmet tous les documents EnAttente d'un stagiaire au Centre.
    /// Seul l'encadrant peut appeler cette méthode, et uniquement pour les docs EnAttente.
    /// </summary>
    public async Task<IEnumerable<DocumentDto>> TransmettreAuCentreAsync(Guid candidatureId, int encadrantId)
    {
        if (candidatureId == Guid.Empty)
            throw new ApplicationException("L'identifiant de la candidature est invalide.");

        var documents = await _repository.GetByCandidatureIdAsync(candidatureId);
        var docsEnAttente = documents.Where(d => d.Statut == DocumentStatut.EnAttente).ToList();

        if (!docsEnAttente.Any())
            throw new ApplicationException(
                "Aucun document en attente à transmettre. Tous les documents sont déjà traités ou aucun document n'a été déposé.");

        var transmis = new List<Document>();
        foreach (var doc in docsEnAttente)
        {
            doc.Statut = DocumentStatut.TransmisAuCentre;
            doc.DestinataireActuel = "Centre";
            doc.VerificateurId = encadrantId;
            doc.DateMiseAJour = DateTime.UtcNow;
            var updated = await _repository.UpdateAsync(doc);
            transmis.Add(updated);
        }

        // Notifier le Centre une seule fois pour le dossier
        if (transmis.Any())
            await _notificationService.NotifyDossierTransmisAuCentreAsync(transmis.First().Id, transmis.First().StagiaireId);

        return transmis.Select(MapToDto);
    }

    /// <summary>
    /// Le Centre valide (accepte) un document.
    /// Si tous les documents de la candidature sont validés, le dossier est automatiquement accepté.
    /// </summary>
    public async Task<DocumentDto> ValiderDocumentAsync(Guid documentId, int verificateurId, string? commentaire, string jwtToken)
    {
        var command = new ValiderDocumentCommand(documentId, verificateurId, commentaire);
        var validationResult = new ValiderDocumentValidator().Validate(command);
        if (!validationResult.IsValid)
            throw new ApplicationException(string.Join(", ", validationResult.Errors));

        var document = await _repository.GetByIdAsync(documentId)
            ?? throw new DocumentNotFoundException(documentId);

        if (document.Statut != DocumentStatut.TransmisAuCentre &&
            document.Statut != DocumentStatut.EnCorrectionSoumise)
            throw new ApplicationException(
                $"Ce document ne peut pas être validé (statut actuel : '{document.Statut}'). " +
                "Seuls les documents transmis au Centre ou en correction soumise peuvent être validés.");

        document.Statut = DocumentStatut.Valide;
        document.DestinataireActuel = "Centre";
        document.VerificateurId = verificateurId;
        document.CommentaireVerificateur = commentaire;
        document.DateValidation = DateTime.UtcNow;
        document.DateMiseAJour = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(document);
        await _notificationService.NotifyDocumentValideAsync(updated.Id, updated.StagiaireId);

        // ── Vérification automatique : tous les docs de la candidature sont-ils validés ? ──
        if (document.CandidatureId.HasValue)
        {
            var tousLesDocs = await _repository.GetByCandidatureIdAsync(document.CandidatureId.Value);
            var tousValides = tousLesDocs.Any() &&
                              tousLesDocs.All(d => d.Statut == DocumentStatut.Valide);

            if (tousValides)
            {
                await _candidaturesClient.MarquerDossierAccepteAsync(
                    document.CandidatureId.Value, jwtToken);
            }
        }

        return MapToDto(updated);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // REFUSER définitivement (Centre uniquement)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Le Centre refuse définitivement un document.
    /// </summary>
    public async Task<DocumentDto> RefuserDocumentAsync(Guid documentId, int verificateurId, string commentaireRefus)
    {
        var command = new RefuserDocumentCommand(documentId, verificateurId, commentaireRefus);
        var validationResult = new RefuserDocumentValidator().Validate(command);
        if (!validationResult.IsValid)
            throw new ApplicationException(string.Join(", ", validationResult.Errors));

        var document = await _repository.GetByIdAsync(documentId)
            ?? throw new DocumentNotFoundException(documentId);

        if (document.Statut != DocumentStatut.TransmisAuCentre &&
            document.Statut != DocumentStatut.EnCorrectionSoumise)
            throw new ApplicationException(
                $"Ce document ne peut pas être refusé (statut actuel : '{document.Statut}').");

        document.Statut = DocumentStatut.Refuse;
        document.DestinataireActuel = "Centre";
        document.VerificateurId = verificateurId;
        document.CommentaireVerificateur = commentaireRefus;
        document.DateValidation = DateTime.UtcNow;
        document.DateMiseAJour = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(document);
        await _notificationService.NotifyDocumentRefuseAsync(updated.Id, updated.StagiaireId, commentaireRefus);

        return MapToDto(updated);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DEMANDER MODIFICATION (Centre uniquement)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Le Centre demande des modifications sur un document.
    /// Le stagiaire est notifié et ses corrections iront directement au Centre.
    /// </summary>
    public async Task<DocumentDto> DemanderModificationAsync(Guid documentId, int centreUserId, string commentaire)
    {
        if (string.IsNullOrWhiteSpace(commentaire) || commentaire.Length < 10)
            throw new ApplicationException("Le commentaire doit contenir au moins 10 caractères.");

        var document = await _repository.GetByIdAsync(documentId)
            ?? throw new DocumentNotFoundException(documentId);

        if (document.Statut != DocumentStatut.TransmisAuCentre &&
            document.Statut != DocumentStatut.EnCorrectionSoumise)
            throw new ApplicationException(
                $"Ce document ne peut pas faire l'objet d'une demande de modification (statut : '{document.Statut}').");

        document.Statut = DocumentStatut.DemandeModification;
        document.DestinataireActuel = "Stagiaire"; // retourne au stagiaire pour correction
        document.VerificateurId = centreUserId;
        document.CommentaireVerificateur = commentaire;
        document.DateValidation = DateTime.UtcNow;
        document.DateMiseAJour = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(document);
        await _notificationService.NotifyModificationDemandeeAsync(updated.Id, updated.StagiaireId, commentaire);

        return MapToDto(updated);
    }

    public async Task SupprimerDocumentAsync(Guid documentId, int stagiaireId)
    {
        var document = await _repository.GetByIdAsync(documentId)
            ?? throw new DocumentNotFoundException(documentId);

        if (document.StagiaireId != stagiaireId)
            throw new ApplicationException("Vous n'êtes pas autorisé à supprimer ce document.");

        // Ne peut supprimer que si le document n'a pas encore été transmis au Centre ou validé
        if (document.Statut == DocumentStatut.Valide ||
            document.Statut == DocumentStatut.TransmisAuCentre ||
            document.Statut == DocumentStatut.EnCorrectionSoumise)
            throw new ApplicationException("Ce document ne peut plus être supprimé (déjà transmis au Centre ou validé).");

        await _repository.DeleteAsync(documentId);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // QUERIES
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Récupère un document par son ID
    /// </summary>
    public async Task<DocumentDto?> GetDocumentByIdAsync(Guid id)
    {
        var document = await _repository.GetByIdAsync(id);
        return document != null ? MapToDto(document) : null;
    }

    /// <summary>
    /// Récupère tous les documents (version courante uniquement) d'un stagiaire
    /// </summary>
    public async Task<IEnumerable<DocumentDto>> GetDocumentsByStagiaireAsync(int stagiaireId)
    {
        if (stagiaireId <= 0)
            throw new ApplicationException("L'identifiant du stagiaire est invalide.");

        var documents = await _repository.GetByStagiaireIdAsync(stagiaireId);
        return documents.Select(MapToDto);
    }

    /// <summary>
    /// Récupère les documents liés à une candidature
    /// </summary>
    public async Task<IEnumerable<DocumentDto>> GetDocumentsByCandidatureAsync(Guid candidatureId)
    {
        if (candidatureId == Guid.Empty)
            throw new ApplicationException("L'identifiant de la candidature est invalide.");

        var documents = await _repository.GetByCandidatureIdAsync(candidatureId);
        return documents.Select(MapToDto);
    }

    /// <summary>
    /// Récupère tous les documents (version courante) — pour les rôles administratifs
    /// </summary>
    public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync()
    {
        var documents = await _repository.GetAllCurrentVersionsAsync();
        return documents.Select(MapToDto);
    }

    /// <summary>
    /// Récupère les documents filtrés par statut
    /// </summary>
    public async Task<IEnumerable<DocumentDto>> GetDocumentsByStatutAsync(DocumentStatut statut)
    {
        var documents = await _repository.GetByStatutAsync(statut);
        return documents.Select(MapToDto);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MAPPING
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Mappe une entité Document en DTO
    /// </summary>
    private static DocumentDto MapToDto(Document document) => new()
    {
        Id = document.Id,
        StagiaireId = document.StagiaireId,
        CandidatureId = document.CandidatureId,
        Type = document.Type,
        TypeLibelle = GetTypeLibelle(document.Type),
        NomFichier = document.NomFichier,
        Extension = document.Extension,
        TailleFichierOctets = document.TailleFichierOctets,
        ContentType = document.ContentType,
        Statut = document.Statut,
        StatutLibelle = GetStatutLibelle(document.Statut),
        DestinataireActuel = document.DestinataireActuel,
        VerificateurId = document.VerificateurId,
        CommentaireVerificateur = document.CommentaireVerificateur,
        DateDepot = document.DateDepot,
        DateValidation = document.DateValidation,
        DateMiseAJour = document.DateMiseAJour,
        EstVersionCourante = document.EstVersionCourante,
        DocumentPrecedentId = document.DocumentPrecedentId,
        Version = document.Version
    };

    private static string GetTypeLibelle(TypeDocument type) => type switch
    {
        TypeDocument.CV => "Curriculum Vitae (CV)",
        TypeDocument.ConventionDeStage => "Convention de stage",
        TypeDocument.Assurance => "Attestation d'assurance",
        TypeDocument.CIN => "Carte d'identité (CIN)",
        TypeDocument.LettreDeRecommandation => "Lettre de recommandation",
        TypeDocument.DemandeManuscrite => "Demande manuscrite",
        TypeDocument.EngagementLegalise => "Engagement légalisé",
        _ => type.ToString()
    };

    private static string GetStatutLibelle(DocumentStatut statut) => statut switch
    {
        DocumentStatut.EnAttente => "En attente — visible par l'encadrant",
        DocumentStatut.TransmisAuCentre => "Transmis au Centre",
        DocumentStatut.DemandeModification => "Modification demandée par le Centre",
        DocumentStatut.EnCorrectionSoumise => "Correction soumise — en attente du Centre",
        DocumentStatut.Valide => "Validé par le Centre",
        DocumentStatut.Refuse => "Refusé par le Centre",
        _ => statut.ToString()
    };
}
