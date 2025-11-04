using Microsoft.Extensions.Logging;

namespace ProjetoSuporteTI;

public partial class App : Application
{
    public App()
    {
        try
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine(" App inicializado com sucesso");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($" Erro na inicialização do App: {ex}");
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🚀 Criando janela principal com AppShell...");
            
            var appShell = new AppShell();
            var window = new Window(appShell)
            {
                Title = "Suporte TI App"
            };
            
            System.Diagnostics.Debug.WriteLine("✅ Janela criada com AppShell - Login será primeira tela");
            return window;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erro ao criar janela: {ex}");
            
            // Fallback para AppShell mesmo assim
            return new Window(new AppShell());
        }
    }
}
