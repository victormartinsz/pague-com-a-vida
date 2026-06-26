namespace Shooter;

public struct PublishPlayerVitalsSystem : ISystem
{
    public void Update()
    {
        if (Game.Query<All<IsPlayer, Hp>>().One(out var player))
        {
            Hp hp = player.Read<Hp>();
            PlayerVitals.Alive = true;
            PlayerVitals.Current = hp.Current;
            PlayerVitals.Max = hp.Max;
            PlayerVitals.ShieldActive = player.Has<ShieldActive>();
        }
        else
        {
            PlayerVitals.Alive = false;
        }
    }
}
