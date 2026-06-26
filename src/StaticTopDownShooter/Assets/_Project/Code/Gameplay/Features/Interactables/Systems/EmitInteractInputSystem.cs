using UnityEngine.InputSystem;

namespace Shooter;

// Tecla E interage com o baú/altar mais próximo (quando dentro do alcance).
public struct EmitInteractInputSystem : ISystem
{
    public void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.eKey.wasPressedThisFrame)
            return;

        if (Game.Query<All<IsPlayer>>().One(out var player))
        {
            Game.SendEvent(new InteractActionPerformed { InputOwner = player.GID });
        }
    }
}
