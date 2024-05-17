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
    /// Lógica de interacción para ComentarioPrincipalUserControl.xaml
    /// </summary>
    public partial class ComentarioPrincipalUserControl : UserControl
    {
        public ComentarioPrincipalUserControl()
        {
            InitializeComponent();
        }

        private void ClicMostrarComentario(object sender, RoutedEventArgs e)
        {
            brdComentarioNuevo.Visibility = Visibility.Visible;
            btnResponder.Visibility = Visibility.Collapsed;
        }

        private void ClicEnviarComentario(object sender, RoutedEventArgs e)
        {
            brdComentarioNuevo.Visibility = Visibility.Collapsed;
            btnResponder.Visibility = Visibility.Visible;
        }
    }
}
