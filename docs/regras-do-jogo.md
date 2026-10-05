# Regras do jogo — PrograMago

## Progressão e código

O jogo tem nove batalhas guiadas. Na fase 9, o jogador escolhe Piromante para gelo, Hidromante para fogo, Eletromante para água e Mago para o alvo neutro usando if/else e uma única chamada polimórfica `lancarMagia(alvo)`.

A build tem até 25 pontos, com cada atributo entre 1 e 15. Vida, dano, alcance, iniciativa e velocidade competem pelo mesmo orçamento. A vida dos inimigos é a declarada no bloco Inimigo, validada pelo catálogo: 10 para o Boneco e 12 para Golem, Elemental e Slime. Os três blocos de código permanecem editáveis conforme a etapa e são preservados ao batalhar.

## Combate da fase 9 — TCC-17

O motor mantém o alvo vivo mais próximo, movimento por casas e turnos definidos pela velocidade de ataque, com iniciativa como desempate. O elemento correto causa o dobro do dano; uma escolha elemental errada não causa dano ao inimigo elemental. O alvo neutro recebe o dano base.

| Inimigo | Papel | Dano | Alcance em casas | Iniciativa | Velocidade |
| --- | --- | ---: | ---: | ---: | ---: |
| Boneco | Treinamento, sem dano | 0 | 1 | 1 | 3 |
| Golem de Gelo | Mais forte e lento | 2 | 2 | 3 | 4 |
| Elemental de Fogo | Pressão à distância | 1 | 10 | 7 | 4 |
| Slime Aquático | Aproximação mais rápida | 1 | 2 | 6 | 6 |

Velocidade maior reduz o intervalo entre ações (`16 - velocidade` ticks). A combinação de alcance e iniciativa permite ao elemental ameaçar builds com pouca vida mesmo que o jogador selecione os elementos corretos. Na fase 9, a formação inicial fica três casas mais próxima do Mago que nas fases anteriores. Isso permite pressão inicial sem aumentar exageradamente o dano. As fases anteriores mantêm os perfis e posições existentes.

### Erro de código e vida da tentativa

Na fase 9, clicar em **Batalhar** com código inválido retira 20% da vida máxima da tentativa, arredondando para cima, com mínimo de 1. O jogador continua vendo o diagnóstico do erro e o código que escreveu. O controle de vida é local à tentativa na sessão; o salvamento mantém código e progressão, sem retomar combate em andamento após recarregar. Digitar ou navegar entre os blocos não causa dano.

A penalidade acumula entre submissões. Corrigir o código ou editar atributos não cura a vida da tentativa. A vida de referência é a última build aprovada no início da tentativa. Antes da primeira build aprovada da fase 9, usar a build aprovada anterior. Se ainda não houver build aprovada, o erro é mostrado sem inventar atributos de vida.

Vida zero significa derrota. **Tentar novamente** restaura a vida e mantém o código; **Editar código** após a derrota também inicia uma nova tentativa para correção; se o erro ainda existir, o jogador pode corrigi-lo antes de submeter novamente. Uma estratégia válida com seleção elemental incorreta entra no combate e pode resultar em derrota por ataques dos inimigos.

### Resultado e apresentação

Um inimigo com vida zero não age nem bloqueia a movimentação no motor. Seu sprite desaparece após seus impactos pendentes terminarem, sem esperar animações de outros inimigos. O golpe final precisa terminar visualmente antes da vitória. Reiniciar um combate restaura a vida e a visibilidade dos inimigos.

## Validação e escopo

As regras de balanceamento foram aprovadas em 04/10/2026 e registradas na TCC-17. Os testes cobrem builds frágeis, vitória com dano, seleção elemental errada, penalidade acumulada, correção sem cura, reinício e desaparecimento dos inimigos.

Esta é a documentação operacional do jogo. A seção acadêmica escrita do TCC será atualizada em outra etapa, conforme combinado.

## Validação de 04/10/2026

12 testes novos aprovados: 5 EditMode e 7 PlayMode. Suítes completas: 386/388 EditMode e 60/61 PlayMode. As três falhas já existentes são a expectativa antiga de Mago-pixel.png, a contagem de objetos ao preparar novamente a cena e o enquadramento do fundo sob pai escalado. Nenhuma falha nova.

Verificação visual do código, destaque do botão selecionado, diagnóstico e perda de vida na Game View. Exemplo de fase 9 validado diretamente a partir de docs/tcc-12-exemplos-por-fase.md. Cópia do progresso e código originais preservada durante a inspeção temporária.
