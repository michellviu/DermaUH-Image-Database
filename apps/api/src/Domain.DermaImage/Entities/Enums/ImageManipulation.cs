namespace Domain.DermaImage.Entities.Enums;

/// <summary>
/// Describes the fidelity of the image to that which was collected at the time of capture.
/// </summary>
public enum ImageManipulation
{
    InstrumentOnly,
    Altered,
    Synthetic,
    Unknown
}
