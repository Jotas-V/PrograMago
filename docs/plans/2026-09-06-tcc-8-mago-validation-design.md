# TCC-8 — validação da criação do Mago

- Data: 6 de setembro de 2026
- Linear: TCC-8
- Estado do desenho: aprovado

## Objetivo

Validar as três batalhas da primeira fase pedagógica sem compilar nem executar
Java real. A análise continuará limitada ao subconjunto tokenizado pelo projeto,
consumirá toda a entrada e produzirá orientações posicionadas.

## Escopo da fase 1

1. `DeclareMagoClass`: aceitar a classe pública e vazia `Mago`.
2. `AddPrivateAttributes`: aceitar a classe com os atributos obrigatórios.
3. `ConstructAndInstantiateMago`: aceitar atributos, construtor, atribuições com
   `this` e uma instanciação com `new`.

Os cinco atributos obrigatórios são `vida`, `dano`, `alcance`, `iniciativa` e
`velocidadeAtaque`. Todos usam `int`, assim como os parâmetros correspondentes e
os valores da instanciação. Cada valor deve ficar entre 1 e 15, inclusive; a
soma pode chegar a no máximo 25. Não é obrigatório usar todos os pontos.

## Arquitetura

O `CodeTokenizer` continuará sendo a única etapa léxica. Depois dele, um
despachante selecionará o validador correspondente ao critério da batalha atual.
Os três validadores compartilharão uma leitura estruturada dos tokens, sem
dependências da Unity:

- o validador de declaração preserva o comportamento já existente;
- o validador de atributos reconhece os cinco campos dentro da classe;
- o validador de construção reconhece campos, construtor, inicialização com
  `this` e instanciação após a classe.

Um cursor pequeno e explícito fará a leitura sequencial. Não será criado um
parser Java genérico. O resultado de sucesso informará o critério satisfeito e,
quando houver instanciação, preservará nome da variável, ordem dos parâmetros,
valores e pontos restantes para uso posterior pela TCC-9.

O caso de uso de submissão validará contra o critério da batalha atual. Assim,
uma declaração simples não aprova a batalha de atributos e uma definição sem
instanciação não aprova a terceira batalha.

## Gramática controlada

Na batalha de atributos, a entrada deve conter `public class Mago`, os cinco
atributos e o fechamento da classe. Cada atributo deve ter a forma
`private int nome;`. A ordem dos atributos pode variar, mas cada nome deve
aparecer exatamente uma vez e membros extras são rejeitados.

Na batalha de construção, a classe deve conter os mesmos atributos e um único
construtor público. O construtor:

- chama-se exatamente `Mago` e não declara tipo de retorno;
- recebe um parâmetro `int` para cada atributo, sem ausências ou duplicatas;
- contém exatamente uma atribuição `this.atributo = atributo;` para cada campo;
- permite variar a ordem das atribuições.

Depois do fechamento da classe deve existir uma única instanciação com a forma
`Mago variavel = new Mago(valor1, ...);`. O nome da variável é livre e
preservado. A ordem dos parâmetros do construtor define a associação dos valores
da instanciação aos atributos. Toda a entrada deve ser consumida.

## Diagnósticos

As falhas serão diferenciadas por categoria:

- léxica: caractere ou literal que o tokenizador não reconhece;
- sintática: delimitador, pontuação ou sequência gramatical ausente;
- estrutural: membro ausente, duplicado ou na região errada, inclusive nome do
  construtor;
- conceitual: ausência de `private`, tipo diferente de `int`, uso incorreto de
  `this` ou instanciação sem `new`;
- regra do jogo: valor fora de 1 a 15 ou soma acima de 25.

Cada diagnóstico manterá código, linha, coluna e detalhe pedagógico. A análise
não corrige automaticamente o texto e não revela decisões estratégicas da
batalha.

## Testes

O desenvolvimento seguirá RED–GREEN–REFACTOR em Edit Mode. A suíte cobrirá:

- espaços, tabulações e quebras de linha equivalentes;
- atributos e atribuições em ordens diferentes;
- chaves, parênteses, vírgulas, ponto e vírgula e conteúdo extra;
- campos ausentes, duplicados, públicos ou com tipo/nome incorreto;
- construtor ausente, deslocado ou com nome incorreto;
- parâmetros e atribuições com `this` ausentes, duplicados ou incompatíveis;
- `new`, classe instanciada e quantidade de argumentos;
- limites individuais 1 e 15, orçamento parcial, total 25 e total excedido;
- integração dos três critérios com submissão e progressão;
- regressão das suítes Edit Mode e Play Mode.

## Fora do escopo

- mapear os valores para o personagem ou mostrar pontos restantes (TCC-9);
- combate, iniciativa, intervalo de ataques e regras elementais (TCC-12);
- coluna de blocos, pausa e reordenação durante o combate (TCC-12);
- ampliar as oito batalhas guiadas com atividades livres ou de consolidação;
- validar inimigos, métodos, herança, sobrescrita ou polimorfismo.
