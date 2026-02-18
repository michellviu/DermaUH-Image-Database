namespace Domain.DermaImage.Entities.Enums;

/// <summary>
/// Method used to confirm the diagnosis classification.
/// </summary>
public enum DiagnosisConfirmType
{
    Histopathology,
    SingleContributorClinicalAssessment,
    SerialImagingShowingNoChange,
    SingleImageExpertConsensus,
    ConfocalMicroscopyWithConsensusDermoscopy
}
