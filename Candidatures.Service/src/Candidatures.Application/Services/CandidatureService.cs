using Candidatures.Application.DTOs;
using Candidatures.Application.Interfaces;
using Candidatures.Application.Validators;
using Candidatures.Domain.Entities;
using Candidatures.Domain.Enums;
using Candidatures.Domain.Interfaces;

namespace Candidatures.Application.Services;

/// <summary>
/// Service métier pour la gestion des candidatures
/// </summary>
public class CandidatureService : ICandidatureService
{
    private readonly ICandidatureRepository _repository;
    private readonly INotificationService _notificationService;

    public CandidatureService(
        ICandidatureRepository repository,
        INotificationService notificationService)
    {
        _repository = repository;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Crée une nouvelle candidature avec le statut EnAttente
    /// </summary>
    public async Task<CandidatureDto> CreateCandidatureAsync(CreateCandidatureDto dto)
    {
        // Validation
        var validator = new CreateCandidatureValidator();
        var validationResult = validator.Validate(new Commands.CreateCandidatureCommand(
            dto.StagiaireId,
            dto.DepartementId,
            dto.SpecialiteId,
            dto.CvFileName,
            dto.CvPath
        ));

        if (!validationResult.IsValid)
            throw new ApplicationException(string.Join(", ", validationResult.Errors));

        // Création de l'entité
        var candidature = new Candidature
        {
            Id = Guid.NewGuid(),
            StagiaireId = dto.StagiaireId,
            DepartementId = dto.DepartementId,
            SpecialiteId = dto.SpecialiteId,
            DureeStageMois = dto.DureeStageMois,
            DateDebut = dto.DateDebut,
            DateFin = dto.DateFin,
            NiveauEtude = dto.NiveauEtude,
            Ecole = dto.Ecole,
            LettreMotivation = dto.LettreMotivation,
            CvFileName = dto.CvFileName,
            CvPath = dto.CvPath,
            Statut = CandidatureStatus.EnAttente,
            DateCreation = DateTime.UtcNow
        };

        // Persister
        var created = await _repository.CreateAsync(candidature);

        // Notifier
        await _notificationService.NotifyCandidatureCreatedAsync(created.Id, created.StagiaireId);

        return MapToDto(created);
    }

    /// <summary>
    /// Récupère une candidature par son ID
    /// </summary>
    public async Task<CandidatureDto?> GetCandidatureByIdAsync(Guid id)
    {
        var candidature = await _repository.GetByIdAsync(id);
        return candidature != null ? MapToDto(candidature) : null;
    }

    /// <summary>
    /// Récupère toutes les candidatures
    /// </summary>
    public async Task<IEnumerable<CandidatureDto>> GetAllCandidaturesAsync()
    {
        var candidatures = await _repository.GetAllAsync();
        return candidatures.Select(MapToDto);
    }

    /// <summary>
    /// Récupère les candidatures d'un stagiaire
    /// </summary>
    public async Task<IEnumerable<CandidatureDto>> GetCandidaturesByStagiaireAsync(int stagiaireId)
    {
        if (stagiaireId <= 0)
            throw new ApplicationException("L'identifiant du stagiaire est invalide.");

        var candidatures = await _repository.GetByStagiaireIdAsync(stagiaireId);
        return candidatures.Select(MapToDto);
    }

    /// <summary>
    /// Accepte une candidature
    /// </summary>
    public async Task<CandidatureDto> AcceptCandidatureAsync(Guid candidatureId, int encadrantId)
    {
        // Validation
        var validator = new AcceptCandidatureValidator();
        var validationResult = validator.Validate(new Commands.AcceptCandidatureCommand(candidatureId, encadrantId));

        if (!validationResult.IsValid)
            throw new ApplicationException(string.Join(", ", validationResult.Errors));

        // Récupérer la candidature
        var candidature = await _repository.GetByIdAsync(candidatureId)
            ?? throw new ApplicationException($"Candidature {candidatureId} non trouvée.");

        // Vérifier qu'elle n'a pas déjà été traitée
        if (candidature.Statut != CandidatureStatus.EnAttente)
            throw new ApplicationException($"La candidature ne peut pas être acceptée car elle a déjà le statut {candidature.Statut}.");

        // Mettre à jour
        candidature.Statut = CandidatureStatus.Acceptee;
        candidature.EncadrantId = encadrantId;
        candidature.DateDecision = DateTime.UtcNow;
        candidature.DateMiseAJour = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(candidature);

        // Notifier
        await _notificationService.NotifyCandidatureAcceptedAsync(updated.Id, updated.StagiaireId);

        return MapToDto(updated);
    }

    /// <summary>
    /// Refuse une candidature avec un commentaire obligatoire
    /// </summary>
    public async Task<CandidatureDto> RejectCandidatureAsync(Guid candidatureId, int encadrantId, string commentaire)
    {
        // Validation
        var validator = new RejectCandidatureValidator();
        var validationResult = validator.Validate(new Commands.RejectCandidatureCommand(candidatureId, encadrantId, commentaire));

        if (!validationResult.IsValid)
            throw new ApplicationException(string.Join(", ", validationResult.Errors));

        // Récupérer la candidature
        var candidature = await _repository.GetByIdAsync(candidatureId)
            ?? throw new ApplicationException($"Candidature {candidatureId} non trouvée.");

        // Vérifier qu'elle n'a pas déjà été traitée
        if (candidature.Statut != CandidatureStatus.EnAttente)
            throw new ApplicationException($"La candidature ne peut pas être refusée car elle a déjà le statut {candidature.Statut}.");

        // Mettre à jour
        candidature.Statut = CandidatureStatus.Refusee;
        candidature.EncadrantId = encadrantId;
        candidature.Commentaire = commentaire;
        candidature.DateDecision = DateTime.UtcNow;
        candidature.DateMiseAJour = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(candidature);

        // Notifier
        await _notificationService.NotifyCandidatureRejectedAsync(updated.Id, updated.StagiaireId, commentaire);

        return MapToDto(updated);
    }

    /// <summary>
    /// Récupère le suivi d'une candidature (visible par le stagiaire)
    /// </summary>
    public async Task<CandidatureSuiviDto?> GetCandidatureSuiviAsync(Guid candidatureId)
    {
        var candidature = await _repository.GetByIdAsync(candidatureId);
        if (candidature == null)
            return null;

        return new CandidatureSuiviDto
        {
            Id = candidature.Id,
            Statut = candidature.Statut,
            DateCreation = candidature.DateCreation,
            DateDecision = candidature.DateDecision,
            Commentaire = candidature.Commentaire
        };
    }

    /// <summary>
    /// Transmet une candidature acceptée à la Direction
    /// </summary>
    public async Task<bool> TransmitToDirectionAsync(Guid candidatureId)
    {
        var candidature = await _repository.GetByIdAsync(candidatureId)
            ?? throw new ApplicationException($"Candidature {candidatureId} non trouvée.");

        if (candidature.Statut != CandidatureStatus.Acceptee)
            throw new ApplicationException("Seules les candidatures acceptées peuvent être transmises à la Direction.");

        candidature.TransmisADirection = true;
        candidature.DateTransmissionDirection = DateTime.UtcNow;
        candidature.DateMiseAJour = DateTime.UtcNow;

        await _repository.UpdateAsync(candidature);

        // Notifier la Direction
        await _notificationService.NotifyTransmittedToDirectionAsync(candidatureId);

        return true;
    }

    /// <summary>
    /// Mappe une entité Candidature en DTO avec enrichissement des noms de département et spécialité
    /// </summary>
    private static CandidatureDto MapToDto(Candidature candidature)
    {
        // Récupérer les noms de département et spécialité
        var departement = Candidatures.Domain.Mappings.DepartementSpecialiteMapping.GetDepartementById(candidature.DepartementId);
        var specialite = Candidatures.Domain.Mappings.DepartementSpecialiteMapping.GetSpecialiteById(candidature.SpecialiteId);

        return new CandidatureDto
        {
            Id = candidature.Id,
            StagiaireId = candidature.StagiaireId,
            DepartementId = candidature.DepartementId,
            DepartementNom = departement?.Nom ?? string.Empty,
            SpecialiteId = candidature.SpecialiteId,
            SpecialiteNom = specialite?.Nom ?? string.Empty,
            DureeStageMois = candidature.DureeStageMois,
            DateDebut = candidature.DateDebut,
            DateFin = candidature.DateFin,
            NiveauEtude = candidature.NiveauEtude,
            Ecole = candidature.Ecole,
            LettreMotivation = candidature.LettreMotivation,
            CvFileName = candidature.CvFileName,
            CvPath = candidature.CvPath,
            Statut = candidature.Statut,
            EncadrantId = candidature.EncadrantId,
            Commentaire = candidature.Commentaire,
            DateCreation = candidature.DateCreation,
            DateMiseAJour = candidature.DateMiseAJour,
            DateDecision = candidature.DateDecision,
            TransmisADirection = candidature.TransmisADirection,
            DateTransmissionDirection = candidature.DateTransmissionDirection
        };
    }
}
