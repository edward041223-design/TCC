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
                
                // Format the error message to handle line breaks properly
                var errorMessage = $"Não foi possível conectar à IA: {ex.Message}";
                
                // Use helper method to format the error message with proper line breaks
                errorMessage = FormatErrorMessage(errorMessage);
                
                AdicionarMensagem("Erro", errorMessage, false);
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
                LineBreakMode = LineBreakMode.WordWrap,
                MaxLines = 10, // Limit number of lines to prevent UI issues
                BackgroundColor = isUsuario ? Microsoft.Maui.Graphics.Colors.DarkBlue : Microsoft.Maui.Graphics.Colors.DarkSlateGray,
                Padding = new Thickness(10, 5),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
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

        // Helper method to format error messages with proper line breaks for MAUI display
        private string FormatErrorMessage(string errorMessage)
        {
            // Replace literal \n (which appears as \\n in the exception message) with actual newlines
            return errorMessage.Replace("\\n", "\n").Replace("\\n\\n", "\n\n");
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
