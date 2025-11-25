using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Cardapio_Inteligente.Servicos
{
    /// <summary>
    /// Gerencia o servidor API local automaticamente no Desktop (Windows/Mac)
    /// No mobile, a API deve ser hospedada na nuvem
    /// </summary>
    public class ApiServerManager : IDisposable
    {
        private Process? _apiProcess;
        private bool _isRunning = false;
        private readonly string _apiExePath;
        private readonly int _apiPort = 5068;

        public bool IsRunning => _isRunning;
        public string ApiUrl => $"http://localhost:{_apiPort}";

        public ApiServerManager()
        {
            // Determina o caminho do executável da API baseado na plataforma
            _apiExePath = GetApiExecutablePath();
        }

        /// <summary>
        /// Retorna o caminho do executável da API
        /// </summary>
        private string GetApiExecutablePath()
        {
#if WINDOWS
            // Windows: procura pela DLL da API na pasta de instalação
            var appPath = AppContext.BaseDirectory;
            var apiPath = Path.Combine(appPath, "API", "Cardapio_Inteligente.Api.dll");
            
            // Se não encontrar, tenta na pasta pai (durante desenvolvimento)
            if (!File.Exists(apiPath))
            {
                var parentPath = Directory.GetParent(appPath)?.Parent?.Parent?.Parent?.Parent?.FullName;
                if (parentPath != null)
                {
                    apiPath = Path.Combine(parentPath, "Cardapio_Inteligente.Api", "bin", "Debug", "net8.0", "Cardapio_Inteligente.Api.dll");
                }
            }
            
            return apiPath;
#elif MACCATALYST
            // macOS: similar ao Windows, mas com caminhos diferentes
            var appPath = AppContext.BaseDirectory;
            var apiPath = Path.Combine(appPath, "API", "Cardapio_Inteligente.Api.dll");
            
            if (!File.Exists(apiPath))
            {
                var parentPath = Directory.GetParent(appPath)?.Parent?.Parent?.Parent?.Parent?.FullName;
                if (parentPath != null)
                {
                    apiPath = Path.Combine(parentPath, "Cardapio_Inteligente.Api", "bin", "Debug", "net8.0", "Cardapio_Inteligente.Api.dll");
                }
            }
            
            return apiPath;
#else
            // Mobile: não usa servidor local
            return string.Empty;
#endif
        }

        /// <summary>
        /// Inicia a API local (apenas Desktop)
        /// </summary>
        public async Task<bool> StartApiAsync()
        {
#if ANDROID || IOS
            // Mobile não inicia API local
            Console.WriteLine("📱 Plataforma mobile detectada - usando API remota");
            return false;
#else
            if (_isRunning)
            {
                Console.WriteLine("⚠️ API já está rodando");
                return true;
            }

            if (string.IsNullOrEmpty(_apiExePath) || !File.Exists(_apiExePath))
            {
                Console.WriteLine($"❌ Executável da API não encontrado em: {_apiExePath}");
                Console.WriteLine("ℹ️ A API deve ser publicada junto com o aplicativo.");
                return false;
            }

            try
            {
                Console.WriteLine($"🚀 Iniciando API local em: {_apiExePath}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{_apiExePath}\" --urls=\"{ApiUrl}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                };

                _apiProcess = Process.Start(startInfo);

                if (_apiProcess == null)
                {
                    Console.WriteLine("❌ Falha ao iniciar processo da API");
                    return false;
                }

                // Monitora saída do processo
                _apiProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine($"[API] {e.Data}");
                };

                _apiProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine($"[API ERROR] {e.Data}");
                };

                _apiProcess.BeginOutputReadLine();
                _apiProcess.BeginErrorReadLine();

                _isRunning = true;
                Console.WriteLine($"✅ API iniciada com sucesso no PID: {_apiProcess.Id}");
                Console.WriteLine($"🌐 API disponível em: {ApiUrl}");

                // Aguarda alguns segundos para a API inicializar
                await Task.Delay(3000);

                // Verifica se está realmente rodando
                if (_apiProcess.HasExited)
                {
                    Console.WriteLine($"❌ API encerrou inesperadamente com código: {_apiProcess.ExitCode}");
                    _isRunning = false;
                    return false;
                }

                // Tenta fazer uma requisição de teste
                return await TestApiConnectionAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao iniciar API: {ex.Message}");
                _isRunning = false;
                return false;
            }
#endif
        }

        /// <summary>
        /// Testa se a API está respondendo
        /// </summary>
        private async Task<bool> TestApiConnectionAsync()
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await client.GetAsync($"{ApiUrl}/api/Pratos");
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ API está respondendo corretamente");
                    return true;
                }
                else
                {
                    Console.WriteLine($"⚠️ API respondeu com status: {response.StatusCode}");
                    return true; // Mesmo com erro, está rodando
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Não foi possível testar conexão com API: {ex.Message}");
                return true; // Assume que está rodando mesmo sem conseguir testar
            }
        }

        /// <summary>
        /// Para a API local
        /// </summary>
        public void StopApi()
        {
            if (_apiProcess != null && !_apiProcess.HasExited)
            {
                try
                {
                    Console.WriteLine("🛑 Parando API local...");
                    _apiProcess.Kill(true); // true = mata árvore de processos
                    _apiProcess.WaitForExit(5000);
                    Console.WriteLine("✅ API parada com sucesso");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Erro ao parar API: {ex.Message}");
                }
                finally
                {
                    _apiProcess?.Dispose();
                    _apiProcess = null;
                    _isRunning = false;
                }
            }
        }

        public void Dispose()
        {
            StopApi();
        }
    }
}
