using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UVemyCliente.Utilidades;
using System.Diagnostics;
using System.Net.Sockets;

namespace UVemyCliente.Conexion
{
    public class APIConexion
    {
        private static HttpClient _cliente;
        public static HttpClient ObtenerClient()
        {
            if (_cliente == null)
            {
                _cliente = new HttpClient();
                _cliente.DefaultRequestHeaders.Accept.Clear();
                _cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/pdf"));
                _cliente.BaseAddress = new Uri("http://localhost:3000/api/");
            }

            //To-DO
            _cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SingletonUsuario.JWT);

            return _cliente;
        }

        public static HttpClient ObtenerClientSinAutenticacion()
        {
            if (_cliente == null)
            {
                _cliente = new HttpClient();
                _cliente.DefaultRequestHeaders.Accept.Clear();
                _cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _cliente.BaseAddress = new Uri("http://localhost:3000/api/");
            }

            _cliente.DefaultRequestHeaders.Authorization = null;

            return _cliente;
        }

        public static async Task<HttpResponseMessage> EnviarRequestAsync(HttpMethod method, string relativeUri, HttpContent content = null)
        {
            HttpResponseMessage respuesta = new HttpResponseMessage();
            try
            {
                respuesta = await ObtenerClient().SendAsync(new HttpRequestMessage(method, relativeUri)
                {
                    Content = content
                });
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex);
                if (ex.InnerException is SocketException socketEx && socketEx.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    respuesta.StatusCode = System.Net.HttpStatusCode.ServiceUnavailable;
                }
                else
                {
                    respuesta.StatusCode = System.Net.HttpStatusCode.InternalServerError; 
                }
            }
            //si la respuesta tiene un jwt token nuevo que se le asigne y que si es un 401 , que ponga el mensaje, error debe volver a iniciar sesión
            return respuesta;
        }

        //Para el inicio de sesión
        public static async Task<HttpResponseMessage> EnviarRequestSinAutenticacionAsync(HttpMethod method, string relativeUri, HttpContent content = null)
        {
            HttpResponseMessage respuesta = new HttpResponseMessage();
            try
            {
                respuesta = await ObtenerClientSinAutenticacion().SendAsync(new HttpRequestMessage(method, relativeUri)
                {
                    Content = content
                });
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex);
                respuesta.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            }
            return respuesta;
        }
    }
}

