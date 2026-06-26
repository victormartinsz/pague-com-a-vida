using UnityEngine.InputSystem;

namespace Shooter;

// Botão direito do mouse aciona o Escudo.
// Lemos o Input System diretamente para NÃO precisar editar o asset
// PlayerInputActions (que é gerado e arriscado de mexer na mão).
public struct EmitShieldInputSystem : ISystem
{
    public void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.rightButton.wasPressedThisFrame)
            return;

        if (Game.Query<All<IsPlayer>>().One(out var player))
        {
            Game.SendEvent(new ShieldActionPerformed { InputOwner = player.GID });
        }
    }
}
