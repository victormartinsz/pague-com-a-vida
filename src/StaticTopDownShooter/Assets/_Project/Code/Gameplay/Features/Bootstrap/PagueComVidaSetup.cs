using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Shooter
{
    // ============================================================================
    //  PAGUE COM A VIDA — Setup automático.
    //  Arraste este componente em UM GameObject vazio na cena e aperte Play.
    //  Configura o jogador, desenha a HUD (IMGUI, sem dependências), telas de fim
    //  de jogo, degradação visual e baús — tudo por código, sem montar nada na mão.
    //  Usa IMGUI (OnGUI) de propósito: não depende do pacote uGUI.
    // ============================================================================
    [DefaultExecutionOrder(1000)] // roda depois do EcsRunner (que cria o mundo no Start)
    public sealed class PagueComVidaSetup : MonoBehaviour
    {
        [Header("Vida do alquimista Malachi")]
        public float StartMaxHp = 100f;

        [Header("Custos em HP (GDD)")]
        public float AttackCost = 1f;
        public float AbilityCost = 5f;
        public float ChestCost = 25f;
        public float ShieldDuration = 2.5f;

        [Header("Estética neon-gótica")]
        public bool TintCamera = true;
        public Color BackgroundColor = new Color(0.05f, 0.06f, 0.10f, 1f);
        public Color Scarlet = new Color(0.84f, 0.15f, 0.24f, 1f);
        public bool DegradeMalachiVisual = true;

        [Header("Baús automáticos")]
        public bool SpawnChests = true;
        public int ChestCount = 3;
        public bool SpawnAltar = true;

        [Header("Andares da torre + Chefe (automático)")]
        public bool AutoFloors = true;     // limpar inimigos = descer um andar
        public int TotalFloors = 3;        // GDD: 3 andares
        public int EnemiesPerFloor = 2;    // inimigos gerados por andar novo
        public bool AutoBoss = true;       // último andar nasce com Chefe
        public float BossHp = 600f;        // 6 tiros para derrotar

        [Header("Áudio (gerado por código — chiptune)")]
        public bool EnableAudio = true;

        [Header("Vitória / Derrota (apenas sem FloorManager)")]
        public bool HandleWinLose = true;

        private float _initialMax;
        private bool _sawEnemies;
        private bool _finished;
        private bool _victory;
        private bool _hasFloorManager;
        private int _floor = 1;
        private readonly List<GameObject> _enemyTemplates = new();

        private AudioSource _sfx, _music;
        private AudioClip _shootClip, _hurtClip, _shieldClip, _chestClip, _victoryClip, _defeatClip;
        private int _lastShoot, _lastHurt, _lastShield, _lastChest;
        private bool _endSoundPlayed;

        private Texture2D _frameTex, _barBgTex, _fillTex, _shieldTex, _overlayTex;
        private GUIStyle _hpStyle, _hintStyle, _bigStyle;

        private void Start()
        {
            _hasFloorManager = FindFirstObjectByType<FloorManager>() != null;
            PlayerVitals.Floor = 1; // reseta ao (re)iniciar a cena
            GameSfx.Reset();

            _frameTex = SolidTex(new Color(0f, 0f, 0f, 0.65f));
            _barBgTex = SolidTex(new Color(0.15f, 0.05f, 0.07f, 1f));
            _fillTex = SolidTex(Scarlet);
            _shieldTex = SolidTex(new Color(0.55f, 0.8f, 1f, 1f));
            _overlayTex = SolidTex(new Color(0.03f, 0.02f, 0.05f, 0.82f));

            ConfigurePlayer();

            if (AutoFloors || AutoBoss)
                CaptureEnemyTemplates();

            // Sem andares mas com chefe: promove um inimigo já presente.
            if (AutoBoss && !AutoFloors)
                PromoteExistingEnemyToBoss();

            if (TintCamera && Camera.main != null)
                Camera.main.backgroundColor = BackgroundColor;

            if (SpawnChests || SpawnAltar)
                TrySpawnInteractables();

            if (EnableAudio)
                SetupAudio();
        }

        // --- Configura o jogador (sem precisar do PlayerCurseRegistrar) ---
        private void ConfigurePlayer()
        {
            if (!Game.Query<All<IsPlayer>>().One(out var player))
            {
                Debug.LogWarning("[PagueComVida] Jogador (IsPlayer) não encontrado. A cena tem um Player?");
                return;
            }

            // Força o HP do GDD (~100). O prefab vem com 1000, o que diluiria a
            // economia de "vida-moeda" (tiro -1 viraria 0,1% em vez de 1%).
            if (!player.Has<Hp>())
                player.Add<Hp>();
            ref Hp hp = ref player.Mut<Hp>();
            hp.Max = StartMaxHp;
            hp.Current = StartMaxHp;

            if (!player.Has<LifeCosts>())
            {
                ref LifeCosts c = ref player.Add<LifeCosts>();
                c.Attack = AttackCost;
                c.Ability = AbilityCost;
                c.Chest = ChestCost;
            }

            if (!player.Has<ShieldDuration>())
                player.Add<ShieldDuration>().Value = ShieldDuration;

            // Munição deixa de ser o limite (a vida é). Mantém o filtro de tiro válido.
            if (player.Has<AmmoCount>())
                player.Mut<AmmoCount>().Value = 9999;

            _initialMax = player.Read<Hp>().Max;
        }

        private void Update()
        {
            if (DegradeMalachiVisual)
                UpdateMalachiTint();

            if (HandleWinLose && !_finished && !_hasFloorManager)
                UpdateWinLose();

            if (EnableAudio)
                UpdateSfx();

            if (_finished && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // --- Malachi degrada visualmente conforme o HP MÁXIMO cai (GDD) ---
        private void UpdateMalachiTint()
        {
            if (_initialMax <= 0f) return;
            if (!Game.Query<All<IsPlayer>>().One(out var player)) return;
            if (!player.Has<SpriteRenderer>()) return;

            var sr = player.Read<SpriteRenderer>().Value;
            if (sr == null) return;

            if (PlayerVitals.ShieldActive)
            {
                sr.color = new Color(0.6f, 0.8f, 1f, 1f); // brilho do escudo
                return;
            }

            float ratio = Mathf.Clamp01(PlayerVitals.Max / _initialMax);
            sr.color = Color.Lerp(new Color(0.45f, 0.35f, 0.45f, 1f), Color.white, ratio);
        }

        private void UpdateWinLose()
        {
            if (!PlayerVitals.Alive && _sawEnemies)
            {
                _finished = true;
                _victory = false;
                return;
            }

            bool anyEnemy = AnyEnemyAlive();
            if (anyEnemy) _sawEnemies = true;

            if (_sawEnemies && !anyEnemy)
            {
                if (AutoFloors && _floor < TotalFloors)
                {
                    _floor++;
                    PlayerVitals.Floor = _floor;
                    _sawEnemies = false;
                    SpawnFloorWave(_floor);
                }
                else
                {
                    _finished = true;
                    _victory = true;
                }
            }
        }

        // ===================== Andares + Chefe =====================
        private void CaptureEnemyTemplates()
        {
            try
            {
                foreach (var provider in FindObjectsByType<GameEntityProvider>(FindObjectsSortMode.None))
                {
                    if (!provider.Entity.Has<IsEnemy>()) continue;

                    var template = Instantiate(provider.gameObject);
                    ClearClonedEntityGids(template); // sem isto, o SetActive(false) do clone DESATIVA o inimigo original
                    template.name = provider.gameObject.name + "_TEMPLATE";
                    template.SetActive(false);
                    _enemyTemplates.Add(template);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PagueComVida] Não preparei os andares ({ex.Message}). Segue como arena única.");
            }
        }

        private void SpawnFloorWave(int floor)
        {
            try
            {
                if (_enemyTemplates.Count == 0)
                {
                    Debug.LogWarning($"[PagueComVida] Sem inimigos-modelo para o andar {floor}; mantendo o estado atual.");
                    return; // falha de spawn NÃO é vitória
                }

                Vector2 center = Vector2.zero;
                if (Game.Query<All<IsPlayer, TransformComponent>>().One(out var player))
                    center = player.Read<TransformComponent>().Value.position;

                bool bossFloor = AutoBoss && floor >= TotalFloors;
                int count = bossFloor ? 1 : Mathf.Max(1, EnemiesPerFloor); // GDD: só o Chefe no andar final

                for (int i = 0; i < count; i++)
                {
                    var template = _enemyTemplates[i % _enemyTemplates.Count];
                    float ang = (Mathf.PI * 2f / count) * i;
                    Vector2 pos = center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * 4f;

                    // Projeta o ponto na NavMesh para o inimigo não nascer "preso" fora dela.
                    if (UnityEngine.AI.NavMesh.SamplePosition(pos, out var navHit, 6f, UnityEngine.AI.NavMesh.AllAreas))
                        pos = navHit.position;

                    var clone = Instantiate(template, pos, Quaternion.identity);
                    clone.name = $"Enemy_F{floor}_{i}";
                    clone.SetActive(true);
                    ActivateEnemyEntity(clone);

                    var agent = clone.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>(true);
                    if (agent != null && agent.isActiveAndEnabled)
                        agent.Warp(pos);

                    if (bossFloor && i == 0)
                        PromoteToBoss(clone);
                }

                // Um baú novo por andar mantém o loop "pague com a vida".
                if (SpawnChests)
                    SpawnInteractable($"Bau_F{floor}", center + new Vector2(0f, -6f),
                        new Color(0.85f, 0.7f, 0.2f), ChestCost, 0f, floor % 4, 15f);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PagueComVida] Falha ao gerar o andar {floor} ({ex.Message}). Mantendo o estado atual.");
            }
        }

        // O clone herda o EntityGid (vivo) do original. Como o OnDisable/OnEnable do
        // provider faz entityGid.TryUnpack(...).Disable()/Enable(), um clone com GID
        // copiado (des)ativaria a ENTIDADE ORIGINAL. Zerar o GID antes de qualquer
        // SetActive corta esse cross-talk; o GID correto é criado depois em CreateEntity().
        private static void ClearClonedEntityGids(GameObject go)
        {
            foreach (var p in go.GetComponentsInChildren<AbstractStaticEcsEntityProvider>(true))
                p.EntityGid = default;
        }

        private static void ActivateEnemyEntity(GameObject clone)
        {
            foreach (var provider in clone.GetComponentsInChildren<AbstractStaticEcsEntityProvider>(true))
                provider.CreateEntity();
            foreach (var root in clone.GetComponentsInChildren<RootGameComponentsRegistrar>(true))
                root.RegisterComponents();
        }

        private void PromoteToBoss(GameObject clone)
        {
            try
            {
                var provider = clone.GetComponentInChildren<GameEntityProvider>(true);
                if (provider == null) return;
                BoostToBoss(provider.Entity);
                clone.transform.localScale *= 1.8f;
                clone.name = "CHEFE";
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PagueComVida] Falha ao criar o chefe ({ex.Message}).");
            }
        }

        private void PromoteExistingEnemyToBoss()
        {
            try
            {
                if (Game.Query<All<IsEnemy>>().One(out var enemy))
                    BoostToBoss(enemy);
            }
            catch { }
        }

        private void BoostToBoss(Game.Entity enemy)
        {
            if (enemy.Has<Hp>())
            {
                ref Hp hp = ref enemy.Mut<Hp>();
                hp.Max = BossHp;
                hp.Current = BossHp;
            }
            else
            {
                ref Hp hp = ref enemy.Add<Hp>();
                hp.Max = BossHp;
                hp.Current = BossHp;
            }
            enemy.Set<IsBoss>();
        }

        private static bool AnyEnemyAlive()
        {
            try { return Game.Query<All<IsEnemy>>().One(out var _ignored); }
            catch { return false; }
        }

        // ===================== HUD via IMGUI =====================
        private void OnGUI()
        {
            EnsureStyles();

            const float x = 24f, y = 22f, w = 360f, h = 26f;

            GUI.DrawTexture(new Rect(x - 4, y - 4, w + 8, h + 8), _frameTex);
            GUI.DrawTexture(new Rect(x, y, w, h), _barBgTex);

            float pct = PlayerVitals.Max > 0f ? Mathf.Clamp01(PlayerVitals.Current / PlayerVitals.Max) : 0f;
            if (!PlayerVitals.Alive) pct = 0f;
            GUI.DrawTexture(new Rect(x, y, w * pct, h), PlayerVitals.ShieldActive ? _shieldTex : _fillTex);

            string vida = PlayerVitals.Alive
                ? $"{Mathf.CeilToInt(PlayerVitals.Current)} / {Mathf.CeilToInt(PlayerVitals.Max)}"
                : "0 / 0";
            GUI.Label(new Rect(x, y, w, h), vida, _hpStyle);

            GUI.Label(new Rect(x, y + h + 6, 400, 24),
                $"Andar {PlayerVitals.Floor}" + (PlayerVitals.ShieldActive ? "   •   ESCUDO ATIVO" : ""), _hintStyle);

            GUI.Label(new Rect(x, Screen.height - 34, Screen.width - 40, 26),
                "Botão Esq: Atirar (-1)    Espaço: Esquiva (-5)    Botão Dir: Escudo (-5)    E: Baú/Altar", _hintStyle);

            if (_finished)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _overlayTex);
                string msg = _victory ? "A TORRE FOI VENCIDA" : "VOCÊ PAGOU O PREÇO FINAL";
                _bigStyle.normal.textColor = _victory ? new Color(0.7f, 0.85f, 1f) : Scarlet;
                GUI.Label(new Rect(0, Screen.height * 0.5f - 80, Screen.width, 80), msg, _bigStyle);
                GUI.Label(new Rect(0, Screen.height * 0.5f + 10, Screen.width, 40), "[R] Recomeçar", _hintStyle2());
            }
        }

        private GUIStyle _hintCenterCache;
        private GUIStyle _hintStyle2()
        {
            if (_hintCenterCache == null)
            {
                _hintCenterCache = new GUIStyle(_hintStyle) { alignment = TextAnchor.UpperCenter, fontSize = 22 };
            }
            return _hintCenterCache;
        }

        private void EnsureStyles()
        {
            if (_hpStyle != null) return;

            _hpStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
            _hpStyle.normal.textColor = Color.white;

            _hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 15 };
            _hintStyle.normal.textColor = new Color(0.85f, 0.85f, 0.95f, 0.95f);

            _bigStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 48,
                fontStyle = FontStyle.Bold
            };
        }

        // ===================== Baús/Altares por código =====================
        private void TrySpawnInteractables()
        {
            try
            {
                Vector2 center = Vector2.zero;
                if (Game.Query<All<IsPlayer, TransformComponent>>().One(out var player))
                    center = player.Read<TransformComponent>().Value.position;

                if (SpawnChests)
                {
                    for (int i = 0; i < ChestCount; i++)
                    {
                        float ang = (Mathf.PI * 2f / Mathf.Max(1, ChestCount)) * i;
                        Vector2 pos = center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * 6f;
                        SpawnInteractable($"Bau_{i}", pos, new Color(0.85f, 0.7f, 0.2f),
                            currentHpPrice: ChestCost, maxHpPercent: 0f, rewardKind: i % 4, rewardHeal: 15f);
                    }
                }

                if (SpawnAltar)
                {
                    SpawnInteractable("Altar", center + new Vector2(0f, 9f), new Color(0.5f, 0.2f, 0.7f),
                        currentHpPrice: 0f, maxHpPercent: 0.08f, rewardKind: 1, rewardHeal: 0f);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PagueComVida] Falha ao criar baús por código ({e.Message}). " +
                                 "Use o InteractableRegistrar manualmente (ver PAGUE_COM_A_VIDA_SETUP.md seção 4).");
            }
        }

        private void SpawnInteractable(string name, Vector2 pos, Color color,
            float currentHpPrice, float maxHpPercent, int rewardKind, float rewardHeal)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            var sr = go.AddComponent<UnityEngine.SpriteRenderer>();
            sr.sprite = MakeSolidSprite(color);
            sr.sortingOrder = 5;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2.2f, 2.2f);

            // Rigidbody2D estático garante que o trigger dispare mesmo que o
            // player perca o seu (não depende implicitamente do corpo do player).
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            // Ordem importa: provider antes do broadcaster (RequireComponent).
            var provider = go.AddComponent<GameEntityProvider>();
            go.AddComponent<CollisionEventBroadcaster>();

            provider.CreateEntity();
            var entity = provider.Entity;

            entity.Set<IsInteractable>();
            ref LifePrice price = ref entity.Add<LifePrice>();
            price.CurrentHp = currentHpPrice;
            price.MaxHpPercent = maxHpPercent;
            entity.Add<RewardKind>().Value = rewardKind;
            entity.Add<RewardHeal>().Value = rewardHeal;
            if (!entity.Has<TransformComponent>())
                entity.Add<TransformComponent>().Value = go.transform;
        }

        // ===================== Áudio chiptune (gerado por código) =====================
        private void SetupAudio()
        {
            try
            {
                _sfx = gameObject.AddComponent<AudioSource>();
                _sfx.playOnAwake = false;

                _music = gameObject.AddComponent<AudioSource>();
                _music.playOnAwake = false;
                _music.loop = true;
                _music.volume = 0.16f;

                _shootClip = Tone(440f, 0.08f, 0.25f, 220f);
                _hurtClip = Tone(160f, 0.18f, 0.30f, 70f);
                _shieldClip = Tone(330f, 0.20f, 0.25f, 680f);
                _chestClip = Tone(523f, 0.20f, 0.25f, 1046f);
                _victoryClip = Arpeggio(new[] { 523f, 659f, 784f, 1046f }, 0.13f, 0.26f);
                _defeatClip = Arpeggio(new[] { 330f, 262f, 196f, 131f }, 0.17f, 0.28f);

                _music.clip = MusicLoop();
                _music.Play();

                _lastShoot = GameSfx.Shoot; _lastHurt = GameSfx.Hurt;
                _lastShield = GameSfx.Shield; _lastChest = GameSfx.Chest;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PagueComVida] Áudio desativado ({ex.Message}).");
            }
        }

        private void UpdateSfx()
        {
            if (_sfx == null) return;

            if (GameSfx.Shoot != _lastShoot) { _lastShoot = GameSfx.Shoot; _sfx.PlayOneShot(_shootClip, 0.55f); }
            if (GameSfx.Hurt != _lastHurt) { _lastHurt = GameSfx.Hurt; _sfx.PlayOneShot(_hurtClip); }
            if (GameSfx.Shield != _lastShield) { _lastShield = GameSfx.Shield; _sfx.PlayOneShot(_shieldClip); }
            if (GameSfx.Chest != _lastChest) { _lastChest = GameSfx.Chest; _sfx.PlayOneShot(_chestClip); }

            if (_finished && !_endSoundPlayed)
            {
                _endSoundPlayed = true;
                if (_music != null) _music.Stop();
                _sfx.PlayOneShot(_victory ? _victoryClip : _defeatClip);
            }
        }

        // Bip de onda quadrada com envelope decrescente (e opcional sweep de frequência).
        private static AudioClip Tone(float freq, float dur, float vol, float sweepTo = -1f)
        {
            const int rate = 44100;
            int n = Mathf.Max(1, (int)(rate * dur));
            var s = new float[n];
            for (int i = 0; i < n; i++)
            {
                float p = (float)i / n;
                float f = sweepTo > 0f ? Mathf.Lerp(freq, sweepTo, p) : freq;
                float w = Mathf.Sin(2f * Mathf.PI * f * (i / (float)rate)) >= 0f ? 1f : -1f;
                s[i] = w * vol * (1f - p);
            }
            var c = AudioClip.Create("sfx", n, 1, rate, false);
            c.SetData(s, 0);
            return c;
        }

        private static AudioClip Arpeggio(float[] notes, float perNote, float vol)
        {
            const int rate = 44100;
            int nn = Mathf.Max(1, (int)(rate * perNote));
            var s = new float[nn * notes.Length];
            for (int k = 0; k < notes.Length; k++)
                for (int i = 0; i < nn; i++)
                {
                    float w = Mathf.Sin(2f * Mathf.PI * notes[k] * (i / (float)rate)) >= 0f ? 1f : -1f;
                    s[k * nn + i] = w * vol * (1f - (float)i / nn);
                }
            var c = AudioClip.Create("arp", s.Length, 1, rate, false);
            c.SetData(s, 0);
            return c;
        }

        // Loop de música estilo masmorra: melodia em onda quadrada + baixo.
        private static AudioClip MusicLoop()
        {
            const int rate = 22050;
            const float noteDur = 0.20f;
            float[] seq =
            {
                196, 233, 196, 174,  196, 233, 262, 233,
                174, 207, 174, 155,  174, 207, 233, 207
            };
            int nn = (int)(rate * noteDur);
            var s = new float[nn * seq.Length];
            for (int k = 0; k < seq.Length; k++)
                for (int i = 0; i < nn; i++)
                {
                    float t = i / (float)rate;
                    float lead = Mathf.Sin(2f * Mathf.PI * seq[k] * t) >= 0f ? 1f : -1f;
                    float bass = Mathf.Sin(2f * Mathf.PI * (seq[k] * 0.5f) * t);
                    float env = Mathf.Clamp01(1f - (float)i / nn * 0.8f);
                    s[k * nn + i] = (lead * 0.35f + bass * 0.5f) * 0.4f * env;
                }
            var c = AudioClip.Create("music", s.Length, 1, rate, false);
            c.SetData(s, 0);
            return c;
        }

        // ===================== utilitários =====================
        private static Texture2D SolidTex(Color c)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, c);
            tex.Apply();
            return tex;
        }

        private static Sprite MakeSolidSprite(Color c)
        {
            var tex = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }
    }
}
