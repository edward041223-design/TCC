using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using Cardapio_Inteligente.Servicos;
using System.Collections.ObjectModel;

namespace Cardapio_Inteligente.Paginas
{
    public partial class ChatPage : ContentPage
    {
        private readonly ApiService _apiService;
        private ObservableCollection<MensagemChat> _mensagens = new();
        private Label? _loadingLabel;

        public ChatPage()
        {
            InitializeComponent();
            
            // ✅ CORREÇÃO: Usa ApiService injetado ou cria uma nova instância
            _apiService = new ApiService();
            
            MessagesStack.BindingContext = _mensagens;
            
            // Mensagem de boas-vindas
            AdicionarMensagem("Assistente", "Olá! Sou o assistente do Cardápio Inteligente. Como posso ajudar você hoje?", false);
        }

        private async void OnSendClicked(object sender, EventArgs e)
        {
            var pergunta = PromptEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(pergunta))
                return;

            // Adiciona mensagem do usuário
            AdicionarMensagem("Você", pergunta, true);
            PromptEntry.Text = string.Empty;

            // ✅ CORREÇÃO: Mostra "Pensando..." com animação
            MostrarLoadingPensando();
            
            // Desabilita input enquanto processa
            SetInputEnabled(false);

            try
            {
                // ✅ CORREÇÃO: Usa ApiService.GerarRespostaIAAsync() ao invés de HttpClient direto
                var resposta = await _apiService.GerarRespostaIAAsync(pergunta);
                
                // Remove "Pensando..."
                RemoverLoading();
                
                // Adiciona resposta da IA
                AdicionarMensagem("Assistente", resposta, false);
            }
            catch (Exception ex)
            {
                RemoverLoading();
                AdicionarMensagem("Erro", $"Não foi possível conectar à IA: {ex.Message}", false);
            }
            finally
            {
                SetInputEnabled(true);
                PromptEntry.Focus();
            }
        }

        private void MostrarLoadingPensando()
        {
            _loadingLabel = new Label
            {
                Text = "💭 Pensando...",
                TextColor = Microsoft.Maui.Graphics.Colors.Gray,
                FontSize = 14,
                Margin = new Thickness(10, 5),
                FontAttributes = FontAttributes.Italic
            };

            MessagesStack.Children.Add(_loadingLabel);
            
            // Scroll para o final
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                await ChatScroll.ScrollToAsync(MessagesStack, ScrollToPosition.End, true);
            });

            // Animação de pulsação
            AnimarLoadingAsync();
        }

        private async void AnimarLoadingAsync()
        {
            if (_loadingLabel == null) return;

            try
            {
                while (_loadingLabel != null && MessagesStack.Children.Contains(_loadingLabel))
                {
                    await _loadingLabel.FadeTo(0.3, 500);
                    await _loadingLabel.FadeTo(1, 500);
                }
            }
            catch
            {
                // Ignora exceções se o label for removido durante animação
            }
        }

        private void RemoverLoading()
        {
            if (_loadingLabel != null && MessagesStack.Children.Contains(_loadingLabel))
            {
                MessagesStack.Children.Remove(_loadingLabel);
                _loadingLabel = null;
            }
        }

        private void AdicionarMensagem(string remetente, string texto, bool isUsuario)
        {
            var mensagem = new Label
            {
                Text = $"{remetente}: {texto}",
                TextColor = isUsuario ? Microsoft.Maui.Graphics.Colors.LightBlue : Microsoft.Maui.Graphics.Colors.White,
                FontSize = 14,
                Margin = new Thickness(10, 5),
                LineBreakMode = LineBreakMode.WordWrap
            };

            MessagesStack.Children.Add(mensagem);
            
            // Scroll para o final
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                await ChatScroll.ScrollToAsync(MessagesStack, ScrollToPosition.End, true);
            });
        }

        private void RemoverUltimaMensagem()
        {
            if (MessagesStack.Children.Count > 0)
                MessagesStack.Children.RemoveAt(MessagesStack.Children.Count - 1);
        }

        private void SetInputEnabled(bool enabled)
        {
            PromptEntry.IsEnabled = enabled;
            SendButton.IsEnabled = enabled;
        }
    }

    // Classe auxiliar para binding (opcional, caso queira usar CollectionView no futuro)
    public class MensagemChat
    {
        public string Remetente { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public bool IsUsuario { get; set; }
    }
}
