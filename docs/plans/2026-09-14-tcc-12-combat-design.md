# TCC-12 — combate automático e regras elementais

- Data: 14 de setembro de 2026
- Linear: TCC-12 (TCC / PrograMago)
- Estado: aprovado pelo usuário

## Contrato do combate

O código validado fornece os cinco atributos do Mago e as fichas dos inimigos.
O Mago base dispõe da magia neutra; subclasses futuras fornecerão fogo, água e
eletricidade. As fichas preservam vida e elemento validados. O Boneco de
Treinamento tem dano 1, alcance 1, iniciativa 1 e velocidade de ataque 3. Golem
de Gelo, Elemental de Fogo e Slime Aquático têm dano 2, alcance 1, iniciativa 5
e velocidade de ataque 5. A posição inicial do Mago é 0; os inimigos começam
nas posições 4, 6, 8 etc., na ordem em que foram instanciados.

O motor avança em passos lógicos de 0,2 segundo, sem aleatoriedade. Na abertura,
maior iniciativa age primeiro; empates favorecem o Mago e depois a ordem dos
inimigos. Após cada ciclo completo do Mago ou ação de um inimigo, a próxima
oportunidade desse ator ocorre em `16 - velocidadeAtaque` passos. Em empates
posteriores, a mesma ordem estável resolve quem age antes.

O ciclo do Mago percorre uma fila recorrente de três blocos, inicialmente
`Analisar alvo → Selecionar magia → Atacar`. Cada ciclo descarta alvo e magia
anteriores. Analisar escolhe o inimigo vivo mais próximo, com desempate pela
ordem de instanciação. Selecionar escolhe a fraqueza do alvo quando a magia
estiver disponível, ou neutra. Atacar sem alvo não causa dano. Fora do alcance,
o Mago avança uma posição e não causa dano nesse ciclo. Cada inimigo mira o
Mago; fora do alcance, avança uma posição em vez de atacar.

Um inimigo neutro recebe o dano-base de qualquer magia. Gelo recebe o dobro do
dano de fogo; fogo, o dobro de água; água, o dobro de eletricidade. Qualquer
outra combinação contra um inimigo elemental causa zero. Ataques inimigos
reduzem a vida do Mago pelo dano fixo da ficha. Cada ação atualiza vida,
posição e feedback visual. A batalha termina imediatamente quando todos os
inimigos morrem ou a vida do Mago chega a zero. A ordem ruim da fila é um
resultado da simulação, não um erro de validação.

## Pausa, interface e integração pedagógica

Os três blocos de código existentes compõem a configuração validada uma vez.
A fila de ações recorrentes aparece em coluna separada entre editor e tutorial
e pode ser reordenada por arrastar e soltar. Durante a batalha, o editor e os
atributos ficam bloqueados. Pausar congela relógio, vida e posições e permite
somente reordenar a fila. Continuar limpa alvo e magia temporários e recomeça
no primeiro bloco recorrente, sem repetir a configuração. A arena informa
vida, alvo, alcance, dano e elemento por cor/efeito.

Vitória chama o fluxo pedagógico existente. Derrota apresenta nova tentativa
com o código aprovado ou retorno à edição; a tentativa recomeça com vida,
posições e relógio iniciais, preservando a ordem escolhida para a fila. As três
atividades da Fase 1 continuam concluídas por validação. A
Fase 2 permanece bloqueada até a issue de progressão, mas suas batalhas com
`OnCombatVictory` já podem usar o motor quando liberadas.

## Verificação

Testes Edit Mode cobrem determinismo, iniciativa, intervalos, alcance,
movimento, seleção de alvo, matriz elemental, reordenação, pausa, vitória e
derrota. Testes de apresentação e Play Mode cobrem sinais visuais, travas do
editor, nova tentativa e transições pedagógicas. Cada comportamento começa
com um teste falhando.

## Alternativas consideradas

Calcular o combate diretamente no MonoBehaviour encurta a implementação, mas
dificulta provar o determinismo. Pré-calcular a luta facilita animações, mas
impede que a fila seja alterada durante uma pausa. Um motor incremental no
domínio separa as regras da Unity e permite testar e retomar o estado.
