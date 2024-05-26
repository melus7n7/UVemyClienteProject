using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UVemyCliente.Utilidades
{
    public class TiposCursos
    {
        public int IdTipoCurso { get; set; }
        public string Nombre { get; set; }

        public override string ToString()
        {
            return Nombre.ToString();
        }
    }
}
