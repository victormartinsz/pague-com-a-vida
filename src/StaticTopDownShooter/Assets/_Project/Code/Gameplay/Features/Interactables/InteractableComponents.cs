namespace Shooter;

// Baús e Altares: objetos com que o jogador interage pagando com a vida.
[Serializable] public struct IsInteractable : ITag {}
[Serializable] public struct IsConsumed : ITag {}

// Preço da interação. Baú: paga HP atual. Altar amaldiçoado: queima % do HP máximo.
[Serializable]
public struct LifePrice : IComponent
{
    public float CurrentHp;    // GDD baú = 25
    public float MaxHpPercent; // GDD melhoria permanente = 0.05..0.10 (0 para baú comum)
}

// Qual recompensa aplicar (ver Upgrades.Apply).
[Serializable] public struct RewardKind : IComponent { public int Value; }

// Cura entregue ao abrir (o "tesouro" do baú). 0 no altar.
[Serializable] public struct RewardHeal : IComponent { public float Value; }

// Colocado no JOGADOR enquanto ele está dentro do alcance de um interagível.
[Serializable] public struct NearInteractable : IComponent { public EntityGID Target; }

// Disparado pela tecla E.
[Serializable] public struct InteractActionPerformed : IEvent { public EntityGID InputOwner; }
