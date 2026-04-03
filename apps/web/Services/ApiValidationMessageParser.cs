using System.Text.Json;

namespace Web.DermaImage.Services;

public static class ApiValidationMessageParser
{
    public static async Task<string> BuildFriendlyErrorMessageAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
        {
            return $"No se pudo completar la operación (HTTP {(int)response.StatusCode}).";
        }

        try
        {
            using var json = JsonDocument.Parse(content);
            if (!json.RootElement.TryGetProperty("errors", out var errorsNode))
            {
                return $"No se pudo completar la operación: {content}";
            }

            var messages = new List<string>();
            foreach (var property in errorsNode.EnumerateObject())
            {
                foreach (var error in property.Value.EnumerateArray())
                {
                    var message = error.GetString();
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        messages.Add($"- {message}");
                    }
                }
            }

            if (messages.Count == 0)
            {
                return $"No se pudo completar la operación (HTTP {(int)response.StatusCode}).";
            }

            return "Revise las validaciones:\n" + string.Join(Environment.NewLine, messages);
        }
        catch
        {
            return $"No se pudo completar la operación: {content}";
        }
    }
}
