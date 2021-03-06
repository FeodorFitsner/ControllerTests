using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using NHibernate;

namespace ControllerTests.Web
{
    static class ContainerConfig
    {
        internal static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(typeof(ContainerConfig).Assembly);
            return builder.Build();
        }

        internal static void SetupDependencyInjection()
        {
            var container = BuildContainer();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }

    class WebRegistrationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterControllers(ThisAssembly);
            builder.RegisterApiControllers(ThisAssembly);
            builder.RegisterModule(new AutofacWebTypesModule());

            RegisterDatabaseComponents(builder);
        }

        private void RegisterDatabaseComponents(ContainerBuilder builder)
        {
            builder.Register(context => NhibernateConfig.CreateSessionFactory().OpenSession())
                .As<ISession>()
                .InstancePerRequest()
                .OnRelease(session =>
                    {
                        NhibernateConfig.CompleteRequest(session);

                        session.Dispose();
                    });
        }
    }
}