namespace Cardapio_Inteligente.Api.Configuracao
{
    public class LlamaSettings
    {
        // Caminho relativo do modelo .gguf
        public string ModelPath { get; set; } = string.Empty;

        // Máximo de tokens gerados pela IA (não é o contexto total)
        public int MaxTokens { get; set; } = 512;

        // Temperatura (0.7–1.2): define a criatividade da resposta
        public double Temperature { get; set; } = 0.8;

        // TopP (0.85–0.95): controla diversidade sem perder coerência
        public double TopP { get; set; } = 0.9;

        // Quantas camadas do modelo usar na GPU (0 = CPU)
        public int GpuLayerCount { get; set; } = 0;

        // Quantidade de threads usadas na CPU
        public int NumThreads { get; set; } = 4;

        // 🔹 Tamanho máximo do contexto do modelo (crucial para o LLamaService)
        public int ContextSize { get; set; } = 4096;
    }
}