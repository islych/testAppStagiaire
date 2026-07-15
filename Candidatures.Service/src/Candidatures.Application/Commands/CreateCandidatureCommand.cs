namespace Candidatures.Application.Commands;

/// <summary>
/// Commande pour créer une candidature
/// </summary>
public class CreateCandidatureCommand
{
    /// <summary>
    /// Identifiant du stagiaire (int - matching Authentication.Service)
    /// </summary>
    public int StagiaireId { get; set; }

    /// <summary>
    /// Identifiant du département (int - matching MasterData)
    /// </summary>
    public int DepartementId { get; set; }

    /// <summary>
    /// Identifiant de la spécialité (int - matching MasterData)
    /// </summary>
    public int SpecialiteId { get; set; }

    /// <summary>
    /// Nom du fichier CV
    /// </summary>
    public string CvFileName { get; set; } = string.Empty;

    /// <summary>
    /// Chemin du fichier CV
    /// </summary>
    public string CvPath { get; set; } = string.Empty;

    public CreateCandidatureCommand(int stagiaireId, int departementId, int specialiteId, string cvFileName, string cvPath)
    {
        StagiaireId = stagiaireId;
        DepartementId = departementId;
        SpecialiteId = specialiteId;
        CvFileName = cvFileName;
        CvPath = cvPath;
    }
}
