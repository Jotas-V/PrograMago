# TCC-11 — classe e instâncias de Inimigo

- Data: 13 de setembro de 2026
- Linear: TCC-11
- Estado do desenho: aprovado pelo usuário

## Resultado

O código validado pode definir uma classe `Inimigo` com `nome`, `vida` e
`elemento` privados, inicializar os três campos no construtor por meio de
`this`, expor `getElemento()` e criar objetos com `new Inimigo(...)`. O resultado
da validação inclui dados imutáveis dos inimigos instanciados. Apenas um
resultado aprovado pode atualizar os objetos da arena; uma edição ou submissão
inválida preserva o último estado aprovado.

## Sintaxe e dados

O editor mantém o código da Fase 1, então o validador desta atividade aceita o
programa validado do Mago junto da classe `Inimigo` e de suas instanciações.
As classes podem aparecer em qualquer ordem e em qualquer bloco do editor,
conforme a TCC-41; seus membros precisam permanecer dentro das próprias chaves.
O parser continua restrito ao subconjunto Java do jogo. Ele confirma nomes e
tipos de campos, parâmetros, atribuições `this`, corpo de `getElemento()` e
argumentos de cada `new Inimigo(...)`, rejeitando conteúdo adicional e nomes de
variáveis duplicados. As quatro fichas de inimigo aceitas são Boneco de
Treinamento (neutro), Golem de Gelo (gelo), Elemental de Fogo (fogo) e Slime
Aquático (água); vida e disponibilidade são definidas por ficha, não por código
arbitrário do jogador. A batalha `enemy-object` exige o Boneco; as demais
fichas ficam prontas para as batalhas posteriores.

## Integração

O domínio representa cada instância por nome de variável e ficha validada. O
resultado da aplicação carrega Mago e inimigos sem confundir a classe declarada
com objetos. O presenter entrega os inimigos à arena somente após sucesso. A
Unity usa marcadores provisórios posicionados à direita e informa nome, vida e
elemento; a pixel art final pertence à TCC-40. A TCC-11 não desbloqueia a Fase
2, não executa combate e não altera o registro local da Fase 1. Essas mudanças
pertencem às TCC-12 e TCC-13.

## Verificação

Testes Edit Mode cobrem sintaxe correta, rejeições de encapsulamento,
construtor, getter e fichas, múltiplas instâncias, valores do resultado e
preservação do estado após erro. Testes de apresentação e Play Mode verificam
que somente instâncias aprovadas aparecem na arena. Cada mudança de comportamento
segue o ciclo teste falhando, implementação mínima e teste passando.

## Alternativas consideradas

Um parser Java genérico traria flexibilidade, mas ampliaria a gramática e o
risco sem atender a um requisito atual. Validar apenas um trecho isolado de
`Inimigo` simplificaria o parser, mas quebraria a continuidade do código que o
jogador construiu na Fase 1. A extensão restrita do validador atual mantém o
fluxo acumulativo e o escopo pedagógico aprovado.
