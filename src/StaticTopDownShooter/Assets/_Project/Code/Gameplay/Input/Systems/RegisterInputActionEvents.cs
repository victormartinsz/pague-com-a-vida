using UnityEngine.InputSystem;

namespace Shooter;

public struct RegisterInputActionEvents : ISystem
{
    public void Init()
    {
        PlayerInputActions actions = Game.GetResource<PlayerInputMap>().Value;

        actions.Locomotion.Dash.performed += SendDodgePerformedEvent;

        actions.Combat.Attack.performed += SendAttackPerformedEvent;
    }

    public void Destroy()
    {
        PlayerInputActions actions = Game.GetResource<PlayerInputMap>().Value;

        actions.Locomotion.Dash.performed -= SendDodgePerformedEvent;

        actions.Combat.Attack.performed -= SendAttackPerformedEvent;
    }

    private void SendAttackPerformedEvent(InputAction.CallbackContext _)
    {
        if (TryGetPlayerGID(out var gid))
            Game.SendEvent(new AttackActionPerformed(gid));
    }

    private void SendDodgePerformedEvent(InputAction.CallbackContext _)
    {
        if (TryGetPlayerGID(out var gid))
            Game.SendEvent(new DodgeActionPerformed(gid));
    }

    // Sem jogador vivo (ex.: após a morte) o input é simplesmente ignorado.
    private static bool TryGetPlayerGID(out EntityGID gid)
    {
        if (Game.Query<All<IsPlayer>>().One(out var entity))
        {
            gid = entity.GID;
            return true;
        }

        gid = default;
        return false;
    }
}