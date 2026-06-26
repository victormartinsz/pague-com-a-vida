namespace Shooter
{
    // Marca um inimigo como o Chefe da torre: muito mais HP.
    // Cada bala causa 100 de dano, então BossHp = 600 -> 6 acertos.
    // Coloque junto do EnemyTagRegistrar na lista do RootGameComponentsRegistrar.
    public sealed class BossRegistrar : GameEntityComponentsRegistrar
    {
        public float BossHp = 600f;

        public override void RegisterComponents(World<GameWT>.Entity entity)
        {
            entity.Set<IsBoss>();

            if (entity.Has<Hp>())
            {
                ref Hp hp = ref entity.Mut<Hp>();
                hp.Max = BossHp;
                hp.Current = BossHp;
            }
            else
            {
                ref Hp hp = ref entity.Add<Hp>();
                hp.Max = BossHp;
                hp.Current = BossHp;
            }
        }
    }
}
