# TCC-15 — Validação de magia, herança e sobrescrita

## Contrato

Este trabalho concretiza os exemplos já aprovados no desenho TCC-12 de 28/09/2026. O analisador reconhece um subconjunto educacional inspirado em Java; não compila nem executa Java real.

- `DefineAndCallSpellMethod`: Mago declara `public void lancarMagia(Inimigo alvo) {}`. Uma chamada usa o nome validado da instância do Mago e uma instância validada de Inimigo, por exemplo `mago.lancarMagia(boneco);`.
- `ExtendMago`: aceita uma ou mais classes públicas Piromante, Hidromante ou Eletromante com `extends Mago` e corpo vazio. As declarações podem aparecer em qualquer ordem. Cada especialização pode ser declarada uma única vez.
- `OverrideSpellWithSuper`: ao menos uma especialização sobrescreve `lancarMagia` com `@Override`, visibilidade pública, retorno void e um único parâmetro Inimigo. O nome do parâmetro pode variar; a chamada `super.lancarMagia` recebe exatamente esse parâmetro.

O único corpo elemental permitido nesta entrega é a delegação aprovada a `super.lancarMagia`. A identidade fogo/água/eletricidade vem das especializações de combate existentes na TCC-14. Não existem novos comandos textuais para fabricar efeitos ou modificar diretamente o estado do combate. Outras construções são apresentadas como fora desta atividade, sem alegar que sejam inválidas em toda a linguagem Java.

## Integração

`ExerciseCodeValidator` encaminha os três critérios a `ElementalSpellValidator`. O validador percorre toda a entrada e respeita o escopo das chaves. Depois de verificar os métodos e subclasses, as classes base, setters, construtores, instâncias e atributos continuam passando pelo `EnemyConstructionValidator` existente. Só adições efetivamente validadas são retiradas dessa segunda análise.

Os tokens preservam posição, linha e coluna originais. O resultado mantém a build e os inimigos validados; a submissão inválida continua usando o fluxo existente de falha, sem aplicar parcialmente o programa.

Diagnósticos: `INHERIT001–003` para relação de herança, especialização desconhecida e duplicação; `OVERRIDE001–002` para anotação; `SPELL001–005` para método base, assinatura, duplicação, corpo e chamada; `SUPER001` para delegação incorreta.

## Limite de escopo

Esta issue implementa os validadores das etapas 6–8. A liberação dessas etapas no fluxo jogável, continuação protegida do editor, conteúdo restante da Fase 2 e estratégia polimórfica permanecem nas issues de progressão relacionadas (TCC-13/TCC-17). Não altera a progressão, balanceamento, sprites nem a cena.

## Verificação

Os testes `ElementalSpellValidationTests` começaram falhando com `VALID001`. Cobrem as três especializações, ordem de classes, whitespace, anotação, assinatura, delegação, comandos não permitidos, duplicações, alvo validado, entrada incompleta, posição original e orçamento da build. Executar também as suítes completas EditMode e PlayMode para regressão.

### Resultado em 30/09/2026

- 42 casos novos de linguagem e 1 teste novo de integração com SubmitCodeUseCase aprovados; o teste antigo de critério indisponível foi atualizado para exigir diagnóstico de método base ausente e também passou.
- EditMode completo: 360/362 aprovados. PlayMode completo: 41/43 aprovados.
- As quatro falhas abaixo foram repetidas com ExerciseCodeValidator restaurado temporariamente à versão HEAD anterior à TCC-15, apresentando os mesmos resultados. A implementação TCC-15 foi restaurada após a comparação.
  - EditMode: `MagoPrefab_UsesProjectPixelArtWithPointFiltering` espera o PNG antigo, mas o prefab usa a folha de animação atual.
  - EditMode: `Scene_PreparingPresentationAgainReusesAuthoredObjects` encontra 163 objetos após preparar a apresentação, contra 159 antes.
  - PlayMode: `Arena_BackdropFillsFrameEvenUnderScaledParent` mede 0,606889129 onde espera 0,5.
  - PlayMode: `FirstEnemyBattle_BonecoCanBeDefeatedAndReviewSurvivesReload` recebe falso onde espera verdadeiro no fluxo da primeira batalha.

Os relatórios locais estão em `tmp/tcc15-*-tests.json`, incluindo os relatórios da comparação anterior. Nenhum arquivo de cena, prefab, animação ou arte foi modificado pela TCC-15.
