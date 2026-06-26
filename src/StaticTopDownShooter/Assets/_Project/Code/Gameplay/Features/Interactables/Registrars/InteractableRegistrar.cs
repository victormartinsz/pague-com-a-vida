namespace Shooter
{
    // Transforma um GameObject em baú ou altar.
    // Requisitos no GameObject: GameEntityProvider + Collider2D (Is Trigger)
    // + CollisionEventBroadcaster + RootGameComponentsRegistrar (com este registrar na lista).
    public sealed class InteractableRegistrar : GameEntityComponentsRegistrar
    {
        [Header("Preço (pago com a vida)")]
        [Tooltip("HP atual cobrado ao interagir. GDD: baú = 25.")]
        public float CurrentHpPrice = 25f;

        [Tooltip("Fração do HP MÁXIMO queimada (altar). GDD: 0.05 a 0.10. Use 0 para baú comum.")]
        [Range(0f, 0.5f)] public float MaxHpPercentPrice = 0f;

        [Header("Recompensa")]
        [Tooltip("0=Velocidade  1=Lâmina Afiada (ataque + barato)  2=Proteção (escudo + longo)  3=Disparo Rápido")]
        public int RewardKind = 0;

        [Tooltip("Cura entregue como tesouro ao abrir. 0 = padrão seguro (altar). Baú: use ~15.")]
        public float RewardHeal = 0f;

        public override void RegisterComponents(World<GameWT>.Entity entity)
        {
            entity.Set<IsInteractable>();

            ref LifePrice price = ref entity.Add<LifePrice>();
            price.CurrentHp = CurrentHpPrice;
            price.MaxHpPercent = MaxHpPercentPrice;

            entity.Add<RewardKind>().Value = RewardKind;
            entity.Add<RewardHeal>().Value = RewardHeal;

            if (!entity.Has<TransformComponent>())
                entity.Add<TransformComponent>().Value = transform;
        }
    }
}
