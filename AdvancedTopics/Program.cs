using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Configuration;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Features.ResolveAnything;
using Microsoft.Extensions.Configuration;
using Autofac.Features.Metadata;
using Autofac.Features.AttributeFilters;
using System.ComponentModel.Composition;
using Autofac.Extras.AttributeMetadata;
using Autofac.Extras.AggregateService;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using System.IO;

namespace AdvancedTopics
{
    public abstract class BaseHandler
    {
        public virtual string Handle(string message)
        {
            return "Handled: " + message;
        }
    }

    public class HandlerA : BaseHandler
    {
        public override string Handle(string message)
        {
            return "Handled by A: " + message;
        }
    }

    public class HandlerB : BaseHandler
    {
        public override string Handle(string message)
        {
            return "Handled by B: " + message;
        }
    }

    public interface IHandlerFactory
    {
        T GetHandler<T>() where T : BaseHandler;
    }

    class HandlerFactory : IHandlerFactory
    {
        public T GetHandler<T>() where T : BaseHandler
        {
            return Activator.CreateInstance<T>();
        }
    }

    public class ConsumerA
    {
        private HandlerA handlerA;

        public ConsumerA(HandlerA handlerA)
        {
            if (handlerA == null)
            {
                throw new ArgumentNullException(paramName: nameof(handlerA));
            }
            this.handlerA = handlerA;
        }

        public void DoWork()
        {
            Console.WriteLine(handlerA.Handle("ConsumerA"));
        }
    }

    public class ConsumerB
    {
        private HandlerB handlerB;


        public ConsumerB(HandlerB handlerB)
        {
            if (handlerB == null)
            {
                throw new ArgumentNullException(paramName: nameof(handlerB));
            }
            this.handlerB = handlerB;
        }

        public void DoWork()
        {
            Console.WriteLine(handlerB.Handle("ConsumerB"));
        }
    }

    public class HandlerRegistrationSource : IRegistrationSource
    {
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            var swt = service as IServiceWithType;
            if (swt == null
                || swt.ServiceType == null
                || !swt.ServiceType.IsAssignableTo<BaseHandler>())
            {
                yield break;
            }

            yield return new ComponentRegistration(
                Guid.NewGuid(),
                new DelegateActivator(
                  swt.ServiceType,
                  (c, p) =>
                  {
                      var provider = c.Resolve<IHandlerFactory>();
                      var method = provider.GetType().GetMethod("GetHandler").MakeGenericMethod(swt.ServiceType);
                      return method.Invoke(provider, null);
                  }
                ),
                new CurrentScopeLifetime(),
                InstanceSharing.None,
                InstanceOwnership.OwnedByLifetimeScope,
                new[] { service },
                new ConcurrentDictionary<string, object>());
        }

        public bool IsAdapterForIndividualComponents => false;
    }


    public interface ICommand
    {
        void Execute();
    }

    public class SaveCommand : ICommand
    {
        public void Execute()
        {
            Console.WriteLine("Saving current file");
        }
    }

    public class OpenCommand : ICommand
    {
        public void Execute()
        {
            Console.WriteLine("Opening a file");
        }
    }

    public class Button
    {
        private ICommand command;
        private string name;

        public Button(ICommand command, string name)
        {
            if (command == null)
            {
                throw new ArgumentNullException(paramName: nameof(command));
            }
            this.command = command;
            this.name = name;
        }
        public Button(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(paramName: nameof(command));
            }
            this.command = command;
        }

        public void Click()
        {
            command.Execute();
        }

        public void PrintMe()
        {
            Console.WriteLine($"I am a button called {name}");
        }
    }

    public class Editor
    {
        private readonly IEnumerable<Button> buttons;

        public IEnumerable<Button> Buttons => buttons;

        public Editor(IEnumerable<Button> buttons)
        {
            this.buttons = buttons;
        }

        public void ClickAll()
        {
            foreach (var btn in buttons)
            {
                btn.Click();
            }
        }
    }


    public interface IReportingService
    {
        void Report();

    }

    public class ReportingService : IReportingService
    {
        public void Report()
        {
            Console.WriteLine("Here is your report");
        }
    }

    public class ReportingServiceWithLogging : IReportingService
    {
        private IReportingService decorated;

        public ReportingServiceWithLogging(IReportingService decorated)
        {
            this.decorated = decorated;
        }

        public void Report()
        {
            Console.WriteLine("Commencing log...");
            decorated.Report();
            Console.WriteLine("Ending log...");
        }
    }



    public class ParentWithProperty
    {
        public ChildWithProperty Child { get; set; }

        public override string ToString()
        {
            return "Parent";
        }
    }

    public class ChildWithProperty
    {
        public ParentWithProperty Parent { get; set; }

        public override string ToString()
        {
            return "Child";
        }
    }

    public class ParentWithConstructor1
    {
        public ChildWithProperty1 Child;

        public ParentWithConstructor1(ChildWithProperty1 child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(paramName: nameof(child));
            }
            Child = child;
        }

        public override string ToString()
        {
            return "Parent with a ChildWithProperty";
        }
    }

    public class ChildWithProperty1
    {
        public ParentWithConstructor1 Parent { get; set; }

        public override string ToString()
        {
            return "Child";
        }
    }




    [MetadataAttribute]
    public class AgeMetadataAttribute : Attribute
    {
        public int Age { get; private set; }

        public AgeMetadataAttribute(int age)
        {
            Age = age;
        }
    }

    public interface IArtwork
    {
        void Display();
    }

    [AgeMetadata(100)]
    public class CenturyArtwork : IArtwork
    {
        public void Display()
        {
            Console.WriteLine("Displaying a century-old piece");
        }
    }

    [AgeMetadata(1000)]
    public class MillenialArtwork : IArtwork
    {
        public void Display()
        {
            Console.WriteLine("Displaying a REALLY old piece of art");
        }
    }

    public class ArtDisplay
    {
        private readonly IArtwork art;

        public ArtDisplay([MetadataFilter("Age", 100)] IArtwork art)
        {
            this.art = art;
        }

        public void Display() { art.Display(); }
    }



    public interface IService1 { }
    public interface IService2 { }
    public interface IService3 { }
    public interface IService4 { }

    public class Class1 : IService1 { }
    public class Class2 : IService2 { }
    public class Class3 : IService3 { }

    public class Class4 : IService4
    {
        private string name;

        public Class4()
        {

        }
        public Class4(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(paramName: nameof(name));
            }
            this.name = name;
        }
    }

    public interface IMyAggregateService
    {
        IService1 Service1 { get; }
        IService2 Service2 { get; }
        IService3 Service3 { get; }
        //IService4 Service4 { get; }

        IService4 GetFourthService(string name);
    }

    public class Consumer
    {
        public IMyAggregateService AllServices;

        public Consumer(IMyAggregateService allServices)
        {
            if (allServices == null)
            {
                throw new ArgumentNullException(paramName: nameof(allServices));
            }
            AllServices = allServices;
        }
    }






    public class CallLogger : IInterceptor
    {
        private TextWriter output;

        public CallLogger(TextWriter output)
        {
            if (output == null)
            {
                throw new ArgumentNullException(paramName: nameof(output));
            }
            this.output = output;
        }

        public void Intercept(IInvocation invocation)
        {
            var methodName = invocation.Method.Name;
            output.WriteLine("Calling method {0} with args {1}",
              methodName,
              string.Join(",",
                invocation.Arguments.Select(a => (a ?? "").ToString())
              )
            );
            invocation.Proceed();
            output.WriteLine("Done calling {0}, result was {1}",
                methodName, invocation.ReturnValue
            );
        }
    }

    public interface IAudit
    {
        int Start(DateTime reportDate);
    }

    [Intercept(typeof(CallLogger))]
    public class Audit : IAudit
    {
        public virtual int Start(DateTime reportDate)
        {
            Console.WriteLine($"Starting report on {reportDate}");
            return 42;
        }
    }



    class Foo : IFoo
    {
        IFoo foo; 
        public Foo(IFoo foo) { this.foo = foo; }
    }

    internal interface IFoo
    {
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Registration Source
            //var builder = new ContainerBuilder();

            //builder.RegisterType<ConsumerA>();
            //builder.RegisterType<ConsumerB>();
            //builder.RegisterType<HandlerFactory>().As<IHandlerFactory>();
            //builder.RegisterSource(new HandlerRegistrationSource());



            //using (var container = builder.Build())
            //{
            //    container.Resolve<ConsumerA>().DoWork();
            //    container.Resolve<ConsumerB>().DoWork();
            //}



            // Adapters
            //var builder = new ContainerBuilder();
            //builder.RegisterType<SaveCommand>().As<ICommand>().WithMetadata("Name", "Save"); ;
            //builder.RegisterType<OpenCommand>().As<ICommand>().WithMetadata("Name", "Open"); ;
            ////builder.RegisterType<Button>();
            ////builder.RegisterAdapter<ICommand, Button>(c => new Button(c));
            //builder.RegisterAdapter<Meta<ICommand>,Button>(c => new Button(c.Value,(string)c.Metadata["Name"]));
            //builder.RegisterType<Editor>();



            //using (var container = builder.Build())
            //{
            //    //container.Resolve<Editor>().ClickAll();
            //    var editor = container.Resolve<Editor>();
            //    editor.ClickAll();
            //    foreach (var btn in editor.Buttons)
            //        btn.PrintMe();
            //}



            // Decorators
            //var builder = new ContainerBuilder();
            //builder.RegisterType<Foo>().As<IFoo>();

            //var food = builder.Build().Resolve<IFoo>();
            


            //builder.RegisterType<ReportingService>().Named<IReportingService>("reporting");
            // builder.RegisterDecorator<IReportingService>((context, service) => new ReportingServiceWithLogging(service), "reporting");

            //using (var container = builder.Build())
            //{
            //    container.Resolve<IReportingService>().Report();
            //}



            // Circular Dependency
            //var builder = new ContainerBuilder();
            //builder.RegisterType<ChildWithProperty>().InstancePerLifetimeScope()
            //    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            //builder.RegisterType<ParentWithProperty>().InstancePerLifetimeScope()
            //    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);


            //using (var container = builder.Build())
            //{
            //    Console.WriteLine(container.Resolve<ChildWithProperty>().Parent);
            //    Console.WriteLine(container.Resolve<ParentWithProperty>().Child);
            //} 

            //var builder = new ContainerBuilder();
            //builder.RegisterType<ChildWithProperty1>().InstancePerLifetimeScope()
            //    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            //builder.RegisterType<ParentWithConstructor1>().InstancePerLifetimeScope()
            //    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            //using (var container = builder.Build())
            //{
            //    Console.WriteLine(container.Resolve<ChildWithProperty1>().Parent);
            //    Console.WriteLine(container.Resolve<ParentWithConstructor1>().Child);
            //}




            // Attribute base MetaData
            //var builder = new ContainerBuilder();
            //builder.RegisterModule<AttributedMetadataModule>();
            //builder.RegisterType<CenturyArtwork>().As<IArtwork>();
            //builder.RegisterType<MillenialArtwork>().As<IArtwork>();
            //builder.RegisterType<ArtDisplay>().WithAttributeFiltering();



            //using (var container = builder.Build())
            //{
            //    container.Resolve<ArtDisplay>().Display();
            //}



            // Agregate Services
            //var cb = new ContainerBuilder();
            //cb.RegisterAggregateService<IMyAggregateService>();
            //cb.RegisterAssemblyTypes(typeof(Program).Assembly)
            //  .Where(t => t.Name.StartsWith("Class"))
            //  .AsImplementedInterfaces();
            //cb.RegisterType<Consumer>();



            //using (var container = cb.Build())
            //{
            //    var consumer = container.Resolve<Consumer>();
            //    //Console.WriteLine(consumer.AllServices.Service3.GetType().Name);
            //    //Console.WriteLine(consumer.AllServices.Service4.GetType().Name);
            //    //Console.WriteLine(consumer.AllServices.Service1.GetType().Name);
            //    Console.WriteLine(consumer.AllServices.GetFourthService("test").GetType().Name);
            //}




            // Type interceptor
            //var builder = new ContainerBuilder();
            //builder.Register(c => new CallLogger(Console.Out)).As<IInterceptor>().AsSelf();
            //builder.RegisterType<Audit>().As<IAudit>().EnableClassInterceptors();
            //builder.RegisterType<Audit>().As<IAudit>().EnableInterfaceInterceptors();


            //using (var container = builder.Build())
            //{
            //    container.Resolve<IAudit>().Start(DateTime.Now);
            //}

        }
    }
}
