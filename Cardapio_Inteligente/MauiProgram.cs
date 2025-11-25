using Microsoft.Extensions.Logging;
using Cardapio_Inteligente.Servicos;

namespace Cardapio_Inteligente
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ✅ Registro do serviço HTTP (para pratos, usuários, IA, etc.)
            // Conecta sempre à API local rodando separadamente
            builder.Services.AddSingleton<ApiService>();

#if ANDROID && LLAMA_AVAILABLE
            // Se a build for Android e LLAMA_AVAILABLE estiver definida, registra a implementação real
            builder.Services.AddSingleton<ILlamaService, LlamaServiceAndroid>();
#else
            // Caso contrário, registra o stub local portátil
            builder.Services.AddSingleton<ILlamaService, LlamaServiceLocal>();
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // ✅ Mensagem de inicialização
            Console.WriteLine("✅ Aplicativo Cardápio Inteligente inicializado!");
            Console.WriteLine("🔗 Conectando à API local...");
            Console.WriteLine($"📱 Plataforma: {DeviceInfo.Platform}");

            return app;
        }
    }
}
