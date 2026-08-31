# TCC-2 — arquitetura inicial e primeira fatia vertical

- Data: 31 de agosto de 2026
- Linear: TCC-2
- Estado: aprovado para implementação

## Objetivo

Estabelecer fronteiras técnicas claras e entregar o primeiro fluxo executável do
PrograMago: receber uma declaração de classe `Mago`, validá-la, refletir o
resultado na arena e permitir reiniciar a atividade.

## Escopo da primeira fatia

O incremento aceita somente uma declaração vazia da classe `Mago`, equivalente
a `public class Mago {}`. Espaços e quebras de linha podem variar. Caracteres ou
tokens desconhecidos produzem diagnóstico; o sistema nunca corrige nem ignora a
entrada silenciosamente.

Quando a declaração é válida, a sessão passa de "sem declaração validada" para
"classe declarada" e a arena exibe a silhueta. Uma submissão inválida apresenta
feedback e preserva o último estado validado. Reenviar a mesma declaração é
idempotente. Reiniciar limpa o progresso, o feedback e a silhueta.

Não fazem parte desta fatia atributos, construtores, instanciação de objetos,
combate, progressão entre atividades ou interpretação geral de Java.

## Módulos e dependências

O código do projeto ficará sob `Assets/PrograMago` e será dividido em cinco
assemblies de runtime:

1. `PrograMago.Domain`: definição da atividade, estado da sessão e transições.
   Não depende da Unity.
2. `PrograMago.Language`: tokens, posições, diagnósticos, tokenização e
   validação. Depende apenas de `Domain` quando precisar da definição do
   exercício.
3. `PrograMago.Application`: casos de uso que coordenam linguagem e domínio.
4. `PrograMago.Presentation`: presenter e contratos pequenos de entrada,
   feedback e arena. Depende da aplicação, sem componentes Unity.
5. `PrograMago.Unity`: componentes visuais e ponto de composição. É a única
   camada da fatia que depende diretamente de `UnityEngine`, UI e TextMesh Pro.

Testes Edit Mode referenciam os módulos sem Unity responsáveis pelas regras. Um
teste de integração Play Mode cobrirá a conexão da cena quando o fluxo do núcleo
estiver estável.

Dados, prefabs, arte, cenas e testes terão diretórios próprios, sem criar
abstrações vazias para funcionalidades futuras.

## Componentes iniciais

- `CodeTokenizer`: transforma o texto inteiro em tokens posicionados ou retorna
  diagnóstico léxico.
- `ClassDeclarationValidator`: reconhece a declaração vazia esperada e retorna
  sucesso exclusivo ou diagnóstico.
- `ExerciseDefinition`: identifica a atividade e o nome esperado da classe.
- `LearningSession`: guarda a atividade e se a classe foi declarada; aplica
  somente dados validados e pode reiniciar.
- `SubmitCodeUseCase`: coordena tokenização, validação e atualização da sessão.
- `GameplayPresenter`: lê o editor, chama o caso de uso e atualiza feedback e
  arena sem repetir regras.
- `GameplayBootstrapper`: cria as dependências e conecta a interface na cena.

## Fluxos

### Submissão válida

1. A view fornece o texto original ao presenter.
2. O caso de uso tokeniza toda a entrada.
3. O validador confirma a declaração esperada.
4. A sessão aplica a declaração validada.
5. O presenter limpa o erro e solicita a exibição da silhueta.

### Submissão inválida

1. Tokenização ou validação produz um diagnóstico com código e posição.
2. A sessão não é alterada.
3. O presenter exibe feedback educativo e mantém a arena no estado validado
   anterior.

### Reinício

1. O presenter solicita o reinício da sessão.
2. A sessão retorna ao estado inicial.
3. Feedback e silhueta são removidos e o texto inicial da atividade é
   restaurado.

## Erros e garantias

`ValidationResult` representa exatamente um dos estados: sucesso com dados
validados ou falha com diagnóstico. `Diagnostic` possui código, posição e dados
necessários à mensagem, sem conhecer UI. Entradas nulas são tratadas como texto
vazio. Nenhuma falha de análise pode alterar a sessão.

## Estratégia de testes

O trabalho segue ciclos RED–GREEN–REFACTOR:

- tokenizer: palavras-chave, identificador, símbolos, espaços, posições e
  caracteres desconhecidos;
- validador: declaração válida, nome incorreto, tokens ausentes e tokens extras;
- sessão: aplicação válida, idempotência e reinício;
- caso de uso: sucesso, propagação de diagnóstico e preservação de estado;
- presenter: atualização das views em sucesso, falha e reinício;
- integração Unity: referências da cena e fluxo básico dos botões.

O projeto deve compilar sem warnings próprios, passar os testes Edit Mode e
abrir a cena principal sem erros antes do encerramento da TCC-2.
