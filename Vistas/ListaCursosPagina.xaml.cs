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
    /// Lógica de interacción para ListaCursosPagina.xaml
    /// </summary>
    public partial class ListaCursosPagina : Page
    {
        private List<CheckBox> _checkBoxes;
        public ListaCursosPagina()
        {
            InitializeComponent();
            _checkBoxes = new List<CheckBox> { checkBox1, checkBox2, checkBox3, checkBox4 };
        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {

        }

        private void ClicBuscarCurso(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox seleccionado = sender as CheckBox;
            if (seleccionado != null)
            {
                foreach (var checkBox in _checkBoxes)
                {
                    if (checkBox != seleccionado)
                    {
                        checkBox.IsChecked = false;
                    }
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void ClicCrearCurso(object sender, RoutedEventArgs e)
        {
            FormularioCursoPagina curso = new FormularioCursoPagina();
            this.NavigationService.Navigate(curso);
        }
    }
}
