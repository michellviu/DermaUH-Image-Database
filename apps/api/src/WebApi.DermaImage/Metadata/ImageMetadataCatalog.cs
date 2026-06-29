using System.Globalization;
using System.Text;
using Domain.DermaImage.Entities;

namespace WebApi.DermaImage.Metadata;

public sealed record ImageMetadataDefinition(
    string Key,
    string Label,
    string Description,
    string DataType,
    string? AllowedValues,
    bool Required,
    string? Notes);

internal sealed record ImageMetadataField(
    string Key,
    string Label,
    string Description,
    string DataType,
    string? AllowedValues,
    bool Required,
    string? Notes,
    Func<DermaImg, string?> ValueResolver)
{
    public ImageMetadataDefinition ToDefinition() => new(Key, Label, Description, DataType, AllowedValues, Required, Notes);
}

public static class ImageMetadataCatalog
{
    private static readonly IReadOnlyList<ImageMetadataField> Fields =
    [
        new(
            "PublicId",
            "ID publico",
            "Identificador público único de la imagen.",
            "texto",
            null,
            true,
            "Formato DERM_0000001",
            image => image.PublicId),
        new(
            "ImageType",
            "Tipo de imagen",
            "Modalidad de captura de la imagen.",
            "enum",
            "Dermoscopic, ClinicalOverview, ClinicalCloseUp, TBPTileOverview, TBPTileCloseUp, RCMMacroscopic, RCMTile, RCMMosaic",
            false,
            null,
            image => image.ImageType?.ToString()),
        new(
            "ImageManipulation",
            "Manipulación de imagen",
            "Nivel de manipulación o sintetización de la imagen.",
            "enum",
            "InstrumentOnly, Altered, Synthetic, Unknown",
            false,
            null,
            image => image.ImageManipulation?.ToString()),
        new(
            "DermoscopicType",
            "Tipo dermatoscópico",
            "Solo aplica cuando el tipo de imagen es Dermoscopic.",
            "enum",
            "ContactPolarized, ContactNonPolarized, NonContactPolarized",
            false,
            null,
            image => image.DermoscopicType?.ToString()),
        new(
            "AgeApprox",
            "Edad aproximada",
            "Edad aproximada del paciente.",
            "entero",
            null,
            true,
            null,
            image => image.AgeApprox?.ToString(CultureInfo.InvariantCulture)),
        new(
            "Sex",
            "Sexo",
            "Sexo biológico del paciente.",
            "enum",
            "Male, Female",
            true,
            null,
            image => image.Sex?.ToString()),
        new(
            "SkinColor",
            "Color de piel",
            "Clasificación de color de piel.",
            "enum",
            "White, Mixed_Race, Black",
            false,
            null,
            image => image.SkinColor?.ToString()),
        new(
            "FotoType",
            "Fototipo",
            "Clasificación de fototipo cutaneo.",
            "enum",
            "I, II, III, IV, V, VI",
            false,
            null,
            image => image.FotoType?.ToString()),
        new(
            "PersonalHistory",
            "Antecedentes personales",
            "Antecedentes personales reportados.",
            "texto",
            null,
            false,
            null,
            image => image.PersonalHistory),
        new(
            "PersonalHxMm",
            "Historial personal de melanoma",
            "Indica si existe historial personal de melanoma.",
            "booleano",
            "true, false",
            false,
            null,
            image => FormatNullableBool(image.PersonalHxMm)),
        new(
            "FamilyHxMm",
            "Historial familiar de melanoma",
            "Indica si existe historial familiar de melanoma.",
            "booleano",
            "true, false",
            false,
            null,
            image => FormatNullableBool(image.FamilyHxMm)),
        new(
            "SunExposure",
            "Exposición al sol",
            "Indica si hay exposición solar relevante.",
            "booleano",
            "true, false",
            true,
            null,
            image => FormatNullableBool(image.SunExposure)),
        new(
            "Provincia",
            "Provincia",
            "Provincia de origen de la imagen.",
            "enum",
            "PinarDelRio, Artemisa, LaHabana, Mayabeque, Matanzas, VillaClara, Cienfuegos, SanctiSpiritus, CiegoDeAvila, Camaguey, LasTunas, Holguin, Granma, SantiagoDeCuba, Guantanamo, IslaDeLaJuventud",
            false,
            null,
            image => image.Provincia?.ToString()),
        new(
            "AnatomSiteGeneral",
            "Sitio anatómico general",
            "Región anatómica general de la lesión.",
            "enum",
            "HeadNeck, UpperExtremity, LowerExtremity, AnteriorTorso, LateralTorso, PosteriorTorso, PalmsSoles, OralGenital",
            true,
            null,
            image => image.AnatomSiteGeneral?.ToString()),
        new(
            "AnatomSiteSpecial",
            "Sitio anatómico especial",
            "Sitio especial (acral, ungueal, etc.).",
            "enum",
            "AcralNOS, NailNOS, Fingernail, Toenail, AcralPalmsOrSoles, OralOrGenital",
            false,
            null,
            image => image.AnatomSiteSpecial?.ToString()),
        new(
            "ClinSizeLongDiamMm",
            "Diámetro mayor (mm)",
            "Diámetro mayor de la lesión en milímetros.",
            "decimal",
            null,
            false,
            "Unidad: mm",
            image => FormatNullableNumber(image.ClinSizeLongDiamMm)),
        new(
            "Diagnosis",
            "Diagnóstico",
            "Diagnóstico textual del caso.",
            "texto",
            null,
            true,
            null,
            image => image.Diagnosis),
        new(
            "HistopathologicalDiagnosis",
            "Diagnóstico histopatológico",
            "Diagnóstico histopatológico del caso.",
            "texto",
            null,
            false,
            null,
            image => image.HistopathologicalDiagnosis),
        new(
            "DiagnosisCategory",
            "Categoría diagnóstica",
            "Categoría general de malignidad.",
            "enum",
            "Benign, Indeterminate, Malignant",
            false,
            null,
            image => image.DiagnosisCategory?.ToString()),
        new(
            "InjuryType",
            "Tipo de lesión",
            "Tipo de lesión clinica.",
            "enum",
            "Melanoma, BasalCellCarcinoma, SquamousCellCarcinoma, Others",
            false,
            null,
            image => image.InjuryType?.ToString()),
        new(
            "DiagnosisConfirmType",
            "Método de confirmación",
            "Método utilizado para confirmar el diagnóstico.",
            "enum",
            "Histopathology, SingleContributorClinicalAssessment, SerialImagingShowingNoChange, SingleImageExpertConsensus, ConfocalMicroscopyWithConsensusDermoscopy",
            false,
            null,
            image => image.DiagnosisConfirmType?.ToString()),
        new(
            "MelThickMm",
            "Espesor de melanoma (mm)",
            "Profundidad de Breslow.",
            "decimal",
            null,
            false,
            "Unidad: mm",
            image => FormatNullableNumber(image.MelThickMm, "F2")),
        new(
            "MelMitoticIndex",
            "Índice mitótico",
            "Índice mitótico para melanoma.",
            "enum",
            "Zero, LessThanOne, One, Two, Three, Four, MoreThanFour",
            false,
            null,
            image => image.MelMitoticIndex?.ToString()),
        new(
            "MelUlcer",
            "Ulceración",
            "Ulceración histopatológica.",
            "booleano",
            "true, false",
            false,
            null,
            image => FormatNullableBool(image.MelUlcer)),
        new(
            "ClinicalNotes",
            "Notas clínicas",
            "Observaciones clínicas adicionales.",
            "texto",
            null,
            false,
            null,
            image => image.ClinicalNotes),
        new(
            "DermoscopicComments",
            "Comentarios dermatoscópicos",
            "Observaciones dermatoscópicas adicionales.",
            "texto",
            null,
            false,
            null,
            image => image.DermoscopicComments),
        new(
            "InformedConsent",
            "Consentimiento informado",
            "Indica si existe consentimiento informado.",
            "booleano",
            "true, false",
            false,
            null,
            image => FormatNullableBool(image.InformedConsent)),
        new(
            "InformedConsentDate",
            "Fecha de consentimiento",
            "Fecha de firma del consentimiento informado.",
            "fecha",
            null,
            false,
            "Formato ISO 8601",
            image => image.InformedConsentDate?.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)),
        new(
            "InformedConsentText",
            "Texto de consentimiento",
            "Texto o referencia del consentimiento informado.",
            "texto",
            null,
            false,
            null,
            image => image.InformedConsentText),
        new(
            "ContributorFullName",
            "Contribuidor",
            "Nombre del contribuidor asociado.",
            "texto",
            null,
            false,
            null,
            image => null),
        new(
            "InstitutionName",
            "Institución",
            "Institución que aportó la imagen.",
            "texto",
            null,
            false,
            null,
            image => image.Institution?.Name)
    ];

    public static IReadOnlyList<ImageMetadataDefinition> Definitions => Fields
        .Select(metadataField => new ImageMetadataDefinition(
            metadataField.Key,
            metadataField.Label,
            metadataField.Description,
            metadataField.DataType,
            metadataField.AllowedValues,
            metadataField.Required,
            metadataField.Notes))
        .ToList();

    public static string BuildImagesCsv(IEnumerable<DermaImg> images)
    {
        var headers = Fields.Select(field => field.Key);
        var rows = images.Select(image => Fields.Select(field => field.ValueResolver(image)));
        return CsvBuilder.Build(headers, rows);
    }

    public static string BuildDefinitionsCsv()
    {
        var headers = new[] { "Campo", "Etiqueta", "Descripción", "Tipo", "Valores Permitidos", "Requerido", "Notas" };
        var rows = Definitions.Select(def => new[]
        {
            def.Key,
            def.Label,
            def.Description,
            def.DataType,
            def.AllowedValues ?? string.Empty,
            def.Required ? "Sí" : "No",
            def.Notes ?? string.Empty
        });

        return CsvBuilder.Build(headers, rows);
    }

    private static string? FormatNullableNumber(double? value, string? format = null)
    {
        return value.HasValue
            ? value.Value.ToString(format ?? "G", CultureInfo.InvariantCulture)
            : null;
    }

    private static string FormatBool(bool value) => value ? "true" : "false";

    private static string? FormatNullableBool(bool? value)
    {
        return value.HasValue ? FormatBool(value.Value) : null;
    }

    private static class CsvBuilder
    {
        public static string Build(IEnumerable<string> headers, IEnumerable<IEnumerable<string?>> rows)
        {
            var builder = new StringBuilder();
            AppendRow(builder, headers);

            foreach (var row in rows)
            {
                AppendRow(builder, row);
            }

            return builder.ToString();
        }

        private static void AppendRow(StringBuilder builder, IEnumerable<string?> values)
        {
            var first = true;
            foreach (var value in values)
            {
                if (!first)
                {
                    builder.Append(',');
                }

                builder.Append(Escape(value));
                first = false;
            }

            builder.AppendLine();
        }

        private static string Escape(string? value)
        {
            var text = value ?? string.Empty;
            var needsQuotes = text.Contains(',') || text.Contains('"') || text.Contains('\n') || text.Contains('\r');

            if (text.Contains('"'))
            {
                text = text.Replace("\"", "\"\"");
            }

            return needsQuotes ? $"\"{text}\"" : text;
        }
    }
}
