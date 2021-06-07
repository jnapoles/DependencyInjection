using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.Indexed;
using Autofac.Features.Metadata;
using Autofac.Features.OwnedInstances;

namespace ImplicitRelationShip
{
    public interface ILog:IDisposable
    {
        void Write(string message);
    }
    public interface IConsole
    {

    }
    public class ConsoleLog : ILog, IConsole
    {
        public ConsoleLog()
        {
            Console.WriteLine($"Console log creator {DateTime.Now}");
        }

        public void Dispose()
        {
            Console.WriteLine("Console Log no longer required");
        }

        public void Write(string message)
        {
            Console.WriteLine(message);
        }
    }
    public class SMSLog : ILog
    {
        private string phoneNumber;

        public SMSLog(string phoneNumber)
        {
            this.phoneNumber = phoneNumber;
        }

        public void Dispose()
        {
            Console.WriteLine("SMS Log no longer required");
        }

        public void Write(string message)
        {
            Console.WriteLine($"SMS to {phoneNumber} : {message}");
        }
    }
    public class Reporting
    {
        private readonly Lazy<ConsoleLog> log;

        public Reporting(Lazy<ConsoleLog> log)
        {
            this.log = log;
            Console.WriteLine("Reporting component created");
        }

        public void Report()
        {
            log.Value.Write($"Log started");
        }
    }
    public class Reporting2
    {
        private Owned<ConsoleLog> log;

        public Reporting2(Owned<ConsoleLog> _log)
        {
            log = _log;
        }

        public void ReportOnce()
        {
            log.Value.Write("Report Started");
            log.Dispose();
        }
    }
    public class Reporting3
    {
        Func<ConsoleLog> log;
        Func<string, SMSLog> smsLog;

        public Reporting3(Func<ConsoleLog> _log, Func<string,SMSLog> _smsLog)
        {
            this.log = _log;
            this.smsLog = _smsLog;
        }

        public void Report()
        {
            log().Write("Reporting to console");
            log().Write("Again");

            smsLog("3314714441").Write("Texting Message");


        }
    }
    public class Reporting4
    {
        private IList<ILog> allLogs;
        public Reporting4(IList<ILog> logs)
        {
            allLogs = logs;
        }

        public void Report()
        {
            foreach (var item in allLogs)
            {
                Console.WriteLine($"This is a {item.GetType().Name}");
            }
        }
    }
    public class Setting
    {
        public string logMode { get; set; }

    }
    public class Reporting5
    {
        private Meta<ConsoleLog,Setting> log;
        public Reporting5(Meta<ConsoleLog,Setting> _log)
        {
            log = _log;
        }

        public void Report()
        {
            //if (log.Metadata["logmode"] as string == "verbose")
            if (log.Metadata.logMode == "verbose")
                log.Value.Write($"VERBOSE MODE: Logger started on {DateTime.Now}");
        }
    }
    public class Reporting6
    {
        private IIndex<string, ILog> logs;
        public Reporting6(IIndex<string,ILog> log)
        {
            logs = log;
        }
        public void Report()
        {
            logs["sms"].Write("starting report output");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {

            // Delayed Instatiation
            //var build = new ContainerBuilder();

            //build.RegisterType<ConsoleLog>();
            //build.RegisterType<Reporting>();



            //using (var c = build.Build())
            //{
            //    Console.WriteLine("Start");
            //    c.Resolve<Reporting>().Report();
            //    Console.WriteLine("End");
            //}

            // Controller Instatiation
            //var build = new ContainerBuilder();

            //build.RegisterType<ConsoleLog>();
            //build.RegisterType<Reporting2>();



            //using (var c= build.Build())
            //{
            //    Console.WriteLine("Start");
            //    c.Resolve<Reporting2>().ReportOnce();
            //    Console.WriteLine("End");
            //}


            // Dynamically Instatiation

            //var build = new ContainerBuilder();
            //build.RegisterType<ConsoleLog>();
            //build.RegisterType<SMSLog>();
            //build.RegisterType<Reporting3>();


            //using (var c = build.Build())
            //{
            //    c.Resolve<Reporting3>().Report();
            //}



            // Enumeration 

            //var build = new ContainerBuilder();
            //build.RegisterType<ConsoleLog>().As<ILog>();
            //build.Register(c => new SMSLog("3314714441")).As<ILog>();

            //build.RegisterType<Reporting4>();

            //using (var c = build.Build())
            //{
            //    c.Resolve<Reporting4>().Report();
            //}


            // Metadata

            //var build = new ContainerBuilder();
            //build.RegisterType<ConsoleLog>()
            //    .WithMetadata<Setting>(c => c.For(x => x.logMode, "verbose"));
            //build.RegisterType<Reporting5>();

            //using (var c = build.Build())
            //{
            //    c.Resolve<Reporting5>().Report();
            //}



            // Keyed
            var build = new ContainerBuilder();
            build.RegisterType<ConsoleLog>().Keyed<ILog>("cmd");
            build.Register(c => new SMSLog("3314714441")).Keyed<ILog>("sms");
            build.RegisterType<Reporting6>();

            using (var c = build.Build())
            {
                c.Resolve<Reporting6>().Report();
            }
        }
    }
}
