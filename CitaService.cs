using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SalonBellezaApp
{
    public class CitaService
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

        public async Task<List<Cita>> ObtenerTodosAsync()
        {
            var citas = new List<Cita>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT DISTINCT c.IdCita, c.IdCliente, c.IdEmpleado, 
                                           c.Fecha, c.HoraInicio, c.Estado,
                                           cl.Nombre + ' ' + cl.Paterno + ' ' + cl.Materno AS NombreCliente,
                                           e.Nombre + ' ' + e.Paterno + ' ' + e.Materno AS NombreEmpleado
                                    FROM Citas c
                                    INNER JOIN Clientes cl ON c.IdCliente = cl.IdCliente
                                    INNER JOIN Empleados e ON c.IdEmpleado = e.IdEmpleado
                                    ORDER BY c.Fecha DESC, c.HoraInicio DESC";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var cita = new Cita
                            {
                                IdCita = reader.GetInt32(0),
                                IdCliente = reader.GetInt32(1),
                                IdEmpleado = reader.GetInt32(2),
                                Fecha = reader.GetDateTime(3),
                                HoraInicio = reader.GetTimeSpan(4),
                                Estado = reader.GetString(5),
                                NombreCliente = reader.GetString(6),
                                NombreEmpleado = reader.GetString(7)
                            };

                            cita.Servicios = await ObtenerServiciosDeCitaAsync(cita.IdCita);
                            cita.NombreServicios = string.Join(", ", cita.Servicios.ConvertAll(s => s.Nombre));

                            citas.Add(cita);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener citas: {ex.Message}");
            }

            return citas;
        }

        public async Task<List<ServicioSimple>> ObtenerServiciosDeCitaAsync(int idCita)
        {
            var servicios = new List<ServicioSimple>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT s.IdServicio, s.Nombre
                                    FROM Servicios s
                                    INNER JOIN Cita_Servicio cs ON s.IdServicio = cs.IdServicio
                                    WHERE cs.IdCita = @IdCita";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdCita", idCita);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                servicios.Add(new ServicioSimple
                                {
                                    IdServicio = reader.GetInt32(0),
                                    Nombre = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener servicios de cita: {ex.Message}");
            }

            return servicios;
        }

        public async Task<List<Cita>> ObtenerCitasDelDiaAsync()
        {
            var citas = new List<Cita>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT DISTINCT c.IdCita, c.IdCliente, c.IdEmpleado, 
                                           c.Fecha, c.HoraInicio, c.Estado,
                                           cl.Nombre + ' ' + cl.Paterno + ' ' + cl.Materno AS NombreCliente,
                                           e.Nombre + ' ' + e.Paterno + ' ' + e.Materno AS NombreEmpleado
                                    FROM Citas c
                                    INNER JOIN Clientes cl ON c.IdCliente = cl.IdCliente
                                    INNER JOIN Empleados e ON c.IdEmpleado = e.IdEmpleado
                                    WHERE CAST(c.Fecha AS DATE) = CAST(GETDATE() AS DATE)
                                    AND c.Estado = 'programada'
                                    ORDER BY c.HoraInicio";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var cita = new Cita
                            {
                                IdCita = reader.GetInt32(0),
                                IdCliente = reader.GetInt32(1),
                                IdEmpleado = reader.GetInt32(2),
                                Fecha = reader.GetDateTime(3),
                                HoraInicio = reader.GetTimeSpan(4),
                                Estado = reader.GetString(5),
                                NombreCliente = reader.GetString(6),
                                NombreEmpleado = reader.GetString(7)
                            };

                            cita.Servicios = await ObtenerServiciosDeCitaAsync(cita.IdCita);
                            cita.NombreServicios = string.Join(", ", cita.Servicios.ConvertAll(s => s.Nombre));

                            citas.Add(cita);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener citas del día: {ex.Message}");
            }

            return citas;
        }

        public async Task<int> AgregarAsync(Cita cita, List<int> idsServicios)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string queryCita = @"INSERT INTO Citas (IdCliente, IdEmpleado, Fecha, HoraInicio, Estado) 
                                               OUTPUT INSERTED.IdCita
                                               VALUES (@IdCliente, @IdEmpleado, @Fecha, @HoraInicio, @Estado)";

                            int idCita;
                            using (var command = new SqlCommand(queryCita, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@IdCliente", cita.IdCliente);
                                command.Parameters.AddWithValue("@IdEmpleado", cita.IdEmpleado);
                                command.Parameters.AddWithValue("@Fecha", cita.Fecha.Date);
                                command.Parameters.AddWithValue("@HoraInicio", cita.HoraInicio);
                                command.Parameters.AddWithValue("@Estado", cita.Estado);

                                idCita = (int)await command.ExecuteScalarAsync();
                            }

                            string queryServicio = @"INSERT INTO Cita_Servicio (IdCita, IdServicio) 
                                                   VALUES (@IdCita, @IdServicio)";

                            foreach (int idServicio in idsServicios)
                            {
                                using (var command = new SqlCommand(queryServicio, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@IdCita", idCita);
                                    command.Parameters.AddWithValue("@IdServicio", idServicio);
                                    await command.ExecuteNonQueryAsync();
                                }
                            }

                            transaction.Commit();
                            return idCita;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar cita: {ex.Message}");
            }
        }

        public async Task<bool> ActualizarAsync(Cita cita, List<int> idsServicios)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string queryCita = @"UPDATE Citas 
                                               SET IdCliente=@IdCliente, IdEmpleado=@IdEmpleado,
                                                   Fecha=@Fecha, HoraInicio=@HoraInicio, Estado=@Estado 
                                               WHERE IdCita=@IdCita";

                            using (var command = new SqlCommand(queryCita, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@IdCita", cita.IdCita);
                                command.Parameters.AddWithValue("@IdCliente", cita.IdCliente);
                                command.Parameters.AddWithValue("@IdEmpleado", cita.IdEmpleado);
                                command.Parameters.AddWithValue("@Fecha", cita.Fecha.Date);
                                command.Parameters.AddWithValue("@HoraInicio", cita.HoraInicio);
                                command.Parameters.AddWithValue("@Estado", cita.Estado);

                                await command.ExecuteNonQueryAsync();
                            }

                            string queryEliminar = "DELETE FROM Cita_Servicio WHERE IdCita = @IdCita";
                            using (var command = new SqlCommand(queryEliminar, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@IdCita", cita.IdCita);
                                await command.ExecuteNonQueryAsync();
                            }

                            string queryServicio = @"INSERT INTO Cita_Servicio (IdCita, IdServicio) 
                                                   VALUES (@IdCita, @IdServicio)";

                            foreach (int idServicio in idsServicios)
                            {
                                using (var command = new SqlCommand(queryServicio, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@IdCita", cita.IdCita);
                                    command.Parameters.AddWithValue("@IdServicio", idServicio);
                                    await command.ExecuteNonQueryAsync();
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar cita: {ex.Message}");
            }
        }

        public async Task<bool> EliminarAsync(int idCita)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = "DELETE FROM Citas WHERE IdCita = @IdCita";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdCita", idCita);
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar cita: {ex.Message}");
            }
        }

        public async Task<List<Cita>> BuscarAsync(string termino)
        {
            var citas = new List<Cita>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT DISTINCT c.IdCita, c.IdCliente, c.IdEmpleado, 
                                           c.Fecha, c.HoraInicio, c.Estado,
                                           cl.Nombre + ' ' + cl.Paterno + ' ' + cl.Materno AS NombreCliente,
                                           e.Nombre + ' ' + e.Paterno + ' ' + e.Materno AS NombreEmpleado
                                    FROM Citas c
                                    INNER JOIN Clientes cl ON c.IdCliente = cl.IdCliente
                                    INNER JOIN Empleados e ON c.IdEmpleado = e.IdEmpleado
                                    WHERE cl.Nombre LIKE @Termino OR cl.Paterno LIKE @Termino OR cl.Materno LIKE @Termino
                                       OR e.Nombre LIKE @Termino OR e.Paterno LIKE @Termino OR e.Materno LIKE @Termino
                                    ORDER BY c.Fecha DESC, c.HoraInicio DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Termino", $"%{termino}%");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var cita = new Cita
                                {
                                    IdCita = reader.GetInt32(0),
                                    IdCliente = reader.GetInt32(1),
                                    IdEmpleado = reader.GetInt32(2),
                                    Fecha = reader.GetDateTime(3),
                                    HoraInicio = reader.GetTimeSpan(4),
                                    Estado = reader.GetString(5),
                                    NombreCliente = reader.GetString(6),
                                    NombreEmpleado = reader.GetString(7)
                                };

                                cita.Servicios = await ObtenerServiciosDeCitaAsync(cita.IdCita);
                                cita.NombreServicios = string.Join(", ", cita.Servicios.ConvertAll(s => s.Nombre));

                                citas.Add(cita);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar citas: {ex.Message}");
            }

            return citas;
        }

        public async Task<List<ClienteSimple>> ObtenerClientesAsync()
        {
            var clientes = new List<ClienteSimple>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT IdCliente, Nombre + ' ' + Paterno + ' ' + Materno AS NombreCompleto FROM Clientes ORDER BY Nombre";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            clientes.Add(new ClienteSimple
                            {
                                IdCliente = reader.GetInt32(0),
                                NombreCompleto = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener clientes: {ex.Message}");
            }

            return clientes;
        }

        public async Task<List<ServicioSimple>> ObtenerServiciosAsync()
        {
            var servicios = new List<ServicioSimple>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT IdServicio, Nombre FROM Servicios ORDER BY Nombre";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            servicios.Add(new ServicioSimple
                            {
                                IdServicio = reader.GetInt32(0),
                                Nombre = reader.GetString(1)
                            });
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

        public async Task<List<EmpleadoSimple>> ObtenerEmpleadosAsync()
        {
            var empleados = new List<EmpleadoSimple>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT IdEmpleado, Nombre + ' ' + Paterno + ' ' + Materno AS NombreCompleto FROM Empleados ORDER BY Nombre";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            empleados.Add(new EmpleadoSimple
                            {
                                IdEmpleado = reader.GetInt32(0),
                                NombreCompleto = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener empleados: {ex.Message}");
            }

            return empleados;
        }
    }

    public class Cita
    {
        public int IdCita { get; set; }
        public int IdCliente { get; set; }
        public int IdEmpleado { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public string Estado { get; set; }

        public string NombreCliente { get; set; }
        public string NombreEmpleado { get; set; }
        public List<ServicioSimple> Servicios { get; set; } = new List<ServicioSimple>();
        public string NombreServicios { get; set; } 
    }

    public class ClienteSimple
    {
        public int IdCliente { get; set; }
        public string NombreCompleto { get; set; }
    }

    public class ServicioSimple
    {
        public int IdServicio { get; set; }
        public string Nombre { get; set; }
    }

    public class EmpleadoSimple
    {
        public int IdEmpleado { get; set; }
        public string NombreCompleto { get; set; }
    }
}
