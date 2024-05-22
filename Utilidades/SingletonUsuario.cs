using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UVemyCliente.Utilidades
{
    public class SingletonUsuario
    {
        public static int IdUsuario { get; set; }

        public static string Nombres { get; set; }

        public static string Apellidos { get; set; }

        public static string CorreoElectronico { get; set; }

        public static string JWT { get; set; }

        public static void Limpiar()
        {
            IdUsuario = 0;
            Nombres = string.Empty;
            Apellidos = string.Empty;
            CorreoElectronico = string.Empty;
            JWT = string.Empty;
        }
    }
}
