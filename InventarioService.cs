using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace SalonBellezaApp
{
    public class ProductoInventario
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public string UnidadMedida { get; set; }
        public DateTime? FechaCaducidad { get; set; }
        public string Estado { get; set; }

        public string EstadoStock
        {
            get
            {
                if (StockActual <= 0) return "❌ Sin Stock";
                if (StockActual <= StockMinimo) return "⚠️ Stock Bajo";
                return "✅ Normal";
            }
        }

        public bool RequiereAtencion => StockActual <= StockMinimo;
        public string FechaCaducidadDisplay => FechaCaducidad?.ToString("dd/MM/yyyy") ?? "N/A";
        public decimal ValorInventario => StockActual * PrecioCompra;
    }

    public class MovimientoInventario
    {
        public int IdMovimiento { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }

       
        public string Categoria { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaCaducidad { get; set; }
        public string FechaCaducidadDisplay => FechaCaducidad?.ToString("dd/MM/yyyy") ?? string.Empty;

        public string TipoMovimiento { get; set; }
        public decimal Cantidad { get; set; }
        public string Motivo { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string Usuario { get; set; }

        public string TipoDisplay
        {
            get
            {
                return TipoMovimiento switch
                {
                    "entrada" => "📥 Entrada",
                    "salida" => "📤 Salida",
                    "ajuste" => "🔧 Ajuste",
                    _ => TipoMovimiento
                };
            }
        }
    }

    public class InventarioService
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

        public async Task<List<ProductoInventario>> ObtenerInventarioAsync()
        {
            List<ProductoInventario> productos = new List<ProductoInventario>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        IdProducto, Nombre, Categoria, StockActual, StockMinimo,
                        PrecioCompra, PrecioVenta, UnidadMedida, FechaCaducidad, Estado
                    FROM Productos
                    WHERE Estado = 'activo'
                    ORDER BY Nombre";

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    await connection.OpenAsync();
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        productos.Add(new ProductoInventario
                        {
                            IdProducto = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Categoria = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            StockActual = reader.GetInt32(3),
                            StockMinimo = reader.GetInt32(4),
                            PrecioCompra = reader.GetDecimal(5),
                            PrecioVenta = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                            UnidadMedida = reader.IsDBNull(7) ? "unidad" : reader.GetString(7),
                            FechaCaducidad = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                            Estado = reader.GetString(9)
                        });
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al obtener inventario: {ex.Message}");
                }
            }

            return productos;
        }

        public async Task<List<ProductoInventario>> ObtenerProductosStockBajoAsync()
        {
            List<ProductoInventario> productos = new List<ProductoInventario>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        IdProducto, Nombre, Categoria, StockActual, StockMinimo,
                        PrecioCompra, PrecioVenta, UnidadMedida, FechaCaducidad, Estado
                    FROM Productos
                    WHERE Estado = 'activo' AND StockActual <= StockMinimo
                    ORDER BY StockActual ASC";

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    await connection.OpenAsync();
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        productos.Add(new ProductoInventario
                        {
                            IdProducto = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Categoria = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            StockActual = reader.GetInt32(3),
                            StockMinimo = reader.GetInt32(4),
                            PrecioCompra = reader.GetDecimal(5),
                            PrecioVenta = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                            UnidadMedida = reader.IsDBNull(7) ? "unidad" : reader.GetString(7),
                            FechaCaducidad = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                            Estado = reader.GetString(9)
                        });
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al obtener productos con stock bajo: {ex.Message}");
                }
            }

            return productos;
        }

        public async Task<List<ProductoInventario>> ObtenerProductosProximosVencerAsync()
        {
            List<ProductoInventario> productos = new List<ProductoInventario>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        IdProducto, Nombre, Categoria, StockActual, StockMinimo,
                        PrecioCompra, PrecioVenta, UnidadMedida, FechaCaducidad, Estado
                    FROM Productos
                    WHERE Estado = 'activo' 
                        AND FechaCaducidad IS NOT NULL 
                        AND FechaCaducidad <= DATEADD(day, 30, GETDATE())
                        AND FechaCaducidad >= GETDATE()
                    ORDER BY FechaCaducidad ASC";

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    await connection.OpenAsync();
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        productos.Add(new ProductoInventario
                        {
                            IdProducto = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Categoria = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            StockActual = reader.GetInt32(3),
                            StockMinimo = reader.GetInt32(4),
                            PrecioCompra = reader.GetDecimal(5),
                            PrecioVenta = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                            UnidadMedida = reader.IsDBNull(7) ? "unidad" : reader.GetString(7),
                            FechaCaducidad = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                            Estado = reader.GetString(9)
                        });
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al obtener productos próximos a vencer: {ex.Message}");
                }
            }

            return productos;
        }

        public async Task<bool> RegistrarMovimientoAsync(int idProducto, string tipoMovimiento, decimal cantidad, string motivo, string usuario = "Sistema")
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    string queryMovimiento = @"
                        INSERT INTO MovimientosInventario 
                        (IdProducto, TipoMovimiento, Cantidad, Motivo, Usuario, FechaMovimiento)
                        VALUES (@IdProducto, @TipoMovimiento, @Cantidad, @Motivo, @Usuario, GETDATE())";

                    SqlCommand cmdMovimiento = new SqlCommand(queryMovimiento, connection, transaction);
                    cmdMovimiento.Parameters.AddWithValue("@IdProducto", idProducto);
                    cmdMovimiento.Parameters.AddWithValue("@TipoMovimiento", tipoMovimiento);
                    cmdMovimiento.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmdMovimiento.Parameters.AddWithValue("@Motivo", motivo ?? "");
                    cmdMovimiento.Parameters.AddWithValue("@Usuario", usuario);

                    await cmdMovimiento.ExecuteNonQueryAsync();

                    string queryStock = "";
                    if (tipoMovimiento == "entrada")
                    {
                        queryStock = "UPDATE Productos SET StockActual = StockActual + @Cantidad WHERE IdProducto = @IdProducto";
                    }
                    else if (tipoMovimiento == "salida")
                    {
                        queryStock = "UPDATE Productos SET StockActual = StockActual - @Cantidad WHERE IdProducto = @IdProducto AND StockActual >= @Cantidad";
                    }
                    else if (tipoMovimiento == "ajuste")
                    {
                        queryStock = "UPDATE Productos SET StockActual = @Cantidad WHERE IdProducto = @IdProducto";
                    }

                    SqlCommand cmdStock = new SqlCommand(queryStock, connection, transaction);
                    cmdStock.Parameters.AddWithValue("@IdProducto", idProducto);
                    cmdStock.Parameters.AddWithValue("@Cantidad", cantidad);

                    int rowsAffected = await cmdStock.ExecuteNonQueryAsync();

                    if (rowsAffected == 0 && tipoMovimiento == "salida")
                    {
                        throw new Exception("Stock insuficiente para realizar la salida");
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception($"Error al registrar movimiento: {ex.Message}");
                }
            }
        }

        public async Task<bool> RegistrarMovimientoLogAsync(int idProducto, string tipoMovimiento, decimal cantidad, string motivo, string usuario = "Sistema")
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = @"
                        INSERT INTO MovimientosInventario 
                        (IdProducto, TipoMovimiento, Cantidad, Motivo, Usuario, FechaMovimiento)
                        VALUES (@IdProducto, @TipoMovimiento, @Cantidad, @Motivo, @Usuario, GETDATE())";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@IdProducto", idProducto);
                    cmd.Parameters.AddWithValue("@TipoMovimiento", tipoMovimiento);
                    cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmd.Parameters.AddWithValue("@Motivo", motivo ?? "");
                    cmd.Parameters.AddWithValue("@Usuario", usuario ?? "Sistema");

                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al registrar movimiento (log): {ex.Message}");
                }
            }
        }

        public async Task<List<MovimientoInventario>> ObtenerMovimientosAsync(int? idProducto = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            List<MovimientoInventario> movimientos = new List<MovimientoInventario>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        m.IdMovimiento AS IdMovimiento, 
                        m.IdProducto AS IdProducto, 
                        ISNULL(p.Nombre, '') AS NombreProducto, 
                        ISNULL(p.Categoria, '') AS Categoria,
                        ISNULL(p.StockActual, 0) AS StockActual,
                        ISNULL(p.StockMinimo, 0) AS StockMinimo,
                        ISNULL(p.Estado, '') AS Estado,
                        p.FechaCaducidad AS FechaCaducidad,
                        ISNULL(m.TipoMovimiento, '') AS TipoMovimiento,
                        ISNULL(m.Cantidad, 0) AS Cantidad, 
                        ISNULL(m.Motivo, '') AS Motivo, 
                        ISNULL(m.FechaMovimiento, GETDATE()) AS FechaMovimiento, 
                        ISNULL(m.Usuario, 'Sistema') AS Usuario
                    FROM MovimientosInventario m
                    LEFT JOIN Productos p ON m.IdProducto = p.IdProducto
                    WHERE 1=1";

                if (idProducto.HasValue)
                    query += " AND m.IdProducto = @IdProducto";
                if (fechaInicio.HasValue)
                    query += " AND m.FechaMovimiento >= @FechaInicio";
                if (fechaFin.HasValue)
                    query += " AND m.FechaMovimiento <= @FechaFin";

                query += " ORDER BY m.FechaMovimiento DESC";

                SqlCommand command = new SqlCommand(query, connection);
                if (idProducto.HasValue)
                    command.Parameters.AddWithValue("@IdProducto", idProducto.Value);
                if (fechaInicio.HasValue)
                    command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Value);
                if (fechaFin.HasValue)
                    command.Parameters.AddWithValue("@FechaFin", fechaFin.Value.AddDays(1));

                try
                {
                    await connection.OpenAsync();
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        var mov = new MovimientoInventario();

                        mov.IdMovimiento = reader.IsDBNull(reader.GetOrdinal("IdMovimiento")) ? 0 : reader.GetInt32(reader.GetOrdinal("IdMovimiento"));
                        mov.IdProducto = reader.IsDBNull(reader.GetOrdinal("IdProducto")) ? 0 : reader.GetInt32(reader.GetOrdinal("IdProducto"));
                        mov.NombreProducto = reader.IsDBNull(reader.GetOrdinal("NombreProducto")) ? string.Empty : reader.GetString(reader.GetOrdinal("NombreProducto"));

                        mov.Categoria = reader.IsDBNull(reader.GetOrdinal("Categoria")) ? string.Empty : reader.GetString(reader.GetOrdinal("Categoria"));
                        mov.StockActual = reader.IsDBNull(reader.GetOrdinal("StockActual")) ? 0 : reader.GetInt32(reader.GetOrdinal("StockActual"));
                        mov.StockMinimo = reader.IsDBNull(reader.GetOrdinal("StockMinimo")) ? 0 : reader.GetInt32(reader.GetOrdinal("StockMinimo"));
                        mov.Estado = reader.IsDBNull(reader.GetOrdinal("Estado")) ? string.Empty : reader.GetString(reader.GetOrdinal("Estado"));
                        mov.FechaCaducidad = reader.IsDBNull(reader.GetOrdinal("FechaCaducidad")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("FechaCaducidad"));

                        mov.TipoMovimiento = reader.IsDBNull(reader.GetOrdinal("TipoMovimiento")) ? string.Empty : reader.GetString(reader.GetOrdinal("TipoMovimiento"));

                        
                        object cantidadObj = reader.GetValue(reader.GetOrdinal("Cantidad"));
                        mov.Cantidad = cantidadObj == DBNull.Value ? 0 : Convert.ToDecimal(cantidadObj);

                        mov.Motivo = reader.IsDBNull(reader.GetOrdinal("Motivo")) ? string.Empty : reader.GetString(reader.GetOrdinal("Motivo"));
                        mov.FechaMovimiento = reader.IsDBNull(reader.GetOrdinal("FechaMovimiento")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("FechaMovimiento"));
                        mov.Usuario = reader.IsDBNull(reader.GetOrdinal("Usuario")) ? "Sistema" : reader.GetString(reader.GetOrdinal("Usuario"));

                        movimientos.Add(mov);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al obterner movimientos: {ex.Message}");
                }
            }

            return movimientos;
        }

        public async Task<decimal> ObtenerValorTotalInventarioAsync()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT SUM(StockActual * PrecioCompra) FROM Productos WHERE Estado = 'activo'";
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    await connection.OpenAsync();
                    object result = await command.ExecuteScalarAsync();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public async Task<(int Total, int StockBajo, int SinStock, int ProximosVencer)> ObtenerEstadisticasAsync()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string queryTotal = "SELECT COUNT(*) FROM Productos WHERE Estado = 'activo'";
                string queryStockBajo = "SELECT COUNT(*) FROM Productos WHERE Estado = 'activo' AND StockActual <= StockMinimo AND StockActual > 0";
                string querySinStock = "SELECT COUNT(*) FROM Productos WHERE Estado = 'activo' AND StockActual = 0";
                string queryProximosVencer = @"
                    SELECT COUNT(*) FROM Productos 
                    WHERE Estado = 'activo' 
                        AND FechaCaducidad IS NOT NULL 
                        AND FechaCaducidad <= DATEADD(day, 30, GETDATE())
                        AND FechaCaducidad >= GETDATE()";

                int total = Convert.ToInt32(await new SqlCommand(queryTotal, connection).ExecuteScalarAsync());
                int stockBajo = Convert.ToInt32(await new SqlCommand(queryStockBajo, connection).ExecuteScalarAsync());
                int sinStock = Convert.ToInt32(await new SqlCommand(querySinStock, connection).ExecuteScalarAsync());
                int proximosVencer = Convert.ToInt32(await new SqlCommand(queryProximosVencer, connection).ExecuteScalarAsync());

                return (total, stockBajo, sinStock, proximosVencer);
            }
        }
    }
}
