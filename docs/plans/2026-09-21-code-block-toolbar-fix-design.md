# TCC-12 — correção da faixa de blocos de código

## Problema

A faixa acima do editor mistura os três arquivos de código com as áreas de
métodos e preparação. O terceiro arquivo ocupa o mesmo espaço horizontal do
botão Métodos, o título invade os botões e os três arquivos perdem seus nomes,
aparecendo apenas como Código. O botão Aprovar método também permanece visível
fora da área de Métodos.

## Desenho aprovado

A faixa terá dois grupos visuais na mesma linha, usando toda a largura do editor:

- **Código da fase:** `1 · Mago`, `2 · Inimigo` e `3 · Código`.
- **Configuração do Mago:** `Métodos` e `Preparação`.

Os grupos terão títulos próprios, espaçamento consistente e nenhuma interseção
entre seus RectTransforms. O botão `Aprovar método` ficará dentro da área de
configuração, mas será exibido somente quando Métodos estiver selecionado. A
timeline inferior continuará reservada aos comandos de batalha.

## Comportamento

Os três arquivos continuam independentes e selecionam a área Classes. Seus
rótulos são estáveis e não dependem do conteúdo digitado. Métodos e Preparação
selecionam seus respectivos documentos. O estado selecionado usa a cor roxa já
existente; itens não selecionados mantêm o tom escuro atual.

## Plano e validação

1. Criar testes que exijam rótulos estáveis, grupos sem sobreposição e aprovação
   visível somente na área Métodos.
2. Ajustar a rotina de layout e o estado dos controles.
3. Preparar e salvar a MainScene no Editor.
4. Executar EditMode e PlayMode e validar a Game View na resolução de referência.
