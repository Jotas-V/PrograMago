# TCC-4 — base de testes e fluxo de execução

- Data: 5 de setembro de 2026
- Linear: TCC-4
- Estado do desenho: aprovado

## Objetivo

Estabelecer uma base de testes repetível para o núcleo do PrograMago e comprovar
a primeira fatia vertical do jogo: o texto digitado pelo jogador é lido,
tokenizado, validado e convertido em feedback e estado visual na arena.

Esta entrega cobre profundamente a primeira regra já definida,
`public class Mago {}`. Regras futuras de atributos, construtor, `this`, `new`,
combate e progressão terão seus próprios ciclos de TDD nas issues responsáveis.

## Escopo

### Testes Edit Mode

- Tokenização de palavras, identificadores, chaves e posições no texto.
- Aceitação de espaços, tabulações e quebras de linha válidos.
- Diagnóstico para caracteres desconhecidos.
- Validação da ordem completa `public class Mago {}`.
- Diagnósticos para entrada vazia, ordem incorreta, nome incorreto, chaves
  ausentes e tokens extras.
- Garantia de que uma submissão inválida não altera o último estado válido.
- Idempotência ao reenviar a mesma declaração válida.
- Reinício da sessão, do editor, do feedback e da arena.
- Comportamento do presenter em sucesso e falha.

### Teste Play Mode

Um teste de integração carregará a cena principal e exercitará os componentes
reais da Unity. O teste preencherá o `TMP_InputField`, acionará a submissão e o
reinício e observará o texto de feedback e a visibilidade da silhueta do Mago.
Ele deve cobrir pelo menos uma entrada válida, uma inválida e o retorno ao estado
inicial.

### Execução repetível

A suíte poderá ser executada pelo Unity Test Runner e pela interface de linha de
comando da versão de Unity fixada no projeto. O repositório documentará os
comandos necessários e os locais dos resultados. Não será introduzido um
serviço de CI nesta issue.

## Arquitetura e fluxo observado

1. `GameplayBootstrapper` expõe `TMP_InputField.text` como uma `string` pelo
   contrato `ICodeEditorView`.
2. `GameplayPresenter` encaminha a submissão ao `SubmitCodeUseCase`.
3. `CodeTokenizer` consome toda a string e devolve tokens ou diagnóstico.
4. `ClassDeclarationValidator` verifica a sequência esperada para a atividade.
5. `LearningSession` muda somente após uma declaração completamente validada.
6. O resultado retorna ao presenter, que atualiza o feedback e a arena.

Os testes Edit Mode verificam cada fronteira sem depender de cena. O teste Play
Mode comprova que as referências e os eventos da interface conectam esse fluxo
na cena real.

## Tratamento de erros e garantias

- Toda entrada é consumida; conteúdo desconhecido ou extra não é ignorado.
- Uma falha contém código e posição úteis para o feedback ao jogador.
- Falhas não apagam nem avançam um estado anteriormente validado.
- Repetir uma submissão válida não duplica entidades nem avança o jogo.
- Reiniciar devolve editor, sessão, feedback e arena ao estado inicial.
- Falhas de configuração da cena devem produzir um teste de integração legível,
  sem falso positivo.

## Estratégia TDD

Cada lacuna será trabalhada em RED–GREEN–REFACTOR:

1. adicionar um teste pequeno para um comportamento ausente;
2. executar e confirmar que falha pela razão esperada;
3. implementar somente o necessário para passar;
4. executar a suíte completa e refatorar mantendo-a verde.

Testes que apenas registram comportamento já implementado serão adicionados
como caracterização, mas novos comportamentos exigirão uma falha observada antes
da alteração de produção.

## Fora do escopo

- Gramática geral de Java.
- Atributos, construtor, encapsulamento, instanciação ou combate.
- Conteúdo e progressão entre múltiplas atividades.
- Pipeline remoto de CI.
- Polimento visual da arena ou do Mago.

## Critérios de conclusão

- As suítes Edit Mode e Play Mode podem ser executadas sob demanda.
- Os cenários fundamentais descritos acima estão automatizados e verdes.
- A cena inicia, submete e reinicia sem intervenção manual fora das ações do
  jogador.
- O procedimento de execução está documentado no repositório.
