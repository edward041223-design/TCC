#if ANDROID
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Microsoft.Maui.Storage;

namespace Cardapio_Inteligente.Servicos
{
 /// <summary>
 /// Helper Android-only que instala o arquivo de modelo (*.gguf) no AppDataDirectory/ModelosIA.
 /// Estratégia:
 ///1) Tenta copiar de Assets/ModelosIA/{fileName} se presente (fallback para side-load/testes).
 ///2) Se não houver asset, tenta baixar de uma URL (suporta resume e verifica checksum opcionais).
 ///3) Respeita opção de "WiFi only"; faz verificação simples do tipo de conexão.
 /// </summary>
 public static class LlamaModelInstaller
 {
 // Default download URL (substitua pela URL real do seu servidor/CDN)
 public const string DefaultModelDownloadUrl = "https://example.com/models/Phi-3-mini-4k-instruct-q4.gguf";

 /// <summary>
 /// Garante que o modelo exista em AppDataDirectory/ModelosIA. Tenta copiar de Assets/ModelosIA e, se não existir, baixa de downloadUrl.
 /// </summary>
 /// <param name="fileName">Nome do arquivo do modelo (ex: Phi-3-mini-4k-instruct-q4.gguf)</param>
 /// <param name="downloadUrl">URL para baixar o modelo se não estiver nos assets. Se nulo, não fará download.</param>
 /// <param name="expectedSha256">Checksum SHA256 hex para validar o arquivo após download (opcional).</param>
 /// <param name="wifiOnly">Se true, só baixa quando estiver conectado via Wi-Fi.</param>
 public static async Task<bool> EnsureModelInstalledAsync(string fileName, string? downloadUrl = DefaultModelDownloadUrl, string? expectedSha256 = null, bool wifiOnly = true)
 {
 try
 {
 var destDir = Path.Combine(FileSystem.Current.AppDataDirectory, "ModelosIA");
 var destPath = Path.Combine(destDir, fileName);

 if (File.Exists(destPath))
 return true;

 Directory.CreateDirectory(destDir);

 //1) Tenta copiar dos assets (Platforms/Android/Assets/ModelosIA/{fileName})
 var assetPath = Path.Combine("ModelosIA", fileName);
 try
 {
 using var assetStream = Android.App.Application.Context.Assets.Open(assetPath);
 if (assetStream != null)
 {
 using var outStream = File.Create(destPath);
 await assetStream.CopyToAsync(outStream);
 Console.WriteLine($"? Modelo copiado de assets para: {destPath}");

 if (!string.IsNullOrEmpty(expectedSha256))
 {
 var ok = await VerifyFileSha256Async(destPath, expectedSha256);
 if (!ok)
 {
 Console.WriteLine("Checksum inválido após copiar dos assets.");
 File.Delete(destPath);
 return false;
 }
 }

 return true;
 }
 }
 catch (Java.IO.FileNotFoundException)
 {
 Console.WriteLine("Asset não encontrado nos assets: {0}", assetPath);
 }
 catch (Exception ex)
 {
 Console.WriteLine($"Erro ao tentar copiar asset do modelo: {ex.Message}");
 }

 //2) Se downloadUrl não fornecido, falha
 if (string.IsNullOrEmpty(downloadUrl))
 {
 Console.WriteLine("Nenhuma URL de download fornecida e asset não encontrado. Abortando instalação do modelo.");
 return false;
 }

 //3) Checa conexão Wi-Fi
 if (wifiOnly && !IsOnWifi())
 {
 Console.WriteLine("Download do modelo requer Wi-Fi e o dispositivo não está conectado via Wi-Fi.");
 return false;
 }

 //4) Baixa com resume suportado
 var tempPath = destPath + ".partial";
 long existingLength =0;
 if (File.Exists(tempPath))
 {
 var fi = new FileInfo(tempPath);
 existingLength = fi.Length;
 }

 using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };

 var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
 if (existingLength >0)
 {
 request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existingLength, null);
 Console.WriteLine($"Resuming download from byte {existingLength}");
 }

 using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
 if (!response.IsSuccessStatusCode)
 {
 Console.WriteLine($"Falha ao iniciar download: {response.StatusCode}");
 return false;
 }

 long totalLength = existingLength;
 if (response.Content.Headers.ContentRange != null && response.Content.Headers.ContentRange.Length.HasValue)
 totalLength = existingLength + response.Content.Headers.ContentRange.Length.Value;
 else if (response.Content.Headers.ContentLength.HasValue)
 totalLength = existingLength + response.Content.Headers.ContentLength.Value;

 using var contentStream = await response.Content.ReadAsStreamAsync();
 // Abre o arquivo para escrita no final
 using var fileStream = new FileStream(tempPath, FileMode.Append, FileAccess.Write, FileShare.None,81920, useAsync: true);

 var buffer = new byte[81920];
 int bytesRead;
 long totalRead = existingLength;
 var lastLogged = DateTime.UtcNow;

 while ((bytesRead = await contentStream.ReadAsync(buffer,0, buffer.Length)) >0)
 {
 await fileStream.WriteAsync(buffer,0, bytesRead);
 totalRead += bytesRead;

 // Log progress a cada meio segundo
 if ((DateTime.UtcNow - lastLogged).TotalMilliseconds >500)
 {
 if (totalLength >0)
 {
 var percent = (double)totalRead / totalLength *100.0;
 Console.WriteLine($"?? Download do modelo: {Math.Round(percent,2)}% ({totalRead}/{totalLength} bytes)");
 }
 else
 {
 Console.WriteLine($"?? Download do modelo: {totalRead} bytes lidos...");
 }
 lastLogged = DateTime.UtcNow;
 }
 }

 // Move temp para destino final
 if (File.Exists(destPath))
 File.Delete(destPath);

 File.Move(tempPath, destPath);
 Console.WriteLine($"? Download concluído e movido para: {destPath}");

 //5) Verifica checksum se fornecido
 if (!string.IsNullOrEmpty(expectedSha256))
 {
 var ok = await VerifyFileSha256Async(destPath, expectedSha256);
 if (!ok)
 {
 Console.WriteLine("Checksum inválido após download.");
 File.Delete(destPath);
 return false;
 }
 }

 return true;
 }
 catch (Exception ex)
 {
 Console.WriteLine($"Erro durante EnsureModelInstalledAsync: {ex.Message}");
 return false;
 }
 }

 static bool IsOnWifi()
 {
 try
 {
 var cm = (ConnectivityManager)Android.App.Application.Context.GetSystemService(Context.ConnectivityService);
 if (cm == null)
 return false;

 if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
 {
 var network = cm.ActiveNetwork;
 if (network == null) return false;
 var caps = cm.GetNetworkCapabilities(network);
 if (caps == null) return false;
 return caps.HasTransport(TransportType.Wifi) || caps.HasTransport(TransportType.Ethernet);
 }
 else
 {
 var info = cm.ActiveNetworkInfo;
 return info != null && info.IsConnected && info.Type == ConnectivityType.Wifi;
 }
 }
 catch (Exception ex)
 {
 Console.WriteLine($"Erro ao verificar Wi-Fi: {ex.Message}");
 return false;
 }
 }

 static async Task<bool> VerifyFileSha256Async(string path, string expectedHex)
 {
 try
 {
 using var stream = File.OpenRead(path);
 using var sha = SHA256.Create();
 var hash = await sha.ComputeHashAsync(stream);
 var hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
 return string.Equals(hex, expectedHex.Replace(" ", "").ToLowerInvariant(), StringComparison.Ordinal);
 }
 catch (Exception ex)
 {
 Console.WriteLine($"Erro ao verificar SHA256: {ex.Message}");
 return false;
 }
 }
 }
}
#endif