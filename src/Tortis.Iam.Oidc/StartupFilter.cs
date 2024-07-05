namespace Tortis.Iam.Oidc;

public class StartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            //app.UseTortisIamOidc();
            next(app);
            app.UseEndpoints(ep => ep.MapControllers());
        };
    }
}