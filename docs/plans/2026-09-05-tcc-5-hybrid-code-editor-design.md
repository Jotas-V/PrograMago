# TCC-5 — editor híbrido e início da batalha

- Data: 5 de setembro de 2026
- Linear: TCC-5
- Estado do desenho: aprovado

## Objetivo

Permitir que o jogador escreva código em um editor multilinha, observe na arena
uma prévia silenciosa das construções válidas e use uma ação explícita para
validar o programa completo. O botão se chamará **Batalhar**, pois no fluxo
final do protótipo ele será a fronteira entre edição e batalha automática.

## Escopo da TCC-5

- Configurar o editor para aceitar múltiplas linhas sem reescrever o texto.
- Atualizar a prévia visual enquanto o texto muda.
- Não exibir diagnósticos durante a digitação.
- Validar o código completo somente quando o jogador clicar em **Batalhar**.
- Preservar o código após tentativas válidas e inválidas.
- Exibir somente orientação de erro no painel lateral após a validação.
- Manter somente o botão **Batalhar**; não oferecer ações de limpar ou
  restaurar.

A primeira construção reconhecida continua sendo a declaração vazia
`public class Mago { }`. Quando ela estiver válida no texto atual, a arena
exibirá a silhueta do Mago. Se o jogador apagar ou invalidar a declaração, a
prévia desaparecerá.

O clique em **Batalhar** ainda não executará animações, turnos, dano ou inimigos.
Essas regras pertencem à TCC-12. Nesta entrega, o clique confirma que o código
completo está válido e deixa preparada a fronteira que iniciará a batalha no
incremento futuro.

## Interação e fluxo

### Edição e prévia

1. O jogador altera o conteúdo do editor.
2. A interface preserva a string original, incluindo espaços, tabulações e
   quebras de linha.
3. Uma inspeção silenciosa tokeniza e valida o texto atual.
4. A arena reflete somente se a declaração esperada existe de forma válida no
   texto atual.
5. Nenhum diagnóstico é apresentado durante essa inspeção.
6. Um feedback de tentativa anterior é removido assim que o jogador volta a
   editar, evitando uma mensagem desatualizada.

### Batalhar com código válido

1. O jogador clica em **Batalhar**.
2. Todo o conteúdo do editor é tokenizado e validado.
3. A sessão recebe o resultado validado.
4. A arena permanece coerente com o código e o painel não exibe mensagem de
   sucesso; a resposta positiva é exclusivamente visual.
5. O texto do editor não é alterado.

### Batalhar com código inválido

1. A tokenização ou validação produz um diagnóstico.
2. A sessão não avança.
3. A prévia continua representando o texto atual inválido, portanto permanece
   oculta.
4. O painel lateral apresenta código, posição e orientação compreensível.
5. O texto do editor não é alterado e a interface continua utilizável.

## Arquitetura

A apresentação terá duas operações distintas:

- **Preview**: consulta o texto atual e obtém um resultado de validação sem
  aplicar progresso à sessão e sem mostrar erros.
- **Battle**: usa o fluxo completo de submissão já existente, aplica dados
  validados à sessão e apresenta sucesso ou diagnóstico.

O `GameplayBootstrapper` conectará `TMP_InputField.onValueChanged` à prévia e o
`Button.onClick` à batalha. O campo multilinha, o botão **Batalhar** e o texto de
feedback devem existir na cena e ser fornecidos por referências serializadas.
Referências ausentes são erro de configuração; a interface não será montada em
tempo de execução.

As regras de sintaxe permanecem nas camadas de linguagem e aplicação. O código
do jogador nunca é compilado nem executado diretamente. A integração Unity não
decide se uma declaração é válida.

## Estado e garantias

O estado confirmado da sessão e a prévia visual são conceitos diferentes. A
prévia sempre acompanha o texto atual; a sessão só avança após **Batalhar**.
Isso permite que alterações futuras usem o mesmo limite para iniciar o combate
sem disparar ações enquanto o jogador ainda escreve.

As seguintes garantias devem ser mantidas:

- digitar não confirma progresso nem mostra erros;
- editar depois de uma tentativa limpa apenas o feedback, não o texto;
- uma tentativa inválida não altera a sessão;
- espaços, tabulações e quebras de linha válidos são aceitos e preservados;
- a interface não oferece atalhos que apaguem o código do jogador;
- múltiplas validações não duplicam estado nem entidades.

## Composição da cena

O botão **Batalhar** ficará na faixa inferior do editor, separado do painel
lateral e com contraste suficiente para representar a principal ação da tela.
Ele será um objeto permanente da cena, visível e ajustável também fora do Play
Mode.

O `TutorialPanel` formará uma coluna própria à direita do editor:

- `Title`: título pequeno no topo, identificando o conteúdo estudado;
- `ObjectiveText`: instrução da atividade abaixo do título;
- `FeedbackText`: área de erros abaixo do objetivo, vazia no estado inicial.

Esses três textos devem permanecer contidos pelo `TutorialPanel`, sem sobrepor
o `CodeInput`. Eles não podem repetir o código do editor nem capturar raycasts.
O `FeedbackText` existente será reutilizado pela integração, em vez da criação
de um texto alternativo em tempo de execução.

## Tratamento de erros

Diagnósticos serão exibidos apenas após o clique em **Batalhar**. A mensagem
deve indicar o tipo do problema e sua posição quando disponível. Entrada vazia,
estrutura incompleta, nome incorreto e caracteres desconhecidos não podem
travar a cena nem modificar o texto. Uma validação bem-sucedida apenas remove
um erro anterior; não apresenta confirmação textual.

## Estratégia de testes

O trabalho seguirá RED–GREEN–REFACTOR:

1. teste de apresentação para prévia válida e inválida sem feedback;
2. teste de apresentação garantindo que editar limpa feedback antigo;
3. teste de apresentação para validação explícita preservando o texto;
4. teste da cena para configuração editável e multilinha, sem elementos do
   painel lateral bloqueando o foco ou os raycasts do editor;
5. teste da cena comprovando que botão e feedback são objetos persistidos e
   referenciados, não filhos criados em runtime;
6. teste Play Mode para prévia acionada pela alteração do campo;
7. teste Play Mode para o botão **Batalhar**, erro, ausência de mensagem de
   sucesso e preservação do conteúdo.

Cada comportamento novo deve falhar pela ausência da funcionalidade antes da
alteração de produção. Ao final, as suítes Edit Mode e Play Mode devem passar
sem erros próprios do projeto.

## Fora do escopo

- combate, turnos, dano, vida e movimentação de inimigos;
- magias e animações de batalha;
- progressão entre fases;
- atributos, construtores, instanciação, herança e polimorfismo;
- destaque de sintaxe ou autocompletar;
- botão para limpar ou restaurar o editor.
