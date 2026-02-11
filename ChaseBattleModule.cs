using AssettoServer.Commands;
using AssettoServer.Server.Plugin;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace ChaseBattlePlugin;

public class ChaseBattleModule : AssettoServerModule<ChaseBattleConfiguration>
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ChaseManager>().AsSelf().SingleInstance();
        builder.RegisterType<ChaseBattlePlugin>().As<IHostedService>().SingleInstance();
    }
}
