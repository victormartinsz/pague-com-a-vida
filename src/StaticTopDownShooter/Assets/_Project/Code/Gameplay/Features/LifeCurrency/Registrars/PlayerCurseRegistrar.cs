namespace Shooter
{
    // Marca o jogador como "amaldiçoado": faz dele quem paga com a vida.
    // Adicione este componente ao GameObject do Player (na lista do
    // RootGameComponentsRegistrar) e ajuste os custos pelo Inspector.
    public sealed class PlayerCurseRegistrar : GameEntityComponentsRegistrar
    {
        [Header("Custos em HP atual (GDD)")]
        public float AttackCost = 1f;
        public float AbilityCost = 5f;
        public float ChestCostReference = 25f;

        [Header("Escudo")]
        public float ShieldDuration = 2.5f;

        public override void RegisterComponents(World<GameWT>.Entity entity)
        {
            ref LifeCosts costs = ref entity.Add<LifeCosts>();
            costs.Attack = AttackCost;
            costs.Ability = AbilityCost;
            costs.Chest = ChestCostReference;

            entity.Add<ShieldDuration>().Value = ShieldDuration;
        }
    }
}
