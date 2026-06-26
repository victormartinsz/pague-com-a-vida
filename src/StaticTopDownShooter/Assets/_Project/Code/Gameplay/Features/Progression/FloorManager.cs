using System.Linq;
using UnityEngine.InputSystem;

namespace Shooter
{
    // Progressão pela torre mutável (GDD: 3 andares + chefe).
    //
    // Cada andar é um GameObject "raiz" contendo seus inimigos/baús/layout.
    // No Awake desativamos todos menos o primeiro, para que o EcsRunner crie
    // apenas as entidades do andar 1 no boot. Ao limpar um andar, mostramos o
    // prompt de descida; ao apertar Enter, ativamos o próximo andar e criamos
    // suas entidades ECS na hora.
    //
    // OBS: este é o recurso mais "manual" de configurar. Se preferir entregar
    // só uma arena, deixe um único FloorRoot — tudo o mais (vida-moeda, escudo,
    // baús, chefe) funciona normalmente sem o FloorManager.
    public sealed class FloorManager : MonoBehaviour
    {
        [Tooltip("Um GameObject raiz por andar, em ordem. O último deve conter o Chefe.")]
        public GameObject[] FloorRoots;

        [Header("UI opcional")]
        public GameObject DescendPrompt;   // "Andar limpo — aperte ENTER para descer"
        public GameObject VictoryScreen;    // mostrado ao vencer o último andar
        public GameObject GameOverScreen;   // mostrado quando Malachi morre

        private int _index;
        private bool _cleared;
        private bool _sawEnemies;
        private bool _finished;

        private void Awake()
        {
            if (FloorRoots == null) return;
            for (int i = 1; i < FloorRoots.Length; i++)
                if (FloorRoots[i] != null) FloorRoots[i].SetActive(false);
        }

        private void Start()
        {
            PlayerVitals.Floor = 1;
            if (DescendPrompt != null) DescendPrompt.SetActive(false);
            if (VictoryScreen != null) VictoryScreen.SetActive(false);
            if (GameOverScreen != null) GameOverScreen.SetActive(false);
        }

        private void Update()
        {
            if (_finished) return;

            if (!PlayerVitals.Alive && _sawEnemies)
            {
                _finished = true;
                if (GameOverScreen != null) GameOverScreen.SetActive(true);
                return;
            }

            bool anyEnemy = AnyEnemyAlive();
            if (anyEnemy) _sawEnemies = true;

            if (!_cleared && _sawEnemies && !anyEnemy)
            {
                _cleared = true;
                if (DescendPrompt != null) DescendPrompt.SetActive(true);
            }

            if (_cleared && DescendPressed())
                Descend();
        }

        private void Descend()
        {
            _cleared = false;
            if (DescendPrompt != null) DescendPrompt.SetActive(false);

            _index++;
            if (FloorRoots == null || _index >= FloorRoots.Length)
            {
                _finished = true;
                if (VictoryScreen != null) VictoryScreen.SetActive(true);
                return;
            }

            ActivateFloor(_index);
            PlayerVitals.Floor = _index + 1;
            _sawEnemies = false;
        }

        // Liga o andar e cria suas entidades ECS (espelha o boot do EcsRunner).
        private void ActivateFloor(int i)
        {
            GameObject root = FloorRoots[i];
            if (root == null) return;
            root.SetActive(true);

            foreach (var provider in root.GetComponentsInChildren<AbstractStaticEcsEntityProvider>(true))
                provider.CreateEntity();

            foreach (var registrar in root.GetComponentsInChildren<MonoBehaviour>(true).OfType<IResourcesRegistrar>())
                registrar.RegisterResources();

            foreach (var registrar in root.GetComponentsInChildren<RootGameComponentsRegistrar>(true))
                registrar.RegisterComponents();
        }

        private static bool AnyEnemyAlive()
        {
            try { return Game.Query<All<IsEnemy>>().One(out var _ignored); }
            catch { return false; }
        }

        private static bool DescendPressed()
        {
            Keyboard kb = Keyboard.current;
            return kb != null && kb.enterKey.wasPressedThisFrame;
        }
    }
}
