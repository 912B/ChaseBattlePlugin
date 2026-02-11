using AssettoServer.Server.Configuration;
using FluentValidation;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace ChaseBattlePlugin;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class ChaseBattleConfiguration : IValidateConfiguration<ChaseBattleConfigurationValidator>
{
    public bool Enabled { get; init; } = true;
    public bool DebugMode { get; init; } = false;
}

[UsedImplicitly]
public class ChaseBattleConfigurationValidator : AbstractValidator<ChaseBattleConfiguration>
{
    public ChaseBattleConfigurationValidator()
    {
    }
}
