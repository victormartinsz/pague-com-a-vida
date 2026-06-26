namespace Shooter;

// Barramento de áudio desacoplado: sistemas ECS incrementam estes contadores;
// o PagueComVidaSetup (MonoBehaviour) toca o som quando o contador muda.
public static class GameSfx
{
    public static int Shoot;   // tiro (-1 HP)
    public static int Hurt;    // jogador levou dano
    public static int Shield;  // escudo ativado
    public static int Chest;   // baú/altar consumido

    public static void Reset()
    {
        Shoot = Hurt = Shield = Chest = 0;
    }
}
