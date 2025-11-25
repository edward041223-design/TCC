using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Cardapio_Inteligente.Modelos;
using System.Net.Http.Headers;
using Microsoft.Maui.Storage;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Cardapio_Inteligente.Servicos
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string? _token;

        // ✅ URLs configuradas por plataforma (APENAS LOCAL)
        private string[] _baseAddresses;

        public ApiService()
        {
            _httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(60) // Aumentado para IA
            };

            // Se houver base salvo nas preferências, usa-o (permite persistir descoberta automática)
            var saved = string.Empty;
            try
            {
                saved = Preferences.Get("api_base_url", string.Empty);
            }
            catch { }

            if (!string.IsNullOrWhiteSpace(saved))
            {
                _baseAddresses = new[] { saved };
            }
            else
            {
                // Detecta plataforma e configura URLs apropriadas (APENAS LOCAL)
                _baseAddresses = GetBaseAddressesForPlatform();
            }

            // Carrega token salvo (se houver)
            try
            {
                var savedToken = Preferences.Get("jwt_token", string.Empty);
                if (!string.IsNullOrWhiteSpace(savedToken))
                    SetToken(savedToken);
            }
            catch
            {
                // Ignora falha em Preferences (alguns ambientes Android)
            }
        }

        // ✅ Detecta plataforma e retorna URLs apropriadas (SOMENTE LOCALHOST)
        private string[] GetBaseAddressesForPlatform()
        {
#if ANDROID
            // Se for emulador, prioriza10.0.2.2 (emulador Android padrão) e10.0.3.2 (Genymotion)
            if (IsAndroidEmulator())
            {
                return new[]
                {
                    "http://10.0.2.2:5068",
                    "http://10.0.3.2:5068"
                };
            }
            else
            {
                // Android físico: tentamos gateway-local discovery depois; começa sem endpoints explícitos
                return Array.Empty<string>();
            }
#elif WINDOWS
            // Windows: localhost direto (API iniciada automaticamente)
            return new[]
            {
                "http://localhost:5068",
                "https://localhost:7068"
            };
#elif IOS || MACCATALYST
            // iOS/Mac: localhost (API iniciada automaticamente no Mac)
            return new[]
            {
                "http://localhost:5068",
                "https://localhost:7068"
            };
#else
            // Fallback genérico
            return new[] { "http://localhost:5068" };
#endif
        }

        // Heurística simples para detectar emulador Android
        private bool IsAndroidEmulator()
        {
#if ANDROID
            try
            {
                var fingerprint = Android.OS.Build.Fingerprint ?? string.Empty;
                var model = Android.OS.Build.Model ?? string.Empty;
                var product = Android.OS.Build.Product ?? string.Empty;

                if (fingerprint.Contains("generic") || fingerprint.Contains("unknown") || model.Contains("Emulator") || model.Contains("sdk_gphone") || model.Contains("Android SDK built for x86") || product.Contains("sdk") || product.Contains("emulator"))
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
#else
            return false;
#endif
        }

        // ----------------------------------------------------------------------
        // 🔹 Lógica de fallback entre múltiplos endpoints
        // ----------------------------------------------------------------------
        private async Task<HttpResponseMessage> SendWithFallbackAsync(Func<Uri, Task<HttpResponseMessage>> action)
        {
            Exception? lastException = null;

            // Try configured endpoints first
            foreach (var baseAddr in _baseAddresses)
            {
                if (string.IsNullOrWhiteSpace(baseAddr))
                    continue;

                try
                {
                    var baseUri = new Uri(baseAddr.EndsWith("/") ? baseAddr : baseAddr + "/");

                    Console.WriteLine($"🔄 Tentando conectar em: {baseAddr}");

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                    var response = await action(baseUri);

                    Console.WriteLine($"✅ Conectado com sucesso em: {baseAddr}");
                    return response;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Console.WriteLine($"⚠️ Falha ao conectar em {baseAddr}: {ex.Message}");
                    continue;
                }
            }

            // Se chegou aqui, tenta descoberta automática na rede local (apenas se não houver endpoints explícitos ou se todos falharam)
            try
            {
                var discovered = await DiscoverApiOnLocalNetworkAsync(port:5068, timeoutMsPerHost:300);
                if (!string.IsNullOrEmpty(discovered))
                {
                    Console.WriteLine($"🔎 API descoberta automaticamente em: {discovered}");

                    // Salva para próximas execuções
                    try { Preferences.Set("api_base_url", discovered); } catch { }

                    var baseUri = new Uri(discovered.EndsWith("/") ? discovered : discovered + "/");
                    var response = await action(baseUri);
                    return response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro durante descoberta automática: {ex.Message}");
            }

            // Se chegou aqui, nenhum endpoint funcionou
            var errorMessage = $"❌ Não foi possível conectar à API local em nenhum endpoint configurado.\n\n" +
                             $"Endpoints testados:\n{string.Join("\n", _baseAddresses.Where(a => !string.IsNullOrWhiteSpace(a)))}\n\n" +
                             $"Último erro: {lastException?.Message}\n\n" +
                             $"Soluções:\n" +
                             $"1. Desktop: Certifique-se que a API foi iniciada automaticamente\n" +
                             $"2. Android no emulador: Use o IP10.0.2.2:5068 para acessar localhost do host\n" +
                             $"3. Android em dispositivo físico: O app tentou descobrir automaticamente o servidor na rede local\n" +
                             $"4. Verifique se o MySQL está rodando (banco: cardapio_db)\n" +
                             $"5. Verifique o firewall do Windows";

            throw new Exception(errorMessage);
        }

        // ----------------------------------------------------------------------
        // 🔹 Descoberta automática: faz scan do segmento de rede local para encontrar host com porta aberta
        // ----------------------------------------------------------------------
        private async Task<string?> DiscoverApiOnLocalNetworkAsync(int port =5068, int timeoutMsPerHost =300)
        {
            try
            {
                // Obtém endereços IPv4 locais
                var localIps = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a.Address))
                .Select(a => new { Address = a.Address, PrefixLength = a.PrefixLength })
                .ToList();

                foreach (var ipInfo in localIps)
                {
                    var ip = ipInfo.Address;
                    var prefix = ipInfo.PrefixLength;

                    // Calcula rede e faixa simples (suporta /24 de forma robusta; para prefixes maiores reduz a faixa)
                    var mask = PrefixLengthToSubnetMask(prefix);
                    var network = GetNetworkAddress(ip, mask);

                    var hosts = GetHostRange(network, mask);

                    // Limita concorrência
                    var sem = new SemaphoreSlim(40);
                    var tasks = new List<Task<string?>>();

                    foreach (var host in hosts)
                    {
                        await sem.WaitAsync();
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                using var client = new TcpClient();
                                var connectTask = client.ConnectAsync(host, port);
                                var delayTask = Task.Delay(timeoutMsPerHost);
                                var completed = await Task.WhenAny(connectTask, delayTask);
                                if (completed == connectTask && client.Connected)
                                {
                                    client.Close();
                                    return $"http://{host}:{port}";
                                }
                            }
                            catch { }
                            finally { sem.Release(); }
                            return null;
                        }));
                    }

                    var results = await Task.WhenAll(tasks);
                    var found = results.FirstOrDefault(r => !string.IsNullOrEmpty(r));
                    if (!string.IsNullOrEmpty(found))
                        return found;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em DiscoverApiOnLocalNetworkAsync: {ex.Message}");
                return null;
            }
        }

        // Helpers para cálculo de máscara e hosts
        private static IPAddress PrefixLengthToSubnetMask(int prefixLength)
        {
            uint mask = prefixLength ==0 ?0 :0xffffffff << (32 - prefixLength);
            var bytes = BitConverter.GetBytes(mask).Reverse().ToArray();
            return new IPAddress(bytes);
        }

        private static IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
        {
            var ipBytes = address.GetAddressBytes();
            var maskBytes = subnetMask.GetAddressBytes();
            var result = new byte[4];
            for (int i =0; i <4; i++) result[i] = (byte)(ipBytes[i] & maskBytes[i]);
            return new IPAddress(result);
        }

        private static IEnumerable<IPAddress> GetHostRange(IPAddress network, IPAddress mask)
        {
            var netBytes = network.GetAddressBytes();
            var maskBytes = mask.GetAddressBytes();

            uint networkInt = BitConverter.ToUInt32(netBytes.Reverse().ToArray(),0);
            uint maskInt = BitConverter.ToUInt32(maskBytes.Reverse().ToArray(),0);
            uint broadcast = (networkInt & maskInt) | (~maskInt);

            var first = (networkInt & maskInt) +1;
            var last = broadcast -1;

            // Limita para /24 para evitar scan massivo
            if (maskInt <0xffffff00)
            {
                first = (networkInt &0xffffff00) +1;
                last = (networkInt &0xffffff00) |0xff -1;
            }

            var hosts = new List<IPAddress>();
            for (uint i = first; i <= last; i++)
            {
                var bytes = BitConverter.GetBytes(i).Reverse().ToArray();
                hosts.Add(new IPAddress(bytes));
            }

            return hosts;
        }

        // ----------------------------------------------------------------------
        // Restante do código (Login, Cadastrar, GetPratos etc.) permanece inalterado
        // ----------------------------------------------------------------------
        public void SetToken(string token)
        {
            _token = token;
            if (!string.IsNullOrWhiteSpace(token)
            )
            {
                if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                try
                {
                    Preferences.Set("jwt_token", token);
                }
                catch { }
            }
            else
            {
                if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                    _httpClient.DefaultRequestHeaders.Remove("Authorization");

                try
                {
                    Preferences.Remove("jwt_token");
                }
                catch { }
            }
        }

        public void Logout()
        {
            SetToken(string.Empty);
        }

        public async Task<LoginResponse?> LoginAsync(string email, string senha)
        {
            var loginData = new { Email = email, Senha = senha };
            var response = await SendWithFallbackAsync(async baseUri =>
            {
                var url = new Uri(baseUri, "api/Usuarios/Login");
                return await _httpClient.PostAsJsonAsync(url, loginData);
            });

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"Falha no login: {response.StatusCode} - {err}");
            }

            var respostaJson = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(respostaJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                SetToken(loginResponse.Token);

            return loginResponse;
        }

        public async Task<bool> CadastrarUsuarioAsync(Usuario novoUsuario)
        {
            var response = await SendWithFallbackAsync(async baseUri =>
            {
                var url = new Uri(baseUri, "api/Usuarios/Cadastrar");
                return await _httpClient.PostAsJsonAsync(url, novoUsuario);
            });

            if (response.IsSuccessStatusCode)
                return true;

            var erro = await response.Content.ReadAsStringAsync();
            throw new Exception($"Falha ao cadastrar: {erro}");
        }

        public async Task<List<Prato>> GetPratosAsync(string? alergias = null, string? categoria = null)
        {
            var urlPath = "api/Pratos";
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(alergias)) queryParams.Add($"alergias={Uri.EscapeDataString(alergias)}");
            if (!string.IsNullOrEmpty(categoria)) queryParams.Add($"categoria={Uri.EscapeDataString(categoria)}");
            if (queryParams.Count >0) urlPath += "?" + string.Join("&", queryParams);

            var response = await SendWithFallbackAsync(async baseUri =>
            {
                var url = new Uri(baseUri, urlPath);
                return await _httpClient.GetAsync(url);
            });

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao buscar pratos: {response.StatusCode} - {err}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var pratos = JsonSerializer.Deserialize<List<Prato>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return pratos ?? new List<Prato>();
        }

        public async Task<List<string>> GetIngredientesAsync()
        {
            var response = await SendWithFallbackAsync(async baseUri =>
            {
                var url = new Uri(baseUri, "api/Ingredientes");
                return await _httpClient.GetAsync(url);
            });

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao buscar ingredientes: {response.StatusCode} - {err}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var ingredientes = JsonSerializer.Deserialize<List<string>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return ingredientes ?? new List<string>();
        }

        public async Task<string> GerarRespostaIAAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt não pode ser vazio.", nameof(prompt));

            var promptObject = new { Prompt = prompt };
            var content = new StringContent(JsonSerializer.Serialize(promptObject), Encoding.UTF8, "application/json");

            var response = await SendWithFallbackAsync(async baseUri =>
            {
                var url = new Uri(baseUri, "api/Pratos/assistente-chat");
                Console.WriteLine($"📤 Enviando pergunta para IA: {prompt}");
                return await _httpClient.PostAsync(url, content);
            });

            var respostaTexto = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro na API (Status {response.StatusCode}): {respostaTexto}");

            try
            {
                using var doc = JsonDocument.Parse(respostaTexto);
                if (doc.RootElement.TryGetProperty("mensagem", out var mens))
                {
                    var mensagem = mens.GetString() ?? string.Empty;
                    Console.WriteLine($"📥 Resposta da IA: {mensagem}");
                    return mensagem;
                }
                var trimmed = respostaTexto.Trim().Trim('"');
                return trimmed;
            }
            catch
            {
                return respostaTexto.Trim().Trim('"');
            }
        }
    }
}
