# TCC-49 — implementação e validação

Branch `codex/TCC-49-review-fixes`, baseada em `74898ea` (áudios aprovados).

## Entrega

- Passos do mapa também durante a caminhada do mago em batalha.
- Hover fora do menu com metade do ganho; menu preservado.
- Inimigos orientados para o mago e deslocamento animado de 0,44 s, com pausa.
- Ctrl+Z independente por documento, até 100 ações, agrupando digitação contínua;
  restaura texto, seleção e rolagem, sem desativar a digitação.
- Texto sem rich text nem tamanho automático; geometria e pivô compatíveis com
  o viewport do TMP para evitar cursor deslocado e rolagem incorreta.
- Barras internas de 8 px no código e nas duas páginas do grimório.
- Inimigos elementais introdutórios: alcance 8, dano 1 e recuperação adicional
  depois de atacar. O boneco segue sem dano e os perfis finais são preservados.
- Ensino de Ajustes: chamadas pelos nomes das instâncias, uma por linha, setters,
  limites de 1–15 e soma de até 25, aplicação antes da batalha e retorno aos
  valores do construtor ao limpar o bloco. Ajuda curta remete à página 4.
- Auditoria dos nove exemplos do grimório: exemplos distintos em outro contexto.
- Dano no mago com flash vermelho e som aprovado. Código inválido retira vida
  somente no desafio final, conforme a regra já existente.

## Verificação

Edit Mode: **410/412**. Os 14 testes novos de balanceamento/ensino passaram,
incluindo curto alcance e o mago introdutório com quatro pontos de vida.

Play Mode: **113/114**. Os dez testes novos de editor/apresentação passaram;
o fluxo existente de batalha final também passou após corrigir o retorno do
texto ao início ao clicar em Batalhar. Total: **24 testes novos aprovados**.

Revisão visual em 1920×1080 e 1440×1080: barra do editor, troca de código longo
para Ajustes curtos, ajuda legível e barras das duas páginas do grimório.
Save restaurado com igualdade byte a byte; execução em segundo plano restaurada.

Build Windows Standalone: **Succeeded**, zero erros, cinco avisos, 275.518.143
bytes. Executável em `tmp/tcc49-build/PrograMago.exe`, com créditos de áudio ao
lado. Inicialização em batch mode sem exceções no log; processo de verificação
encerrado após a checagem. Configurações geradas pela build foram preservadas em
backup e os seis arquivos de configuração restaurados ao estado anterior.

Falhas anteriores à tarefa, também presentes na baseline de TCC-45:

- `MagoPrefab_UsesProjectPixelArtWithPointFiltering`: referência esperada ao
  sprite antigo, enquanto o prefab utiliza a folha de animação atual.
- `Scene_PreparingPresentationAgainReusesAuthoredObjects`: 165 versus 169 objetos.
- `Arena_BackdropFillsFrameEvenUnderScaledParent` (Play Mode): escala 0,464 versus
  0,5 esperada.

Os testes preservam o save e as preferências de áudio. A revisão visual usa
backup do save e restaura a cena, a resolução e a execução em segundo plano.
