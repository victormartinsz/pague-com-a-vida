namespace Shooter;

// "Pague com a Vida": quanto de HP ATUAL cada ação custa.
// Fica no jogador (e em qualquer entidade que deva pagar com a vida).
[Serializable]
public struct LifeCosts : IComponent
{
    public float Attack;   // GDD: ataque básico = 1
    public float Ability;  // GDD: habilidade especial (dash/escudo) = 5
    public float Chest;    // GDD: abrir baú = 25 (referência; o preço real fica no baú)
}

// --- Habilidade Escudo (custa "Ability" de HP) ---
[Serializable] public struct ShieldActive : ITag {}
[Serializable] public struct ShieldTimer : IComponent { public float Value; }
[Serializable] public struct ShieldDuration : IComponent { public float Value; }

[Serializable]
public struct ShieldActionPerformed : IEvent
{
    public EntityGID InputOwner;
}
