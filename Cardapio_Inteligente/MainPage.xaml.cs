using Cardapio_Inteligente.Paginas;
using Microsoft.Maui.Controls;

namespace Cardapio_Inteligente
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void btn_VerCardapio_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new PaginaInicial());
        }

        private async void btn_JaSouCliente_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Tela_Login());
        }
    }
}
