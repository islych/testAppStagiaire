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
    private readonly IUserLookupService _userLookup;

    public CandidatureService(
        ICandidatureRepository repository,
        INotificationService notificationService,
        IUserLookupService userLookup)
    {
        _repository = repository;
        _notificationService = notificationService;
        _userLookup = userLookup;
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

    public async Task<IEnumerable<CandidatureDto>> GetAllCandidaturesAsync()
    {
        var candidatures = await _repository.GetAllAsync();
        return candidatures.Select(MapToDto);
    }

    public async Task<IEnumerable<CandidatureDto>> GetCandidaturesByDepartementAsync(int departementId)
    {
        var candidatures = await _repository.GetByDepartementIdAsync(departementId);
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

    /// <summary>Encadrant transmet la candidature à la Direction</summary>
    public async Task<CandidatureDto> TransmettreADirectionAsync(Guid candidatureId, int encadrantId)
    {
        var candidature = await _repository.GetByIdAsync(candidatureId)
            ?? throw new ApplicationException($"Candidature {candidatureId} non trouvée.");

        if (candidature.Statut != CandidatureStatus.EnAttente)
            throw new ApplicationException("Seules les candidatures en attente peuvent être transmises à la Direction.");

        candidature.Statut = CandidatureStatus.TransmiseADirection;
        candidature.EncadrantId = encadrantId;
        candidature.DateMiseAJour = DateTime.UtcNow;
        candidature.TransmisADirection = true;
        candidature.DestinataireTransmission = "Direction";
        candidature.DateTransmissionDirection = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(candidature);
        await _notificationService.NotifyTransmittedToDirectionAsync(updated.Id);

        return MapToDto(updated);
    }

    /// <summary>Direction transmet la candidature au Centre</summary>
    public async Task<CandidatureDto> TransmettreCentreAsync(Guid candidatureId)
    {
        var candidature = await _repository.GetByIdAsync(candidatureId)
            ?? throw new ApplicationException($"Candidature {candidatureId} non trouvée.");

        if (candidature.Statut != CandidatureStatus.TransmiseADirection)
            throw new ApplicationException("Seules les candidatures transmises à la Direction peuvent être envoyées au Centre.");

        candidature.DestinataireTransmission = "Centre";
        candidature.DateMiseAJour = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(candidature);

        return MapToDto(updated);
    }

    /// <summary>Centre transmet la candidature acceptée au RH</summary>
    public async Task<CandidatureDto> TransmettreRHAsync(Guid candidatureId)
    {
        var candidature = await _repository.GetByIdAsync(candidatureId)
            ?? throw new ApplicationException($"Candidature {candidatureId} non trouvée.");

        if (candidature.Statut != CandidatureStatus.Acceptee)
            throw new ApplicationException("Seules les candidatures acceptées peuvent être transmises au RH.");

        candidature.DestinataireTransmission = "RH";
        candidature.DateMiseAJour = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(candidature);

        return MapToDto(updated);
    }

    /// <summary>RH intègre le stagiaire dans le système (étape finale)</summary>
    public async Task<CandidatureDto> IntegrerStagiaireAsync(Guid candidatureId)
    {
        var candidature = await _repository.GetByIdAsync(candidatureId)
            ?? throw new ApplicationException($"Candidature {candidatureId} non trouvée.");

        if (!string.Equals(candidature.DestinataireTransmission, "RH", StringComparison.OrdinalIgnoreCase))
            throw new ApplicationException("Ce dossier n'a pas été transmis au RH.");

        candidature.DestinataireTransmission = "RH_Integre";
        candidature.DateMiseAJour = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(candidature);

        return MapToDto(updated);
    }

    /// <summary>Encadrant refuse directement une candidature</summary>
    public async Task<CandidatureDto> RejectCandidatureAsync(Guid candidatureId, int encadrantId, string commentaire)
    {
        var candidature = await _repository.GetByIdAsync(candidatureId)
            ?? throw new ApplicationException($"Candidature {candidatureId} non trouvée.");

        if (candidature.Statut != CandidatureStatus.EnAttente)
            throw new ApplicationException($"Cette candidature ne peut pas être refusée (statut actuel : {candidature.Statut}).");

        candidature.Statut = CandidatureStatus.Refusee;
        candidature.EncadrantId = encadrantId;
        candidature.Commentaire = commentaire;
        candidature.DateDecision = DateTime.UtcNow;
        candidature.DateMiseAJour = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(candidature);
        await _notificationService.NotifyCandidatureRejectedAsync(updated.Id, updated.StagiaireId, commentaire);

        return MapToDto(updated);
    }

    /// <summary>Direction accepte → email stagiaire + encadrant</summary>
    public async Task<CandidatureDto> AccepterParDirectionAsync(Guid candidatureId, string token)
    {
        var candidature = await _repository.GetByIdAsync(candidatureId)
            ?? throw new ApplicationException($"Candidature {candidatureId} non trouvée.");

        if (candidature.Statut != CandidatureStatus.TransmiseADirection)
            throw new ApplicationException("Seules les candidatures transmises à la Direction peuvent être acceptées ici.");

        candidature.Statut = CandidatureStatus.Acceptee;
        candidature.DateDecision = DateTime.UtcNow;
        candidature.DateMiseAJour = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(candidature);

        // Récupérer email + nom du stagiaire
        var userInfo = await _userLookup.GetUserInfoAsync(updated.StagiaireId, token);
        var email = userInfo?.Email ?? string.Empty;
        var nom = userInfo?.NomComplet ?? string.Empty;

        await _notificationService.NotifyAcceptedByDirectionAsync(
            updated.Id, updated.StagiaireId,
            updated.EncadrantId ?? 0,
            email, nom, token);

        return MapToDto(updated);
    }

    /// <summary>Direction refuse la candidature</summary>
    public async Task<CandidatureDto> RefuserParDirectionAsync(Guid candidatureId, string commentaire)
    {
        var candidature = await _repository.GetByIdAsync(candidatureId)
            ?? throw new ApplicationException($"Candidature {candidatureId} non trouvée.");

        if (candidature.Statut != CandidatureStatus.TransmiseADirection)
            throw new ApplicationException("Seules les candidatures transmises à la Direction peuvent être refusées ici.");

        candidature.Statut = CandidatureStatus.Refusee;
        candidature.Commentaire = commentaire;
        candidature.DateDecision = DateTime.UtcNow;
        candidature.DateMiseAJour = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(candidature);
        await _notificationService.NotifyCandidatureRejectedAsync(updated.Id, updated.StagiaireId, commentaire);

        return MapToDto(updated);
    }

    public async Task<CandidatureSuiviDto?> GetCandidatureSuiviAsync(Guid candidatureId)
    {
        var candidature = await _repository.GetByIdAsync(candidatureId);
        if (candidature == null) return null;

        return new CandidatureSuiviDto
        {
            Id = candidature.Id,
            Statut = candidature.Statut,
            DateCreation = candidature.DateCreation,
            DateDecision = candidature.DateDecision,
            Commentaire = candidature.Commentaire
        };
    }

    /// <summary>Conservé pour compatibilité — plus utilisé dans le nouveau flux</summary>
    public async Task<bool> TransmitToDirectionAsync(Guid candidatureId, string destinataire)
    {
        await TransmettreADirectionAsync(candidatureId, 0);
        return true;
    }

    /// <summary>
    /// Appelé automatiquement par Documents.Service quand tous les documents sont validés par le Centre.
    /// Met DestinataireTransmission = "DossierAccepte".
    /// </summary>
    public async Task<CandidatureDto> MarquerDossierAccepteAsync(Guid candidatureId)
    {
        var candidature = await _repository.GetByIdAsync(candidatureId)
            ?? throw new ApplicationException($"Candidature {candidatureId} non trouvée.");

        // Accepter uniquement si le dossier est bien en cours de traitement par le Centre
        if (candidature.Statut != CandidatureStatus.Acceptee)
            throw new ApplicationException(
                $"Le dossier ne peut pas être marqué accepté (statut actuel : {candidature.Statut}).");

        candidature.DestinataireTransmission = "DossierAccepte";
        candidature.DateMiseAJour = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(candidature);

        return MapToDto(updated);
    }
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
            DestinataireTransmission = candidature.DestinataireTransmission,
            DateTransmissionDirection = candidature.DateTransmissionDirection
        };
    }
}
