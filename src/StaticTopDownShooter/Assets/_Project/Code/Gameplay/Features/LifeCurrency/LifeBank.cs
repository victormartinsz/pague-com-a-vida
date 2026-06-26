namespace Shooter;

// Cofre central do tema "Pague com a Vida".
// Toda transação que gasta HP atual passa por aqui, garantindo a mesma regra:
// você nunca consegue se matar com uma compra rotineira (precisa SOBRAR vida),
// mas pode ficar com 1 de HP. Tudo tem um preço.
public static class LifeBank
{
    public static bool CanAfford(Game.Entity payer, float amount)
    {
        if (amount <= 0f) return payer.Has<Hp>();
        return payer.Has<Hp>() && payer.Read<Hp>().Current > amount;
    }

    // Tenta pagar 'amount' de HP atual. Retorna false (sem cobrar) se não houver vida suficiente.
    // Para ações CARAS (esquiva/escudo/baú): você nunca pode se matar (precisa SOBRAR vida).
    public static bool TrySpend(Game.Entity payer, float amount)
    {
        if (!payer.Has<Hp>()) return false;

        ref Hp hp = ref payer.Mut<Hp>();

        if (amount <= 0f) return true;          // ação gratuita
        if (hp.Current <= amount) return false; // pagar mataria o alquimista -> bloqueia

        hp.Current -= amount;
        return true;
    }

    // Variante do ATAQUE: permite gastar até ZERAR a vida (o "preço final").
    // Evita o softlock de ficar preso em 1 HP sem poder atacar.
    public static bool TrySpendToZero(Game.Entity payer, float amount)
    {
        if (!payer.Has<Hp>()) return false;

        ref Hp hp = ref payer.Mut<Hp>();

        if (amount <= 0f) return true;
        if (hp.Current < amount) return false;  // não tem nem o suficiente para o disparo

        hp.Current -= amount;
        return true;
    }
}
