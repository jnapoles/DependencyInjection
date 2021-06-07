using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoFacSample
{

    public interface ILog
    {
        void Write(string message);
    }
    public interface IConsole
    {

    }
    public class ConsoleLog : ILog, IConsole
    {
        public void Write(string message)
        {
            Console.WriteLine(message);
        }
    }
    public class EmailLog : ILog
    {
        private const string adminEmail = "admin@foo.com";

        public void Write(string message)
        {
            Console.WriteLine($"Email sent to {adminEmail} : {message}");
        }
    }
    public class SMSLog : ILog
    {
        private string phoneNumber;

        public SMSLog(string phoneNumber)
        {
            this.phoneNumber = phoneNumber;
        }

        public void Write(string message)
        {
            Console.WriteLine($"SMS to {phoneNumber} : {message}");
        }
    }
    public class Engine
    {
        private ILog log;
        private int id;

        public Engine(ILog log)
        {
            this.log = log;
            id = new Random().Next();
        }
        public Engine(ILog log, int id)
        {
            this.log = log;
            this.id = id;
        }

        public void Ahead(int power)
        {
            log.Write($"Engine [{id}] ahead {power}");
        }
    }
    public class Car
    {
        private Engine engine;
        private ILog log;

        public Car(Engine engine)
        {
            this.engine = engine;
            this.log = new EmailLog();
        }
        public Car(Engine engine, ILog log)
        {
            this.engine = engine;
            this.log = log;
        }

        public void Go()
        {
            engine.Ahead(100);
            log.Write("Car going forward...");
        }
    }
    public class Parent
    {
        public override string ToString()
        {
            return "I am your father";
        }
    }
    public class Child
    {
        public string Name { get; set; }
        public Parent Parent { get; set; }

        public void SetParent(Parent parent)
        {
            Parent = parent;
        }
    }

    public class ParentChildModule: Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Parent>();
            builder.Register(c => new Child
            {
                Parent = c.Resolve<Parent>()
            });
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            // Withoud DI
            //var log = new ConsoleLog();
            //var engine = new Engine(log);
            //var car = new Car(engine, log);
            //car.Go();



            // With DI
            //var builder = new ContainerBuilder();
            //builder.RegisterType<ConsoleLog>().As<ILog>().AsSelf();
            //builder.RegisterType<Engine>();
            //builder.RegisterType<Car>();

            //var container = builder.Build();

            //var log = container.Resolve<ConsoleLog>();

            //var car = container.Resolve<Car>();

            //car.Go();


            // Default Registration
            //var builder = new ContainerBuilder();
            //builder.RegisterType<EmailLog>()
            //  .As<ILog>();
            //builder.RegisterType<ConsoleLog>()
            //       .As<ILog>()

            //       .As<IConsole>()
            //       .PreserveExistingDefaults();
            //builder.RegisterType<Engine>();
            //builder.RegisterType<Car>();

            //IContainer container = builder.Build();

            //var car = container.Resolve<Car>();
            //car.Go();


            // Choice of constructor
            //var builder = new ContainerBuilder();
            //builder.RegisterType<ConsoleLog>().As<ILog>();
            //builder.RegisterType<Engine>();
            //builder.RegisterType<Car>()
            //  .UsingConstructor(typeof(Engine));

            //IContainer container = builder.Build();

            //var car = container.Resolve<Car>();
            //car.Go();


            //Registration instance Components
            //var builder = new ContainerBuilder();
            ////builder.RegisterType<ConsoleLog>().As<ILog>();

            //var log = new ConsoleLog();
            //builder.RegisterInstance(log).As<ILog>();

            //builder.RegisterType<Engine>();
            //builder.RegisterType<Car>()
            //  .UsingConstructor(typeof(Engine));

            //IContainer container = builder.Build();

            //var car = container.Resolve<Car>();
            //car.Go();


            // Lambda registration
            //var builder = new ContainerBuilder();
            //builder.RegisterType<ConsoleLog>().As<ILog>();

            //builder.Register((IComponentContext c) =>
            //  new Engine(c.Resolve<ILog>(), 123));

            ////builder.RegisterType<Engine>();
            //builder.RegisterType<Car>();

            //IContainer container = builder.Build();

            //var car = container.Resolve<Car>();
            //car.Go();


            //Open Generic Components
            //var builder = new ContainerBuilder();

            //// IList<T> --> List<T>
            //// IList<int> --> List<int>
            //builder.RegisterGeneric(typeof(List<>)).As(typeof(IList<>));

            //IContainer container = builder.Build();

            //var myList = container.Resolve<IList<int>>();
            //Console.WriteLine(myList.GetType());


            // Seccion 3 Advanced Registration
            //var builder = new ContainerBuilder();

            //// named parameter
            ////      builder.RegisterType<SMSLog>()
            ////        .As<ILog>()
            ////        .WithParameter("phoneNumber", "+12345678");

            //// typed parameter
            ////      builder.RegisterType<SMSLog>()
            ////        .As<ILog>()
            ////        .WithParameter(new TypedParameter(typeof(string), "+12345678"));

            //// resolved parameter
            ////      builder.RegisterType<SMSLog>()
            ////        .As<ILog>()
            ////        .WithParameter(
            ////          new ResolvedParameter(
            ////            // predicate
            ////            (pi, ctx) => pi.ParameterType == typeof(string) && pi.Name == "phoneNumber",
            ////            // value accessor
            ////            (pi, ctx) => "+12345678"
            ////          )
            ////        );
            ////
            ////
            //Random random = new Random();
            //builder.Register((c, p) => new SMSLog(p.Named<string>("phoneNumber")))
            //  .As<ILog>();

            //Console.WriteLine("About to build container...");
            //var container = builder.Build();

            //var log = container.Resolve<ILog>(new NamedParameter("phoneNumber", random.Next().ToString()));
            //log.Write("Testing");





            // Property and method injection
            //var builder = new ContainerBuilder();
            //builder.RegisterType<Parent>();

            // injection by property by default
            //builder.RegisterType<Child>().PropertiesAutowired();


            // injection by property
            //builder.RegisterType<Child>()
            //  .WithProperty("Parent", new Parent());


            // injection by method lambda expresion
            //builder.Register(c =>  
            //{
            //    var child = new Child();
            //    child.SetParent(c.Resolve<Parent>());
            //    return child;
            //});

            // injection by method lambda expresion
            //builder.RegisterType<Child>()
            //  .OnActivated((IActivatedEventArgs<Child> e) =>
            //  {
            //      var p = e.Context.Resolve<Parent>();
            //      e.Instance.SetParent(p);
            //  });

            //var container = builder.Build();
            //var parent = container.Resolve<Child>().Parent;
            //Console.WriteLine(parent);




            // Scanning for types
            //var assembly = Assembly.GetExecutingAssembly();
            //var builder = new ContainerBuilder();
            //builder.RegisterAssemblyTypes(assembly)
            //    .Where(t => t.Name.EndsWith("Log"))
            //    .Except<SMSLog>()
            //    .Except<ConsoleLog>(c => c.As<ILog>().SingleInstance())
            //    .AsSelf();

            ////builder.RegisterAssemblyTypes(assembly)                
            ////    .Except<SMSLog>()
            ////    .Where(t => t.Name.EndsWith("Log"))
            ////    .As(t => t.GetInterfaces()[0]);


            //var container = builder.Build();

            //var log = container.Resolve<ILog>();

            //log.Write("Hola");



            var builder = new ContainerBuilder();

            //builder.RegisterAssemblyModules(typeof(Program).Assembly);

            builder.RegisterAssemblyModules<ParentChildModule>(typeof(Program).Assembly);


            var container = builder.Build();

            Console.WriteLine(container.Resolve<Child>().Parent);

        }
    }
}
