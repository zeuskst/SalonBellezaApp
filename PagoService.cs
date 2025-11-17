using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SalonBellezaApp
{
    public class PagoService
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

        public async Task<List<Pago>> ObtenerTodosAsync()
        {
            var pagos = new List<Pago>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT p.IdPago, p.IdCita, p.Monto, p.MetodoPago, p.FechaPago,
                                           cl.Nombre + ' ' + cl.Paterno + ' ' + cl.Materno AS NombreCliente,
                                           c.Fecha AS FechaCita
                                    FROM Pagos p
                                    INNER JOIN Citas c ON p.IdCita = c.IdCita
                                    INNER JOIN Clientes cl ON c.IdCliente = cl.IdCliente
                                    ORDER BY p.FechaPago DESC";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            pagos.Add(new Pago
                            {
                                IdPago = reader.GetInt32(0),
                                IdCita = reader.GetInt32(1),
                                Monto = reader.GetDecimal(2),
                                MetodoPago = reader.GetString(3),
                                FechaPago = reader.GetDateTime(4),
                                NombreCliente = reader.GetString(5),
                                FechaCita = reader.GetDateTime(6)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener pagos: {ex.Message}");
            }

            return pagos;
        }

        public async Task<List<CitaParaPago>> ObtenerCitasSinPagarAsync()
        {
            var citas = new List<CitaParaPago>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT c.IdCita, 
                                           cl.Nombre + ' ' + cl.Paterno + ' ' + cl.Materno AS NombreCliente,
                                           c.Fecha,
                                           c.HoraInicio,
                                           c.Estado
                                    FROM Citas c
                                    INNER JOIN Clientes cl ON c.IdCliente = cl.IdCliente
                                    WHERE c.IdCita NOT IN (SELECT IdCita FROM Pagos)
                                    AND c.Estado IN ('programada', 'completada')
                                    ORDER BY c.Fecha DESC, c.HoraInicio";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            citas.Add(new CitaParaPago
                            {
                                IdCita = reader.GetInt32(0),
                                NombreCliente = reader.GetString(1),
                                Fecha = reader.GetDateTime(2),
                                HoraInicio = reader.GetTimeSpan(3),
                                Estado = reader.GetString(4)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener citas sin pagar: {ex.Message}");
            }

            return citas;
        }

        public async Task<decimal> CalcularMontoCitaAsync(int idCita)
        {
            decimal monto = 0;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT SUM(s.Precio) AS Total
                                    FROM Cita_Servicio cs
                                    INNER JOIN Servicios s ON cs.IdServicio = s.IdServicio
                                    WHERE cs.IdCita = @IdCita";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdCita", idCita);
                        var result = await command.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            monto = Convert.ToDecimal(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular monto: {ex.Message}");
            }

            return monto;
        }

        public async Task<string> ObtenerDetallesCitaAsync(int idCita)
        {
            string detalles = "";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT s.Nombre, s.Precio
                                    FROM Cita_Servicio cs
                                    INNER JOIN Servicios s ON cs.IdServicio = s.IdServicio
                                    WHERE cs.IdCita = @IdCita";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdCita", idCita);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var servicios = new List<string>();
                            while (await reader.ReadAsync())
                            {
                                string nombre = reader.GetString(0);
                                decimal precio = reader.GetDecimal(1);
                                servicios.Add($"{nombre} (${precio:F2})");
                            }
                            detalles = string.Join("\n", servicios);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener detalles de cita: {ex.Message}");
            }

            return detalles;
        }

        public async Task<bool> AgregarAsync(Pago pago)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"INSERT INTO Pagos (IdCita, Monto, MetodoPago, FechaPago) 
                                   VALUES (@IdCita, @Monto, @MetodoPago, @FechaPago)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdCita", pago.IdCita);
                        command.Parameters.AddWithValue("@Monto", pago.Monto);
                        command.Parameters.AddWithValue("@MetodoPago", pago.MetodoPago);
                        command.Parameters.AddWithValue("@FechaPago", pago.FechaPago);

                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar pago: {ex.Message}");
            }
        }

        public async Task<bool> ActualizarAsync(Pago pago)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"UPDATE Pagos 
                                   SET IdCita=@IdCita, Monto=@Monto, MetodoPago=@MetodoPago, FechaPago=@FechaPago 
                                   WHERE IdPago=@IdPago";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdPago", pago.IdPago);
                        command.Parameters.AddWithValue("@IdCita", pago.IdCita);
                        command.Parameters.AddWithValue("@Monto", pago.Monto);
                        command.Parameters.AddWithValue("@MetodoPago", pago.MetodoPago);
                        command.Parameters.AddWithValue("@FechaPago", pago.FechaPago);

                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar pago: {ex.Message}");
            }
        }

        public async Task<bool> EliminarAsync(int idPago)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "DELETE FROM Pagos WHERE IdPago = @IdPago";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdPago", idPago);
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar pago: {ex.Message}");
            }
        }

        public async Task<List<Pago>> BuscarAsync(string termino)
        {
            var pagos = new List<Pago>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT p.IdPago, p.IdCita, p.Monto, p.MetodoPago, p.FechaPago,
                                           cl.Nombre + ' ' + cl.Paterno + ' ' + cl.Materno AS NombreCliente,
                                           c.Fecha AS FechaCita
                                    FROM Pagos p
                                    INNER JOIN Citas c ON p.IdCita = c.IdCita
                                    INNER JOIN Clientes cl ON c.IdCliente = cl.IdCliente
                                    WHERE cl.Nombre LIKE @Termino OR cl.Paterno LIKE @Termino 
                                       OR cl.Materno LIKE @Termino OR p.MetodoPago LIKE @Termino
                                    ORDER BY p.FechaPago DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Termino", $"%{termino}%");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                pagos.Add(new Pago
                                {
                                    IdPago = reader.GetInt32(0),
                                    IdCita = reader.GetInt32(1),
                                    Monto = reader.GetDecimal(2),
                                    MetodoPago = reader.GetString(3),
                                    FechaPago = reader.GetDateTime(4),
                                    NombreCliente = reader.GetString(5),
                                    FechaCita = reader.GetDateTime(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar pagos: {ex.Message}");
            }

            return pagos;
        }

        public async Task<List<Pago>> ObtenerPagosMesActualAsync()
        {
            var pagos = new List<Pago>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT p.IdPago, p.IdCita, p.Monto, p.MetodoPago, p.FechaPago,
                                           cl.Nombre + ' ' + cl.Paterno + ' ' + cl.Materno AS NombreCliente,
                                           c.Fecha AS FechaCita
                                    FROM Pagos p
                                    INNER JOIN Citas c ON p.IdCita = c.IdCita
                                    INNER JOIN Clientes cl ON c.IdCliente = cl.IdCliente
                                    WHERE YEAR(p.FechaPago) = YEAR(GETDATE()) 
                                      AND MONTH(p.FechaPago) = MONTH(GETDATE())
                                    ORDER BY p.FechaPago DESC";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            pagos.Add(new Pago
                            {
                                IdPago = reader.GetInt32(0),
                                IdCita = reader.GetInt32(1),
                                Monto = reader.GetDecimal(2),
                                MetodoPago = reader.GetString(3),
                                FechaPago = reader.GetDateTime(4),
                                NombreCliente = reader.GetString(5),
                                FechaCita = reader.GetDateTime(6)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener pagos del mes actual: {ex.Message}");
            }

            return pagos;
        }
    }

    public class Pago
    {
        public int IdPago { get; set; }
        public int IdCita { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; }
        public DateTime FechaPago { get; set; }

        public string NombreCliente { get; set; }
        public DateTime FechaCita { get; set; }
    }

    public class CitaParaPago
    {
        public int IdCita { get; set; }
        public string NombreCliente { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public string Estado { get; set; }

        public string DisplayText => $"Cita #{IdCita} - {NombreCliente} ({Fecha:dd/MM/yyyy})";
    }
}
