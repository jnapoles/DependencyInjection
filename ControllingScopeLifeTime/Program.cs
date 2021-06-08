using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllingScopeLifeTime
{
    public interface ILog
    {
        void Write(string message);
    }
    public interface IConsole
    {

    }
    public class ConsoleLog : ILog, IConsole,IDisposable
    {
        public ConsoleLog()
        {
            Console.WriteLine("Creating a console log!");
        }

        public void Dispose()
        {
            Console.WriteLine("Destroyed a console log!");
        }

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
    public interface IResource
    {

    }
    class SingletonResource : IResource
    {
    }
    public class InstancePerDependencyResource : IResource, IDisposable
    {
        public InstancePerDependencyResource()
        {
            Console.WriteLine("Instance per dep created");
        }

        public void Dispose()
        {
            Console.WriteLine("Instance per dep destroyed");
        }
    }
    public class ResourceManager
    {
        public ResourceManager(IEnumerable<IResource> resources)
        {
            if (resources == null)
            {
                throw new ArgumentNullException(paramName: nameof(resources));
            }
            Resources = resources;
        }

        public IEnumerable<IResource> Resources { get; set; }
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
        public Child()
        {
            Console.WriteLine("Child being created");
        }
        public void SetParent(Parent parent)
        {
            Parent = parent;
        }
        public override string ToString()
        {
            return "Hi there";
        }
    }
    class BadChild : Child
    {
        public override string ToString()
        {
            return "I hate you";
        }
    }
    public class ParentChildModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Parent>();
            builder.Register(c => new Child() { Parent = c.Resolve<Parent>() });
        }
    }
    public class MyClass : IStartable
    {
        public MyClass()
        {
            Console.WriteLine("MyClass is created!!!!");
        }
        public void Start()
        {
            Console.WriteLine("MyClass is start!!!!!");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            // Default Instance
            //var builder = new ContainerBuilder();
            //builder.RegisterType<ConsoleLog>();

            //var container = builder.Build();

            //// 2 constructor, 1 by request
            //container.Resolve<ConsoleLog>().Write("Testing 1 !!!");
            //container.Resolve<ConsoleLog>().Write("Testing 2 !!!");


            // Single Instance
            //var builder = new ContainerBuilder();
            //builder.RegisterType<ConsoleLog>().SingleInstance();

            //var container = builder.Build();

            //container.Resolve<ConsoleLog>().Write("Testing 1 !!!");
            //container.Resolve<ConsoleLog>().Write("Testing 2 !!!");


            //By Scope 
            //var builder = new ContainerBuilder();
            //builder.RegisterType<ConsoleLog>().InstancePerLifetimeScope();

            //var container = builder.Build();

            //using (var scope1 = container.BeginLifetimeScope())
            //{
            //    for (int i = 0; i < 3; i++)
            //    {
            //        scope1.Resolve<ConsoleLog>().Write($"Testing {i} !!!");
            //    }
            //}

            //using (var scope2 = container.BeginLifetimeScope())
            //{
            //    for (int i = 0; i < 3; i++)
            //    {
            //        scope2.Resolve<ConsoleLog>().Write($"Testing {i} !!!");
            //    }
            //}


            //By Scope 
            //var builder = new ContainerBuilder();
            //builder.RegisterType<ConsoleLog>().InstancePerMatchingLifetimeScope("food");

            //var container = builder.Build();

            //using (var scope1 = container.BeginLifetimeScope("food"))
            //{
            //    for (int i = 0; i < 3; i++)
            //    {
            //        scope1.Resolve<ConsoleLog>().Write($"Testing {i} !!!");
            //    }


            //    using (var scope2 = scope1.BeginLifetimeScope())
            //    {
            //        for (int i = 0; i < 3; i++)
            //        {
            //            scope2.Resolve<ConsoleLog>().Write($"Testing {i} !!!");
            //        }
            //    }
            //}

            //using (var scope3 = container.BeginLifetimeScope("food"))
            //{
            //    for (int i = 0; i < 3; i++)
            //    {
            //        scope3.Resolve<ConsoleLog>().Write($"Testing {i} !!!");
            //    }
            //}



            // Captive Dependencies
            //var builder = new ContainerBuilder();
            //builder.RegisterType<ResourceManager>().SingleInstance();
            //builder.RegisterType<SingletonResource>().As<IResource>().SingleInstance();
            //builder.RegisterType<InstancePerDependencyResource>().As<IResource>();



            //using (var container = builder.Build())
            //{
            //    using (var scope = container.BeginLifetimeScope())
            //    {
            //        scope.Resolve<ResourceManager>(); 
            //    }

            //    using (var scope2 = container.BeginLifetimeScope())
            //    {
            //        scope2.Resolve<ResourceManager>();
            //    }
            //}


            //Disposable
            //var builder = new ContainerBuilder();
            ////builder.RegisterType<ConsoleLog>();
            //builder.RegisterInstance(new ConsoleLog());
            ////var container = builder.Build();
            //using (var container = builder.Build())
            //{
            //    using (var scope = container.BeginLifetimeScope())
            //    {
            //        scope.Resolve<ConsoleLog>();
            //    }
            //}



            // LifeTime Scope
            //var builder = new ContainerBuilder();
            //builder.RegisterType<Parent>();
            //builder.RegisterType<Child>()
            //    .OnActivating(c =>
            //    {
            //        Console.WriteLine("Child Activating!!");
            //        //c.Instance.Parent = c.Context.Resolve<Parent>();
            //        c.ReplaceInstance(new BadChild());
            //    }
            //    )
            //    .OnActivated(c=>
            //    {
            //        Console.WriteLine("Child Activated");
            //        //c.Instance.Parent = c.Context.Resolve<Parent>();
            //    }    
            //    )
            //    .OnRelease(c=>
            //    {
            //        Console.WriteLine("Child destroyed!!!!");
            //    })
            //    ;

            ////builder.RegisterType<ConsoleLog>()
            ////    .As<ILog>()
            ////    .OnActivating(
            ////            c =>
            ////            {
            ////                c.ReplaceInstance(new SMSLog("+3314714441"));
            ////            }
            ////    );

            //builder.RegisterType<ConsoleLog>().AsSelf();
            //builder.Register<ILog>(c => c.Resolve<ConsoleLog>())
            //    .OnActivating(c =>
            //    {
            //        c.ReplaceInstance(new SMSLog("+3314714441"));
            //    });

            //using (var container = builder.Build().BeginLifetimeScope())
            //{
            //    var child = container.Resolve<Child>();
            //    var parent = child.Parent;
            //    Console.WriteLine(child);
            //    Console.WriteLine(parent);



            //    var log = container.Resolve<ILog>();
            //    log.Write("Testing");
            //}




            // running code of startup
            var builder = new ContainerBuilder();
            builder.RegisterType<MyClass>().AsSelf().As<IStartable>().SingleInstance();


            var container = builder.Build();


            var myclass = container.Resolve<MyClass>();
        }
    }
}
