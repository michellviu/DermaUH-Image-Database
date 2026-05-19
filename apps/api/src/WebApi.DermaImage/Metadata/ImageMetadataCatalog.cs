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
            "Identificador publico unico de la imagen.",
            "texto",
            null,
            true,
            "Formato DERM_0000001",
            image => image.PublicId),
        new(
            "FileName",
            "Nombre de archivo original",
            "Nombre con el que la imagen fue subida por el contribuidor.",
            "texto",
            null,
            true,
            null,
            image => image.FileName),
        new(
            "ContentType",
            "Tipo de contenido",
            "MIME type del archivo (ej. image/jpeg).",
            "texto",
            "image/jpeg, image/png, image/webp",
            true,
            null,
            image => image.ContentType),
        new(
            "FileSizeBytes",
            "Tamano del archivo (bytes)",
            "Peso del archivo en bytes.",
            "numero",
            null,
            true,
            null,
            image => image.FileSize > 0 ? image.FileSize.ToString(CultureInfo.InvariantCulture) : null),
        new(
            "CreatedAtUtc",
            "Fecha de carga (UTC)",
            "Fecha de creacion del registro en UTC.",
            "fecha",
            null,
            true,
            "Formato ISO 8601",
            image => image.CreatedAt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)),
        new(
            "IsPublic",
            "Visible publicamente",
            "Indica si la imagen es visible para usuarios anonimos.",
            "booleano",
            "true, false",
            true,
            null,
            image => FormatBool(image.IsPublic)),
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
            "Manipulacion de imagen",
            "Nivel de manipulacion o sintetizacion de la imagen.",
            "enum",
            "InstrumentOnly, Altered, Synthetic, Unknown",
            false,
            null,
            image => image.ImageManipulation?.ToString()),
        new(
            "DermoscopicType",
            "Tipo dermatoscopico",
            "Solo aplica cuando el tipo de imagen es Dermoscopic.",
            "enum",
            "ContactPolarized, ContactNonPolarized, NonContactPolarized",
            false,
            null,
            image => image.DermoscopicType?.ToString()),
        new(
            "PatientName",
            "Nombre del paciente",
            "Nombre del paciente asociado al registro.",
            "texto",
            null,
            true,
            null,
            image => image.PatientName),
        new(
            "ClinicalHistoryNumber",
            "Historia clinica",
            "Numero de historia clinica del paciente.",
            "texto",
            null,
            false,
            null,
            image => image.ClinicalHistoryNumber),
        new(
            "AgeApprox",
            "Edad aproximada",
            "Edad aproximada del paciente en intervalos de 5 anos.",
            "numero",
            null,
            true,
            null,
            image => image.AgeApprox?.ToString(CultureInfo.InvariantCulture)),
        new(
            "Sex",
            "Sexo",
            "Sexo biologico del paciente.",
            "enum",
            "Male, Female",
            true,
            null,
            image => image.Sex?.ToString()),
        new(
            "SkinColor",
            "Color de piel",
            "Clasificacion de color de piel.",
            "enum",
            "Blanca, Mestiza, Oscura",
            false,
            null,
            image => image.SkinColor?.ToString()),
        new(
            "FotoType",
            "Fototipo",
            "Clasificacion de fototipo cutaneo.",
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
            "Exposicion al sol",
            "Indica si hay exposicion solar relevante.",
            "booleano",
            "true, false",
            true,
            null,
            image => FormatNullableBool(image.SunExposure)),
        new(
            "AnatomSiteGeneral",
            "Sitio anatomico general",
            "Region anatomica general de la lesion.",
            "enum",
            "HeadNeck, UpperExtremity, LowerExtremity, AnteriorTorso, LateralTorso, PosteriorTorso, PalmsSoles, OralGenital",
            true,
            null,
            image => image.AnatomSiteGeneral?.ToString()),
        new(
            "AnatomSiteSpecial",
            "Sitio anatomico especial",
            "Sitio especial (acral, ungueal, etc.).",
            "enum",
            "AcralNOS, NailNOS, Fingernail, Toenail, AcralPalmsOrSoles, OralOrGenital",
            false,
            null,
            image => image.AnatomSiteSpecial?.ToString()),
        new(
            "ClinSizeLongDiamMm",
            "Diametro mayor (mm)",
            "Diametro mayor de la lesion en milimetros.",
            "numero",
            null,
            false,
            "Unidad: mm",
            image => FormatNullableNumber(image.ClinSizeLongDiamMm)),
        new(
            "Diagnosis",
            "Diagnostico",
            "Diagnostico textual del caso.",
            "texto",
            null,
            true,
            null,
            image => image.Diagnosis),
        new(
            "HistopathologicalDiagnosis",
            "Diagnostico histopatologico",
            "Diagnostico histopatologico del caso.",
            "texto",
            null,
            false,
            null,
            image => image.HistopathologicalDiagnosis),
        new(
            "DiagnosisCategory",
            "Categoria diagnostica",
            "Categoria general de malignidad.",
            "enum",
            "Benign, Indeterminate, Malignant",
            false,
            null,
            image => image.DiagnosisCategory?.ToString()),
        new(
            "InjuryType",
            "Tipo de lesion",
            "Tipo de lesion clinica.",
            "enum",
            "Melanoma, BasalCellCarcinoma, SquamousCellCarcinoma, Others",
            false,
            null,
            image => image.InjuryType?.ToString()),
        new(
            "DiagnosisConfirmType",
            "Metodo de confirmacion",
            "Metodo utilizado para confirmar el diagnostico.",
            "enum",
            "Histopathology, SingleContributorClinicalAssessment, SerialImagingShowingNoChange, SingleImageExpertConsensus, ConfocalMicroscopyWithConsensusDermoscopy",
            false,
            null,
            image => image.DiagnosisConfirmType?.ToString()),
        new(
            "MelThickMm",
            "Espesor de melanoma (mm)",
            "Profundidad de Breslow.",
            "numero",
            null,
            false,
            "Unidad: mm",
            image => FormatNullableNumber(image.MelThickMm, "F2")),
        new(
            "MelMitoticIndex",
            "Indice mitotico",
            "Indice mitotico para melanoma.",
            "enum",
            "Zero, LessThanOne, One, Two, Three, Four, MoreThanFour",
            false,
            null,
            image => image.MelMitoticIndex?.ToString()),
        new(
            "MelUlcer",
            "Ulceracion",
            "Ulceracion histopatologica.",
            "booleano",
            "true, false",
            false,
            null,
            image => FormatNullableBool(image.MelUlcer)),
        new(
            "ClinicalNotes",
            "Notas clinicas",
            "Observaciones clinicas adicionales.",
            "texto",
            null,
            false,
            null,
            image => image.ClinicalNotes),
        new(
            "DermoscopicComments",
            "Comentarios dermatoscopicos",
            "Observaciones dermatoscopicas adicionales.",
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
            image => image.Contributor?.FullName),
        new(
            "InstitutionName",
            "Institucion",
            "Institucion que aporto la imagen.",
            "texto",
            null,
            false,
            null,
            image => image.InstitutionName)
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
        var headers = new[] { "Campo", "Etiqueta", "Descripcion", "Tipo", "ValoresPermitidos", "Requerido", "Notas" };
        var rows = Definitions.Select(def => new[]
        {
            def.Key,
            def.Label,
            def.Description,
            def.DataType,
            def.AllowedValues ?? string.Empty,
            def.Required ? "Si" : "No",
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
