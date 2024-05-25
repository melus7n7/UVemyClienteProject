using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
using UVemyCliente.Conexion;
using UVemyCliente.DTO;
using UVemyCliente.Utilidades;

namespace UVemyCliente.Vistas
{
    /// <summary>
    /// Lógica de interacción para ListaCursosPagina.xaml
    /// </summary>
    public partial class ListaCursosPagina : Page
    {
        private List<CheckBox> _checkBoxes;
        private ObservableCollection<EtiquetaDTO> _etiquetas = new ObservableCollection<EtiquetaDTO>();
        private ObservableCollection<TiposCursos> _tiposCursos = new ObservableCollection<TiposCursos>();
        private List<CursoLista> _listaCursos;
        private int pagina = 0;
        public ListaCursosPagina()
        {
            pagina = 0;
            InitializeComponent();
            _checkBoxes = new List<CheckBox> { checkBox1, checkBox2, checkBox3, checkBox4 };
            _tiposCursos = new ObservableCollection<TiposCursos>
            {
                new TiposCursos
                {
                    IdTipoCurso = 1,
                    Nombre = "Cursos creados"
                },
                new TiposCursos
                {
                    IdTipoCurso = 2,
                    Nombre = "Cursos inscritos"
                },
            };
            CargarComboboxTipoCurso();
            _ = CargarEtiquetasAsync();
        }

        private void CargarComboboxTipoCurso()
        {
            cmbBoxTipoCurso.Items.Clear();
            cmbBoxTipoCurso.ItemsSource = _tiposCursos;
        }

        private async Task CargarEtiquetasAsync()
        {
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, "etiquetas");
            if (respuestaHttp.IsSuccessStatusCode)
            {
                var json = await respuestaHttp.Content.ReadAsStringAsync();
                _etiquetas = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<EtiquetaDTO>>(json) ?? [];
                cmbBoxEtiquetaCurso.ItemsSource = _etiquetas;
                Debug.WriteLine(json);
                _ = CargarCursosPagina(pagina);
            }
            else
            {
                ErrorMensaje errorMensaje = new("Error. No se pudo conectar con el servidor. Inténtelo de nuevo o hágalo más tarde.");
                errorMensaje.Show();
            }
        }

        private async Task CargarCursosPagina(int pagina)
        {
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, "cursoslistas/"+pagina);
            Debug.WriteLine(respuestaHttp);
            if (respuestaHttp.IsSuccessStatusCode)
            {
                var json = await respuestaHttp.Content.ReadAsStringAsync();
                Debug.WriteLine(json);
                List<CursoListBox> cursos = JsonConvert.DeserializeObject<List<CursoListBox>>(json);
                Debug.WriteLine(cursos);
                Debug.WriteLine(cursos.Count);
                int registrosPorFila = 3;

                for (int f = 0; f < (cursos.Count / registrosPorFila) - 1; f++)
                {
                    for (int i = 0; i < registrosPorFila; i++)
                    {
                        _listaCursos = new List<CursoLista>()
                        {
                            new CursoLista()
                            {

                            }
                        };
                    }
                }

                lstBoxCreditos.ItemsSource = _listaCursos;
            }
            else
            {
                ErrorMensaje errorMensaje = new("Error. No se pudo conectar con el servidor. Inténtelo de nuevo o hágalo más tarde.");
                errorMensaje.Show();
            }
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

        public class CursoListBox
        {
            [JsonProperty("idCurso")]
            public int IdCurso { get; set; }
            [JsonProperty("titulo")]
            public string Titulo { get; set; }
            [JsonProperty("documentos")]
            public List<DocumentoListBox> Documento { get; set; }
        }

        public class DocumentoListBox
        {
            [JsonProperty("idDocumento")]
            public int IdDocumento { get; set; }
            [JsonProperty("archivo")]
            public ArchivoListBox Archivo { get; set; }
        }

        public class ArchivoListBox
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("data")]
            public byte[] Data { get; set; }
        }

        public class CursoLista
        {
            public int idCurso { get; set; }
            public byte[] Archivo { get; set; }
            public string Titulo { get; set; }
            public int idCurso2 { get; set; }
            public byte[] Archivo2 { get; set; }
            public string Titulo2 { get; set; }
            public int idCurso3 { get; set; }
            public byte[] Archivo3 { get; set; }
            public string Titulo3 { get; set; }

        }
    }
}
