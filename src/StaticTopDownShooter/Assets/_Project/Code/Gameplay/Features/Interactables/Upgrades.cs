namespace Shooter;

// Recompensas / melhorias permanentes do alquimista Malachi.
// O PREÇO (HP atual ou % do HP máximo) é cobrado antes, em InteractionSystem.
// Aqui só aplicamos o BENEFÍCIO (+ a cura de tesouro, no caso dos baús).
public static class Upgrades
{
    public const int Swiftness = 0;  // mais velocidade
    public const int HonedBlade = 1; // ataque mais barato
    public const int Warding = 2;    // escudo mais longo
    public const int RapidFire = 3;  // dispara mais rápido

    public static void Apply(Game.Entity player, int kind, float heal)
    {
        if (heal > 0f && player.Has<Hp>())
        {
            ref Hp hp = ref player.Mut<Hp>();
            hp.Current = Mathf.Min(hp.Max, hp.Current + heal);
        }

        switch (kind)
        {
            case Swiftness:
                if (player.Has<Speed>()) player.Mut<Speed>().Value *= 1.15f;
                break;

            case HonedBlade:
                if (player.Has<LifeCosts>())
                {
                    ref LifeCosts c = ref player.Mut<LifeCosts>();
                    c.Attack = Mathf.Max(0f, c.Attack - 0.5f);
                }
                break;

            case Warding:
                if (player.Has<ShieldDuration>()) player.Mut<ShieldDuration>().Value += 1f;
                break;

            case RapidFire:
                if (player.Has<ShootPerSec>()) player.Mut<ShootPerSec>().Value *= 1.25f;
                break;
        }
    }
}
