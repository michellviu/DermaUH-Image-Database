using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using System.Text.Json;

namespace Tests.Load.DermaImage;

/// <summary>
/// Escenarios de prueba de carga utilizando NBomber.
/// Simulan tráfico concurrente al API de la base de datos dermatológica
/// para evaluar el comportamiento bajo condiciones de estrés, como indicó
/// el oponente de la tesis.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        // NOTA: Para ejecutar estas pruebas de forma confiable, el API
        // debe estar corriendo (ej. http://localhost:5000 o un entorno de pruebas dedicado).
        const string baseUrl = "http://localhost:5131"; // Ajustar al puerto local o URL real
        
        using var httpClient = new HttpClient();

        // ── Escenario 1: Navegación de galería pública (Traffic alto, Anónimo) ──
        
        var getImagesScenario = Scenario.Create("get_public_gallery", async context =>
        {
            var request = Http.CreateRequest("GET", $"{baseUrl}/api/images?page=1&pageSize=20");
            var response = await Http.Send(httpClient, request);

            if (response.Payload.Value.IsSuccessStatusCode)
                return Response.Ok(statusCode: ((int)response.Payload.Value.StatusCode).ToString());
            
            return Response.Fail(statusCode: ((int)response.Payload.Value.StatusCode).ToString(), message: "Fallo al obtener galería");
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 50, during: TimeSpan.FromSeconds(30))
        );

        var getPreviewScenario = Scenario.Create("get_image_preview", async context =>
        {
            var request = Http.CreateRequest("GET", $"{baseUrl}/api/images/public/DERM_0000001");
            var response = await Http.Send(httpClient, request);

            var statusCode = (int)response.Payload.Value.StatusCode;
            if (statusCode == 200 || statusCode == 404)
            {
                var length = response.Payload.Value.Content.Headers.ContentLength ?? 0;
                return Response.Ok(statusCode: statusCode.ToString(), sizeBytes: length);
            }
            
            return Response.Fail(statusCode: statusCode.ToString());
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 100, during: TimeSpan.FromSeconds(20))
        );

        NBomberRunner
            .RegisterScenarios(getImagesScenario, getPreviewScenario)
            .WithReportFileName("DermaUH_LoadTest_Report")
            .WithReportFolder("./LoadTestReports")
            .Run();
    }
}
