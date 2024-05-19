using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UVemyCliente.DTO
{
    public class UsuarioCursoDTO
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("idCurso")]
        public int IdCurso { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("idUsuario")]
        public int IdUsuario { get; set; }
        [JsonPropertyName("calificacion")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [System.Text.Json.Serialization.JsonConverter(typeof(CalificacionConverter))]
        public int? Calificacion { get; set; } = 0;
    }

    public class CalificacionConverter : System.Text.Json.Serialization.JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return 0;
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out int value))
            {
                return value;
            }

            if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out value))
            {
                return value;
            }

            return 0;
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

}
