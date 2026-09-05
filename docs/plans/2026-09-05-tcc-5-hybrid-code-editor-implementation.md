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
5. Adicionar testes Play Mode para campo multilinha, prévia por
   `onValueChanged`, ausência de diagnóstico durante edição e existência de um
   único botão `BattleButton` com rótulo **Batalhar**. Confirmar RED.
6. Adaptar o `GameplayBootstrapper`: configurar o `TMP_InputField`, conectar os
   eventos, criar o botão de batalha e remover da interface as ações de envio
   genérico e reinício.
7. Executar Edit Mode e Play Mode completos, corrigir somente regressões ligadas
   à TCC-5 e revisar logs, XMLs e diff.
8. Commitar a implementação e registrar testes e decisões finais na TCC-5.

## Componentes previstos

- `PreviewCodeUseCase`: valida o texto atual e informa se a declaração esperada
  pode ser pré-visualizada, sem modificar a sessão.
- `GameplayPresenter.Preview`: atualiza a arena e remove feedback antigo, sem
  apresentar erros.
- `GameplayPresenter.Battle`: executa a submissão completa e apresenta sucesso
  ou diagnóstico.
- `GameplayBootstrapper`: conecta edição e batalha aos respectivos eventos da
  Unity e configura o editor como multilinha.

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
