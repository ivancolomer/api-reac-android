using System;
using Nancy;
using Nancy.Diagnostics;

namespace REAC2_AndroidAPI
{
    

    public class DemoBootstrapper : DefaultNancyBootstrapper
    {
        public override void Configure(Nancy.Configuration.INancyEnvironment environment)
        {
            environment.Diagnostics(
                enabled: true,
                password: "password");
            environment.Tracing(enabled: false, displayErrorTraces: true);
        }
    }
}
