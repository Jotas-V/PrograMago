# TCC-5 — plano de implementação do editor híbrido

## Sequência TDD

1. Adicionar testes Edit Mode que expressem a inspeção silenciosa do código sem
   alterar a sessão. Executar e confirmar a falha pela ausência do caso de uso
   de prévia.
2. Implementar o caso de uso mínimo de prévia, reutilizando o tokenizador e o
   validador da declaração de classe sem aplicar dados à `LearningSession`.
3. Adicionar testes do `GameplayPresenter` para atualizar a silhueta durante a
   edição, esconder a prévia inválida e limpar feedback desatualizado sem mostrar
   um novo diagnóstico. Confirmar RED e implementar o mínimo necessário.
4. Alterar os testes do presenter para substituir `Submit` e `Restart` pela ação
   `Battle`, garantindo validação explícita e preservação integral do texto.
   Confirmar RED e adaptar a apresentação.
5. Adicionar um teste do presenter exigindo que o sucesso limpe o painel sem
   apresentar mensagem textual. Confirmar RED e remover o feedback positivo.
6. Adicionar testes da cena para referências serializadas, conteúdo e hierarquia
   do `TutorialPanel`, botão `BattleButton` persistido e ausência de elementos
   que bloqueiem o raycast do `CodeInput`. Confirmar RED.
7. Corrigir a cena: configurar o editor, posicionar título, objetivo e feedback,
   criar o botão **Batalhar** e ligar as referências do bootstrapper.
8. Remover do `GameplayBootstrapper` a criação de controles em tempo de
   execução e manter somente a conexão dos eventos.
9. Executar Edit Mode e Play Mode completos, corrigir somente regressões ligadas
   à TCC-5 e revisar logs, XMLs e diff.
10. Commitar a implementação e registrar testes e decisões finais na TCC-5.

## Componentes previstos

- `PreviewCodeUseCase`: valida o texto atual e informa se a declaração esperada
  pode ser pré-visualizada, sem modificar a sessão.
- `GameplayPresenter.Preview`: atualiza a arena e remove feedback antigo, sem
  apresentar erros.
- `GameplayPresenter.Battle`: executa a submissão completa, limpa o painel no
  sucesso e apresenta diagnóstico somente na falha.
- `GameplayBootstrapper`: conecta edição e batalha aos respectivos eventos da
  Unity usando controles existentes e referenciados na cena.

## Verificação

Na raiz do repositório, com o projeto fechado no editor:

```powershell
.\scripts\run-unity-tests.ps1 -Platform EditMode
.\scripts\run-unity-tests.ps1 -Platform PlayMode
```

Depois, executar a suíte completa:

```powershell
.\scripts\run-unity-tests.ps1 -Platform All
```

## Limites

Não implementar combate, inimigos, turnos, dano, magias, progressão de fases ou
novas regras do subconjunto Java. O botão **Batalhar** apenas estabelece a ação
explícita que receberá essas consequências na TCC-12.
