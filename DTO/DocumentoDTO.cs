using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UVemyCliente.DTO
{
    public class DocumentoDTO
    {
        public int IdDocumento { get; set; }
        public byte[] Archivo { get; set; }
        public string Nombre { get; set; }
        public int IdTipoArchivo { get; set; }
        public int IdCurso { get; set; }
        public int IdClase { get; set; }
    }
}
