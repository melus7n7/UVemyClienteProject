using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using UVemyCliente.Conexion;
using UVemyCliente.DTO;
using UVemyCliente.Utilidades;

namespace UVemyCliente.Vistas
{
    /// <summary>
    /// Lógica de interacción para FormularioUsuarioPagina.xaml
    /// </summary>
    public partial class FormularioUsuarioPagina : Page
    {
        private static readonly int BYTES_POR_KILOBYTE = 1024;
        private static readonly int KILOBYTES_POR_MEGABYTE = 1024;

        private string _nombres = string.Empty;
        private string _apellidos = string.Empty;
        private string _correoElectronico = string.Empty;
        private string _contrasena = string.Empty;
        private string _confirmarContrasena = string.Empty;
        private byte[] _imagenPerfil = [];

        public FormularioUsuarioPagina()
        {
            InitializeComponent();
        }

        private void ClicRegistrar(object sender, RoutedEventArgs e)
        {
            if(ValidarCampos())
            {
                ContinuarRegistro();
            }
        }

        private bool ValidarCampos()
        {
            _nombres = txtBoxNombres.Text.Trim();
            _apellidos = txtBoxApellidos.Text.Trim();
            _correoElectronico = txtBoxCorreoElectronico.Text.Trim();
            _contrasena = pwdBoxContrasena.Password.Trim();
            _confirmarContrasena = pwdBoxContrasenaRepetida.Password.Trim();

            bool sonValidos = true;
            string mensajeError = string.Empty;

            if (string.IsNullOrEmpty(_nombres))
            {
                mensajeError = "Nombre/s requerido";
                sonValidos = false;
            }

            if(string.IsNullOrEmpty(_apellidos))
            {
                mensajeError += string.IsNullOrEmpty(mensajeError) ? "Apellido/s requerido" : ", apellido/s requerido";
                sonValidos = false;
            }

            if(string.IsNullOrEmpty(_correoElectronico))
            {
                mensajeError += string.IsNullOrEmpty(mensajeError) ? "Correo electrónico requerido" : ", correo electrónico requerido";
                sonValidos = false;
            }
            else if(!CredencialesValidador.EsCorreoValido(_correoElectronico))
            {
                mensajeError += string.IsNullOrEmpty(mensajeError) ? "Correo electrónico inválido" : ", correo electrónico inválido";
                sonValidos = false;
            }

            if(string.IsNullOrEmpty(_contrasena) || string.IsNullOrEmpty(_confirmarContrasena))
            {
                mensajeError += string.IsNullOrEmpty(mensajeError) ? "Contraseñas requeridas" : ", contraseñas requeridas";
                sonValidos = false;
            }
            else if(_contrasena != _confirmarContrasena)
            {
                mensajeError += string.IsNullOrEmpty(mensajeError) ? "Las contraseñas no coinciden" : ", las contraseñas no coinciden";
                sonValidos = false;
            }
            else if(!CredencialesValidador.EsContraseñaSegura(_contrasena))
            {
                mensajeError += string.IsNullOrEmpty(mensajeError) ? "Contraseña inválida (debe contener al menos una minúscula, una mayúscula y un número)" : ", contraseña inválida (debe contener al menos una minúscula, una mayúscula y un número)";
                sonValidos = false;
            }

            if (!sonValidos)
            {
                ErrorMensaje errorMensaje = new(mensajeError);
                errorMensaje.ShowDialog();
            }

            return sonValidos;
        }

        private void ContinuarRegistro()
        {
            UsuarioDTO usuario = new()
            {
                Nombres = _nombres,
                Apellidos = _apellidos,
                CorreoElectronico = _correoElectronico,
                Contrasena = _contrasena,
                Imagen = _imagenPerfil
            };

            SeleccionEtiquetasPagina seleccionEtiquetasPagina = new(usuario);
            this.NavigationService.Navigate(seleccionEtiquetasPagina);
        }

        private void ClicActualizar(object sender, RoutedEventArgs e)
        {

        }

        private void ClicBorrarImagen(object sender, MouseButtonEventArgs e)
        {
            _imagenPerfil = [];
            var uri = new Uri("pack://application:,,,/Recursos/default_profile_image.png");
            var bitmap = new BitmapImage(uri);
            imgPerfil.Source = bitmap;
            imgBorrarImagen.Visibility = Visibility.Collapsed;
            btnCambiarImagen.Visibility = Visibility.Collapsed;
            btnSubirImagen.Visibility = Visibility.Visible;
        }

        private void ClicCambiarImagen(object sender, RoutedEventArgs e)
        {
            ClicSubirImagen(sender,e);
        }

        private void ClicSubirImagen(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialogo = new()
            {
                Filter = "Archivos de imagen|*.png;"
            };

            if (dialogo.ShowDialog() == true)
            {
                string rutaImagen = dialogo.FileName;
                ValidarTamanioImagen(rutaImagen);
            }
        }

        private void ValidarTamanioImagen(string rutaImagen)
        {
            try
            {
                FileInfo informacionArchivo = new(rutaImagen);
                long tamañoEnBytes = informacionArchivo.Length;
                long tamañoEnKilobytes = tamañoEnBytes / BYTES_POR_KILOBYTE;

                if (tamañoEnKilobytes <= KILOBYTES_POR_MEGABYTE)
                {
                    BitmapImage bitmapImage = new(new Uri(rutaImagen));
                    imgPerfil.Source = bitmapImage;
                    _imagenPerfil = File.ReadAllBytes(rutaImagen);
                    btnCambiarImagen.Visibility = Visibility.Visible;
                    imgBorrarImagen.Visibility = Visibility.Visible;
                    btnSubirImagen.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ErrorMensaje errorMensaje = new("La imagen no puede superar 1MB");
                    errorMensaje.ShowDialog();
                }
            }
            catch (IOException)
            {
                ErrorMensaje errorMensaje = new("Error al cargar la imagen");
                errorMensaje.ShowDialog();
            }
        }

        private void ClicEtiquetas(object sender, RoutedEventArgs e)
        {

        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
    }
}
