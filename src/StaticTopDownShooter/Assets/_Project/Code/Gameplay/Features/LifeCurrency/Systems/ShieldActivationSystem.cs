namespace Shooter;

public struct ShieldActivationSystem : ISystem
{
    private EventReceiver<GameWT, ShieldActionPerformed> _receiver;

    public void Init()
    {
        _receiver = Game.RegisterEventReceiver<ShieldActionPerformed>();
    }

    public void Update()
    {
        foreach (var shieldEvent in _receiver)
        {
            if (!shieldEvent.Value.InputOwner.TryUnpack<GameWT>(out var player))
                continue;

            // Já está com escudo? Ignora (não paga de novo).
            if (player.Has<ShieldActive>())
                continue;

            float cost = player.Has<LifeCosts>() ? player.Read<LifeCosts>().Ability : 5f;

            // Paga 5 de vida. Se não tiver vida suficiente, não ativa.
            if (!LifeBank.TrySpend(player, cost))
                continue;

            float duration = player.Has<ShieldDuration>() ? player.Read<ShieldDuration>().Value : 2.5f;

            player.Set<ShieldActive>();
            player.Add<ShieldTimer>().Value = duration;
            GameSfx.Shield++;
        }
    }

    public void Destroy()
    {
        Game.DeleteEventReceiver(ref _receiver);
    }
}
