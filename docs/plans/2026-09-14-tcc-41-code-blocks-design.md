# TCC-41 — três blocos de código no editor

- Data: 14 de setembro de 2026
- Linear: TCC-41
- Estado do desenho: aprovado pelo usuário

## Interação

Três botões pequenos com os números 1, 2 e 3 ficam abaixo do editor, como no
esboço enviado. Um único campo de texto permanece visível. Ao selecionar outro
botão, o conteúdo atual é guardado e o campo mostra o texto daquele bloco; o
texto dos demais continua intacto. O botão ativo recebe uma aparência distinta.
Os números não determinam o conteúdo: qualquer classe ou instrução pode estar
em qualquer bloco.

## Validação

`Batalhar` monta uma fonte virtual com os três textos em ordem 1, 2 e 3,
separados por quebra de linha. O tokenizador examina a fonte inteira. A análise
identifica declarações de classe pelo escopo de chaves e aceita `Mago` e
`Inimigo` em qualquer ordem, sem aceitar membros da classe errada. A atividade
atual continua a determinar quais requisitos precisam estar completos. Um erro
aponta o bloco e a linha local calculados a partir da posição na fonte virtual.
Somente a validação completa atualiza a arena.

## Persistência

O registro local da Fase 1 passa a guardar os três textos e o índice ativo,
além do progresso já existente. Registros antigos com um único `sourceCode`
entram no bloco 1 para não perder o trabalho do jogador. Trocar de bloco e
editar acionam o salvamento existente. O código aprovado permanece separado do
texto ainda em edição.

## Limites

Os blocos são seções de uma única fonte, não arquivos Java separados. Dividir
uma declaração entre blocos é aceito apenas quando a concatenação resultar em
sintaxe válida. Esta mudança não executa Java arbitrário, não inicia combate e
não libera a Fase 2 antes das issues correspondentes.

## Verificação

Testes Edit Mode verificam troca sem perda, composição, mapeamento de erro,
ordem livre das classes e migração do registro antigo. Testes Play Mode
verificam os botões, texto restaurado e `Batalhar` lendo conteúdo de blocos
ocultos. Cada comportamento novo começa por um teste que falha.
