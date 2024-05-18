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
using UVemyCliente.DTO;

namespace UVemyCliente.Vistas
{
    /// <summary>
    /// Lógica de interacción para CodigoVerificacionPagina.xaml
    /// </summary>
    public partial class CodigoVerificacionPagina : Page
    {
        private UsuarioDTO _usuario;

        public CodigoVerificacionPagina(UsuarioDTO usuario)
        {
            InitializeComponent();
            _usuario = usuario;
        }

        private void ClicConfirmar(object sender, RoutedEventArgs e)
        {

        }

        private void VerificarEntradaNumérica(object sender, TextCompositionEventArgs e)
        {
            if (!EsNumerico(e.Text) || e.Text.Contains(" "))
            {
                e.Handled = true;
            }
        }

        private static bool EsNumerico(string texto)
        {
            return int.TryParse(texto, out _);
        }
    }
}
