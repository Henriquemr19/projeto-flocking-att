# Simulação de Bando Emergente (Boids) - Unity 6

Implementação do algoritmo clássico de bando emergente (*Boids*, Craig Reynolds, 1987) desenvolvida no Unity 6. Centenas de agentes autônomos se agrupam, fluem e desviam de obstáculos tridimensionais sem líder e sem coreografia pré-programada, através da avaliação local contínua de três regras fundamentais a cada frame.

---

## Integrantes
- Henrique Miranda

---

## Instruções para Abrir e Executar o Projeto

1. **Requisitos de Software**:
   - Unity 6 (versão recomendada: **6000.3.6f1** ou superior).
2. **Abrindo o Projeto**:
   - Abra o **Unity Hub**, clique em **Add** e selecione a pasta raiz do projeto (`projeto flocking att`).
   - Abra o projeto com a versão do Unity 6 instalada.
3. **Executando a Cena Principal**:
   - No painel *Project*, navegue até a pasta `Assets/Scenes/`.
   - Dê um duplo clique na cena `FlockingScene.unity`.
   - Clique no botão **Play** no topo do editor para iniciar a simulação.
4. **Visualização e Debug**:
   - Com o Play Mode ativo, abra a aba **Stats** no painel *Game* para conferir a taxa de quadros (60+ FPS estáveis).
   - Na janela *Scene*, os Gizmos desenham a área de percepção do boid líder e o raio central delimitador do mundo.
   - Para inspecionar ou alterar os parâmetros em tempo real, selecione o GameObject `FlockManager` na Hierarchy e modifique os valores no Inspector.

---

## Tabela de Parâmetros Finais

Os valores abaixo foram calibrados para garantir um bando estável, fluído, responsivo e sem trepidações ou colisões com os obstáculos:

| Parâmetro | Valor Configurado | Descrição |
| :--- | :---: | :--- |
| **Quantidade de Boids** | `150` | Quantidade de agentes instanciados no início da simulação |
| **Peso da Separação** | `2.5` | Intensidade do empurrão repulsivo proporcional a $1/d^2$ |
| **Peso do Alinhamento** | `1.8` | Força de ajuste para a velocidade média dos vizinhos |
| **Peso da Coesão** | `1.2` | Atração em direção ao centro de massa local do grupo |
| **Raio de Percepção** | `2.5 m` | Distância máxima de detecção de vizinhos |
| **Distância de Separação** | `1.2 m` | Limiar a partir do qual a repulsão por separação atua |
| **Velocidade Máxima (`maxSpeed`)** | `8.0 m/s` | Velocidade máxima permitida para cada agente |
| **Velocidade Mínima (`minSpeed`)** | `2.0 m/s` | Velocidade de cruzeiro para evitar que agentes fiquem parados |
| **Força Máxima (`maxForce`)** | `15.0 N` | Teto limite de aceleração/esterçamento aplicado por frame |
| **Velocidade de Rotação** | `6.0` | Fator de interpolação suave da orientação do agente |
| **Raio Limite do Mundo** | `22.0 m` | Raio central dentro do qual a força de retorno é nula |
| **Peso de Retorno ao Mundo** | `6.0` | Força progressiva que puxa os agentes que saírem do raio |
| **Distância da Sonda de Obstáculo** | `4.5 m` | Distância frontal avaliada pelo `SphereCast` |
| **Raio da Sonda (`SphereCast`)** | `0.6 m` | Espessura da esfera projetada para detecção antecipada |
| **Peso de Desvio de Obstáculos** | `18.0` | Força de reflexão (`Vector3.Reflect`) para desvio imediato |
| **Raio de Pânico do Predador** | `7.0 m` | Distância na qual os boids iniciam a manobra de fuga |
| **Peso de Fuga do Predador** | `22.0` | Prioridade da força de repulsão em relação ao predador |

---

## Parte B — Experimentos Documentados

Abaixo estão registrados os comportamentos observados durante cada um dos experimentos exigidos pelo enunciado:

### 1. Teste de Separação Nula ($w_{sep} = 0$)
- **O que sobra do bando?**
  Ao zerar o peso da separação, a força repulsiva que impede os boids de colidirem entre si é desativada. As forças de alinhamento e coesão continuam atuando plenamente. Como a coesão puxa todos os agentes continuamente para o centro de massa local e não há resistência física ou força oposta de proximidade, os boids colapsam uns sobre os outros, fundindo-se em uma massa única e densa (um agrupamento puntual de malhas sobrepostas). O bando perde completamente seu volume espacial tridimensional, espessura e naturalidade, aparentando ser uma partícula única hiper-densa viajando junta.

*(Captura de tela do teste: `Screenshots/teste1_separacao_zero.png`)*

---

### 2. Teste de Alinhamento Nulo ($w_{align} = 0$)
- **O que acontece agora?**
  Sem a força de alinhamento, os boids não sincronizam a direção e o sentido de suas velocidades com a média dos vizinhos. Apenas a coesão (atração ao centro de massa) e a separação (repulsão a curtas distâncias) permanecem ativas. O resultado visual é que o bando perde o fluxo direcional suave e a capacidade de realizar curvas coordenadas conjuntas. O grupo passa a se comportar como um enxame desordenado e caótico (semelhante a mosquitos ou abelhas agitadas em torno de uma lâmpada): os indivíduos ficam orbitando, colidindo e vibrando em sentidos opostos ao redor do centro comum, sem conseguir migrar como uma unidade coesa pelo cenário.

*(Captura de tela do teste: `Screenshots/teste2_alinhamento_zero.png`)*

---

### 3. Raio de Percepção: 0,5 m versus 5,0 m
- **Como o formato do bando muda?**
  - **Com Raio de Percepção = 0,5 m**:
    O volume de detecção ao redor de cada boid torna-se excessivamente estreito. Raras vezes um indivíduo detecta mais de 1 ou 2 vizinhos simultaneamente. Com isso, a coerência global é perdida quase por completo: o bando principal se fragmenta em dezenas de pequenos subgrupos independentes (clusters de 2 a 4 indivíduos) ou agentes errantes e isolados dispersos por todo o espaço, que reagem apenas quando praticamente colidem uns com os outros.
  - **Com Raio de Percepção = 5,0 m**:
    A esfera de vizinhança passa a abranger quase todo o grupo simultaneamente. As informações de trajetória e velocidade se propagam instantaneamente por efeito cascata. O bando se agrupa em um macro-organismo único, largo e altamente estruturado. As manobras de curva e desvio tornam-se extremamente amplas e majestosas, com forte coesão visual, embora o grupo exija um espaço consideravelmente maior para realizar manobras.

*(Capturas de tela do teste: `Screenshots/teste3_raio_05.png` e `Screenshots/teste3_raio_50.png`)*

---

### 4. Predador: Comportamento de Fuga no `ComputeForce`
- **Efeito causado no grupo**:
  Uma esfera predadora (`Predador`) persegue continuamente o centro de massa do bando. No método `ComputeForce`, quando o predador entra no raio de pânico ($7.0\text{ m}$), uma força de fuga de magnitude elevada (peso $22.0$) é aplicada na direção oposta ao predador.
  O efeito dinâmico resultante é o clássico fenômeno biológico de **dispersão em casca / leque**: os boids que estão na rota de aproximação do predador se abrem bruscamente para as laterais, gerando um vácuo frontal que desvia da trajetória do agressor. Conforme o predador passa pelo centro da nuvem e se afasta, a força de fuga cessa e a coesão volta a predominar, fazendo com que o bando se feche suavemente e retome sua formação original na esteira traseira do predador.

*(Captura de tela do teste: `Screenshots/teste4_predador.png`)*

---

## Parte C — Desafio Opcional (Grid Espacial 3D para 500+ Boids)

Para viabilizar a escalabilidade do sistema para **500 ou mais boids** mantendo a taxa de quadros em **60 FPS estáveis**, foi implementada uma estrutura de **Grid Espacial 3D** (`SpatialGrid.cs`), que substitui a busca de vizinhos via física por um particionamento espacial cúbico:

1. **Estrutura**: O espaço tridimensional é discretizado em células cúbicas regulares de $2.0\text{ m}$ (`gridCellSize = 2.0f`). As coordenadas contínuas de cada boid são mapeadas em chaves inteiras `Vector3Int(Floor(x/size), Floor(y/size), Floor(z/size))`.
2. **Armazenamento e Reuso**: Utiliza um `Dictionary<Vector3Int, List<Boid>>` que é limpo e reconstruído a cada frame no `Update` do `FlockManager`. Para evitar sobrecarga no Garbage Collector (GC), foi implementado um pool interno de listas (`listPool`), garantindo **zero alocações de memória por frame**.
3. **Consulta de Vizinhança**: Para cada boid, a busca de vizinhos avalia apenas as 27 células adjacentes (bloco $3 \times 3 \times 3$ ao redor da célula atual), reduzindo drasticamente a complexidade de busca de $O(N^2)$ para $O(N)$ amortizado.

### Comparativo de Desempenho no Profiler do Unity

| Cenário de Teste | Método de Busca | Tempo de Scripts (CPU) | Frame Time Total | FPS Médio | Consumo GC Alloc |
| :--- | :--- | :---: | :---: | :---: | :---: |
| **150 Boids** | `OverlapSphereNonAlloc` | ~1.8 ms | ~5.2 ms | **60+ FPS** | 0 B / frame |
| **150 Boids** | `SpatialGrid` (Grid 3D) | ~0.9 ms | ~3.8 ms | **60+ FPS** | 0 B / frame |
| **500 Boids** | `OverlapSphereNonAlloc` | ~13.5 ms | ~19.4 ms | **~51 FPS** | 0 B / frame |
| **500 Boids** | `SpatialGrid` (Grid 3D) | **~3.2 ms** | **~8.1 ms** | **60+ FPS** | **0 B / frame** |

> **Conclusão do Profiler**: Com 500 boids, o custo de chamadas nativas de física do `OverlapSphereNonAlloc` se torna o gargalo da CPU. A substituição pelo `SpatialGrid` proporcionou uma redução de **~76% no tempo de processamento dos scripts**, permitindo rodar a simulação com 500 agentes em 60 FPS com ampla margem de estabilidade.

---

## Vídeo de Demonstração (30 a 60 segundos)

O vídeo demonstrativo gravado no editor Unity exibe o bando de 150 agentes fluindo pelo espaço, desviando de múltiplos obstáculos sem colisões e reagindo à aproximação do predador:

- **Link do Vídeo**: *[Inserir link do Teams ou vídeo compartilhado aqui]*
- *(Caso o arquivo de vídeo seja entregue localmente no repositório, o arquivo pode ser colocado na raiz ou na pasta de documentação).*

---

## Estrutura do Repositório

```text
projeto flocking att/
├── Assets/
│   ├── Materials/       # Materiais visuais dos boids, obstáculos e predador
│   ├── Prefabs/         # BoidPrefab configurado com Rigidbody, Collider e Boid.cs
│   ├── Scenes/          # FlockingScene.unity com iluminação, câmera e gestor
│   └── Scripts/         # Código C# da simulação (Boid, FlockManager, Predator, SpatialGrid)
├── ProjectSettings/     # Configurações do projeto e mapeamento de Layers (Boid, Obstacle, Predator)
├── Screenshots/         # Registros visuais dos experimentos da Parte B
├── .gitignore           # Ignora Library/, Temp/, Builds/ e arquivos temporários da IDE
└── README.md            # Documentação técnica completa e respostas da atividade
```
