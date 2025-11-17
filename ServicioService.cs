using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SalonBellezaApp
{
    public class ServicioService
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

        public async Task<List<Servicio>> ObtenerTodosAsync()
        {
            var servicios = new List<Servicio>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT IdServicio, Nombre, Precio, Duracion, Descripcion FROM Servicios ORDER BY Nombre";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var servicio = new Servicio
                            {
                                IdServicio = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Precio = reader.GetDecimal(2),
                                Duracion = reader.GetInt32(3),
                                Descripcion = reader.IsDBNull(4) ? "" : reader.GetString(4)
                            };
                            servicio.Productos = await ObtenerProductosDelServicioAsync(servicio.IdServicio);
                            servicio.CategoriasPermitidas = await ObtenerCategoriasDelServicioAsync(servicio.IdServicio);
                            servicios.Add(servicio);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener servicios: {ex.Message}");
            }

            return servicios;
        }

        public async Task<List<string>> ObtenerCategoriasDelServicioAsync(int idServicio)
        {
            var categorias = new List<string>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT Categoria FROM Servicio_Categoria_Producto WHERE IdServicio = @IdServicio ORDER BY Categoria";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdServicio", idServicio);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                categorias.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener categorías del servicio: {ex.Message}");
            }

            return categorias;
        }

        public async Task<bool> AgregarCategoriaAlServicioAsync(int idServicio, string categoria, string descripcion = "")
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"INSERT INTO Servicio_Categoria_Producto (IdServicio, Categoria, Descripcion) 
                                   VALUES (@IdServicio, @Categoria, @Descripcion)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdServicio", idServicio);
                        command.Parameters.AddWithValue("@Categoria", categoria);
                        command.Parameters.AddWithValue("@Descripcion", descripcion ?? "");

                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar categoría al servicio: {ex.Message}");
            }
        }

        public async Task<bool> EliminarCategoriaDelServicioAsync(int idServicio, string categoria)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "DELETE FROM Servicio_Categoria_Producto WHERE IdServicio = @IdServicio AND Categoria = @Categoria";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdServicio", idServicio);
                        command.Parameters.AddWithValue("@Categoria", categoria);

                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar categoría del servicio: {ex.Message}");
            }
        }

        public async Task<List<ServicioProducto>> ObtenerProductosDelServicioAsync(int idServicio)
        {
            var productos = new List<ServicioProducto>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT sp.IdServicioProducto, sp.IdServicio, sp.IdProducto, 
                                           p.Nombre, sp.Cantidad, sp.Descripcion
                                    FROM Servicio_Producto sp
                                    INNER JOIN Productos p ON sp.IdProducto = p.IdProducto
                                    WHERE sp.IdServicio = @IdServicio
                                    ORDER BY p.Nombre";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdServicio", idServicio);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                productos.Add(new ServicioProducto
                                {
                                    IdServicioProducto = reader.GetInt32(0),
                                    IdServicio = reader.GetInt32(1),
                                    IdProducto = reader.GetInt32(2),
                                    NombreProducto = reader.GetString(3),
                                    Cantidad = reader.GetDecimal(4),
                                    Descripcion = reader.IsDBNull(5) ? "" : reader.GetString(5)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener productos del servicio: {ex.Message}");
            }

            return productos;
        }

        public async Task<List<Servicio>> ObtenerServiciosPorProductoAsync(int idProducto)
        {
            var servicios = new List<Servicio>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT DISTINCT s.IdServicio, s.Nombre, s.Precio, s.Duracion, s.Descripcion
                                     FROM Servicios s
                                     INNER JOIN Servicio_Producto sp ON s.IdServicio = sp.IdServicio
                                     WHERE sp.IdProducto = @IdProducto
                                     ORDER BY s.Nombre";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdProducto", idProducto);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var servicio = new Servicio
                                {
                                    IdServicio = reader.GetInt32(0),
                                    Nombre = reader.GetString(1),
                                    Precio = reader.GetDecimal(2),
                                    Duracion = reader.GetInt32(3),
                                    Descripcion = reader.IsDBNull(4) ? "" : reader.GetString(4)
                                };
                                servicio.Productos = await ObtenerProductosDelServicioAsync(servicio.IdServicio);
                                servicio.CategoriasPermitidas = await ObtenerCategoriasDelServicioAsync(servicio.IdServicio);
                                servicios.Add(servicio);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener servicios por producto: {ex.Message}");
            }

            return servicios;
        }

        public async Task<bool> AgregarProductoAlServicioAsync(int idServicio, int idProducto, decimal cantidad, string descripcion = "")
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var cmdCategoria = new SqlCommand("SELECT Categoria FROM Productos WHERE IdProducto = @IdProducto", connection);
                    cmdCategoria.Parameters.AddWithValue("@IdProducto", idProducto);
                    var categoriaObj = await cmdCategoria.ExecuteScalarAsync();
                    if (categoriaObj == null || categoriaObj == DBNull.Value)
                        throw new Exception("Producto no encontrado.");

                    string categoriaProducto = categoriaObj.ToString();

                    var cmdCategoriasServicio = new SqlCommand("SELECT COUNT(*) FROM Servicio_Categoria_Producto WHERE IdServicio = @IdServicio AND Categoria = @Categoria", connection);
                    cmdCategoriasServicio.Parameters.AddWithValue("@IdServicio", idServicio);
                    cmdCategoriasServicio.Parameters.AddWithValue("@Categoria", categoriaProducto);
                    int permitida = (int)await cmdCategoriasServicio.ExecuteScalarAsync();

                    if (permitida == 0)
                        throw new Exception($"La categoría '{categoriaProducto}' no está permitida para este servicio.");

                    // Obtener stock actual del producto
                    var cmdStock = new SqlCommand("SELECT StockActual FROM Productos WHERE IdProducto = @IdProducto", connection);
                    cmdStock.Parameters.AddWithValue("@IdProducto", idProducto);
                    var stockObj = await cmdStock.ExecuteScalarAsync();
                    if (stockObj == null || stockObj == DBNull.Value)
                        throw new Exception("Producto no encontrado.");

                    int stockActual = Convert.ToInt32(stockObj);

                    var cmdAssigned = new SqlCommand("SELECT ISNULL(SUM(Cantidad), 0) FROM Servicio_Producto WHERE IdProducto = @IdProducto", connection);
                    cmdAssigned.Parameters.AddWithValue("@IdProducto", idProducto);
                    var assignedObj = await cmdAssigned.ExecuteScalarAsync();
                    decimal assignedTotal = assignedObj == DBNull.Value ? 0 : Convert.ToDecimal(assignedObj);

                    if (assignedTotal + cantidad > stockActual)
                        throw new Exception($"Stock insuficiente. Stock actual: {stockActual}, ya asignado: {assignedTotal}.");

                    int existingId = 0;
                    decimal existingCantidad = 0;
                    var cmdExist = new SqlCommand("SELECT IdServicioProducto, Cantidad FROM Servicio_Producto WHERE IdServicio = @IdServicio AND IdProducto = @IdProducto", connection);
                    cmdExist.Parameters.AddWithValue("@IdServicio", idServicio);
                    cmdExist.Parameters.AddWithValue("@IdProducto", idProducto);

                    using (var rdr = await cmdExist.ExecuteReaderAsync())
                    {
                        if (await rdr.ReadAsync())
                        {
                            existingId = rdr.GetInt32(0);
                            existingCantidad = rdr.GetDecimal(1);
                        }
                    }

                    if (existingId > 0)
                    {
                        var cmdUpdate = new SqlCommand("UPDATE Servicio_Producto SET Cantidad = Cantidad + @Cantidad, Descripcion = @Descripcion WHERE IdServicioProducto = @IdServicioProducto", connection);
                        cmdUpdate.Parameters.AddWithValue("@Cantidad", cantidad);
                        cmdUpdate.Parameters.AddWithValue("@Descripcion", descripcion ?? "");
                        cmdUpdate.Parameters.AddWithValue("@IdServicioProducto", existingId);

                        await cmdUpdate.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        var cmdInsert = new SqlCommand("INSERT INTO Servicio_Producto (IdServicio, IdProducto, Cantidad, Descripcion) VALUES (@IdServicio, @IdProducto, @Cantidad, @Descripcion)", connection);
                        cmdInsert.Parameters.AddWithValue("@IdServicio", idServicio);
                        cmdInsert.Parameters.AddWithValue("@IdProducto", idProducto);
                        cmdInsert.Parameters.AddWithValue("@Cantidad", cantidad);
                        cmdInsert.Parameters.AddWithValue("@Descripcion", descripcion ?? "");

                        await cmdInsert.ExecuteNonQueryAsync();
                    }

                    var cmdNombreProducto = new SqlCommand("SELECT Nombre FROM Productos WHERE IdProducto = @IdProducto", connection);
                    cmdNombreProducto.Parameters.AddWithValue("@IdProducto", idProducto);
                    var nombreProducto = await cmdNombreProducto.ExecuteScalarAsync();

                    var cmdMovimiento = new SqlCommand(@"INSERT INTO MovimientosInventario 
                                                        (IdProducto, TipoMovimiento, Cantidad, Motivo, Usuario, FechaMovimiento)
                                                        VALUES (@IdProducto, @TipoMovimiento, @Cantidad, @Motivo, @Usuario, GETDATE())", connection);
                    cmdMovimiento.Parameters.AddWithValue("@IdProducto", idProducto);
                    cmdMovimiento.Parameters.AddWithValue("@TipoMovimiento", "salida");
                    cmdMovimiento.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmdMovimiento.Parameters.AddWithValue("@Motivo", $"Asignado al servicio");
                    cmdMovimiento.Parameters.AddWithValue("@Usuario", "Sistema");

                    await cmdMovimiento.ExecuteNonQueryAsync();

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar producto al servicio: {ex.Message}");
            }
        }

        public async Task<bool> EliminarProductoDelServicioAsync(int idServicioProducto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var cmdInfo = new SqlCommand("SELECT IdProducto, Cantidad FROM Servicio_Producto WHERE IdServicioProducto = @IdServicioProducto", connection);
                    cmdInfo.Parameters.AddWithValue("@IdServicioProducto", idServicioProducto);

                    int idProducto = 0;
                    decimal cantidad = 0;

                    using (var rdr = await cmdInfo.ExecuteReaderAsync())
                    {
                        if (await rdr.ReadAsync())
                        {
                            idProducto = rdr.GetInt32(0);
                            cantidad = rdr.GetDecimal(1);
                        }
                    }

                    var cmdDelete = new SqlCommand("DELETE FROM Servicio_Producto WHERE IdServicioProducto = @IdServicioProducto", connection);
                    cmdDelete.Parameters.AddWithValue("@IdServicioProducto", idServicioProducto);
                    await cmdDelete.ExecuteNonQueryAsync();

                    if (idProducto > 0 && cantidad > 0)
                    {
                        var cmdMovimiento = new SqlCommand(@"INSERT INTO MovimientosInventario 
                                                            (IdProducto, TipoMovimiento, Cantidad, Motivo, Usuario, FechaMovimiento)
                                                            VALUES (@IdProducto, @TipoMovimiento, @Cantidad, @Motivo, @Usuario, GETDATE())", connection);
                        cmdMovimiento.Parameters.AddWithValue("@IdProducto", idProducto);
                        cmdMovimiento.Parameters.AddWithValue("@TipoMovimiento", "entrada");
                        cmdMovimiento.Parameters.AddWithValue("@Cantidad", cantidad);
                        cmdMovimiento.Parameters.AddWithValue("@Motivo", "Removido de asignación del servicio");
                        cmdMovimiento.Parameters.AddWithValue("@Usuario", "Sistema");

                        await cmdMovimiento.ExecuteNonQueryAsync();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar producto del servicio: {ex.Message}");
            }
        }

        public async Task<bool> ActualizarProductoDelServicioAsync(int idServicioProducto, decimal cantidad, string descripcion)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var cmdInfo = new SqlCommand(@"SELECT sp.Cantidad, sp.IdProducto, p.StockActual
                                                   FROM Servicio_Producto sp
                                                   INNER JOIN Productos p ON sp.IdProducto = p.IdProducto
                                                   WHERE sp.IdServicioProducto = @IdServicioProducto", connection);
                    cmdInfo.Parameters.AddWithValue("@IdServicioProducto", idServicioProducto);

                    decimal currentCantidad = 0;
                    int idProducto = 0;
                    int stockActual = 0;

                    using (var rdr = await cmdInfo.ExecuteReaderAsync())
                    {
                        if (await rdr.ReadAsync())
                        {
                            currentCantidad = rdr.GetDecimal(0);
                            idProducto = rdr.GetInt32(1);
                            stockActual = rdr.GetInt32(2);
                        }
                        else
                        {
                            throw new Exception("Registro no encontrado.");
                        }
                    }

                    var cmdAssigned = new SqlCommand("SELECT ISNULL(SUM(Cantidad), 0) FROM Servicio_Producto WHERE IdProducto = @IdProducto AND IdServicioProducto <> @IdServicioProducto", connection);
                    cmdAssigned.Parameters.AddWithValue("@IdProducto", idProducto);
                    cmdAssigned.Parameters.AddWithValue("@IdServicioProducto", idServicioProducto);
                    var assignedObj = await cmdAssigned.ExecuteScalarAsync();
                    decimal assignedOther = assignedObj == DBNull.Value ? 0 : Convert.ToDecimal(assignedObj);

                    if (assignedOther + cantidad > stockActual)
                        throw new Exception($"Stock insuficiente. Stock actual: {stockActual}, ya asignado: {assignedOther}.");

                    decimal diferencia = cantidad - currentCantidad;

                    var cmdUpdate = new SqlCommand(@"UPDATE Servicio_Producto 
                                                    SET Cantidad = @Cantidad, Descripcion = @Descripcion
                                                    WHERE IdServicioProducto = @IdServicioProducto", connection);
                    cmdUpdate.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmdUpdate.Parameters.AddWithValue("@Descripcion", descripcion ?? "");
                    cmdUpdate.Parameters.AddWithValue("@IdServicioProducto", idServicioProducto);

                    await cmdUpdate.ExecuteNonQueryAsync();

                    if (diferencia != 0)
                    {
                        string tipoMov = diferencia > 0 ? "salida" : "entrada";
                        decimal cantidadMov = Math.Abs(diferencia);

                        var cmdMovimiento = new SqlCommand(@"INSERT INTO MovimientosInventario 
                                                            (IdProducto, TipoMovimiento, Cantidad, Motivo, Usuario, FechaMovimiento)
                                                            VALUES (@IdProducto, @TipoMovimiento, @Cantidad, @Motivo, @Usuario, GETDATE())", connection);
                        cmdMovimiento.Parameters.AddWithValue("@IdProducto", idProducto);
                        cmdMovimiento.Parameters.AddWithValue("@TipoMovimiento", tipoMov);
                        cmdMovimiento.Parameters.AddWithValue("@Cantidad", cantidadMov);
                        cmdMovimiento.Parameters.AddWithValue("@Motivo", "Ajuste de cantidad en asignación del servicio");
                        cmdMovimiento.Parameters.AddWithValue("@Usuario", "Sistema");

                        await cmdMovimiento.ExecuteNonQueryAsync();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar producto del servicio: {ex.Message}");
            }
        }

        public async Task<bool> AgregarAsync(Servicio servicio)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"INSERT INTO Servicios (Nombre, Precio, Duracion, Descripcion) 
                                   VALUES (@Nombre, @Precio, @Duracion, @Descripcion)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Nombre", servicio.Nombre);
                        command.Parameters.AddWithValue("@Precio", servicio.Precio);
                        command.Parameters.AddWithValue("@Duracion", servicio.Duracion);
                        command.Parameters.AddWithValue("@Descripcion", servicio.Descripcion ?? "");

                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar servicio: {ex.Message}");
            }
        }

        public async Task<bool> ActualizarAsync(Servicio servicio)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"UPDATE Servicios 
                                   SET Nombre=@Nombre, Precio=@Precio, Duracion=@Duracion, Descripcion=@Descripcion 
                                   WHERE IdServicio=@IdServicio";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdServicio", servicio.IdServicio);
                        command.Parameters.AddWithValue("@Nombre", servicio.Nombre);
                        command.Parameters.AddWithValue("@Precio", servicio.Precio);
                        command.Parameters.AddWithValue("@Duracion", servicio.Duracion);
                        command.Parameters.AddWithValue("@Descripcion", servicio.Descripcion ?? "");

                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar servicio: {ex.Message}");
            }
        }

        public async Task<bool> EliminarAsync(int idServicio)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string verificarQuery = "SELECT COUNT(*) FROM Cita_Servicio WHERE IdServicio = @IdServicio";
                    using (var verificarCmd = new SqlCommand(verificarQuery, connection))
                    {
                        verificarCmd.Parameters.AddWithValue("@IdServicio", idServicio);
                        int citasAsociadas = (int)await verificarCmd.ExecuteScalarAsync();

                        if (citasAsociadas > 0)
                        {
                            throw new Exception("No se puede eliminar el servicio porque tiene citas asociadas.");
                        }
                    }

                    string query = "DELETE FROM Servicios WHERE IdServicio = @IdServicio";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdServicio", idServicio);
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar servicio: {ex.Message}");
            }
        }

        public async Task<List<Servicio>> BuscarAsync(string termino)
        {
            var servicios = new List<Servicio>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT IdServicio, Nombre, Precio, Duracion, Descripcion 
                                   FROM Servicios 
                                   WHERE Nombre LIKE @Termino OR Descripcion LIKE @Termino
                                   ORDER BY Nombre";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Termino", $"%{termino}%");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var servicio = new Servicio
                                {
                                    IdServicio = reader.GetInt32(0),
                                    Nombre = reader.GetString(1),
                                    Precio = reader.GetDecimal(2),
                                    Duracion = reader.GetInt32(3),
                                    Descripcion = reader.IsDBNull(4) ? "" : reader.GetString(4)
                                };
                                servicio.Productos = await ObtenerProductosDelServicioAsync(servicio.IdServicio);
                                servicio.CategoriasPermitidas = await ObtenerCategoriasDelServicioAsync(servicio.IdServicio);
                                servicios.Add(servicio);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar servicios: {ex.Message}");
            }

            return servicios;
        }
    }

    public class Servicio
    {
        public int IdServicio { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Duracion { get; set; }
        public string Descripcion { get; set; }
        public List<ServicioProducto> Productos { get; set; } = new List<ServicioProducto>();
        public List<string> CategoriasPermitidas { get; set; } = new List<string>();
    }

    public class ServicioProducto
    {
        public int IdServicioProducto { get; set; }
        public int IdServicio { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public decimal Cantidad { get; set; }
        public string Descripcion { get; set; }
    }
}
