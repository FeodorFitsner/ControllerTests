using System;
using System.Web.Mvc;
using Autofac;
using Autofac.Core.Lifetime;
using MVCControllerTestsWithLocalDb.Web;
using NCrunch.Framework;
using NHibernate;
using Subtext.TestLibrary;

namespace MVCControllerTestsWithLocalDb.Tests.Helpers
{
    [ExclusivelyUses(ApiControllerTest.NCrunchSingleThreadForDb)]     // don't run these transaction db tests in parallel else deadlocks
    public abstract class MVCControllerTest<TController> : IDisposable where TController : Controller
    {
        private bool _disposed;
        private readonly HttpSimulator _httpRequest;

        static MVCControllerTest()
        {
            Db.Program.Main(new[] { Config.DatabaseConnectionString });
        }

        protected MVCControllerTest()
        {
            var container = ContainerConfig.BuildContainer();
            var lts = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);

            _httpRequest = new HttpSimulator().SimulateRequest();

            Controller = lts.Resolve<TController>();
            Session = lts.Resolve<ISession>();
            Session.BeginTransaction();
        }

        protected TController Controller { get; private set; }
        protected ISession Session { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MVCControllerTest()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Session.Transaction.Dispose();  // tear down transaction to release locks
                _httpRequest.Dispose();
            }

            _disposed = true;
        }
    }
}