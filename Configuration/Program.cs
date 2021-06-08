using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autofac;
using Autofac.Configuration;
using Autofac.Core;
using Autofac.Features.Indexed;
using Autofac.Features.Metadata;
using Autofac.Features.OwnedInstances;
using Microsoft.Extensions.Configuration;
using Module = Autofac.Module;


namespace Configuration
{
    public interface IVehicle
    {
        void Go();
    }
    class Truck : IVehicle
    {
        private IDriver driver;

        public Truck(IDriver driver)
        {
            if (driver == null)
            {
                throw new ArgumentNullException(paramName: nameof(driver));
            }
            this.driver = driver;
        }

        public void Go()
        {
            driver.Drive();
        }
    }
    public interface IDriver
    {
        void Drive();
    }
    class CrazyDriver : IDriver
    {
        public void Drive()
        {
            Console.WriteLine("Going too fast and crashing into a tree");
        }
    }
    public class SaneDriver : IDriver
    {
        public void Drive()
        {
            Console.WriteLine("Driving safely to destination");
        }
    }
    public class TransporterModule:Module
    {
        public bool ObeySpeedLimit { get; set; }
        protected override void Load(ContainerBuilder builder)
        {
            if (ObeySpeedLimit)
                builder.RegisterType<CrazyDriver>().As<IDriver>();
            else
                builder.RegisterType<SaneDriver>().As<IDriver>();

            builder.RegisterType<Truck>().As<IVehicle>();
        }
    }



    public interface IOperation
    {
        float Calculate(float a, float b);
    }
    public interface IOtherOperation
    {
        float SumValue(float a, float b);
    }

    public class Addition : IOperation,IOtherOperation
    {
        public float Calculate(float a, float b)
        {
            return a + b;
        }

        public float SumValue(float a, float b)
        {
            return (a + b) / 2 * 100;
        }
    }

    public class Multiplication : IOperation,IOtherOperation
    {
        public float Calculate(float a, float b)
        {
            return a * b;
        }

        public float SumValue(float a, float b)
        {
            return (a + b) / 2 * 100;
        }
    }

    public class Division : IOperation
    {
        public float Calculate(float a, float b)
        {
            return a * b;
        }
    }


    public class CalculationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Multiplication>().As<IOperation>();
            builder.RegisterType<Addition>().As<IOperation>();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            // Modules
            //var builder = new ContainerBuilder(); 
            //builder.RegisterModule(new TransporterModule { 
            //    ObeySpeedLimit = true
            //});

            //var container = builder.Build();

            //var car = container.Resolve<IVehicle>();

            //car.Go();


            // Config Json File



            var configBuilder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              //.AddJsonFile("config.json");
              .AddJsonFile("ConfigModule.json");
            var configuration = configBuilder.Build();

            var containerBuilder = new ContainerBuilder();
            var configModule = new ConfigurationModule(configuration);
            containerBuilder.RegisterModule(configModule);

            using (var container = containerBuilder.Build())
            {
                float a = 3, b = 4;

                foreach (IOperation op in container.Resolve<IList<IOperation>>())
                {
                    Console.WriteLine($"{op.GetType().Name} of {a} and {b} = {op.Calculate(a, b)}");
                }
            }
        }
    }
}
