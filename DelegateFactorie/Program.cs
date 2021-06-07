using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autofac;

namespace DelegateFactorie
{
    public class Service
    {
        public string DoSomeThing(int value)
        {
            return $"I have {value}";
        }
    }

    public class DomainObject
    {
        Service service;
        int value;

        public delegate DomainObject Factory(int value);
        public DomainObject(Service _service, int _value)
        {
            this.service = _service;
            this.value = _value;
        }
        public override string ToString()
        {
            return service.DoSomeThing(value);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {

            var builder = new ContainerBuilder();
            builder.RegisterType<Service>();
            builder.RegisterType<DomainObject>();


            var container = builder.Build();

            //var domainObject = container.Resolve<DomainObject>(
            //    new PositionalParameter(1,42)
            //    );

            //Console.WriteLine(domainObject.ToString());


            var factory = container.Resolve<DomainObject.Factory>();
            var domainObject2 = factory(42);

            Console.WriteLine(domainObject2.ToString());
        }
    }
}
