using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using biblio;

namespace proyecto
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("------GRUPO 7------");
            Console.WriteLine("Iniciando simulador SCI...");
            var panel = new PanelController();

            
            foreach (var s in Config.SensoresPredeterminados())
            {
                panel.RegistrarSensor(s);
            }

            bool salir = false;
            while (!salir)
            {
                Console.WriteLine("\nAcciones:");
                Console.WriteLine("1 - Simular lectura de sensor");
                Console.WriteLine("2 - Simular falla de energía principal");
                Console.WriteLine("3 - Restaurar energía principal");
                Console.WriteLine("4 - Listar sensores");
                Console.WriteLine("0 - Salir");
                Console.Write("Elige opción: ");
                var op = Console.ReadLine();
                switch (op)
                {
                    case "1":
                        SimularLectura(panel);
                        break;
                    case "2":
                        panel.SimularFallaEnergia();
                        break;
                    case "3":
                        panel.RestaurarEnergia();
                        break;
                    case "4":
                        ListarSensores(panel);
                        break;
                    case "0":
                        salir = true;
                        break;
                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }
            }

            Console.WriteLine("Simulador finalizado.");
        }

        private static void SimularLectura(PanelController panel)
        {
            Console.Write("Ingrese SensorID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("ID inválido.");
                return;
            }

            Console.Write("Ingrese valor (numérico): ");
            if (!double.TryParse(Console.ReadLine(), out double val))
            {
                Console.WriteLine("Valor inválido.");
                return;
            }

            var sensor = panel.SensorManager.ObtenerSensor(id);
            string tipo = sensor != null ? sensor.Tipo : "Desconocido";
            var ev = new SensorEvent(id, tipo, val);

           
            panel.ProcesarEvento(ev);

            
            if (sensor != null && val < 2 * sensor.Umbral && val >= sensor.Umbral)
            {
                Console.Write("¿Confirmar alarma manualmente? (s/n): ");
                var r = Console.ReadLine();
                if (r != null && r.Trim().ToLower() == "s")
                {
                    
                    panel.SensorManager.ProcesarLectura(new SensorEvent(id, tipo, val * 2)); 
                }
                else
                {
                    Console.WriteLine("Alarma no confirmada por operatoria.");
                }
            }
        }

        private static void ListarSensores(PanelController panel)
        {
            Console.WriteLine("Sensores registrados:");
            foreach (var s in panel.SensorManager.ListarSensores())
            {
                Console.WriteLine(s);
            }
        }
    }
}