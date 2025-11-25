using System.Collections.ObjectModel;
using Cardapio_Inteligente.Modelos;
using Cardapio_Inteligente.Servicos;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cardapio_Inteligente.Paginas;

public partial class Tela_Cadastro : ContentPage
{
    private readonly ApiService _apiService;
    private List<CheckBox> checkPreferencias = new();
    private List<string> ingredientes = new();

    public Tela_Cadastro()
    {
        InitializeComponent();
        _apiService = new ApiService();
        _ = CarregarIngredientesAsync();
    }

    private async Task CarregarIngredientesAsync()
    {
        try
        {
            ingredientes = await _apiService.GetIngredientesAsync();

            stackPreferencias.Children.Clear();
            checkPreferencias.Clear();

            if (ingredientes == null || ingredientes.Count == 0)
            {
                var labelEmpty = new Label 
                { 
                    Text = "Nenhum ingrediente disponível no momento.", 
                    TextColor = Microsoft.Maui.Graphics.Colors.Gray,
                    Margin = new Thickness(10)
                };
                stackPreferencias.Children.Add(labelEmpty);
                return;
            }

            foreach (var ing in ingredientes)
            {
                var check = new CheckBox
                {
                    Color = Microsoft.Maui.Graphics.Color.FromArgb("#00BFFF"),
                    VerticalOptions = LayoutOptions.Center
                };
                checkPreferencias.Add(check);

                var label = new Label 
                { 
                    Text = ing, 
                    VerticalOptions = LayoutOptions.Center, 
                    TextColor = Microsoft.Maui.Graphics.Colors.White,
                    FontSize = 14,
                    Padding = new Thickness(5, 0, 0, 0)
                };

                // 🔹 Frame com toque para facilitar seleção em dispositivos touch
                var frame = new Frame
                {
                    BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#081B22"),
                    CornerRadius = 8,
                    Padding = new Thickness(10, 8),
                    Margin = new Thickness(0, 4),
                    HasShadow = false,
                    BorderColor = Microsoft.Maui.Graphics.Color.FromArgb("#1A3A4A"),
                    MinimumWidthRequest = 200
                };

                var horizontal = new HorizontalStackLayout 
                { 
                    Spacing = 10,
                    VerticalOptions = LayoutOptions.Center
                };
                horizontal.Children.Add(check);
                horizontal.Children.Add(label);

                frame.Content = horizontal;

                // 🔹 Adiciona gesture recognizer para clicar no frame e marcar/desmarcar o checkbox
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => 
                {
                    check.IsChecked = !check.IsChecked;
                };
                frame.GestureRecognizers.Add(tapGesture);
                label.GestureRecognizers.Add(tapGesture);

                stackPreferencias.Children.Add(frame);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível carregar os ingredientes: {ex.Message}", "OK");
        }
    }

    private void MudarEstadoUI(bool carregando)
    {
        activityIndicator.IsRunning = carregando;
        activityIndicator.IsVisible = carregando;
        btnSalvar.IsEnabled = !carregando;
        btnCancelar.IsEnabled = !carregando;
        txtNome.IsEnabled = !carregando;
        txtEmail.IsEnabled = !carregando;
        txtSenha.IsEnabled = !carregando;
        txtTelefone.IsEnabled = !carregando;
    }

    private async void btnSalvar_Clicked(object sender, EventArgs e)
    {
        // Validação dos campos obrigatórios
        if (string.IsNullOrWhiteSpace(txtNome.Text))
        {
            await DisplayAlert("Aviso", "Por favor, informe seu nome.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(txtEmail.Text))
        {
            await DisplayAlert("Aviso", "Por favor, informe seu e-mail.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(txtSenha.Text))
        {
            await DisplayAlert("Aviso", "Por favor, informe sua senha.", "OK");
            return;
        }

        // 🔹 Obtém ingredientes selecionados dos checkboxes
        var ingredientesSelecionados = new List<string>();
        for (int i = 0; i < checkPreferencias.Count; i++)
        {
            var check = checkPreferencias[i];
            if (check.IsChecked)
            {
                // Obtém o frame que contém o checkbox
                var frame = stackPreferencias.Children[i] as Frame;
                if (frame?.Content is HorizontalStackLayout horizontal)
                {
                    var label = horizontal.Children[1] as Label;
                    if (label != null && !string.IsNullOrWhiteSpace(label.Text))
                    {
                        ingredientesSelecionados.Add(label.Text);
                    }
                }
            }
        }

        var usuario = new Usuario
        {
            Nome = txtNome.Text?.Trim() ?? "",
            Email = txtEmail.Text?.Trim() ?? "",
            Senha = txtSenha.Text?.Trim() ?? "",
            Telefone = txtTelefone.Text?.Trim() ?? "",
            IngredientesNaoGosta = string.Join(", ", ingredientesSelecionados),
            Alergias = rbtLactose.IsChecked ? "Lactose" : "Nenhuma",
            DataCadastro = DateTime.UtcNow
        };

        try
        {
            MudarEstadoUI(true);
            await _apiService.CadastrarUsuarioAsync(usuario);
            await DisplayAlert("Sucesso", "Cadastro realizado! Você será redirecionado para o Login.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha no cadastro: {ex.Message}", "OK");
        }
        finally
        {
            MudarEstadoUI(false);
        }
    }

    private async void btnCancelar_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
