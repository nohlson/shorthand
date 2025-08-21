using System;
using System.Linq;                // <-- needed for Skip/Take
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shorthand.Windows
{
    public class OllamaService : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;

        public OllamaService(string baseUrl = "http://127.0.0.1:11434")
        {
            this.baseUrl = baseUrl;
            this.httpClient = new HttpClient();
        }

        public async Task<string> GenerateCommandAsync(string prompt, bool allowUnsafe = false)
        {
            try
            {
                var request = new OllamaChatRequest
                {
                    Model = "cmdgen",
                    Messages = new[]
                    {
                        new OllamaMessage
                        {
                            Role = "user",
                            Content = allowUnsafe ? $"{prompt} --unsafe" : prompt
                        }
                    },
                    Stream = false
                };

                var json = JsonSerializer.Serialize(request);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await httpClient.PostAsync($"{baseUrl}/api/chat", content)
                                                     .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var ollamaResponse = JsonSerializer.Deserialize<OllamaChatResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!string.IsNullOrWhiteSpace(ollamaResponse?.Message?.Content))
                {
                    return SanitizeCommand(ollamaResponse.Message.Content);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new OllamaException($"Failed to generate command: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                using var response = await httpClient.GetAsync($"{baseUrl}/api/tags").ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private static string SanitizeCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return string.Empty;

            var sanitized = command.Trim();

            // Strip fenced code blocks like ```bash\n...\n```
            if (sanitized.StartsWith("```", StringComparison.Ordinal))
            {
                var lines = sanitized.Split('\n');
                if (lines.Length > 2)
                {
                    sanitized = string.Join("\n", lines.Skip(1).Take(lines.Length - 2));
                }
            }

            // Remove stray backticks and common prompt prefixes
            sanitized = sanitized.Replace("`", "");
            sanitized = sanitized.TrimStart('$', '❯', '>', 'P', 'S', '>', '%', ' '); // handles $, ❯, >, PS>, %, etc.

            return sanitized.Trim();
        }

        public void Dispose()
        {
            httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class OllamaChatRequest
    {
        public string Model { get; set; } = string.Empty;
        public OllamaMessage[] Messages { get; set; } = Array.Empty<OllamaMessage>();
        public bool Stream { get; set; }
    }

    public class OllamaMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class OllamaChatResponse
    {
        public OllamaMessage? Message { get; set; }
    }

    public class OllamaException : Exception
    {
        public OllamaException(string message, Exception innerException) : base(message, innerException) { }
    }
}
