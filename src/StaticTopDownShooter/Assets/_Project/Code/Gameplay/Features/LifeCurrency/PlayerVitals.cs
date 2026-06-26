namespace Shooter;

// Ponte simples entre o mundo ECS e a UI (MonoBehaviour).
// Um sistema (PublishPlayerVitalsSystem) escreve aqui todo frame;
// a barra de vida apenas lê. Mantém a UI desacoplada do ECS.
public static class PlayerVitals
{
    public static bool Alive;
    public static float Current;
    public static float Max;
    public static bool ShieldActive;
    public static int Floor = 1;
}
