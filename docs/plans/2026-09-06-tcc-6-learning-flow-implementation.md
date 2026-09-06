# TCC-6 — plano de implementação do fluxo pedagógico

## Sequência TDD

### 1. Modelo de conteúdo

1. Criar testes Edit Mode para exigir uma definição de batalha com todos os
   textos pedagógicos, dicas e critério de validação.
2. Executar os testes e confirmar RED pela ausência dos tipos.
3. Implementar `BattleDefinition`, `BattleVictoryContent` e
   `ValidationCriterion` no domínio com validação mínima dos dados.
4. Executar os testes e confirmar GREEN.

Arquivos principais:

- `Assets/PrograMago/Runtime/Domain/BattleDefinition.cs`
- `Assets/PrograMago/Runtime/Domain/BattleVictoryContent.cs`
- `Assets/PrograMago/Runtime/Domain/ValidationCriterion.cs`
- `Assets/PrograMago/Tests/EditMode/Domain/BattleDefinitionTests.cs`

### 2. Caminho e estado de aprendizagem

1. Criar testes para oito batalhas ordenadas, identificadores únicos e batalha
   inicial.
2. Criar testes para as transições `Editing`, `BattleInProgress`,
   `VictoryReview` e `JourneyCompleted`.
3. Cobrir critério incompatível, avanço prematuro, vitória, derrota, reinício e
   preservação da atividade atual.
4. Confirmar RED, implementar o mínimo e confirmar GREEN a cada comportamento.

Arquivos principais:

- `Assets/PrograMago/Runtime/Domain/LearningPath.cs`
- `Assets/PrograMago/Runtime/Domain/LearningProgress.cs`
- `Assets/PrograMago/Runtime/Domain/LearningStage.cs`
- `Assets/PrograMago/Tests/EditMode/Domain/LearningPathTests.cs`
- `Assets/PrograMago/Tests/EditMode/Domain/LearningProgressTests.cs`

### 3. Pressionamento contínuo para reinício

1. Testar que menos de cinco segundos não reinicia.
2. Testar cancelamento ao soltar a tecla.
3. Testar disparo aos cinco segundos e somente uma vez até nova soltura.
4. Implementar um controlador puro, independente do teclado e do relógio da
   Unity.

Arquivos principais:

- `Assets/PrograMago/Runtime/Application/HoldToRestartController.cs`
- `Assets/PrograMago/Tests/EditMode/Application/HoldToRestartControllerTests.cs`

### 4. Critério satisfeito pela submissão

1. Alterar primeiro os testes para exigir que um resultado válido carregue o
   critério `DeclareMagoClass`.
2. Testar que resultados inválidos não carregam critério satisfeito.
3. Adaptar `SubmitCodeResult` e `SubmitCodeUseCase` com a mudança mínima.
4. Executar toda a suíte rápida para detectar regressões.

Arquivos principais:

- `Assets/PrograMago/Runtime/Application/SubmitCodeResult.cs`
- `Assets/PrograMago/Runtime/Application/SubmitCodeUseCase.cs`
- `Assets/PrograMago/Tests/EditMode/Application/SubmitCodeUseCaseTests.cs`

### 5. Apresentação do fluxo pedagógico

1. Criar interfaces para painel pedagógico, overlay de vitória, estado da
   batalha e feedback do reinício.
2. Testar no presenter o carregamento da primeira batalha.
3. Testar tentativa inválida e revelação progressiva de dicas.
4. Testar submissão válida iniciando batalha sem liberar avanço.
5. Testar vitória, revisão, botão de próxima batalha e conclusão final.
6. Testar derrota e reinício manual preservando `SourceCode`.
7. Adaptar o presenter com o mínimo necessário e manter os testes existentes
   verdes.

Arquivos principais:

- `Assets/PrograMago/Runtime/Presentation/ILearningContentView.cs`
- `Assets/PrograMago/Runtime/Presentation/IVictoryView.cs`
- `Assets/PrograMago/Runtime/Presentation/IRestartView.cs`
- `Assets/PrograMago/Runtime/Presentation/GameplayPresenter.cs`
- `Assets/PrograMago/Tests/EditMode/Presentation/GameplayPresenterTests.cs`

### 6. Dados Unity e oito batalhas

1. Testar o adaptador do asset e a rejeição de dados incompletos.
2. Implementar `LearningPathAsset` e os DTOs serializáveis.
3. Criar o asset com as oito batalhas, quatro dicas e revisão por atividade.
4. Adicionar teste de asset para quantidade, ordem, IDs e conteúdo obrigatório.

Arquivos principais:

- `Assets/PrograMago/Runtime/Unity/LearningPathAsset.cs`
- `Assets/PrograMago/Content/LearningPath.asset`
- `Assets/PrograMago/Tests/EditMode/LearningPathAssetTests.cs`

### 7. Cena e integração Unity

1. Adicionar testes de cena exigindo os novos textos, overlay oculto, fundo
   preto semitransparente em tela cheia, card opaco, botão de avanço e
   referências serializadas.
2. Confirmar RED antes de alterar `SampleScene.unity`.
3. Atualizar a cena e o bootstrapper para carregar o asset e conectar as views.
4. Integrar `Keyboard.current.rKey` ao controlador usando
   `Time.unscaledDeltaTime`.
5. Expor métodos públicos para a TCC-12 informar vitória e derrota.
6. Adicionar testes Play Mode para conteúdo inicial, vitória, avanço e
   preservação do código ao reiniciar.

Arquivos principais:

- `Assets/Scenes/SampleScene.unity`
- `Assets/PrograMago/Runtime/Unity/GameplayBootstrapper.cs`
- `Assets/PrograMago/Tests/EditMode/GameplaySceneAssetTests.cs`
- `Assets/PrograMago/Tests/PlayMode/Integration/GameplaySceneTests.cs`

## Verificação final

Executar, com o editor Unity fechado:

```powershell
.\scripts\run-unity-tests.ps1 -Platform EditMode
.\scripts\run-unity-tests.ps1 -Platform PlayMode
.\scripts\run-unity-tests.ps1 -Platform All
```

Revisar também:

- XML e logs sem falhas ou exceções do projeto;
- `git diff --check`;
- assets e respectivos arquivos `.meta`;
- ausência de alterações nos arquivos não rastreados preexistentes.

## Encerramento no Linear

Depois da verificação:

1. criar um commit da implementação;
2. comentar na TCC-6 o resumo, decisões, testes e commit;
3. mover a TCC-6 para o estado de teste disponível no time TCC.
