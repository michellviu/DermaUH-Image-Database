using System.Text.Json;
using System.Text.RegularExpressions;

namespace Web.DermaImage.Services;

public partial class AuthService
{
    private static async Task<string> TryGetError(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return response.ReasonPhrase ?? "Error desconocido.";
            }

            var formatted = FormatApiError(content);
            if (!string.IsNullOrWhiteSpace(formatted))
            {
                return formatted;
            }

            return response.ReasonPhrase ?? "Error desconocido.";
        }
        catch
        {
            return response.ReasonPhrase ?? "Error desconocido.";
        }
    }

    private static string? FormatApiError(string content)
    {
        var trimmed = content.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            var root = doc.RootElement;

            if (root.TryGetProperty("message", out var messageElement)
                && messageElement.ValueKind == JsonValueKind.String)
            {
                return TranslateGeneralMessage(messageElement.GetString());
            }

            if (root.TryGetProperty("errors", out var errorsElement)
                && errorsElement.ValueKind == JsonValueKind.Object)
            {
                var validationMessages = BuildValidationMessages(errorsElement);
                if (validationMessages.Count > 0)
                {
                    return string.Join("\n", validationMessages);
                }

                return "Hay errores de validación. Revisa los datos e inténtalo nuevamente.";
            }

            if (root.TryGetProperty("title", out var titleElement)
                && titleElement.ValueKind == JsonValueKind.String)
            {
                var translatedTitle = TranslateGeneralMessage(titleElement.GetString());
                if (!string.IsNullOrWhiteSpace(translatedTitle))
                {
                    return translatedTitle;
                }
            }

            return TranslateGeneralMessage(trimmed);
        }
        catch
        {
            return TranslateGeneralMessage(trimmed);
        }
    }

    private static List<string> BuildValidationMessages(JsonElement errorsElement)
    {
        var messages = new List<string>();

        foreach (var field in errorsElement.EnumerateObject())
        {
            var fieldNameEs = TranslateFieldName(field.Name);
            if (field.Value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var item in field.Value.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var raw = item.GetString();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                var translated = TranslateValidationMessage(raw, fieldNameEs);
                if (!string.IsNullOrWhiteSpace(translated))
                {
                    messages.Add($"- {translated}");
                }
            }
        }

        return messages.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string TranslateValidationMessage(string rawMessage, string fieldNameEs)
    {
        var message = rawMessage.Trim();

        if (message.Contains("minimum length", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(message, @"minimum length of '?(\d+)'?");
            if (match.Success)
            {
                return $"El campo {fieldNameEs} debe tener al menos {match.Groups[1].Value} caracteres.";
            }

            return $"El campo {fieldNameEs} no cumple con la longitud mínima requerida.";
        }

        if (message.Contains("maximum length", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(message, @"maximum length of '?(\d+)'?");
            if (match.Success)
            {
                return $"El campo {fieldNameEs} permite como máximo {match.Groups[1].Value} caracteres.";
            }

            return $"El campo {fieldNameEs} excede la longitud máxima permitida.";
        }

        if (message.Contains("is required", StringComparison.OrdinalIgnoreCase))
        {
            return $"El campo {fieldNameEs} es obligatorio.";
        }

        if (message.Contains("not a valid e-mail", StringComparison.OrdinalIgnoreCase)
            || message.Contains("must be a valid email", StringComparison.OrdinalIgnoreCase))
        {
            return "El correo electrónico no tiene un formato válido.";
        }

        return TranslateGeneralMessage(message)
            .Replace("Password", "Contraseña", StringComparison.OrdinalIgnoreCase)
            .Replace("Email", "Correo electrónico", StringComparison.OrdinalIgnoreCase);
    }

    private static string TranslateGeneralMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Error desconocido.";
        }

        var value = message.Trim().Trim('"');

        if (value.Equals("One or more validation errors occurred.", StringComparison.OrdinalIgnoreCase))
        {
            return "Hay errores de validación. Revisa los datos e inténtalo nuevamente.";
        }

        if (value.Contains("Passwords must be at least", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(value, @"at least\s+(\d+)");
            if (match.Success)
            {
                return $"La contraseña debe tener al menos {match.Groups[1].Value} caracteres.";
            }

            return "La contraseña no cumple con la longitud mínima requerida.";
        }

        if (value.Contains("Passwords must have at least one non alphanumeric", StringComparison.OrdinalIgnoreCase))
        {
            return "La contraseña debe incluir al menos un carácter especial.";
        }

        if (value.Contains("Passwords must have at least one digit", StringComparison.OrdinalIgnoreCase))
        {
            return "La contraseña debe incluir al menos un número.";
        }

        if (value.Contains("Passwords must have at least one uppercase", StringComparison.OrdinalIgnoreCase))
        {
            return "La contraseña debe incluir al menos una letra mayúscula.";
        }

        if (value.Contains("Passwords must have at least one lowercase", StringComparison.OrdinalIgnoreCase))
        {
            return "La contraseña debe incluir al menos una letra minúscula.";
        }

        return value;
    }

    private static string TranslateFieldName(string fieldName)
    {
        return fieldName switch
        {
            "FirstName" => "Nombre",
            "LastName" => "Apellido",
            "Email" => "Correo electrónico",
            "Password" => "Contraseña",
            "ConfirmPassword" => "Confirmación de contraseña",
            "CurrentPassword" => "Contraseña actual",
            "NewPassword" => "Nueva contraseña",
            "InstitutionId" => "Institución",
            _ => fieldName,
        };
    }
}
