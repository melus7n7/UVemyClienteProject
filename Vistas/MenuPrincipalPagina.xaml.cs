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
    /// Lógica de interacción para MenuPrincipalPagina.xaml
    /// </summary>
    public partial class MenuPrincipalPagina : Page
    {
        public MenuPrincipalPagina()
        {
            InitializeComponent();
        }

        private void CargarPagina(object sender, RoutedEventArgs e)
        {
            txtBlockNombre.Text = "Bienvenido " + SingletonUsuario.Nombres + " " + SingletonUsuario.Apellidos;
        }

        private void ClicSalirInicioSesion(object sender, RoutedEventArgs e)
        {
            SingletonUsuario.Limpiar();
            NavigationService.Navigate(new InicioSesionPagina());
        }

        private void ClicConsultarPerfil(object sender, RoutedEventArgs e)
        {
            FormularioUsuarioPagina formularioUsuarioPagina = new();
            formularioUsuarioPagina.CargarPaginaConsultaPerfil();
            NavigationService.Navigate(formularioUsuarioPagina);
        }

        private void ClicBuscar(object sender, RoutedEventArgs e)
        {

        }

        private void ClicCursos(object sender, RoutedEventArgs e)
        {
            ListaCursosPagina listaCursosPagina = new();
            NavigationService.Navigate(listaCursosPagina);
        }
    }
}
