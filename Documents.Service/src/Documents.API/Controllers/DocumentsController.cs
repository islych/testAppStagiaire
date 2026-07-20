using Documents.Application.DTOs;
using Documents.Application.Interfaces;
using Documents.Domain.Enums;
using Documents.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Documents.API.Controllers;

/// <summary>
/// Contrôleur pour la gestion des documents administratifs
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _service;
    private readonly ILogger<DocumentsController> _logger;

    // Dossier de stockage physique des fichiers uploadés
    private const string DossierUpload = "uploads/documents";

    public DocumentsController(
        IDocumentService service,
        ILogger<DocumentsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UPLOAD
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Dépose un nouveau document (Stagiaire uniquement)
    /// Accepte un fichier en multipart/form-data
    /// </summary>
    /// <param name="fichier">Fichier à uploader</param>
    /// <param name="type">Type de document (CV, ConventionDeStage, Assurance, etc.)</param>
    /// <param name="candidatureId">ID de la candidature associée (optionnel)</param>
    /// <returns>Document créé</returns>
    /// <response code="201">Document uploadé avec succès</response>
    /// <response code="400">Fichier invalide ou données manquantes</response>
    /// <response code="401">Non authentifié</response>
    /// <response code="403">Rôle non autorisé</response>
    [HttpPost("upload")]
    [Authorize(Roles = "Stagiaire")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<DocumentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        IFormFile fichier,
        [FromForm] TypeDocument type,
        [FromForm] Guid? candidatureId = null)
    {
        try
        {
            var stagiaireId = GetCurrentUserId();
            if (stagiaireId == null)
                return Unauthorized(Erreur("Identifiant du stagiaire invalide."));

            if (fichier == null || fichier.Length == 0)
                return BadRequest(Erreur("Aucun fichier fourni."));

            _logger.LogInformation(
                "Upload document de type {Type} par le stagiaire {StagiaireId}",
                type, stagiaireId);

            // Sauvegarder le fichier sur le serveur
            var (nomStockage, chemin) = await SauvegarderFichierAsync(fichier);
            var extension = Path.GetExtension(fichier.FileName).ToLower();

            // Extraire le JWT depuis le header Authorization
            var jwtToken = Request.Headers["Authorization"]
                .ToString().Replace("Bearer ", "").Trim();

            var document = await _service.UploadDocumentAsync(
                stagiaireId.Value,
                candidatureId,
                type,
                fichier.FileName,
                nomStockage,
                chemin,
                extension,
                fichier.Length,
                fichier.ContentType,
                jwtToken);

            return CreatedAtAction(nameof(GetById), new { id = document.Id },
                Succes("Document déposé avec succès. Il est en attente de vérification.", document));
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning("Erreur upload document : {Message}", ex.Message);
            return BadRequest(Erreur(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de l'upload");
            return StatusCode(500, Erreur("Erreur serveur."));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SOUMETTRE CORRECTIONS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Soumet une nouvelle version d'un document refusé (Stagiaire uniquement)
    /// </summary>
    /// <param name="id">ID du document original refusé</param>
    /// <param name="fichier">Nouveau fichier corrigé</param>
    /// <param name="type">Type de document</param>
    /// <returns>Nouvelle version du document</returns>
    /// <response code="201">Correction soumise avec succès</response>
    /// <response code="400">Document non refusé ou fichier invalide</response>
    /// <response code="404">Document original introuvable</response>
    [HttpPost("{id:guid}/corrections")]
    [Authorize(Roles = "Stagiaire")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<DocumentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoumettreCorrections(
        Guid id,
        IFormFile fichier,
        [FromForm] TypeDocument type)
    {
        try
        {
            var stagiaireId = GetCurrentUserId();
            if (stagiaireId == null)
                return Unauthorized(Erreur("Identifiant du stagiaire invalide."));

            if (fichier == null || fichier.Length == 0)
                return BadRequest(Erreur("Aucun fichier fourni."));

            _logger.LogInformation(
                "Correction document {DocumentId} par le stagiaire {StagiaireId}", id, stagiaireId);

            var (nomStockage, chemin) = await SauvegarderFichierAsync(fichier);
            var extension = Path.GetExtension(fichier.FileName).ToLower();

            var document = await _service.SoumettreCorrectionsAsync(
                id,
                stagiaireId.Value,
                type,
                fichier.FileName,
                nomStockage,
                chemin,
                extension,
                fichier.Length,
                fichier.ContentType);

            return CreatedAtAction(nameof(GetById), new { id = document.Id },
                Succes("Correction soumise. Le document est en attente de re-vérification.", document));
        }
        catch (DocumentNotFoundException)
        {
            return NotFound(Erreur($"Document {id} introuvable."));
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning("Erreur correction document : {Message}", ex.Message);
            return BadRequest(Erreur(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la soumission de correction");
            return StatusCode(500, Erreur("Erreur serveur."));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // TRANSMETTRE AU CENTRE
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// L'encadrant transmet tous les documents en attente d'une candidature au Centre.
    /// </summary>
    [HttpPost("candidature/{candidatureId:guid}/transmettre-centre")]
    [Authorize(Roles = "Encadrant")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DocumentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TransmettreAuCentre(Guid candidatureId)
    {
        try
        {
            var encadrantId = GetCurrentUserId();
            if (encadrantId == null)
                return Unauthorized(Erreur("Identifiant de l'encadrant invalide."));

            _logger.LogInformation(
                "Transmission au Centre des documents candidature {CandidatureId} par encadrant {EncadrantId}",
                candidatureId, encadrantId);

            var documents = await _service.TransmettreAuCentreAsync(candidatureId, encadrantId.Value);

            return Ok(Succes("Documents transmis au Centre avec succès.", documents));
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning("Erreur transmission au Centre : {Message}", ex.Message);
            return BadRequest(Erreur(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la transmission au Centre");
            return StatusCode(500, Erreur("Erreur serveur."));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // VALIDER (Centre uniquement)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Valide un document (Centre uniquement — document doit être TransmisAuCentre ou EnCorrectionSoumise)
    /// </summary>
    [HttpPost("{id:guid}/valider")]
    [Authorize(Roles = "Centre,Administrateur")]
    [ProducesResponseType(typeof(ApiResponse<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Valider(Guid id, [FromBody] ValiderDocumentDto dto)
    {
        try
        {
            var verificateurId = GetCurrentUserId();
            if (verificateurId == null)
                return Unauthorized(Erreur("Identifiant du vérificateur invalide."));

            _logger.LogInformation(
                "Validation du document {DocumentId} par le vérificateur {VerificateurId}",
                id, verificateurId);

            var document = await _service.ValiderDocumentAsync(id, verificateurId.Value, dto.Commentaire,
                Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim());

            return Ok(Succes("Document validé avec succès.", document));
        }
        catch (DocumentNotFoundException)
        {
            return NotFound(Erreur($"Document {id} introuvable."));
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning("Erreur validation document : {Message}", ex.Message);
            return BadRequest(Erreur(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la validation");
            return StatusCode(500, Erreur("Erreur serveur."));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // REFUSER (Centre uniquement)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Refuse définitivement un document (Centre uniquement)
    /// </summary>
    [HttpPost("{id:guid}/refuser")]
    [Authorize(Roles = "Centre,Administrateur")]
    [ProducesResponseType(typeof(ApiResponse<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Refuser(Guid id, [FromBody] RefuserDocumentDto dto)
    {
        try
        {
            var verificateurId = GetCurrentUserId();
            if (verificateurId == null)
                return Unauthorized(Erreur("Identifiant du vérificateur invalide."));

            _logger.LogInformation(
                "Refus du document {DocumentId} par le vérificateur {VerificateurId}",
                id, verificateurId);

            var document = await _service.RefuserDocumentAsync(id, verificateurId.Value, dto.CommentaireRefus);

            return Ok(Succes("Document refusé. Le stagiaire a été notifié.", document));
        }
        catch (DocumentNotFoundException)
        {
            return NotFound(Erreur($"Document {id} introuvable."));
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning("Erreur refus document : {Message}", ex.Message);
            return BadRequest(Erreur(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors du refus");
            return StatusCode(500, Erreur("Erreur serveur."));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SUPPRIMER
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Supprime un document en attente (Stagiaire propriétaire uniquement)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Stagiaire")]
    public async Task<IActionResult> Supprimer(Guid id)
    {
        try
        {
            var stagiaireId = GetCurrentUserId();
            if (stagiaireId == null)
                return Unauthorized(Erreur("Identifiant invalide."));

            await _service.SupprimerDocumentAsync(id, stagiaireId.Value);

            return Ok(new { success = true, message = "Document supprimé." });
        }
        catch (DocumentNotFoundException)
        {
            return NotFound(Erreur($"Document {id} introuvable."));
        }
        catch (ApplicationException ex)
        {
            return BadRequest(Erreur(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur suppression document {Id}", id);
            return StatusCode(500, Erreur("Erreur serveur."));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DEMANDER MODIFICATION (Centre uniquement)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Le Centre demande des modifications sur un document.
    /// Met le document en statut DemandeModification et notifie le stagiaire.
    /// La correction soumise par le stagiaire ira directement au Centre.
    /// </summary>
    [HttpPost("{id:guid}/demander-modification")]
    [Authorize(Roles = "Centre,Administrateur")]
    [ProducesResponseType(typeof(ApiResponse<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DemanderModification(Guid id, [FromBody] DemanderCorrectionDto dto)
    {
        try
        {
            var centreUserId = GetCurrentUserId();
            if (centreUserId == null)
                return Unauthorized(Erreur("Identifiant invalide."));

            if (string.IsNullOrWhiteSpace(dto.Commentaire))
                return BadRequest(Erreur("Un commentaire est obligatoire pour demander une modification."));

            _logger.LogInformation(
                "Demande de modification sur document {DocumentId} par Centre {UserId}",
                id, centreUserId);

            var document = await _service.DemanderModificationAsync(id, centreUserId.Value, dto.Commentaire);

            return Ok(Succes("Modification demandée. Le stagiaire a été notifié.", document));
        }
        catch (DocumentNotFoundException)
        {
            return NotFound(Erreur($"Document {id} introuvable."));
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning("Erreur demande modification : {Message}", ex.Message);
            return BadRequest(Erreur(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la demande de modification");
            return StatusCode(500, Erreur("Erreur serveur."));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // QUERIES
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Récupère un document par son ID
    /// </summary>
    /// <param name="id">ID du document</param>
    /// <returns>Document trouvé</returns>
    /// <response code="200">Document trouvé</response>
    /// <response code="404">Document introuvable</response>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            _logger.LogInformation("Récupération du document {DocumentId}", id);
            var document = await _service.GetDocumentByIdAsync(id);

            if (document == null)
                return NotFound(Erreur($"Document {id} introuvable."));

            return Ok(Succes("Document trouvé.", document));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la récupération du document");
            return StatusCode(500, Erreur("Erreur serveur."));
        }
    }

    /// <summary>
    /// Récupère les documents du stagiaire connecté
    /// </summary>
    /// <returns>Liste des documents du stagiaire (version courante)</returns>
    /// <response code="200">Documents récupérés</response>
    [HttpGet("me")]
    [Authorize(Roles = "Stagiaire")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DocumentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMesDocuments()
    {
        try
        {
            var stagiaireId = GetCurrentUserId();
            if (stagiaireId == null)
                return Unauthorized(Erreur("Identifiant du stagiaire invalide."));

            _logger.LogInformation(
                "Récupération des documents du stagiaire {StagiaireId}", stagiaireId);

            var documents = await _service.GetDocumentsByStagiaireAsync(stagiaireId.Value);

            return Ok(Succes($"{documents.Count()} document(s) trouvé(s).", documents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la récupération des documents du stagiaire");
            return StatusCode(500, Erreur("Erreur serveur."));
        }
    }

    /// <summary>
    /// Récupère les documents d'un stagiaire par son ID (Encadrant / RH / Centre / Administrateur)
    /// </summary>
    /// <param name="stagiaireId">ID du stagiaire</param>
    /// <returns>Documents du stagiaire</returns>
    /// <response code="200">Documents récupérés</response>
    [HttpGet("stagiaire/{stagiaireId:int}")]
    [Authorize(Roles = "Encadrant,RH,Centre,Direction,Administrateur")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DocumentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStagiaire(int stagiaireId)
    {
        try
        {
            _logger.LogInformation(
                "Récupération des documents du stagiaire {StagiaireId}", stagiaireId);

            var documents = await _service.GetDocumentsByStagiaireAsync(stagiaireId);

            return Ok(Succes($"{documents.Count()} document(s) trouvé(s).", documents));
        }
        catch (ApplicationException ex)
        {
            return BadRequest(Erreur(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la récupération des documents");
            return StatusCode(500, Erreur("Erreur serveur."));
        }
    }

    /// <summary>
    /// Récupère les documents liés à une candidature
    /// </summary>
    /// <param name="candidatureId">ID de la candidature</param>
    /// <returns>Documents de la candidature</returns>
    /// <response code="200">Documents récupérés</response>
    [HttpGet("candidature/{candidatureId:guid}")]
    [Authorize(Roles = "Encadrant,RH,Centre,Administrateur,Stagiaire")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DocumentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCandidature(Guid candidatureId)
    {
        try
        {
            _logger.LogInformation(
                "Récupération des documents de la candidature {CandidatureId}", candidatureId);

            var documents = await _service.GetDocumentsByCandidatureAsync(candidatureId);

            return Ok(Succes($"{documents.Count()} document(s) trouvé(s).", documents));
        }
        catch (ApplicationException ex)
        {
            return BadRequest(Erreur(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la récupération des documents de candidature");
            return StatusCode(500, Erreur("Erreur serveur."));
        }
    }

    /// <summary>
    /// Récupère tous les documents — version courante (Encadrant / RH / Centre / Administrateur)
    /// </summary>
    /// <returns>Tous les documents</returns>
    /// <response code="200">Documents récupérés</response>
    [HttpGet]
    [Authorize(Roles = "Encadrant,RH,Centre,Administrateur")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DocumentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            _logger.LogInformation("Récupération de tous les documents");
            var documents = await _service.GetAllDocumentsAsync();

            return Ok(Succes($"{documents.Count()} document(s) trouvé(s).", documents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la récupération de tous les documents");
            return StatusCode(500, Erreur("Erreur serveur."));
        }
    }

    /// <summary>
    /// Récupère les documents filtrés par statut (Encadrant / RH / Centre)
    /// </summary>
    /// <param name="statut">Statut à filtrer (EnAttente, Valide, Refuse, EnCorrectionSoumise)</param>
    /// <returns>Documents filtrés par statut</returns>
    /// <response code="200">Documents récupérés</response>
    [HttpGet("statut/{statut}")]
    [Authorize(Roles = "Encadrant,RH,Centre,Administrateur")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DocumentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStatut(DocumentStatut statut)
    {
        try
        {
            _logger.LogInformation("Récupération des documents avec statut {Statut}", statut);
            var documents = await _service.GetDocumentsByStatutAsync(statut);

            return Ok(Succes($"{documents.Count()} document(s) avec statut '{statut}'.", documents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la récupération par statut");
            return StatusCode(500, Erreur("Erreur serveur."));
        }
    }

    /// <summary>
    /// Télécharge un fichier document (propriétaire ou rôle autorisé)
    /// </summary>
    /// <param name="id">ID du document</param>
    /// <returns>Fichier en téléchargement</returns>
    /// <response code="200">Fichier retourné</response>
    /// <response code="404">Document ou fichier introuvable</response>
    [HttpGet("{id:guid}/telecharger")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Telecharger(Guid id)
    {
        try
        {
            _logger.LogInformation("Téléchargement du document {DocumentId}", id);

            var document = await _service.GetDocumentByIdAsync(id);
            if (document == null)
                return NotFound(Erreur($"Document {id} introuvable."));

            // Vérifier l'accès : le stagiaire ne peut télécharger que ses propres documents
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value
                        ?? User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            var estStagiaire = userRole == "Stagiaire";
            if (estStagiaire && userId != document.StagiaireId)
                return Forbid();

            // Chercher le fichier sur le disque
            var cheminFichier = Path.Combine(
                Directory.GetCurrentDirectory(),
                DossierUpload,
                document.NomFichier); // fallback sur NomFichier si CheminFichier non absolu

            // Lire CheminFichier depuis le DTO (non exposé, on utilise le service pour récupérer l'entité)
            // Le chemin physique est reconstruit depuis le dossier upload + nom de stockage
            var cheminPhysique = Path.Combine(Directory.GetCurrentDirectory(), DossierUpload);
            var fichierPath = Directory.GetFiles(cheminPhysique, "*", SearchOption.AllDirectories)
                .FirstOrDefault(f => Path.GetFileName(f).StartsWith(id.ToString()));

            if (fichierPath == null || !System.IO.File.Exists(fichierPath))
                return NotFound(Erreur("Fichier physique introuvable sur le serveur."));

            var contenu = await System.IO.File.ReadAllBytesAsync(fichierPath);
            return File(contenu, document.ContentType, document.NomFichier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors du téléchargement");
            return StatusCode(500, Erreur("Erreur serveur."));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS PRIVÉS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Extrait l'ID de l'utilisateur connecté depuis le JWT
    /// </summary>
    private int? GetCurrentUserId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idStr, out var id) ? id : null;
    }

    /// <summary>
    /// Sauvegarde le fichier sur le disque et retourne (nomStockage, chemin)
    /// Le nom de stockage inclut l'ID unique pour éviter les collisions
    /// </summary>
    private async Task<(string nomStockage, string chemin)> SauvegarderFichierAsync(IFormFile fichier)
    {
        var dossier = Path.Combine(Directory.GetCurrentDirectory(), DossierUpload);
        Directory.CreateDirectory(dossier);

        var extension = Path.GetExtension(fichier.FileName);
        var nomStockage = $"{Guid.NewGuid()}{extension}";
        var chemin = Path.Combine(dossier, nomStockage);

        await using var stream = new FileStream(chemin, FileMode.Create);
        await fichier.CopyToAsync(stream);

        return (nomStockage, chemin);
    }

    /// <summary>Construit une réponse de succès</summary>
    private static ApiResponse<T> Succes<T>(string message, T data) => new()
    {
        Success = true,
        Message = message,
        Data = data
    };

    /// <summary>Construit une réponse d'erreur</summary>
    private static ApiResponse<object> Erreur(string message) => new()
    {
        Success = false,
        Message = message
    };
}
