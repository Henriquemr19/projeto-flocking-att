# Simulação de Bando Emergente (Boids) - Unity 6

Projeto desenvolvido no Unity 6 utilizando o algoritmo **Boids**, criado por Craig Reynolds em 1987. A simulação permite que 150 agentes se movimentem em grupo, desviem de obstáculos e reajam ao ambiente sem precisar de um líder ou movimentos programados.

O comportamento dos agentes é controlado por três regras principais: separação, alinhamento e coesão.

---

## Integrantes
- Henrique Miranda

---

## Como Abrir e Executar o Projeto

1. **Requisitos:** Unity 6 (recomendado: versão 6000.3.6f1 ou superior).
2. **Abrir o projeto:** Abra o Unity Hub, clique em **Add** e selecione a pasta `projeto flocking att`.
3. **Abrir a cena:** No painel Project, acesse `Assets/Scenes/` e abra `FlockingScene.unity`.
4. **Iniciar:** Clique em **Play** no topo do editor.

### Visualização e Debug

- Na aba **Stats**, dentro do painel Game, é possível acompanhar o FPS.
- Na janela Scene, os Gizmos mostram a área de percepção dos boids e o limite do mundo.
- Para alterar os parâmetros, selecione `FlockManager` na Hierarchy e edite os valores no Inspector.

---

## Tabela de Parâmetros

Valores utilizados na configuração final da simulação:

| Parâmetro | Valor | Descrição |
| :--- | :---: | :--- |
| **Quantidade de Boids** | `150` | Quantidade de agentes na simulação |
| **Peso da Separação** | `2.5` | Força que evita a aproximação excessiva |
| **Peso do Alinhamento** | `1.8` | Ajusta a direção dos agentes com base nos vizinhos |
| **Peso da Coesão** | `1.2` | Atrai os agentes para o centro do grupo |
| **Raio de Percepção** | `2.5 m` | Distância máxima para detectar vizinhos |
| **Distância de Separação** | `1.2 m` | Distância em que a repulsão começa a atuar |
| **Velocidade Máxima (`maxSpeed`)** | `8.0 m/s` | Velocidade máxima dos agentes |
| **Velocidade Mínima (`minSpeed`)** | `2.0 m/s` | Evita que os agentes fiquem parados |
| **Força Máxima (`maxForce`)** | `15.0 N` | Limite da força de movimentação |
| **Velocidade de Rotação** | `6.0` | Suavidade na mudança de direção |
| **Raio Limite do Mundo** | `22.0 m` | Limite central da área de movimentação |
| **Peso de Retorno ao Mundo** | `6.0` | Força que traz os agentes de volta |
| **Distância da Sonda de Obstáculo** | `4.5 m` | Distância frontal de detecção |
| **Raio da Sonda (`SphereCast`)** | `0.6 m` | Raio utilizado na detecção de obstáculos |
| **Peso de Desvio de Obstáculos** | `18.0` | Força aplicada para desviar dos obstáculos |
| **Raio de Pânico do Predador** | `7.0 m` | Distância em que os agentes começam a fugir |
| **Peso de Fuga do Predador** | `22.0` | Intensidade da força de fuga |

---

## Parte B — Experimentos

A seguir estão os resultados dos testes realizados para observar como cada parâmetro afeta o comportamento do bando.

### 1. Teste de Separação Nula (`w_sep = 0`)

Ao zerar a separação, os agentes deixam de evitar uns aos outros. Como a coesão continua atraindo todos para o centro do grupo, os boids acabam se sobrepondo, formando uma massa única e muito concentrada.

O bando perde seu formato tridimensional e deixa de apresentar uma movimentação natural.

*Captura de tela: `Screenshots/teste1_separacao_zero.png`*

### 2. Teste de Alinhamento Nulo (`w_align = 0`)

Sem o alinhamento, os agentes deixam de sincronizar suas direções com as dos vizinhos.

A coesão e a separação continuam funcionando, mas o grupo perde sua movimentação coordenada. Os boids passam a se movimentar de maneira desordenada, orbitando e mudando de direção constantemente.

*Captura de tela: `Screenshots/teste2_alinhamento_zero.png`*

### 3. Raio de Percepção: 0,5 m versus 5,0 m

**Com raio de 0,5 m:**

Os agentes detectam poucos vizinhos, dificultando a organização do grupo. O bando se divide em pequenos grupos de 2 a 4 indivíduos, além de alguns agentes isolados.

**Com raio de 5,0 m:**

Os agentes conseguem detectar muito mais vizinhos, aumentando a coesão do grupo. O resultado é um bando maior e mais organizado, com movimentos amplos e curvas mais suaves.

*Capturas de tela: `Screenshots/teste3_raio_05.png` e `Screenshots/teste3_raio_50.png`*

### 4. Predador — Comportamento de Fuga

Uma esfera chamada `Predador` persegue o centro do bando.

Quando entra no raio de pânico de `7.0 m`, os agentes recebem uma força de fuga com peso `22.0`, fazendo com que se afastem do predador.

O grupo se abre para os lados, criando espaço para evitar o ataque. Quando o predador se afasta, a força de fuga deixa de atuar e a coesão faz os agentes se reunirem novamente.

*Captura de tela: `Screenshots/teste4_predador.png`*

---

## Parte C — Desafio Opcional: Grid Espacial 3D

Para melhorar o desempenho com **500 ou mais boids**, foi implementado um sistema de Grid Espacial 3D (`SpatialGrid.cs`).

Essa estrutura organiza os agentes em células, reduzindo a quantidade de verificações necessárias para encontrar vizinhos.

### Funcionamento

1. **Divisão do espaço:** O ambiente é dividido em células cúbicas de `2.0 m` (`gridCellSize = 2.0f`).
2. **Armazenamento:** Um `Dictionary<Vector3Int, List<Boid>>` organiza os agentes. As listas são reutilizadas por meio de um pool (`listPool`), evitando alocações desnecessárias de memória a cada frame.
3. **Busca de vizinhos:** Cada agente verifica apenas as 27 células próximas, reduzindo o custo da busca de vizinhos de `O(N²)` para `O(N)` amortizado.

### Comparativo de Desempenho no Unity Profiler

| Cenário | Método de Busca | Tempo de Scripts (CPU) | Frame Time | FPS Médio | GC Alloc |
| :--- | :--- | :---: | :---: | :---: | :---: |
| **150 Boids** | `OverlapSphereNonAlloc` | ~1.8 ms | ~5.2 ms | **60+ FPS** | 0 B/frame |
| **150 Boids** | `SpatialGrid` | ~0.9 ms | ~3.8 ms | **60+ FPS** | 0 B/frame |
| **500 Boids** | `OverlapSphereNonAlloc` | ~13.5 ms | ~19.4 ms | **~51 FPS** | 0 B/frame |
| **500 Boids** | `SpatialGrid` | **~3.2 ms** | **~8.1 ms** | **60+ FPS** | **0 B/frame** |

### Conclusão

Com 500 boids, o `OverlapSphereNonAlloc` apresenta um custo maior de processamento.

A implementação do `SpatialGrid` reduziu em aproximadamente **76% o tempo de processamento dos scripts**, permitindo executar a simulação com 500 agentes a 60 FPS ou mais, mantendo o consumo de GC em zero.

---

## Vídeo de Demonstração (30 a 60 segundos)

O vídeo mostra a movimentação dos 150 agentes, o desvio de obstáculos e a reação do grupo à aproximação do predador.

Enviei o video no teams na propria atividade pois estava muito pesado por algum motivo


---

## Estrutura do Repositório

```text
projeto flocking att/
├── Assets/
│   ├── Materials/       # Materiais dos boids, obstáculos e predador
│   ├── Prefabs/         # Prefabs utilizados na simulação
│   ├── Scenes/          # Cena principal
│   └── Scripts/         # Scripts C# da simulação
├── ProjectSettings/     # Configurações do projeto e Layers
├── Screenshots/         # Capturas dos experimentos
├── .gitignore           # Arquivos e pastas ignorados pelo Git
└── README.md            # Documentação do projeto
```
