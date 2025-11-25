namespace Cardapio_Inteligente
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Abrir MainPage dentro de NavigationPage para permitir PushAsync
            MainPage = new NavigationPage(new MainPage());
        }
    }
}
