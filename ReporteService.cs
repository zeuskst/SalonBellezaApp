using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SalonBellezaApp
{
    public class ReporteService
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

        public async Task<List<ReporteIngreso>> ObtenerIngresosPorPeriodoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var ingresos = new List<ReporteIngreso>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT 
                                        CAST(p.FechaPago AS DATE) AS Fecha,
                                        COUNT(DISTINCT p.IdPago) AS TotalPagos,
                                        SUM(p.Monto) AS MontoTotal
                                    FROM Pagos p
                                    WHERE CAST(p.FechaPago AS DATE) >= @FechaInicio 
                                      AND CAST(p.FechaPago AS DATE) <= @FechaFin
                                    GROUP BY CAST(p.FechaPago AS DATE)
                                    ORDER BY Fecha DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ingresos.Add(new ReporteIngreso
                                {
                                    Fecha = reader.GetDateTime(0),
                                    TotalPagos = reader.GetInt32(1),
                                    MontoTotal = reader.GetDecimal(2)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener ingresos por período: {ex.Message}");
            }

            return ingresos;
        }

        public async Task<List<ReporteServicio>> ObtenerServiciosMasPopularesAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var servicios = new List<ReporteServicio>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT TOP 10
                                        s.IdServicio,
                                        s.Nombre,
                                        COUNT(cs.IdCita) AS TotalVentas,
                                        s.Precio,
                                        (COUNT(cs.IdCita) * s.Precio) AS MontoTotal
                                    FROM Servicios s
                                    INNER JOIN Cita_Servicio cs ON s.IdServicio = cs.IdServicio
                                    INNER JOIN Citas c ON cs.IdCita = c.IdCita
                                    WHERE CAST(c.Fecha AS DATE) >= @FechaInicio 
                                      AND CAST(c.Fecha AS DATE) <= @FechaFin
                                    GROUP BY s.IdServicio, s.Nombre, s.Precio
                                    ORDER BY TotalVentas DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                servicios.Add(new ReporteServicio
                                {
                                    IdServicio = reader.GetInt32(0),
                                    NombreServicio = reader.GetString(1),
                                    TotalVentas = reader.GetInt32(2),
                                    Precio = reader.GetDecimal(3),
                                    MontoTotal = reader.GetDecimal(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener servicios más populares: {ex.Message}");
            }

            return servicios;
        }

        public async Task<List<ReporteEmpleado>> ObtenerDesempenoEmpleadosAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var empleados = new List<ReporteEmpleado>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT 
                                        e.IdEmpleado,
                                        e.Nombre + ' ' + e.Paterno + ' ' + e.Materno AS NombreCompleto,
                                        COUNT(DISTINCT c.IdCita) AS TotalCitas,
                                        COUNT(DISTINCT CASE WHEN c.Estado = 'completada' THEN c.IdCita END) AS CitasCompletadas,
                                        ISNULL(SUM(p.Monto), 0) AS MontoTotal
                                    FROM Empleados e
                                    LEFT JOIN Citas c ON e.IdEmpleado = c.IdEmpleado
                                      AND CAST(c.Fecha AS DATE) >= @FechaInicio 
                                      AND CAST(c.Fecha AS DATE) <= @FechaFin
                                    LEFT JOIN Pagos p ON c.IdCita = p.IdCita
                                    GROUP BY e.IdEmpleado, e.Nombre, e.Paterno, e.Materno
                                    ORDER BY TotalCitas DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                empleados.Add(new ReporteEmpleado
                                {
                                    IdEmpleado = reader.GetInt32(0),
                                    NombreCompleto = reader.GetString(1),
                                    TotalCitas = reader.GetInt32(2),
                                    CitasCompletadas = reader.GetInt32(3),
                                    MontoTotal = reader.GetDecimal(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener desempeño de empleados: {ex.Message}");
            }

            return empleados;
        }

        public async Task<List<ReporteCliente>> ObtenerClientesFrecuentesAsync(DateTime fechaInicio, DateTime fechaFin, int top = 10)
        {
            var clientes = new List<ReporteCliente>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @$"SELECT TOP {top}
                                        cl.IdCliente,
                                        cl.Nombre + ' ' + cl.Paterno + ' ' + cl.Materno AS NombreCompleto,
                                        COUNT(DISTINCT c.IdCita) AS TotalCitas,
                                        ISNULL(SUM(p.Monto), 0) AS MontoGastado
                                    FROM Clientes cl
                                    INNER JOIN Citas c ON cl.IdCliente = c.IdCliente
                                    LEFT JOIN Pagos p ON c.IdCita = p.IdCita
                                    WHERE CAST(c.Fecha AS DATE) >= @FechaInicio 
                                      AND CAST(c.Fecha AS DATE) <= @FechaFin
                                    GROUP BY cl.IdCliente, cl.Nombre, cl.Paterno, cl.Materno
                                    ORDER BY TotalCitas DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                clientes.Add(new ReporteCliente
                                {
                                    IdCliente = reader.GetInt32(0),
                                    NombreCompleto = reader.GetString(1),
                                    TotalCitas = reader.GetInt32(2),
                                    MontoGastado = reader.GetDecimal(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener clientes frecuentes: {ex.Message}");
            }

            return clientes;
        }

        public async Task<List<ReporteMetodoPago>> ObtenerMetodosPagoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var metodos = new List<ReporteMetodoPago>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT 
                                        p.MetodoPago,
                                        COUNT(p.IdPago) AS TotalTransacciones,
                                        SUM(p.Monto) AS MontoTotal,
                                        AVG(p.Monto) AS MontoPromedio
                                    FROM Pagos p
                                    WHERE CAST(p.FechaPago AS DATE) >= @FechaInicio 
                                      AND CAST(p.FechaPago AS DATE) <= @FechaFin
                                    GROUP BY p.MetodoPago
                                    ORDER BY MontoTotal DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                metodos.Add(new ReporteMetodoPago
                                {
                                    MetodoPago = reader.GetString(0),
                                    TotalTransacciones = reader.GetInt32(1),
                                    MontoTotal = reader.GetDecimal(2),
                                    MontoPromedio = reader.GetDecimal(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener métodos de pago: {ex.Message}");
            }

            return metodos;
        }

        public async Task<ReporteGeneralAsync> ObtenerEstadisticasGeneralesAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var estadisticas = new ReporteGeneralAsync();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string queryIngresos = @"SELECT ISNULL(SUM(Monto), 0) 
                                            FROM Pagos 
                                            WHERE CAST(FechaPago AS DATE) >= @FechaInicio 
                                              AND CAST(FechaPago AS DATE) <= @FechaFin";

                    using (var command = new SqlCommand(queryIngresos, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);
                        estadisticas.IngresoTotal = (decimal)await command.ExecuteScalarAsync();
                    }

                    string queryCitas = @"SELECT COUNT(DISTINCT IdCita) 
                                         FROM Citas 
                                         WHERE CAST(Fecha AS DATE) >= @FechaInicio 
                                           AND CAST(Fecha AS DATE) <= @FechaFin";

                    using (var command = new SqlCommand(queryCitas, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);
                        estadisticas.TotalCitas = (int)await command.ExecuteScalarAsync();
                    }

                    string queryClientes = @"SELECT COUNT(DISTINCT IdCliente) 
                                           FROM Citas 
                                           WHERE CAST(Fecha AS DATE) >= @FechaInicio 
                                             AND CAST(Fecha AS DATE) <= @FechaFin";

                    using (var command = new SqlCommand(queryClientes, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);
                        estadisticas.TotalClientesAtendidos = (int)await command.ExecuteScalarAsync();
                    }

                    string queryPagos = @"SELECT COUNT(IdPago) 
                                         FROM Pagos 
                                         WHERE CAST(FechaPago AS DATE) >= @FechaInicio 
                                           AND CAST(FechaPago AS DATE) <= @FechaFin";

                    using (var command = new SqlCommand(queryPagos, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);
                        estadisticas.TotalPagos = (int)await command.ExecuteScalarAsync();
                    }

                    estadisticas.PromedioVenta = estadisticas.TotalPagos > 0 
                        ? estadisticas.IngresoTotal / estadisticas.TotalPagos 
                        : 0;

                    estadisticas.FechaInicio = fechaInicio;
                    estadisticas.FechaFin = fechaFin;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener estadísticas generales: {ex.Message}");
            }

            return estadisticas;
        }

        public async Task<List<ReporteCitaPorEstado>> ObtenerCitasPorEstadoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var reportes = new List<ReporteCitaPorEstado>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT 
                                        Estado,
                                        COUNT(IdCita) AS TotalCitas
                                    FROM Citas
                                    WHERE CAST(Fecha AS DATE) >= @FechaInicio 
                                      AND CAST(Fecha AS DATE) <= @FechaFin
                                    GROUP BY Estado
                                    ORDER BY TotalCitas DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                reportes.Add(new ReporteCitaPorEstado
                                {
                                    Estado = reader.GetString(0),
                                    TotalCitas = reader.GetInt32(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener citas por estado: {ex.Message}");
            }

            return reportes;
        }

        public async Task<List<ReporteProductoUsado>> ObtenerProductosMasUsadosAsync(DateTime fechaInicio, DateTime fechaFin, int top = 10)
        {
            var productos = new List<ReporteProductoUsado>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @$"SELECT TOP {top}
                                        p.IdProducto,
                                        p.Nombre,
                                        p.Categoria,
                                        COUNT(DISTINCT cs.IdServicio) AS ServiciosQueUsan,
                                        ISNULL(SUM(sp.Cantidad), 0) AS CantidadUsada,
                                        p.StockActual,
                                        p.StockMinimo
                                    FROM Productos p
                                    LEFT JOIN Servicio_Producto sp ON p.IdProducto = sp.IdProducto
                                    LEFT JOIN Cita_Servicio cs ON sp.IdServicio = cs.IdServicio
                                    LEFT JOIN Citas c ON cs.IdCita = c.IdCita
                                    WHERE CAST(c.Fecha AS DATE) >= @FechaInicio 
                                      AND CAST(c.Fecha AS DATE) <= @FechaFin
                                      AND c.IdCita IS NOT NULL
                                    GROUP BY p.IdProducto, p.Nombre, p.Categoria, p.StockActual, p.StockMinimo
                                    ORDER BY CantidadUsada DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                productos.Add(new ReporteProductoUsado
                                {
                                    IdProducto = reader.GetInt32(0),
                                    NombreProducto = reader.GetString(1),
                                    Categoria = reader.GetString(2),
                                    ServiciosQueUsan = reader.GetInt32(3),
                                    CantidadUsada = reader.GetDecimal(4),
                                    StockActual = reader.GetInt32(5),
                                    StockMinimo = reader.GetInt32(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener productos más usados: {ex.Message}");
            }

            return productos;
        }

        public async Task<List<ReporteTarjeta>> ObtenerResumenTarjetasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var tarjetas = new List<ReporteTarjeta>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT 
                                        ISNULL(NumeroTarjeta, 'SIN ESPECIFICAR') AS NumeroTarjeta,
                                        COUNT(IdPago) AS TotalTransacciones,
                                        SUM(Monto) AS MontoTotal,
                                        AVG(Monto) AS MontoPromedio,
                                        MIN(Monto) AS MontoMinimo,
                                        MAX(Monto) AS MontoMaximo
                                    FROM Pagos
                                    WHERE CAST(FechaPago AS DATE) >= @FechaInicio 
                                      AND CAST(FechaPago AS DATE) <= @FechaFin
                                      AND MetodoPago = 'Tarjeta'
                                    GROUP BY NumeroTarjeta
                                    ORDER BY MontoTotal DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tarjetas.Add(new ReporteTarjeta
                                {
                                    NumeroTarjeta = reader.GetString(0),
                                    TotalTransacciones = reader.GetInt32(1),
                                    MontoTotal = reader.GetDecimal(2),
                                    MontoPromedio = reader.GetDecimal(3),
                                    MontoMinimo = reader.GetDecimal(4),
                                    MontoMaximo = reader.GetDecimal(5)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener resumen de tarjetas: {ex.Message}");
            }

            return tarjetas;
        }

        public async Task<List<ReporteHorario>> ObtenerCitasPorHorarioAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var horarios = new List<ReporteHorario>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT 
                                        DATEPART(HOUR, Hora) AS Hora,
                                        COUNT(IdCita) AS TotalCitas,
                                        SUM(CASE WHEN Estado = 'completada' THEN 1 ELSE 0 END) AS CitasCompletadas,
                                        SUM(CASE WHEN Estado = 'cancelada' THEN 1 ELSE 0 END) AS CitasCanceladas,
                                        ISNULL(SUM(p.Monto), 0) AS MontoTotal
                                    FROM Citas c
                                    LEFT JOIN Pagos p ON c.IdCita = p.IdCita
                                    WHERE CAST(c.Fecha AS DATE) >= @FechaInicio 
                                      AND CAST(c.Fecha AS DATE) <= @FechaFin
                                    GROUP BY DATEPART(HOUR, Hora)
                                    ORDER BY Hora ASC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                horarios.Add(new ReporteHorario
                                {
                                    Hora = reader.GetInt32(0),
                                    TotalCitas = reader.GetInt32(1),
                                    CitasCompletadas = reader.GetInt32(2),
                                    CitasCanceladas = reader.GetInt32(3),
                                    MontoTotal = reader.GetDecimal(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener citas por horario: {ex.Message}");
            }

            return horarios;
        }

        public async Task<List<ReporteDiasSemana>> ObtenerCitasPorDiasSemanaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var dias = new List<ReporteDiasSemana>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT 
                                        DATENAME(WEEKDAY, Fecha) AS DiaSemana,
                                        DATEPART(WEEKDAY, Fecha) AS NumeroDia,
                                        COUNT(IdCita) AS TotalCitas,
                                        SUM(CASE WHEN Estado = 'completada' THEN 1 ELSE 0 END) AS CitasCompletadas,
                                        ISNULL(SUM(p.Monto), 0) AS MontoTotal,
                                        AVG(CAST(DATEDIFF(MINUTE, Hora, Hora) AS FLOAT)) AS DuracionPromedio
                                    FROM Citas c
                                    LEFT JOIN Pagos p ON c.IdCita = p.IdCita
                                    WHERE CAST(c.Fecha AS DATE) >= @FechaInicio 
                                      AND CAST(c.Fecha AS DATE) <= @FechaFin
                                    GROUP BY DATENAME(WEEKDAY, Fecha), DATEPART(WEEKDAY, Fecha)
                                    ORDER BY NumeroDia ASC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                dias.Add(new ReporteDiasSemana
                                {
                                    DiaSemana = reader.GetString(0),
                                    NumeroDia = reader.GetInt32(1),
                                    TotalCitas = reader.GetInt32(2),
                                    CitasCompletadas = reader.GetInt32(3),
                                    MontoTotal = reader.GetDecimal(4),
                                    DuracionPromedio = reader.IsDBNull(5) ? 0 : reader.GetDouble(5)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener citas por días de semana: {ex.Message}");
            }

            return dias;
        }

        public async Task<List<ReporteCancelacion>> ObtenerRazonesCanelacionAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var cancelaciones = new List<ReporteCancelacion>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT 
                                        ISNULL(Notas, 'SIN MOTIVO ESPECIFICADO') AS Motivo,
                                        COUNT(IdCita) AS TotalCancelaciones,
                                        COUNT(DISTINCT IdCliente) AS ClientesAfectados
                                    FROM Citas
                                    WHERE Estado = 'cancelada'
                                      AND CAST(Fecha AS DATE) >= @FechaInicio 
                                      AND CAST(Fecha AS DATE) <= @FechaFin
                                    GROUP BY Notas
                                    ORDER BY TotalCancelaciones DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                cancelaciones.Add(new ReporteCancelacion
                                {
                                    Motivo = reader.GetString(0),
                                    TotalCancelaciones = reader.GetInt32(1),
                                    ClientesAfectados = reader.GetInt32(2)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener razones de cancelación: {ex.Message}");
            }

            return cancelaciones;
        }

        public async Task<ReporteComparisonPeriodos> ObtenerComparacionPeriodosAsync(DateTime periodo1Inicio, DateTime periodo1Fin, DateTime periodo2Inicio, DateTime periodo2Fin)
        {
            var comparacion = new ReporteComparisonPeriodos();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Período 1
                    string query1 = @"SELECT 
                                        ISNULL(SUM(Monto), 0) AS IngresoTotal,
                                        COUNT(DISTINCT IdPago) AS TotalPagos,
                                        COUNT(DISTINCT IdCita) AS TotalCitas
                                    FROM Pagos p
                                    INNER JOIN Citas c ON p.IdCita = c.IdCita
                                    WHERE CAST(p.FechaPago AS DATE) >= @FechaInicio1 
                                      AND CAST(p.FechaPago AS DATE) <= @FechaFin1";

                    using (var command = new SqlCommand(query1, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio1", periodo1Inicio.Date);
                        command.Parameters.AddWithValue("@FechaFin1", periodo1Fin.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                comparacion.Periodo1 = new ReportePeriodo
                                {
                                    FechaInicio = periodo1Inicio,
                                    FechaFin = periodo1Fin,
                                    IngresoTotal = reader.GetDecimal(0),
                                    TotalPagos = reader.GetInt32(1),
                                    TotalCitas = reader.GetInt32(2)
                                };
                            }
                        }
                    }

                    // Período 2
                    string query2 = @"SELECT 
                                        ISNULL(SUM(Monto), 0) AS IngresoTotal,
                                        COUNT(DISTINCT IdPago) AS TotalPagos,
                                        COUNT(DISTINCT IdCita) AS TotalCitas
                                    FROM Pagos p
                                    INNER JOIN Citas c ON p.IdCita = c.IdCita
                                    WHERE CAST(p.FechaPago AS DATE) >= @FechaInicio2 
                                      AND CAST(p.FechaPago AS DATE) <= @FechaFin2";

                    using (var command = new SqlCommand(query2, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio2", periodo2Inicio.Date);
                        command.Parameters.AddWithValue("@FechaFin2", periodo2Fin.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                comparacion.Periodo2 = new ReportePeriodo
                                {
                                    FechaInicio = periodo2Inicio,
                                    FechaFin = periodo2Fin,
                                    IngresoTotal = reader.GetDecimal(0),
                                    TotalPagos = reader.GetInt32(1),
                                    TotalCitas = reader.GetInt32(2)
                                };
                            }
                        }
                    }

                    // Calcular variaciones
                    if (comparacion.Periodo1 != null && comparacion.Periodo2 != null)
                    {
                        comparacion.VariacionIngreso = comparacion.Periodo2.IngresoTotal - comparacion.Periodo1.IngresoTotal;
                        comparacion.VariacionCitas = comparacion.Periodo2.TotalCitas - comparacion.Periodo1.TotalCitas;
                        comparacion.PorcentajeVariacionIngreso = comparacion.Periodo1.IngresoTotal > 0 
                            ? (comparacion.VariacionIngreso / comparacion.Periodo1.IngresoTotal) * 100 
                            : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener comparación de períodos: {ex.Message}");
            }

            return comparacion;
        }

        // NUEVOS REPORTES
        public async Task<List<ReporteIngresosEmpleado>> ObtenerIngresosPorEmpleadoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var resultados = new List<ReporteIngresosEmpleado>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT 
                                        e.IdEmpleado,
                                        e.Nombre + ' ' + e.Paterno + ' ' + e.Materno AS NombreCompleto,
                                        COUNT(DISTINCT c.IdCita) AS TotalCitas,
                                        ISNULL(SUM(p.Monto), 0) AS MontoTotal
                                    FROM Empleados e
                                    LEFT JOIN Citas c ON e.IdEmpleado = c.IdEmpleado
                                      AND CAST(c.Fecha AS DATE) >= @FechaInicio
                                      AND CAST(c.Fecha AS DATE) <= @FechaFin
                                    LEFT JOIN Pagos p ON c.IdCita = p.IdCita
                                    GROUP BY e.IdEmpleado, e.Nombre, e.Paterno, e.Materno
                                    ORDER BY MontoTotal DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                        command.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                resultados.Add(new ReporteIngresosEmpleado
                                {
                                    IdEmpleado = reader.GetInt32(0),
                                    NombreCompleto = reader.GetString(1),
                                    TotalCitas = reader.GetInt32(2),
                                    MontoTotal = reader.GetDecimal(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener ingresos por empleado: {ex.Message}");
            }

            return resultados;
        }

        public async Task<List<ReporteCategoriaStock>> ObtenerStockPorCategoriaAsync()
        {
            var categorias = new List<ReporteCategoriaStock>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT 
                                        ISNULL(Categoria, 'SIN CATEGORIA') AS Categoria,
                                        COUNT(*) AS TotalProductos,
                                        ISNULL(SUM(StockActual), 0) AS StockTotal,
                                        ISNULL(SUM(CASE WHEN StockActual <= StockMinimo THEN 1 ELSE 0 END), 0) AS ProductosBajoStock
                                    FROM Productos
                                    WHERE Estado = 'activo'
                                    GROUP BY ISNULL(Categoria, 'SIN CATEGORIA')
                                    ORDER BY StockTotal DESC";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categorias.Add(new ReporteCategoriaStock
                            {
                                Categoria = reader.GetString(0),
                                TotalProductos = reader.GetInt32(1),
                                StockTotal = reader.GetInt32(2),
                                ProductosBajoStock = reader.GetInt32(3)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener stock por categoría: {ex.Message}");
            }

            return categorias;
        }

        public async Task<List<ReporteMovimientoInventario>> ObtenerMovimientosInventarioAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var movimientos = new List<ReporteMovimientoInventario>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"SELECT 
                                        m.IdMovimiento,
                                        m.IdProducto,
                                        ISNULL(p.Nombre, '') AS NombreProducto,
                                        ISNULL(m.TipoMovimiento, '') AS TipoMovimiento,
                                        ISNULL(m.Cantidad, 0) AS Cantidad,
                                        ISNULL(m.Motivo, '') AS Motivo,
                                        m.FechaMovimiento,
                                        ISNULL(m.Usuario, 'Sistema') AS Usuario
                                    FROM MovimientosInventario m
                                    LEFT JOIN Productos p ON m.IdProducto = p.IdProducto
                                    WHERE 1=1";

                    if (fechaInicio.HasValue)
                        query += " AND CAST(m.FechaMovimiento AS DATE) >= @FechaInicio";
                    if (fechaFin.HasValue)
                        query += " AND CAST(m.FechaMovimiento AS DATE) <= @FechaFin";

                    query += " ORDER BY m.FechaMovimiento DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        if (fechaInicio.HasValue) command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Value.Date);
                        if (fechaFin.HasValue) command.Parameters.AddWithValue("@FechaFin", fechaFin.Value.Date);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                movimientos.Add(new ReporteMovimientoInventario
                                {
                                    IdMovimiento = reader.GetInt32(0),
                                    IdProducto = reader.GetInt32(1),
                                    NombreProducto = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                    TipoMovimiento = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                    Cantidad = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                                    Motivo = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                    FechaMovimiento = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                    Usuario = reader.IsDBNull(7) ? "Sistema" : reader.GetString(7)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener movimientos de inventario: {ex.Message}");
            }

            return movimientos;
        }
    }

    public class ReporteIngreso
    {
        public DateTime Fecha { get; set; }
        public int TotalPagos { get; set; }
        public decimal MontoTotal { get; set; }
    }

    public class ReporteServicio
    {
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; }
        public int TotalVentas { get; set; }
        public decimal Precio { get; set; }
        public decimal MontoTotal { get; set; }
    }

    public class ReporteEmpleado
    {
        public int IdEmpleado { get; set; }
        public string NombreCompleto { get; set; }
        public int TotalCitas { get; set; }
        public int CitasCompletadas { get; set; }
        public decimal MontoTotal { get; set; }
    }

    public class ReporteCliente
    {
        public int IdCliente { get; set; }
        public string NombreCompleto { get; set; }
        public int TotalCitas { get; set; }
        public decimal MontoGastado { get; set; }
    }

    public class ReporteMetodoPago
    {
        public string MetodoPago { get; set; }
        public int TotalTransacciones { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoPromedio { get; set; }
    }

    public class ReporteGeneralAsync
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal IngresoTotal { get; set; }
        public int TotalCitas { get; set; }
        public int TotalClientesAtendidos { get; set; }
        public int TotalPagos { get; set; }
        public decimal PromedioVenta { get; set; }
    }

    public class ReporteCitaPorEstado
    {
        public string Estado { get; set; }
        public int TotalCitas { get; set; }
    }

    public class ReporteProductoUsado
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public string Categoria { get; set; }
        public int ServiciosQueUsan { get; set; }
        public decimal CantidadUsada { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
    }

    public class ReporteTarjeta
    {
        public string NumeroTarjeta { get; set; }
        public int TotalTransacciones { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoPromedio { get; set; }
        public decimal MontoMinimo { get; set; }
        public decimal MontoMaximo { get; set; }
    }

    public class ReporteHorario
    {
        public int Hora { get; set; }
        public int TotalCitas { get; set; }
        public int CitasCompletadas { get; set; }
        public int CitasCanceladas { get; set; }
        public decimal MontoTotal { get; set; }
    }

    public class ReporteDiasSemana
    {
        public string DiaSemana { get; set; }
        public int NumeroDia { get; set; }
        public int TotalCitas { get; set; }
        public int CitasCompletadas { get; set; }
        public decimal MontoTotal { get; set; }
        public double DuracionPromedio { get; set; }
    }

    public class ReporteCancelacion
    {
        public string Motivo { get; set; }
        public int TotalCancelaciones { get; set; }
        public int ClientesAfectados { get; set; }
    }

    public class ReporteComparisonPeriodos
    {
        public ReportePeriodo Periodo1 { get; set; }
        public ReportePeriodo Periodo2 { get; set; }
        public decimal VariacionIngreso { get; set; }
        public int VariacionCitas { get; set; }
        public decimal PorcentajeVariacionIngreso { get; set; }
    }

    public class ReportePeriodo
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal IngresoTotal { get; set; }
        public int TotalPagos { get; set; }
        public int TotalCitas { get; set; }
    }

    // Nuevos modelos de reportes
    public class ReporteIngresosEmpleado
    {
        public int IdEmpleado { get; set; }
        public string NombreCompleto { get; set; }
        public int TotalCitas { get; set; }
        public decimal MontoTotal { get; set; }
    }

    public class ReporteCategoriaStock
    {
        public string Categoria { get; set; }
        public int TotalProductos { get; set; }
        public int StockTotal { get; set; }
        public int ProductosBajoStock { get; set; }
    }

    public class ReporteMovimientoInventario
    {
        public int IdMovimiento { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public string TipoMovimiento { get; set; }
        public decimal Cantidad { get; set; }
        public string Motivo { get; set; }
        public DateTime? FechaMovimiento { get; set; }
        public string Usuario { get; set; }
    }
}
