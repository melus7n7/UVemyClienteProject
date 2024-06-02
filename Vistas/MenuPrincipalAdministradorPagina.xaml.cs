using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UVemyCliente.Utilidades;

namespace UVemyCliente.Vistas
{
    /// <summary>
    /// Interaction logic for MenuPrincipalAdministradorPagina.xaml
    /// </summary>
    public partial class MenuPrincipalAdministradorPagina : Page
    {
        public MenuPrincipalAdministradorPagina()
        {
            InitializeComponent();
        }

        private void CargarPagina(object sender, RoutedEventArgs e)
        {
            txtBlockNombre.Text = "Bienvenido " + SingletonUsuario.Nombres + " " + SingletonUsuario.Apellidos;
        }

        private void ClicConsultarEtiquetas(object sender, RoutedEventArgs e)
        {
            EtiquetasExistentesPagina etiquetasExistentesPagina = new();
            this.NavigationService.Navigate(etiquetasExistentesPagina);
        }

        private void ClicConsultarUsuarios(object sender, RoutedEventArgs e)
        {

        }

        private void ClicSalirInicioSesion(object sender, RoutedEventArgs e)
        {
            SingletonUsuario.Limpiar();
            NavigationService.Navigate(new InicioSesionPagina());
        }
    }
}
