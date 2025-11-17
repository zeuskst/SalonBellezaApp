using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SalonBellezaApp
{
    public class ProductoService
    {
        private readonly string _connectionString = @"Data Source=LAPTOP-28BR3PVP\SQLEXPRESS;
Initial Catalog=SalonBellezaDB;
Integrated Security=True;
Persist Security Info=False;
Pooling=False;
MultipleActiveResultSets=False;
Encrypt=True;
TrustServerCertificate=True;
Application Name=""MiAppCSharp"";
Command Timeout=30;";

        public async Task<List<Producto>> ObtenerTodosAsync()
        {
            var productos = new List<Producto>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT IdProducto, Nombre, Categoria, Descripcion, 
                                           StockActual, StockMinimo, PrecioCompra, PrecioVenta, 
                                           UnidadMedida, FechaEntrada, FechaCaducidad, Estado
                                    FROM Productos
                                    ORDER BY Nombre";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            productos.Add(new Producto
                            {
                                IdProducto = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Categoria = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Descripcion = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                StockActual = reader.GetInt32(4),
                                StockMinimo = reader.GetInt32(5),
                                PrecioCompra = reader.GetDecimal(6),
                                PrecioVenta = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),
                                UnidadMedida = reader.IsDBNull(8) ? "" : reader.GetString(8),
                                FechaEntrada = reader.GetDateTime(9),
                                FechaCaducidad = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                                Estado = reader.GetString(11)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener productos: {ex.Message}");
            }

            return productos;
        }

        public async Task<bool> AgregarAsync(Producto producto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"INSERT INTO Productos (Nombre, Categoria, Descripcion, StockActual, StockMinimo, 
                                                          PrecioCompra, PrecioVenta, UnidadMedida, FechaEntrada, 
                                                          FechaCaducidad, Estado) 
                                   VALUES (@Nombre, @Categoria, @Descripcion, @StockActual, @StockMinimo, 
                                          @PrecioCompra, @PrecioVenta, @UnidadMedida, @FechaEntrada, 
                                          @FechaCaducidad, @Estado)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Nombre", producto.Nombre);
                        command.Parameters.AddWithValue("@Categoria", producto.Categoria ?? "");
                        command.Parameters.AddWithValue("@Descripcion", producto.Descripcion ?? "");
                        command.Parameters.AddWithValue("@StockActual", producto.StockActual);
                        command.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
                        command.Parameters.AddWithValue("@PrecioCompra", producto.PrecioCompra);
                        command.Parameters.AddWithValue("@PrecioVenta", producto.PrecioVenta.HasValue ? (object)producto.PrecioVenta.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@UnidadMedida", producto.UnidadMedida ?? "");
                        command.Parameters.AddWithValue("@FechaEntrada", producto.FechaEntrada);
                        command.Parameters.AddWithValue("@FechaCaducidad", producto.FechaCaducidad.HasValue ? (object)producto.FechaCaducidad.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@Estado", producto.Estado);

                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar producto: {ex.Message}");
            }
        }

        public async Task<bool> ActualizarAsync(Producto producto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"UPDATE Productos 
                                   SET Nombre=@Nombre, Categoria=@Categoria, Descripcion=@Descripcion, 
                                       StockActual=@StockActual, StockMinimo=@StockMinimo,
                                       PrecioCompra=@PrecioCompra, PrecioVenta=@PrecioVenta, 
                                       UnidadMedida=@UnidadMedida, FechaCaducidad=@FechaCaducidad, Estado=@Estado
                                   WHERE IdProducto=@IdProducto";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdProducto", producto.IdProducto);
                        command.Parameters.AddWithValue("@Nombre", producto.Nombre);
                        command.Parameters.AddWithValue("@Categoria", producto.Categoria ?? "");
                        command.Parameters.AddWithValue("@Descripcion", producto.Descripcion ?? "");
                        command.Parameters.AddWithValue("@StockActual", producto.StockActual);
                        command.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
                        command.Parameters.AddWithValue("@PrecioCompra", producto.PrecioCompra);
                        command.Parameters.AddWithValue("@PrecioVenta", producto.PrecioVenta.HasValue ? (object)producto.PrecioVenta.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@UnidadMedida", producto.UnidadMedida ?? "");
                        command.Parameters.AddWithValue("@FechaCaducidad", producto.FechaCaducidad.HasValue ? (object)producto.FechaCaducidad.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@Estado", producto.Estado);

                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar producto: {ex.Message}");
            }
        }

        public async Task<bool> EliminarAsync(int idProducto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Verificar si está en uso en algún servicio
                    string verificarQuery = "SELECT COUNT(*) FROM Servicio_Producto WHERE IdProducto = @IdProducto";
                    using (var verificarCmd = new SqlCommand(verificarQuery, connection))
                    {
                        verificarCmd.Parameters.AddWithValue("@IdProducto", idProducto);
                        int serviciosAsociados = (int)await verificarCmd.ExecuteScalarAsync();

                        if (serviciosAsociados > 0)
                        {
                            throw new Exception("No se puede eliminar el producto porque está asociado a servicios.");
                        }
                    }

                    string query = "DELETE FROM Productos WHERE IdProducto = @IdProducto";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdProducto", idProducto);
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar producto: {ex.Message}");
            }
        }

        public async Task<List<Producto>> BuscarAsync(string termino)
        {
            var productos = new List<Producto>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT IdProducto, Nombre, Categoria, Descripcion, 
                                           StockActual, StockMinimo, PrecioCompra, PrecioVenta, 
                                           UnidadMedida, FechaEntrada, FechaCaducidad, Estado
                                    FROM Productos
                                    WHERE Nombre LIKE @Termino OR Categoria LIKE @Termino
                                    ORDER BY Nombre";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Termino", $"%{termino}%");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                productos.Add(new Producto
                                {
                                    IdProducto = reader.GetInt32(0),
                                    Nombre = reader.GetString(1),
                                    Categoria = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    Descripcion = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                    StockActual = reader.GetInt32(4),
                                    StockMinimo = reader.GetInt32(5),
                                    PrecioCompra = reader.GetDecimal(6),
                                    PrecioVenta = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),
                                    UnidadMedida = reader.IsDBNull(8) ? "" : reader.GetString(8),
                                    FechaEntrada = reader.GetDateTime(9),
                                    FechaCaducidad = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                                    Estado = reader.GetString(11)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar productos: {ex.Message}");
            }

            return productos;
        }
    }

    // Clase modelo para Producto
    public class Producto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public string Descripcion { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal? PrecioVenta { get; set; }
        public string UnidadMedida { get; set; }
        public DateTime FechaEntrada { get; set; }
        public DateTime? FechaCaducidad { get; set; }
        public string Estado { get; set; }
    }
}
