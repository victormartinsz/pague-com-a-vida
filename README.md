# Pague com a Vida (TUDO TEM PREÇO)

Jogo desenvolvido como **trabalho da disciplina (produzido durante a aula)** —
Turma 13873, 2026/1.

**Integrantes da equipe:**
- João Victor Lima Martins
- Ranilson Folha

---

## Resumo do projeto (do GDD)

**Pague com a Vida** é um *rogue-lite* de exploração de masmorra em que **a sua barra
de vida é a sua única moeda**. Você controla **Malachi, um alquimista amaldiçoado**,
que precisa descer pelos andares de uma torre mutável gastando HP em tudo:

- **Atacar** custa 1 de vida por tiro;
- **Habilidades** (esquiva e escudo) custam 5 de vida;
- **Abrir baús** custa 25 de vida;
- **Melhorias permanentes** (altar) queimam parte do seu HP **máximo**.

O tema central é: **tudo tem um preço** — cada ação é uma decisão entre poder e
sobrevivência. A estética é pixel art com clima neon-gótico (paleta fria e escura,
vida em vermelho-escarlate), e o personagem se degrada visualmente conforme o HP
máximo diminui.

Escopo: 3 andares, inimigos com IA de utilidade (procuram cobertura e revidam) e um
**Chefe** no andar final. Áudio chiptune (música em loop + efeitos sonoros de tiro,
dano, escudo e baú) é gerado proceduralmente por código.

## Como jogar

| Ação | Tecla |
|------|-------|
| Mover | WASD / setas |
| Mirar | Mouse |
| Atirar (−1 HP) | Botão esquerdo |
| Esquiva (−5 HP) | Espaço |
| Escudo (−5 HP) | Botão direito |
| Abrir baú / altar (−25 HP) | E |
| Descer / recomeçar | (automático) / R |

Objetivo: limpe os inimigos de cada andar e derrote o Chefe da torre — sem zerar a
sua vida, que é gasta a cada ação.

## Executável

A pasta [`build/`](./build) contém o executável do jogo (`.exe`).

## Como abrir o projeto (Unity)

1. Unity **6000.3.10f1**.
2. Abra a pasta `src/StaticTopDownShooter/` pelo Unity Hub (primeiro import resolve
   os pacotes via git — precisa de internet).
3. Abra a cena `Assets/_Project/Scenes/Battle.unity` e aperte **Play**. Tudo já está
   configurado na cena (o componente `PagueComVidaSetup` cuida do setup automático).

Guia detalhado das mecânicas e da configuração: [`PAGUE_COM_A_VIDA_SETUP.md`](./PAGUE_COM_A_VIDA_SETUP.md).

## Direitos dos autores / créditos de terceiros

Arte, ferramentas e projeto-base de terceiros usados:

- **Projeto base / template:** *StaticTopDownShooter* por **d4nilevi4** — usado sob
  licença MIT (ver [`LICENSE.md`](./LICENSE.md)). Forneceu o núcleo do top-down
  shooter (movimento, tiro, inimigos com Utility AI, estrutura ECS).
- **Tiny Swords** (pixel art) — por *Pixel Frog*: https://pixelfrog-assets.itch.io/tiny-swords
- **StaticEcs** (framework ECS) — Felid-Force Studios: https://github.com/Felid-Force-Studios/StaticEcs
- **StaticEcs-Unity / StaticPack** — Felid-Force Studios
- **NavMeshPlus** (NavMesh 2D) — h8man: https://github.com/h8man/NavMeshPlus
- **Wise Feline (Lite)** (Utility AI) — NoOpArmy

As **mecânicas de "Pague com a Vida"** — vida como moeda, escudo, HUD, sistema de
baús e altares, andares, chefe, degradação visual do Malachi e áudio chiptune —
foram acrescentadas pela equipe sobre o template, durante a aula.
