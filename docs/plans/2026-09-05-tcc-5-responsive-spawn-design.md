# TCC-5 — Spawn do mago e layout responsivo

## Contexto

O playtest mostrou três problemas na implementação atual: a prévia do mago foi
representada por vários objetos de silhueta em vez de nascer no spawn point, a
tecla Enter não cria uma nova linha no editor e a interface usa dimensões fixas
que não acompanham o tamanho do Game View.

## Decisão de arquitetura

O `WizardSpawnPoint` permanece como um `Transform` vazio dentro de
`ArenaWorld`. A representação provisória do mago será um prefab de mundo
chamado `Mago`, composto por um único quadrado com `SpriteRenderer`.

O `GameplayBootstrapper` receberá referências serializadas para o spawn point e
para o prefab. Quando a prévia reconhecer uma declaração válida, o prefab será
instanciado na posição e rotação do spawn point. A instância será reutilizada e
apenas alternará entre ativa e inativa conforme o código ficar válido ou
inválido. Assim, a arte definitiva poderá substituir o conteúdo do prefab sem
alterar o fluxo de gameplay.

Como o Canvas está em Screen Space Overlay, o fundo opaco da arena atualmente
esconde objetos do mundo. O `ArenaFrame` continuará delimitando a área superior,
mas seu gráfico não cobrirá a renderização da câmera. A câmera fornecerá o fundo
branco da arena e o prefab continuará pertencendo ao mundo.

## Layout responsivo

O `CanvasScaler` usará `Scale With Screen Size`, com referência 1920×1080 e
equilíbrio entre largura e altura. Os contêineres serão configurados com
`RectTransform` e âncoras relativas:

- `ArenaFrame` ocupará toda a largura da faixa superior.
- `BottomArea` ocupará toda a largura e o espaço restante.
- `CodeEditorPanel` preencherá aproximadamente 75% da faixa inferior.
- `TutorialPanel` preencherá aproximadamente 25% da faixa inferior.
- O separador e o botão Batalhar acompanharão seus respectivos contêineres.

O layout permanecerá lado a lado mesmo em janelas estreitas, reduzindo a escala
e preservando todos os elementos dentro da área do Game View.

## Entrada de código

O editor continuará multilinha e editável. Pressionar Enter enquanto o campo
estiver focado deverá inserir uma quebra de linha no cursor, manter o foco e não
acionar Batalhar.

## Verificação

Os testes EditMode validarão a estrutura persistida da cena, as âncoras, o
`CanvasScaler`, o spawn point vazio e a referência ao prefab. Os testes PlayMode
validarão a instanciação no spawn, a alternância da prévia e a inserção de nova
linha. Ao final, os mesmos caminhos serão conferidos manualmente no Unity em
mais de um tamanho de Game View.
