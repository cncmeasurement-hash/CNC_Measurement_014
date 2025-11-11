using devDept.Geometry;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace _014.Managers.Data
{
    /// <summary>
    /// Point3D için JSON converter
    /// System.Text.Json ile Eyeshot Point3D serialization/deserialization
    /// </summary>
    public class Point3DConverter : JsonConverter<Point3D>
    {
        /// <summary>
        /// Point3D'yi JSON'a çevirir
        /// Format: {"X": 10.5, "Y": 20.3, "Z": 5.0}
        /// </summary>
        public override void Write(Utf8JsonWriter writer, Point3D value, JsonSerializerOptions options)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            try
            {
                writer.WriteStartObject();
                writer.WriteNumber("X", value.X);
                writer.WriteNumber("Y", value.Y);
                writer.WriteNumber("Z", value.Z);
                writer.WriteEndObject();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Point3DConverter Write hatası: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// JSON'dan Point3D oluşturur
        /// </summary>
        public override Point3D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Point3D JSON object değil!");
                }

                double x = 0, y = 0, z = 0;
                bool hasX = false, hasY = false, hasZ = false;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString();
                        reader.Read(); // Value'ya geç

                        switch (propertyName)
                        {
                            case "X":
                                x = reader.GetDouble();
                                hasX = true;
                                break;
                            case "Y":
                                y = reader.GetDouble();
                                hasY = true;
                                break;
                            case "Z":
                                z = reader.GetDouble();
                                hasZ = true;
                                break;
                        }
                    }
                }

                // Validation
                if (!hasX || !hasY || !hasZ)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Point3D JSON eksik property (X, Y veya Z yok)");
                }

                return new Point3D(x, y, z);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Point3DConverter Read hatası: {ex.Message}");
                throw new JsonException($"Point3D deserialize edilemedi: {ex.Message}", ex);
            }
        }
    }
}
