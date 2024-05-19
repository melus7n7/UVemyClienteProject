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

namespace UVemyCliente.Vistas
{
    /// <summary>
    /// Lógica de interacción para DetallesCursoPagina.xaml
    /// </summary>
    public partial class DetallesCurso : Page
    {
        public DetallesCurso()
        {
            InitializeComponent();
        }

        private void ClicAgregarClase(object sender, RoutedEventArgs e)
        {
            FormularioClase formulario = new FormularioClase();
            NavigationService.Navigate(formulario);
        }

        private void ClicVerClase(object sender, RoutedEventArgs e)
        {
            DetallesClase detalles = new DetallesClase();
            NavigationService.Navigate(detalles);
        }

        private void ClicVerEstadisticasCurso(object sender, RoutedEventArgs e)
        {
            EstadisticasCurso estadisticas = new EstadisticasCurso(1);
            NavigationService.Navigate(estadisticas);
        }
    }
}
