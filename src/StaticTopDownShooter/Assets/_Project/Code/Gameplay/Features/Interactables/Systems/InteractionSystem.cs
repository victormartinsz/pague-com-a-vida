namespace Shooter;

// Ao apertar E perto de um baú/altar: cobra o preço em vida e aplica a recompensa.
public struct InteractionSystem : ISystem
{
    private EventReceiver<GameWT, InteractActionPerformed> _receiver;

    public void Init()
    {
        _receiver = Game.RegisterEventReceiver<InteractActionPerformed>();
    }

    public void Update()
    {
        foreach (var interact in _receiver)
        {
            if (!interact.Value.InputOwner.TryUnpack<GameWT>(out var player))
                continue;
            if (!player.Has<NearInteractable>())
                continue;

            EntityGID targetGid = player.Read<NearInteractable>().Target;
            if (!targetGid.TryUnpack<GameWT>(out var target))
                continue;
            if (!target.Has<IsInteractable>() || target.Has<IsConsumed>())
                continue;

            LifePrice price = target.Has<LifePrice>() ? target.Read<LifePrice>() : default;

            // Vida suficiente para pagar o custo em HP atual?
            if (!LifeBank.CanAfford(player, price.CurrentHp))
                continue;

            LifeBank.TrySpend(player, price.CurrentHp);

            // Altar amaldiçoado: queima parte do HP MÁXIMO (preço permanente).
            if (price.MaxHpPercent > 0f && player.Has<Hp>())
            {
                ref Hp hp = ref player.Mut<Hp>();
                hp.Max *= (1f - price.MaxHpPercent);
                if (hp.Current > hp.Max)
                    hp.Current = hp.Max;
            }

            int kind = target.Has<RewardKind>() ? target.Read<RewardKind>().Value : 0;
            float heal = target.Has<RewardHeal>() ? target.Read<RewardHeal>().Value : 0f;
            Upgrades.Apply(player, kind, heal);

            target.Set<IsConsumed>();
            player.Delete<NearInteractable>();
            GameSfx.Chest++;

            // Some com o objeto: feedback claro de que foi consumido.
            if (target.Has<TransformComponent>())
            {
                Object.Destroy(target.Mut<TransformComponent>().Value.gameObject);
                target.Destroy();
            }
        }
    }

    public void Destroy()
    {
        Game.DeleteEventReceiver(ref _receiver);
    }
}
