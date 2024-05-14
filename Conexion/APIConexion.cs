using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UVemyCliente.Utilidades;

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
                _cliente.BaseAddress = new Uri("http://localhost:3000/api/");
            }

            //To-DO
            SingletonUsuario.JWT = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZ2l2ZW5uYW1lIjoicyIsImlzcyI6IlVWZW15U2Vydmlkb3JKV1QiLCJhdWQiOiJVc3Vhcmlvc1VWZW15SldUIiwiaWF0IjoxNzE1NjY0NDAwLCJleHAiOjE3MTU2NjU2MDB9.jU_km6ouz1uTJMUUPEIoLSk4iINWmIXe95_iYSJp7l8";
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
                Console.WriteLine(ex);
                respuesta.StatusCode = System.Net.HttpStatusCode.InternalServerError;
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

