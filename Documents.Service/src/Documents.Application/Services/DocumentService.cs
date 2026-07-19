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
    /// Soumet une nouvelle version d'un document refusé.
    /// L'ancienne version est marquée comme non courante.
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
        // Vérifier que le document original existe et est bien refusé
        var original = await _repository.GetByIdAsync(documentOriginalId)
            ?? throw new DocumentNotFoundException(documentOriginalId);

        if (original.Statut != DocumentStatut.Refuse)
            throw new ApplicationException(
                "Seul un document refusé peut faire l'objet d'une correction.");

        if (original.StagiaireId != stagiaireId)
            throw new ApplicationException(
                "Vous n'êtes pas autorisé à soumettre une correction pour ce document.");

        // Valider la nouvelle version
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

        // Créer la nouvelle version
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
            EstVersionCourante = true,
            DocumentPrecedentId = documentOriginalId,
            Version = original.Version + 1,
            DateDepot = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(correction);

        // Notifier l'encadrant de la correction
        await _notificationService.NotifyCorrectionSoumiseAsync(created.Id, stagiaireId);

        return MapToDto(created);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // VALIDER
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Valide un document (Encadrant / RH / Centre)
    /// </summary>
    public async Task<DocumentDto> ValiderDocumentAsync(Guid documentId, int verificateurId, string? commentaire)
    {
        var command = new ValiderDocumentCommand(documentId, verificateurId, commentaire);
        var validationResult = new ValiderDocumentValidator().Validate(command);
        if (!validationResult.IsValid)
            throw new ApplicationException(string.Join(", ", validationResult.Errors));

        var document = await _repository.GetByIdAsync(documentId)
            ?? throw new DocumentNotFoundException(documentId);

        // Seuls les documents EnAttente ou EnCorrectionSoumise peuvent être validés
        if (document.Statut != DocumentStatut.EnAttente &&
            document.Statut != DocumentStatut.EnCorrectionSoumise)
            throw new ApplicationException(
                $"Le document ne peut pas être validé car il a le statut '{document.Statut}'.");

        document.Statut = DocumentStatut.Valide;
        document.VerificateurId = verificateurId;
        document.CommentaireVerificateur = commentaire;
        document.DateValidation = DateTime.UtcNow;
        document.DateMiseAJour = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(document);

        // Notifier le stagiaire
        await _notificationService.NotifyDocumentValideAsync(updated.Id, updated.StagiaireId);

        return MapToDto(updated);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // REFUSER
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Refuse un document avec un commentaire obligatoire
    /// </summary>
    public async Task<DocumentDto> RefuserDocumentAsync(Guid documentId, int verificateurId, string commentaireRefus)
    {
        var command = new RefuserDocumentCommand(documentId, verificateurId, commentaireRefus);
        var validationResult = new RefuserDocumentValidator().Validate(command);
        if (!validationResult.IsValid)
            throw new ApplicationException(string.Join(", ", validationResult.Errors));

        var document = await _repository.GetByIdAsync(documentId)
            ?? throw new DocumentNotFoundException(documentId);

        if (document.Statut != DocumentStatut.EnAttente &&
            document.Statut != DocumentStatut.EnCorrectionSoumise)
            throw new ApplicationException(
                $"Le document ne peut pas être refusé car il a le statut '{document.Statut}'.");

        document.Statut = DocumentStatut.Refuse;
        document.VerificateurId = verificateurId;
        document.CommentaireVerificateur = commentaireRefus;
        document.DateValidation = DateTime.UtcNow;
        document.DateMiseAJour = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(document);

        // Notifier le stagiaire
        await _notificationService.NotifyDocumentRefuseAsync(updated.Id, updated.StagiaireId, commentaireRefus);

        return MapToDto(updated);
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
        TypeDocument.CV => "Curriculum Vitae",
        TypeDocument.ConventionDeStage => "Convention de stage",
        TypeDocument.Assurance => "Attestation d'assurance",
        TypeDocument.CIN => "Carte d'identité nationale",
        TypeDocument.LettreDeRecommandation => "Lettre de recommandation",
        TypeDocument.RapportDeStage => "Rapport de stage",
        TypeDocument.AttestationDeStage => "Attestation de stage",
        TypeDocument.DemandeManuscrite => "Demande manuscrite",
        TypeDocument.FicheAppreciation => "Fiche d'appréciation",
        TypeDocument.FicheDeSynthese => "Fiche de synthèse",
        _ => type.ToString()
    };

    private static string GetStatutLibelle(DocumentStatut statut) => statut switch
    {
        DocumentStatut.EnAttente => "En attente de vérification",
        DocumentStatut.Valide => "Validé",
        DocumentStatut.Refuse => "Refusé — correction demandée",
        DocumentStatut.EnCorrectionSoumise => "Correction soumise — en attente de vérification",
        _ => statut.ToString()
    };
}
