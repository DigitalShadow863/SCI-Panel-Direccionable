using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace biblio
{
    public class Sensor
    {
        public int Id { get; set; }
        public string Tipo { get; set; }
        public int Zona { get; set; }
        public double Umbral { get; set; }
        public bool Activo { get; set; } = true;

        public Sensor(int id, string tipo, int zona, double umbral)
        {
            Id = id;
            Tipo = tipo;
            Zona = zona;
            Umbral = umbral;
        }

        public override string ToString()
        {
            return $"Sensor[ID={Id}, Tipo={Tipo}, Zona={Zona}, Umbral={Umbral}]";
        }
    }

    public class SensorEvent
    {
        public int SensorId { get; set; }
        public string Tipo { get; set; }
        public double Valor { get; set; }
        public DateTime Timestamp { get; set; }

        public SensorEvent(int sensorId, string tipo, double valor)
        {
            SensorId = sensorId;
            Tipo = tipo;
            Valor = valor;
            Timestamp = DateTime.Now;
        }
    }

    public static class Logger
    {
        private static readonly string logPath = "eventos_sci.log";

        public static void Info(string categoria, string mensaje)
        {
            Write("INFO", categoria, mensaje);
        }

        public static void Error(string categoria, string mensaje)
        {
            Write("ERROR", categoria, mensaje);
        }

        private static void Write(string nivel, string categoria, string mensaje)
        {
            string texto = $"{DateTime.Now:u} | {nivel} | {categoria} | {mensaje}";
            Console.WriteLine(texto);

            try { File.AppendAllText(logPath, texto + Environment.NewLine); }
            catch { Console.WriteLine("No se pudo escribir en el log."); }
        }
    }

    public class Notifier
    {
        public void NotificarMonitoreo(int sensorId, string tipo, double valor, string energia)
        {
            Logger.Info("NOTIF", $"Monitoreo => Sensor={sensorId}, Tipo={tipo}, Valor={valor}, Energia={energia}");
        }

        public void ActivarEstroboscopicas(int zona)
        {
            Logger.Info("SALIDA", $"Estroboscópicas activadas en zona {zona}");
        }

        public void ActivarSirena(int zona)
        {
            Logger.Info("SALIDA", $"Sirena activada en zona {zona}");
        }
    }

    public class EnergyManager
    {
        public enum Estado { PRINCIPAL, RESPALDO }
        public Estado EstadoActual { get; private set; } = Estado.PRINCIPAL;

        private readonly Notifier _notifier;

        public EnergyManager(Notifier notifier)
        {
            _notifier = notifier;
        }

        public void FallaPrincipal()
        {
            EstadoActual = Estado.RESPALDO;
            Logger.Info("ENERGIA", "Conmutación a RESPALDO");
            _notifier.NotificarMonitoreo(-1, "ENERGIA", 0, EstadoActual.ToString());
        }

        public void RestaurarPrincipal()
        {
            EstadoActual = Estado.PRINCIPAL;
            Logger.Info("ENERGIA", "Energía PRINCIPAL restaurada");
            _notifier.NotificarMonitoreo(-1, "ENERGIA", 0, EstadoActual.ToString());
        }

        public string ObtenerEstado() => EstadoActual.ToString();
    }

    public class SensorManager
    {
        private readonly Dictionary<int, Sensor> _sensores = new Dictionary<int, Sensor>();
        private readonly Notifier _notifier;
        private readonly EnergyManager _energia;

        public SensorManager(Notifier notifier, EnergyManager energia)
        {
            _notifier = notifier;
            _energia = energia;
        }

        public void RegistrarSensor(Sensor s)
        {
            if (_sensores.ContainsKey(s.Id))
            {
                Logger.Error("SENSOR", $"Sensor duplicado ID={s.Id}");
                return;
            }

            _sensores[s.Id] = s;
            Logger.Info("CONFIG", $"Registrado {s}");
        }

        public Sensor ObtenerSensor(int id)
        {
            _sensores.TryGetValue(id, out var sensor);
            return sensor;
        }

        public IReadOnlyCollection<Sensor> ListarSensores() => _sensores.Values;

        public void ProcesarLectura(SensorEvent ev)
        {
            Logger.Info("LECTURA", $"Lectura ID={ev.SensorId} Valor={ev.Valor}");

            var s = ObtenerSensor(ev.SensorId);
            if (s == null)
            {
                Logger.Error("SENSOR", $"No existe sensor con ID={ev.SensorId}");
                return;
            }

            if (ev.Valor >= s.Umbral)
            {
                Logger.Info("SENSOR", $"Valor supera umbral. Verificando...");

                Thread.Sleep(700); // simulación
                ConfirmarAlarma(s, ev);
            }
            else
            {
                Logger.Info("SENSOR", "Lectura normal.");
            }
        }

        private void ConfirmarAlarma(Sensor s, SensorEvent ev)
        {
            Logger.Info("ALARMA", $"ALERTA CONFIRMADA en sensor {s.Id}");

            _notifier.ActivarEstroboscopicas(s.Zona);
            _notifier.ActivarSirena(s.Zona);
            _notifier.NotificarMonitoreo(s.Id, s.Tipo, ev.Valor, _energia.ObtenerEstado());
        }
    }

    public class PanelController
    {
        public SensorManager SensorManager { get; }
        public Notifier Notificador { get; }
        public EnergyManager Energia { get; }

        public PanelController()
        {
            Notificador = new Notifier();
            Energia = new EnergyManager(Notificador);
            SensorManager = new SensorManager(Notificador, Energia);
        }

        public void RegistrarSensor(Sensor s) => SensorManager.RegistrarSensor(s);

        public void ProcesarEvento(SensorEvent ev) => SensorManager.ProcesarLectura(ev);

        public void SimularFallaEnergia() => Energia.FallaPrincipal();

        public void RestaurarEnergia() => Energia.RestaurarPrincipal();
    }

    public static class Config
    {
        public static List<Sensor> SensoresPredeterminados()
        {
            return new List<Sensor>()
            {
                new Sensor(101, "Humo", 1, 50),
                new Sensor(102, "Temp", 1, 70),
                new Sensor(201, "Humo", 2, 50),
                new Sensor(999, "Manual", 1, 1)
            };
        }
    }
}
