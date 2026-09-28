# TCC-12: Combate automático e faixa inferior

**Status:** aprovado para implementação em 28/09/2026  
**Issue:** TCC-12 — Implementar combate automático e regras elementais  
**Branch:** `codex/TCC-12`

## Objetivo

Restaurar a área de código sem elementos sobrepostos e deixar a faixa inferior explicar o fluxo real: configurar o Mago, selecionar um único comando de ataque e iniciar uma batalha automática. Builds válidas com pouco alcance devem continuar viáveis, usando o movimento na arena para chegar ao alvo.

## Decisões aprovadas

- Os cinco seletores — Mago, Inimigo, Código, Métodos e Preparação — permanecem na faixa inferior, junto à timeline e ao botão **Batalhar**. A faixa não cobre o editor de código.
- A timeline mostra um único bloco **Atacar**. Ele continua selecionável para editar a chamada, mas não pode ser duplicado, removido ou reordenado. A batalha repete esse comando automaticamente.
- A preparação da ficha acontece uma vez antes da batalha. Os cinco atributos validados continuam dirigindo vida, dano, alcance, iniciativa e velocidade de ataque. Mantêm-se os limites atuais de 1–15 por atributo e 25 pontos no total; esta entrega não rebalanceia valores.
- A arena mantém 16 casas. O Mago começa na casa 1 e o Boneco na casa 16. Uma build válida pode começar a batalha com qualquer alcance aceito.
- No turno do Mago, se o alvo mais próximo estiver fora do alcance, o Mago avança uma casa na direção dele e não causa dano naquele turno. Ao entrar no alcance, ataca normalmente.
- Inimigos móveis também avançam uma casa por turno na direção do Mago enquanto estiverem fora do próprio alcance; quando estão ao alcance, atacam. O Boneco de Treinamento permanece imóvel e não ataca.
- O `CombatEngine` continua sendo a fonte das regras e do resultado determinístico. A interface apresenta posições, vida, mensagens e efeitos.
- Saves antigos preservam a ficha e o progresso pedagógico. Blocos e ordens de combate antigos são normalizados para um único `lancarMagia();` e uma sequência fixa dos seletores.
- O tutorial deixa de exigir alcance 15 e de ensinar arrastar, reordenar, adicionar ou remover comandos. Explica avanço automático e preparação dos atributos.

## Fluxo do jogador

1. Escreve e conclui as declarações exigidas pelas fases iniciais.
2. Seleciona **Métodos** ou **Preparação** na faixa inferior e ajusta a ficha antes de iniciar.
3. Confirma que a build está completa e dentro dos limites; erros rejeitam a alteração inteira e mantêm a edição disponível.
4. Seleciona **Atacar**, confirma `lancarMagia();` no editor e clica **Batalhar**.
5. A cada turno do Mago, o motor ataca se o alvo estiver no alcance; caso contrário, move uma casa em direção ao alvo. A batalha segue até vitória ou derrota.
6. Em derrota, o jogador pode tentar novamente ou editar a build. A nova tentativa começa das posições, vida e ficha iniciais validadas.

## Comportamento de alcance e movimento

Distância é a diferença absoluta entre as casas ocupadas. O ataque é permitido quando `distância <= alcance`. Se `distância > alcance`, a entidade móvel muda sua posição em exatamente uma casa em direção ao alvo no próprio turno e emite um evento de movimento; o combate não aplica dano nesse turno. O Boneco tem dano zero e, portanto, não recebe turno de movimento nem de ataque.

Para a batalha inicial, com Mago na casa 1 e Boneco na casa 16, alcance 15 permite atacar no primeiro turno. Alcance 5 faz o Mago avançar uma casa por turno até alcançar a distância permitida. Os atributos de vida, dano, iniciativa e velocidade continuam a controlar os efeitos que já possuem no motor.

## Persistência e compatibilidade

- A ordem dos seletores de código é fixa e não depende de gestos de arrastar.
- Ao carregar um save, descartar a multiplicidade/ordem antiga de ações de combate e restaurar um único bloco com a chamada canônica. Preservar classes, métodos aprovados, preparação, distribuição validada e progresso de fases.
- Ao salvar, persistir o comando único e a ordem fixa para que reload e retry não recriem a timeline antiga.
- Pausar não é pré-requisito para editar uma ordem. Se o controle de pausa permanecer na cena para interromper a simulação, ele não habilita reordenação.

## Critérios de aceite e validação

- A cena em Edit Mode e Play Mode posiciona os cinco seletores e **Atacar** na faixa inferior, sem sobreposição sobre o editor.
- Não há arraste, `+`, `−` ou edição de ordem para os comandos de batalha; clicar em **Atacar** ainda abre o código do bloco.
- Build válida abaixo de alcance 15 inicia a batalha e desloca o Mago uma casa por turno até atacar o Boneco.
- O Boneco continua imóvel e sem ataque; inimigos móveis avançam uma casa por turno e atacam apenas quando estão no próprio alcance.
- Atributos da preparação aprovada chegam ao motor uma vez e continuam controlando o combate; inválidos são rejeitados por inteiro.
- Um save legado carrega com a mesma ficha/progresso e uma timeline de apenas um comando.
- O tutorial ensina o fluxo novo e não exige alcance 15 nem reordenação.
- Executar EditMode e PlayMode no Unity e inspecionar visualmente a MainScene no Editor.

## Fora de escopo

- Ajuste de balanceamento numérico.
- Novas magias, ataques especiais, padrões de movimento ou comportamento ofensivo do Boneco.
- Alterações aos limites aprovados da build ou à progressão de fases.
