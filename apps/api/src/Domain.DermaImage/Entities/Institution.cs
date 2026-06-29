namespace Domain.DermaImage.Entities;

/// <summary>
/// Represents a contributing institution.
/// The primary key is a Guid Id (standard). The Name field is unique and is
/// used as the lookup key when contributors submit new images.
/// </summary>
public class Institution : BaseEntity
{
    /// <summary>Unique name of the institution — used as the natural lookup key.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional textual description of the institution.</summary>
    public string? Description { get; set; }

    /// <summary>Country where the institution is located.</summary>
    public string? Country { get; set; }

    // ── Navigation ────────────────────────────────────────────────────
    /// <summary>Images contributed by this institution.</summary>
    public ICollection<DermaImg> Images { get; set; } = [];
}
