$ErrorActionPreference = 'Stop'

function Get-LinearKey {
    if ($env:LINEAR_API_KEY) { return $env:LINEAR_API_KEY }
    foreach ($file in @('.env.local', '.env')) {
        if (Test-Path -LiteralPath $file) {
            $line = Get-Content -LiteralPath $file |
                Where-Object { $_ -match '^\s*LINEAR_API_KEY\s*=' } |
                Select-Object -First 1
            if ($line) {
                return (($line -split '=', 2)[1]).Trim().Trim('"').Trim("'")
            }
        }
    }
    throw 'LINEAR_API_KEY ausente'
}

function Invoke-Linear {
    param(
        [Parameter(Mandatory = $true)][string]$Query,
        [Parameter(Mandatory = $true)][hashtable]$Variables
    )

    $body = @{ query = $Query; variables = $Variables } | ConvertTo-Json -Depth 20 -Compress
    $request = @{
        Uri = 'https://api.linear.app/graphql'
        Method = 'Post'
        Headers = @{ Authorization = $script:LinearKey; 'Content-Type' = 'application/json' }
        Body = $body
    }
    $response = Invoke-RestMethod @request

    if ($response.errors) {
        throw ($response.errors | ConvertTo-Json -Depth 10 -Compress)
    }
    return $response.data
}

function Update-Issue {
    param(
        [Parameter(Mandatory = $true)][string]$Id,
        [Parameter(Mandatory = $true)][hashtable]$PatchInput
    )

    $query = @'
mutation UpdateIssue($id: String!, $input: IssueUpdateInput!) {
  issueUpdate(id: $id, input: $input) {
    success
    issue { id identifier title dueDate state { name type } url }
  }
}
'@
    $data = Invoke-Linear -Query $query -Variables @{ id = $Id; input = $PatchInput }
    if (-not $data.issueUpdate.success) { throw "Falha ao atualizar issue $Id" }
    return $data.issueUpdate.issue
}

$script:LinearKey = Get-LinearKey
$teamId = 'a50e986e-01db-41fe-b5d0-8a1499cc2780'
$projectId = '30851c65-f385-4397-864a-da3184dcc423'
$todoStateId = '6e23b6ce-f122-4b72-be26-9891a5b5f8cb'
$inProgressStateId = 'dcd141b8-433c-4c62-affb-65d5700e4075'
$backlogStateId = 'ba0b27e1-dced-48b7-b75a-00a8e368d988'

$issues = @{
    'TCC-20' = '7d35371a-82eb-40b5-861e-574e2d352b9b'
    'TCC-21' = 'c0ebbf9b-14ea-491c-b66a-80328d8f4d0b'
    'TCC-22' = 'f6663e69-c4cf-483c-bbb9-fd3cf2a39bac'
    'TCC-23' = '72f641c3-d4b8-4e1b-9122-e27b6082ac4c'
    'TCC-24' = 'a0193898-3bef-4df3-be68-00a12a0f4983'
    'TCC-25' = '33386f85-0c36-413c-a2ca-650a5ce6178c'
    'TCC-26' = 'dba769dc-15d8-4638-b8f9-abb2460d7f86'
    'TCC-27' = '7399d207-4690-42e7-8dbe-a1c878ef26f4'
    'TCC-28' = 'ae855998-2bd5-4a9a-91aa-773fdac26223'
    'TCC-29' = '6401fe68-ba6d-4408-adcf-cd65cb617789'
    'TCC-30' = '9f3d143b-2140-4565-957c-57c3c2452ac1'
    'TCC-34' = '6e3063c5-c44f-42d7-9fac-1c6c243bede6'
    'TCC-35' = '9449e3c4-5444-43c0-bd33-06446198e78c'
}

$updates = @(
    @{
        key = 'TCC-22'
        input = @{
            title = '[Pesquisa] Entregar introdução'
            dueDate = '2026-09-10'
            stateId = $inProgressStateId
            description = @'
## Entrega escrita 1 de 4

**Prazo principal:** 10/09/2026.

## Objetivo

Produzir a introdução do artigo no modelo institucional. Caso a versão completa não fique pronta no dia 10/09, deve existir ao menos uma parte substancial redigida e revisável, mantendo o cartão em andamento até a conclusão integral.

## Conteúdo obrigatório

* Contextualização do tema e sua relevância
* Pergunta-problema
* Objetivo geral e objetivos específicos
* Justificativa
* Breve descrição dos procedimentos metodológicos previstos

## Diretrizes

* Organizar o texto do geral para o problema específico
* Usar literatura acadêmica sem transformar a introdução em uma revisão extensa
* Não antecipar resultados nem afirmar a eficácia do protótipo
* Redigir em terceira pessoa e seguir o modelo institucional

## Critérios de aceite

* O texto está coeso e pronto para leitura do orientador
* Problema, objetivos e justificativa estão alinhados
* As afirmações relevantes possuem fonte e as referências correspondem às citações
* A proposta PrograMago é apresentada sem conclusões antecipadas
'@
        }
    },
    @{
        key = 'TCC-29'
        input = @{
            title = '[Pesquisa] Entregar procedimento experimental — materiais e métodos'
            dueDate = '2026-09-17'
            stateId = $todoStateId
            description = @'
## Entrega escrita 2 de 4

**Versão completa pronta:** 17/09/2026.

**Período adicional para ajustes após orientação:** até 21/09/2026.

## Estrutura institucional

### 2 Procedimento Experimental

Descrever o desenho do estudo e o procedimento adotado.

### 2.1 Materiais

* Recursos utilizados na pesquisa
* Protótipo, softwares, instrumentos e demais materiais
* Participantes ou amostra, tamanho e forma de determinação

### 2.2 Métodos

* Natureza, abordagem, objetivos e procedimentos técnicos da pesquisa
* Instrumentos utilizados, como questionários, entrevistas, pré-teste e pós-teste
* Sequência cronológica de aplicação
* Formas de registro, tabulação e tratamento dos dados
* Aspectos éticos, consentimento, anonimização e armazenamento

## Critérios de aceite

* A versão integral está redigida até 17/09
* O procedimento é detalhado o suficiente para ser compreendido e reproduzido
* As escolhas metodológicas estão justificadas
* Materiais e métodos estão separados quando essa divisão melhorar a clareza
* O texto segue o modelo institucional e está pronto para receber ajustes até 21/09
'@
        }
    },
    @{
        key = 'TCC-34'
        input = @{
            title = '[Pesquisa] Entregar resultados e discussões'
            dueDate = '2026-10-01'
            stateId = $todoStateId
            description = @'
## Entrega escrita 3 de 4

**Prazo:** 01/10/2026.

## Objetivo

Apresentar os resultados obtidos no procedimento experimental e analisá-los em diálogo com a literatura e com os objetivos do trabalho.

## Escopo

* Organizar e apresentar os dados obtidos
* Utilizar tabelas, figuras ou gráficos quando contribuírem para a compreensão
* Discutir convergências e divergências em relação a estudos anteriores
* Relacionar os achados à pergunta-problema e aos objetivos
* Registrar limitações, dados ausentes e resultados contrários à expectativa

## Critérios de aceite

* Dados, interpretação e inferências estão claramente distinguidos
* Toda tabela ou figura possui identificação e fonte
* A discussão é sustentada pelos resultados e pela literatura
* Não são feitas generalizações além do que a amostra permite

## Dependências

Aplicação do protótipo, coleta e análise dos dados concluídas em tempo hábil.
'@
        }
    },
    @{
        key = 'TCC-35'
        input = @{
            title = '[Pesquisa] Entregar considerações finais'
            dueDate = '2026-10-01'
            stateId = $todoStateId
            description = @'
## Entrega escrita 4 de 4

**Prazo:** 01/10/2026.

## Objetivo

Encerrar o artigo respondendo à pergunta-problema com base nos resultados efetivamente obtidos.

## Escopo

* Retomar o objetivo geral e os objetivos específicos
* Responder à pergunta-problema
* Sintetizar as principais contribuições e conquistas do estudo
* Apresentar limitações e reconsiderações
* Indicar possibilidades de trabalhos futuros

## Critérios de aceite

* As conclusões decorrem dos resultados apresentados
* As limitações estão explícitas
* Não há alegação de eficácia além das evidências disponíveis
* A seção encerra o foco da pesquisa sem introduzir resultados novos

## Dependências

Resultados e discussões redigidos.
'@
        }
    },
    @{
        key = 'TCC-21'
        input = @{
            dueDate = '2026-09-17'
            stateId = $todoStateId
            description = @'
## Insumo para Procedimento Experimental

**Prazo:** 17/09/2026.

## Objetivo

Definir e registrar os aspectos éticos necessários para que a seção de Materiais e Métodos fique completa e a aplicação futura não tenha impedimentos.

## Escopo

* Confirmar com orientador e instituição as exigências para pesquisa com participantes
* Definir público provável, forma de convite e local da aplicação
* Planejar consentimento, anonimização, armazenamento e uso dos dados
* Registrar restrições de prazo e disponibilidade da turma

## Critérios de aceite

* As exigências institucionais estão registradas
* O texto metodológico descreve os cuidados éticos e o tratamento dos dados
* Nenhuma coleta é iniciada sem as autorizações e documentos necessários

## Relação

Insumo obrigatório para TCC-29.
'@
        }
    },
    @{
        key = 'TCC-30'
        input = @{
            dueDate = '2026-09-17'
            stateId = $todoStateId
            description = @'
## Insumo para Procedimento Experimental

**Prazo para definição dos instrumentos:** 17/09/2026.

## Objetivo

Definir os instrumentos que serão descritos na seção de Materiais e Métodos e posteriormente utilizados na avaliação.

## Escopo

* Criar ou estruturar pré-teste e pós-teste equivalentes
* Cobrir os conceitos de POO avaliados pelo protótipo
* Criar questões de usabilidade, motivação e percepção de aprendizagem
* Definir identificação anônima e instruções de aplicação
* Planejar os dados registrados pelo jogo

## Critérios de aceite

* Os instrumentos estão definidos o suficiente para serem descritos em TCC-29
* Cada resultado de aprendizagem possui item de avaliação
* Pré-teste e pós-teste têm dificuldade comparável
* Percepção e desempenho observado são tratados separadamente
'@
        }
    },
    @{
        key = 'TCC-20'
        input = @{
            title = '[Pesquisa/Apoio] Referências e estrutura da introdução'
            dueDate = $null
            stateId = $backlogStateId
            description = @'
## Material de apoio

**Planejamento atualizado em 10/09/2026.** Este cartão deixou de ser uma entrega independente e foi incorporado à TCC-22.

## Conteúdo a apoiar

* Fontes acadêmicas sobre ensino de programação, POO, jogos educacionais e gamificação
* Sequência de contextualização, problema, objetivos e justificativa
* Registro das referências completas e das afirmações que exigem evidência

## Uso no artigo

Utilizar este levantamento na Introdução sem transformá-la em uma revisão bibliográfica extensa.
'@
        }
    },
    @{
        key = 'TCC-23'
        input = @{
            title = '[Pesquisa/Apoio] Ensino e aprendizagem de programação'
            dueDate = $null
            stateId = $backlogStateId
            description = @'
## Material de apoio

Este cartão não é mais uma seção ou entrega independente. O conteúdo deve apoiar a contextualização da TCC-22 e, quando necessário, a discussão da TCC-34.

## Conteúdo a levantar

* Ensino e aprendizagem de programação
* Dificuldades de estudantes iniciantes
* Estratégias de apoio e feedback
* Relações entre abstração, prática e visualização
'@
        }
    },
    @{
        key = 'TCC-24'
        input = @{
            title = '[Pesquisa/Apoio] POO e dificuldades de aprendizagem'
            dueDate = $null
            stateId = $backlogStateId
            description = @'
## Material de apoio

Este cartão não é mais uma seção ou entrega independente. O conteúdo deve apoiar a contextualização da TCC-22, a delimitação conceitual da TCC-29 e a discussão da TCC-34.

## Conteúdo a levantar

* Classes, objetos, atributos, métodos e construtores
* Encapsulamento, herança, sobrescrita e polimorfismo
* Dificuldades e concepções equivocadas descritas na literatura
* Conceitos efetivamente avaliados nas fases do protótipo
'@
        }
    },
    @{
        key = 'TCC-25'
        input = @{
            title = '[Pesquisa/Apoio] Jogos educacionais e jogos sérios'
            dueDate = $null
            stateId = $backlogStateId
            description = @'
## Material de apoio

Este cartão não é mais uma seção ou entrega independente. O conteúdo deve apoiar a Introdução e a discussão dos resultados.

## Conteúdo a levantar

* Definições de jogos educacionais e jogos sérios
* Finalidade pedagógica e relação com entretenimento
* Desafios, regras, feedback e progressão
* Relação entre os elementos do PrograMago e a literatura, sem presumir eficácia
'@
        }
    },
    @{
        key = 'TCC-26'
        input = @{
            title = '[Pesquisa/Apoio] Gamificação e aprendizagem baseada em jogos'
            dueDate = $null
            stateId = $backlogStateId
            description = @'
## Material de apoio

Este cartão não é mais uma seção ou entrega independente. O conteúdo deve apoiar a Introdução e a discussão dos resultados.

## Conteúdo a levantar

* Definição de gamificação
* Definição de aprendizagem baseada em jogos
* Diferenças e pontos de contato entre as abordagens
* Relação entre fases, objetivos, desafios, feedback e progressão do protótipo
'@
        }
    },
    @{
        key = 'TCC-27'
        input = @{
            title = '[Pesquisa/Apoio] Jogos e gamificação no ensino de programação'
            dueDate = $null
            stateId = $backlogStateId
            description = @'
## Material de apoio

Este cartão não é mais uma seção ou entrega independente. O conteúdo deve apoiar a Introdução e a discussão dos resultados.

## Conteúdo a levantar

* Estratégias gamificadas no ensino de programação
* Jogos e representação visual da execução de código
* Evidências sobre motivação, desempenho e limitações
* Relação entre a literatura e as decisões pedagógicas do PrograMago
'@
        }
    },
    @{
        key = 'TCC-28'
        input = @{
            title = '[Pesquisa/Apoio] Trabalhos relacionados'
            dueDate = $null
            stateId = $backlogStateId
            description = @'
## Material de apoio

Este cartão não é mais uma seção ou entrega independente. O conteúdo deve apoiar a contextualização da Introdução e a comparação feita em Resultados e Discussões.

## Conteúdo a levantar

* Jogos e ambientes voltados ao ensino de programação e POO
* Público, conteúdo, mecânica, tecnologia e método de avaliação
* Semelhanças, diferenças e limitações
* Contribuição proposta pelo PrograMago sem alegação de novidade não comprovada
'@
        }
    }
)

$results = [System.Collections.Generic.List[object]]::new()
foreach ($update in $updates) {
    $result = Update-Issue -Id $issues[$update.key] -PatchInput $update.input
    $results.Add($result)
}

$contextQuery = @'
query Context($id: String!) {
  issue(id: $id) { assignee { id } team { id key } project { id name } }
}
'@
$context = Invoke-Linear -Query $contextQuery -Variables @{ id = $issues['TCC-29'] }
if ($context.issue.team.key -ne 'TCC' -or $context.issue.project.id -ne $projectId) {
    throw 'Escopo Linear inesperado ao criar checkpoint'
}

$createQuery = @'
mutation CreateIssue($input: IssueCreateInput!) {
  issueCreate(input: $input) {
    success
    issue { id identifier title dueDate state { name type } url }
  }
}
'@
$createInput = @{
    teamId = $teamId
    projectId = $projectId
    assigneeId = $context.issue.assignee.id
    stateId = $todoStateId
    title = '[Pesquisa] Revisar materiais e métodos após orientação'
    dueDate = '2026-09-21'
    description = @'
## Checkpoint de revisão

**Prazo final de ajustes:** 21/09/2026.

## Objetivo

Aplicar os ajustes necessários à versão de Procedimento Experimental — Materiais e Métodos que deve estar pronta em 17/09.

## Escopo

* Incorporar o retorno do orientador
* Conferir coerência entre desenho da pesquisa, participantes, instrumentos e análise
* Completar detalhes necessários à reprodução do procedimento
* Revisar aspectos éticos, citações, referências e formatação institucional

## Critérios de aceite

* Todo feedback recebido foi aplicado ou teve uma decisão registrada
* A versão revisada está coerente com a Introdução
* A seção está pronta para integrar a próxima entrega do artigo

## Dependência

TCC-29 concluída em versão integral até 17/09.
'@
}
$created = Invoke-Linear -Query $createQuery -Variables @{ input = $createInput }
if (-not $created.issueCreate.success) { throw 'Falha ao criar checkpoint de revisão' }
$results.Add($created.issueCreate.issue)

$results |
    Sort-Object { [int]($_.identifier -replace 'TCC-', '') } |
    Select-Object identifier, title, dueDate, @{ n = 'state'; e = { $_.state.name } }, url |
    ConvertTo-Json -Depth 5
