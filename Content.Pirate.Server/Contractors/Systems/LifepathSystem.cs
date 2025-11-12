using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Preferences;
using Content.Shared._Pirate.Contractors.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Localization;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts.Effects;

[DataDefinition]
public sealed partial class CharacterLifepathRequirement : LoadoutEffect
{
    [DataField(required: true)]
    public List<string> Lifepaths = new();

    [DataField]
    public bool Inverted { get; private set; } = false;

    public override bool Validate(
        HumanoidCharacterProfile profile,
        RoleLoadout loadout,
        ICommonSession? session,
        IDependencyCollection collection,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;

        var isContained = Lifepaths.Contains(profile.Lifepath);
        var shouldFail = false;

        if (Inverted)
        {
            if (isContained)
                shouldFail = true;
        }
        else
        {
            if (profile.Lifepath == string.Empty || !isContained)
                shouldFail = true;
        }

        if (shouldFail)
        {
            var protoManager = collection.Resolve<IPrototypeManager>();

            var lifepathNames = Lifepaths.Select(id =>
                protoManager.TryIndex<LifepathPrototype>(id, out var proto)
                    ? Loc.GetString(proto.NameKey)
                    : id)
                .Select(name => $"[color=#ff0000]{name}[/color]");

            var listString = string.Join(", ", lifepathNames);
            string reasonString;

            if (Inverted)
            {
                reasonString = $"Цей життєвий шлях заборонений. Заборонені шляхи: {listString}.";
            }
            else
            {
                reasonString = $"Вимагається один із життєвих шляхів: {listString}.";
            }

            reason = FormattedMessage.FromMarkup(reasonString);
            return false;
        }

        return true;
    }

    public override void Apply(RoleLoadout loadout) {}
}