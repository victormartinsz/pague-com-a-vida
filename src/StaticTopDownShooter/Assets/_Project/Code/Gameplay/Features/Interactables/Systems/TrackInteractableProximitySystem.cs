namespace Shooter;

// Mantém o componente NearInteractable no jogador enquanto ele estiver
// sobre o trigger de um baú/altar. Usa a mesma infra de trigger das balas:
// o interagível tem um CollisionEventBroadcaster que dispara TriggerEnter/Exit.
public struct TrackInteractableProximitySystem : ISystem
{
    private EventReceiver<GameWT, TriggerEnterEvent> _enterReceiver;
    private EventReceiver<GameWT, TriggerExitEvent> _exitReceiver;

    public void Init()
    {
        _enterReceiver = Game.RegisterEventReceiver<TriggerEnterEvent>();
        _exitReceiver = Game.RegisterEventReceiver<TriggerExitEvent>();
    }

    public void Update()
    {
        foreach (var enter in _enterReceiver)
        {
            if (!enter.Value.Source.TryUnpack<GameWT>(out var source))
                continue;
            if (!source.Has<IsInteractable>() || source.Has<IsConsumed>())
                continue;
            if (!TryResolvePlayer(enter.Value.OtherColliderId, out var player))
                continue;

            if (player.Has<NearInteractable>())
                player.Mut<NearInteractable>().Target = source.GID;
            else
                player.Add<NearInteractable>().Target = source.GID;
        }

        foreach (var exit in _exitReceiver)
        {
            if (!exit.Value.Source.TryUnpack<GameWT>(out var source))
                continue;
            if (!source.Has<IsInteractable>())
                continue;
            if (!TryResolvePlayer(exit.Value.OtherColliderId, out var player))
                continue;

            // Só limpa se o jogador estava marcado para ESTE interagível.
            if (player.Has<NearInteractable>() && player.Read<NearInteractable>().Target == source.GID)
                player.Delete<NearInteractable>();
        }
    }

    private static bool TryResolvePlayer(int otherColliderId, out Game.Entity player)
    {
        player = default;
        EntityGID otherGid = ColliderRegistry.GetEntityGid(otherColliderId);
        if (otherGid.Raw == 0)
            return false;
        if (!otherGid.TryUnpack<GameWT>(out player))
            return false;
        return player.Has<IsPlayer>();
    }

    public void Destroy()
    {
        Game.DeleteEventReceiver(ref _enterReceiver);
        Game.DeleteEventReceiver(ref _exitReceiver);
    }
}
