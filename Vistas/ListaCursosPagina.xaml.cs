using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
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

        private int _paginaActual = 0;
        private int _paginaAnterior;
        private string _tituloCurso = string.Empty;
        private int _calificacionCurso;
        private int _idEtiqueta;
        private int _idTipoCurso;
        public ListaCursosPagina()
        {
            _paginaActual = 0;
            _calificacionCurso = 0;
            _idEtiqueta = 0;
            _idTipoCurso = 0;
            InitializeComponent();
            _checkBoxes = new List<CheckBox> { chckBox1, chckBox2, chckBox3, chckBox4 };
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

                _ = CargarCursosPagina(_paginaActual, string.Empty, _calificacionCurso, _idTipoCurso, _idEtiqueta);
            }
            else
            {
                ErrorMensaje errorMensaje = new("Error. No se pudo conectar con el servidor. Inténtelo de nuevo o hágalo más tarde.");
                errorMensaje.Show();
            }
        }

        private async Task CargarCursosPagina(int pagina, string titulo, int calificacion, int idTipoCurso, int idEtiqueta)
        {
            string urlBusqueda = "cursoslistas/" + pagina;
            if (titulo != string.Empty)
            {
                urlBusqueda += $"?titulo={HttpUtility.UrlEncode(titulo)}";
            }
            else if (calificacion != 0)
            {
                urlBusqueda += $"?calificacion={HttpUtility.UrlEncode(calificacion.ToString())}";
            }
            else if (idTipoCurso != 0)
            {
                urlBusqueda += $"?tipoCursos={HttpUtility.UrlEncode(idTipoCurso.ToString())}";
            }
            else if (idEtiqueta != 0)
            {
                urlBusqueda += $"?etiqueta={HttpUtility.UrlEncode(idEtiqueta.ToString())}";
            }

            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, urlBusqueda);

            Debug.WriteLine(respuestaHttp);
            if (respuestaHttp.IsSuccessStatusCode)
            {
                var json = await respuestaHttp.Content.ReadAsStringAsync();
                List<CursoListBox> cursos = JsonConvert.DeserializeObject<List<CursoListBox>>(json);
                CargarCursosListBox(cursos);
            }
            else if (respuestaHttp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                ErrorMensaje errorMensaje = new("Error. No existen mas cursos en el sistema.");
                errorMensaje.Show();
                _paginaActual = _paginaAnterior;
            }
            else
            {
                ErrorMensaje errorMensaje = new("Error. No se pudo conectar con el servidor. Inténtelo de nuevo o hágalo más tarde.");
                errorMensaje.Show();
            }
            txtBlockPagina.Text = "Pagina "+_paginaActual;
        }

        private void CargarCursosListBox(List<CursoListBox> cursos)
        {
            ObservableCollection<CursoListBox> Cursos1 = new ObservableCollection<CursoListBox>();
            ObservableCollection<CursoListBox> Cursos2 = new ObservableCollection<CursoListBox>();
            ObservableCollection<CursoListBox> Cursos3 = new ObservableCollection<CursoListBox>();
            int cursosPorFilas = 3;
            Cursos1.Clear();
            Cursos2.Clear();
            Cursos3.Clear();

            for (int i = 0; i < cursos.Count; i++)
            {
                if (i % cursosPorFilas == 0)
                {
                    Cursos1.Add(cursos[i]);
                }
                else if (i % cursosPorFilas == 1)
                {
                    Cursos2.Add(cursos[i]);
                }
                else if (i % cursosPorFilas == 2)
                {
                    Cursos3.Add(cursos[i]);
                }
            }

            lstBoxCreditos1.ItemsSource = Cursos1;
            lstBoxCreditos2.ItemsSource = Cursos2;
            lstBoxCreditos3.ItemsSource = Cursos3;
        }



        private void ClicRegresar(object sender, RoutedEventArgs e)
        {

        }

        private void ClicBuscarCurso(object sender, RoutedEventArgs e)
        {
            _tituloCurso = string.Empty;
            if(txtBoxBarraBuscar.Text == string.Empty)
            {
                LimpiarCampos();
            }
            _tituloCurso = txtBoxBarraBuscar.Text;
            _paginaActual = 0;
            _calificacionCurso = 0;
            _idTipoCurso = 0;
            _idEtiqueta = 0;
            _ = CargarCursosPagina(_paginaActual, _tituloCurso, _calificacionCurso, _idTipoCurso, _idEtiqueta);

        }

        private void LimpiarCampos()
        {
            _tituloCurso = string.Empty;
            cmbBoxEtiquetaCurso.SelectedIndex = -1;
            cmbBoxTipoCurso.SelectedIndex = -1;
            chckBox1.IsChecked = false;
            chckBox2.IsChecked = false;
            chckBox3.IsChecked = false;
            chckBox4.IsChecked = false;
        }

        private void SeleccionarCurso(object sender, RoutedEventArgs e)
        {
            ListBoxItem listBoxItem = sender as ListBoxItem;
            if (listBoxItem != null)
            {
                CursoListBox cursoListBox = listBoxItem.DataContext as CursoListBox;
                if (cursoListBox != null)
                {
                    Debug.WriteLine(cursoListBox.IdCurso);
                    Debug.WriteLine(cursoListBox.Titulo);
                    Debug.WriteLine(cursoListBox.Documento[0].IdDocumento);
                    CursoDTO curso = new CursoDTO 
                    {
                        IdCurso = cursoListBox.IdCurso,
                        Titulo = cursoListBox.Titulo,
                        idDocumento = cursoListBox.Documento[0].IdDocumento,
                        Archivo = cursoListBox.Documento[0].Archivo.Data
                    };
                    DetallesCurso detalles = new DetallesCurso(curso);
                    this.NavigationService.Navigate(detalles);
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        { 
            _calificacionCurso = 0;
            _tituloCurso = string.Empty;
            txtBoxBarraBuscar.Text = string.Empty;
            _idTipoCurso = 0;
            _idEtiqueta = 0;
            CheckBox seleccionado = sender as CheckBox;
            if (seleccionado != null)
            {
                try
                {
                    _calificacionCurso = int.Parse((string)seleccionado.Tag);
                    foreach (var checkBox in _checkBoxes)
                    {
                    
                        if (checkBox != seleccionado && _calificacionCurso!=0)
                        {
                            checkBox.IsChecked = false;
                        }
                    }
                    CargarCursosPagina(_paginaActual,string.Empty, _calificacionCurso, _idTipoCurso, _idEtiqueta);

                }
                catch (FormatException ex)
                {
                    _calificacionCurso = 0;
                    ErrorMensaje errorMensaje = new("Error. No se ha podido obtener la calificacion a buscar.");
                    errorMensaje.Show();
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

        private void ClicAnteriorPagina(object sender, RoutedEventArgs e)
        {
            if (_paginaActual <= 0)
            {
                _paginaActual = 0;
            }
            else
            {
                _paginaActual = _paginaActual - 1;
                _ = CargarCursosPagina(_paginaActual, _tituloCurso, _calificacionCurso, _idTipoCurso, _idEtiqueta);
            }
        }

        private void ClicSiguientePagina(object sender, RoutedEventArgs e)
        {
            _paginaAnterior = _paginaActual;
            _paginaActual = _paginaActual + 1;
            _ = CargarCursosPagina(_paginaActual, _tituloCurso, _calificacionCurso, _idTipoCurso, _idEtiqueta);
        }

        private void SeleccionarTipoCurso(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox.SelectedItem != null)
            {
                TiposCursos seleccionado = comboBox.SelectedItem as TiposCursos;
                _idTipoCurso = seleccionado.IdTipoCurso;
                cmbBoxEtiquetaCurso.SelectedIndex = -1;
                chckBox1.IsChecked = false;
                chckBox2.IsChecked = false;
                chckBox3.IsChecked = false;
                chckBox4.IsChecked = false;
                _tituloCurso = string.Empty;
                _paginaActual = 0;
                _calificacionCurso = 0;
                _idEtiqueta = 0;
                _ = CargarCursosPagina(_paginaActual, _tituloCurso, _calificacionCurso, _idTipoCurso, _idEtiqueta);
            } 
        }

        private void SeleccionarEtiquetas(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox.SelectedItem != null)
            {
                EtiquetaDTO seleccionado = comboBox.SelectedItem as EtiquetaDTO;
                _idEtiqueta = seleccionado.IdEtiqueta;
                cmbBoxTipoCurso.SelectedIndex = -1;
                chckBox1.IsChecked = false;
                chckBox2.IsChecked = false;
                chckBox3.IsChecked = false;
                chckBox4.IsChecked = false;
                _tituloCurso = string.Empty;
                _paginaActual = 0;
                _calificacionCurso = 0;
                _idTipoCurso = 0;
                _ = CargarCursosPagina(_paginaActual, _tituloCurso, _calificacionCurso, _idTipoCurso, _idEtiqueta);
            }
        }


        public class CursoListBox
        {
            [JsonProperty("idCurso")]
            public int IdCurso { get; set; }
            [JsonProperty("titulo")]
            public string Titulo { get; set; }
            [JsonProperty("documentos")]
            public List<DocumentoListBox> Documento { get; set; }

            public BitmapImage Archivo
            {
                get
                {
                    if (Documento != null && Documento.Count > 0)
                    {
                        byte[] imageData = Documento[0].Archivo.Data;
                        BitmapImage bitmap = new BitmapImage();
                        using (MemoryStream stream = new MemoryStream(imageData))
                        {
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                        }
                        return bitmap;
                    }
                    return null;
                }
            }
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
    }
}
