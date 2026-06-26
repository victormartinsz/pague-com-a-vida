namespace Shooter;

public struct ShieldTimerTickSystem : ISystem
{
    public void Update()
    {
        TimeData timeData = Game.GetResource<TimeData>();

        Game.Query().For(timeData, static (ref TimeData time, Game.Entity entity, ref ShieldTimer timer) =>
        {
            timer.Value -= time.DeltaTime;

            if (timer.Value <= 0)
            {
                entity.Delete<ShieldTimer>();
                entity.Delete<ShieldActive>();
            }
        });
    }
}
