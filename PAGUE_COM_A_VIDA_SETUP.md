# Pague com a Vida — Guia de Configuração

Este projeto (top-down shooter ECS) foi adaptado para o tema **"Pague com a Vida"**:
sua barra de vida é a sua única moeda. Todo o **código** já está pronto. Faltam
alguns passos no **Editor do Unity** (criar prefabs, UI e arrastar referências) —
eu não consigo abrir o Unity, então siga este guia.

> ⚠️ Importante: abra o projeto no Unity **6000.3.10f1** e deixe ele importar/compilar.
> Os scripts novos geram seus `.meta` automaticamente no primeiro import.

---

## 0. ⭐ MODO FÁCIL — 1 clique (recomendado para entregar hoje)

Em vez de configurar tudo na mão (seções 2 a 6), use o setup automático:

1. Abra a cena `Assets/_Project/Scenes/Battle.unity`.
2. `GameObject > Create Empty`, renomeie para **`PagueComVida`**.
3. **Add Component → `PagueComVidaSetup`**.
4. Aperte **Play**.

Ele faz sozinho, em tempo de execução:
- Configura o Malachi (vida 100, custos 1/5/25, escudo) — sem registrars.
- Monta a **HUD** (barra escarlate, vida, andar, dicas de controle).
- Telas de **Game Over** e **Vitória** (com **R** para recomeçar).
- **Degradação visual** do Malachi conforme o HP máximo cai.
- Tinta a câmera no tom **neon-gótico** escuro.
- Espalha **3 baús + 1 altar** ao redor do jogador.

Tudo é ajustável no Inspector do componente (custos, cores, nº de baús etc.).

> Se algo no Console reclamar dos baús por código, é só desmarcar `Spawn Chests`
> e usar a seção 4 (baús manuais). O resto continua funcionando.
>
> As seções abaixo (2 a 6) são o **modo manual/avançado** — só precisa delas se
> quiser controle total ou se o modo fácil falhar em algo.

---

## 1. O que já funciona em código (sem mexer em nada)

| Mecânica do GDD | Como ficou |
|---|---|
| Ataque básico custa **1 HP** | `BulletSpawnSystem` desconta `LifeCosts.Attack` ao atirar |
| Habilidade (dash) custa **5 HP** | `DashPhysSystem` desconta `LifeCosts.Ability` |
| **Escudo** (nova habilidade, 5 HP) | Botão **direito do mouse** → 2.5s de imunidade |
| Baú custa **25 HP** | Sistema de interação (tecla **E**) |
| Melhoria permanente **−5%..10% HP máx** | "Altar" — mesmo sistema, paga com HP máximo |
| Inimigos + Chefe | Inimigos atuais + `BossRegistrar` (muito mais HP) |
| 3 andares da torre | `FloorManager` (opcional — ver seção 6) |
| Vida em escarlate / HUD | `LifeBarUI` (você monta o Canvas) |

**Regra-chave:** você nunca consegue se matar com uma compra — uma ação é
bloqueada se o custo levaria seu HP a 0 (precisa **sobrar** vida).

### Controles
| Ação | Tecla |
|---|---|
| Mover | WASD / setas |
| Mirar | Mouse |
| Atirar (−1 HP) | Botão esquerdo |
| Esquiva/Dash (−5 HP) | Espaço |
| **Escudo (−5 HP)** | **Botão direito** |
| **Interagir baú/altar (E)** | **E** |
| **Descer andar** | **automático** (modo fácil) / **Enter** (modo manual, seção 6) |

---

## 2. Configurar o Jogador (Malachi) — MODO MANUAL (pule se usou a Seção 0)

> Só faça esta seção se NÃO estiver usando o `PagueComVidaSetup` (Seção 0). O setup
> automático já adiciona `LifeCosts` e `ShieldDuration` ao Player em runtime, então o
> `PlayerCurseRegistrar` é desnecessário no caminho de 1 clique.

No GameObject do **Player** na cena `Battle.unity`:

1. Confirme que ele tem o componente **`Hp`** no `GameEntityProvider` (lista de
   componentes). Defina, por ex., **Max = 100, Current = 100**.
2. Confirme que ele tem **`AmmoCount`** (qualquer valor, ex. 999 — não é mais
   gasto enquanto houver `LifeCosts`).
3. No `RootGameComponentsRegistrar` do Player, na lista **ComponentsRegistrars**,
   adicione o componente novo **`PlayerCurseRegistrar`** (Add Component) e arraste-o
   para a lista. Valores padrão já seguem o GDD:
   - Attack Cost = **1**
   - Ability Cost = **5**
   - Chest Cost Reference = **25**
   - Shield Duration = **2.5**

> Sem o `PlayerCurseRegistrar`, o jogador não "paga com a vida" (atira/dasha de graça).

---

## 3. HUD da vida (barra escarlate) — AUTOMÁTICA

Não precisa montar Canvas. O `PagueComVidaSetup` (seção 0) já desenha a HUD por
código (IMGUI): barra escarlate de vida, "atual / máximo", andar, indicador de
escudo, dicas de controle e telas de Game Over / Vitória.

> Decisão técnica: a HUD usa IMGUI (`OnGUI`) de propósito, porque o assembly
> `Shooter` não referencia o pacote uGUI — assim nada quebra a compilação.
> Se quiser depois trocar por uma HUD uGUI bonita, é só adicionar a referência
> `UnityEngine.UI` no `Shooter.asmdef` e montar um Canvas normal.

---

## 4. Baús (custam 25 HP) — RECOMENDADO

Crie um Prefab "Bau":

1. GameObject vazio (ou com o sprite do baú).
2. Adicione **`GameEntityProvider`**.
3. Adicione um **Collider2D** (ex. `BoxCollider2D`) e marque **Is Trigger ✔**.
   Deixe-o um pouco maior que o sprite (área de interação).
4. Adicione **`CollisionEventBroadcaster`** (já existe no projeto).
5. Adicione **`RootGameComponentsRegistrar`** e no campo `EntityProvider` arraste
   o próprio `GameEntityProvider`.
6. Adicione o componente **`InteractableRegistrar`** e arraste-o para a lista
   `ComponentsRegistrars` do passo 5. Configure:
   - Current Hp Price = **25**
   - Max Hp Percent Price = **0**
   - Reward Kind = `0` Velocidade, `1` Lâmina Afiada, `2` Proteção, `3` Disparo Rápido
   - Reward Heal = **15** (o "tesouro")
7. Espalhe instâncias do baú pela cena. Chegue perto e aperte **E**.

> Para o **PLAYER** ser reconhecido pelo baú, ele precisa de um Collider2D
> registrado — o que já acontece (é assim que as balas o acertam). Nada a fazer.

---

## 5. Altar Amaldiçoado (melhoria permanente, −% HP máx) — OPCIONAL

Igual ao baú (seção 4), mas no `InteractableRegistrar`:
- Current Hp Price = **0**
- Max Hp Percent Price = **0.08** (= −8% do HP máximo, dentro do 5–10% do GDD)
- Reward Heal = **0**
- Reward Kind = a melhoria que quiser

---

## 6. Andares da torre + Chefe — OPCIONAL (mais trabalhoso)

> Se faltar tempo, **pule**: deixe tudo numa arena só. O tema já está completo
> sem isto. Os andares são o único ponto que não consegui testar.

1. **Chefe**: num inimigo do último andar, adicione **`BossRegistrar`** à lista de
   registrars (Boss Hp = 600 → aguenta 6 tiros).
2. **Organize por andar**: crie 3 GameObjects vazios `Andar1`, `Andar2`, `Andar3`
   e coloque os inimigos/baús de cada andar como **filhos** do respectivo andar.
3. Num GameObject vazio, adicione **`FloorManager`**:
   - `Floor Roots` = [Andar1, Andar2, Andar3] (nessa ordem).
   - `Descend Prompt` = um texto "Andar limpo — ENTER para descer" (opcional).
   - `Victory Screen` / `Game Over Screen` = telas opcionais.
4. Funciona assim: o `FloorManager` desativa Andar2/3 no início; ao limpar os
   inimigos do andar atual aparece o prompt; **Enter** ativa o próximo andar e
   cria as entidades dele. Vencer o último = tela de vitória; morrer = game over.

---

## 7. Reskin visual / narrativo (neon-gótico)

O código não muda a arte. Para o clima do GDD:
- **Paleta**: fundo/tilemap em tons frios e escuros (azul-acinzentado, roxo). Na
  Camera (URP) você pode escurecer o ambiente e deixar a vida em **escarlate**
  como único ponto quente.
- **Malachi**: renomeie o GameObject do Player para `Malachi` e troque o sprite
  por um alquimista, se tiver. (A arte atual é "Tiny Swords" — medieval; serve de
  placeholder até trocar.)
- **Degradação visual**: o GDD pede que Malachi piore conforme o HP máximo cai.
  Gancho fácil: um pequeno MonoBehaviour que lê `PlayerVitals.Max` e escurece/
  dessatura o `SpriteRenderer` do player. (Posso escrever se quiser.)
- **Áudio**: música chiptune + 4 SFX (tiro, dano, escudo, baú).

---

## 8. Checklist mínimo para HOJE

**Caminho rápido (modo fácil):**
- [ ] Abrir no Unity 6000.3.10f1, deixar compilar (sem erros no Console).
- [ ] Criar GameObject vazio + componente **`PagueComVidaSetup`** (seção 0).
- [ ] Dar Play: HUD aparece, atirar baixa 1 HP, dash 5, botão direito = escudo,
      baús com E, matar todos = Vitória, morrer = Game Over.
- [ ] (Opcional) Marcar um inimigo como Chefe (`BossRegistrar`) e/ou andares (seção 6).
- [ ] (Opcional) Trocar arte para o clima neon-gótico (seção 7).

---

## Arquivos de código adicionados

```
Features/Bootstrap/      PagueComVidaSetup (setup automático + HUD IMGUI)
Features/LifeCurrency/   LifeCosts, LifeBank, PlayerVitals, Escudo, PlayerCurseRegistrar
Features/Interactables/  Baús/Altares, Upgrades, sistemas de interação
Features/Progression/    IsBoss, BossRegistrar, FloorManager
```
Sistemas registrados em `Core/Behaviours/EcsRunner.cs`. Edições em
`BulletSpawnSystem`, `DashPhysSystem`, `ApplyDamageSystem`.
