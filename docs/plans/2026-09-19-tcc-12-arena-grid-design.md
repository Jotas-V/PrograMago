# TCC-12 — arena em casas e sequência única

Solicitação do jogador em 19/09/2026, continuando na branch codex/TCC-12 e no
mesmo checkout. Mantém os cinco atributos extraídos do código e o teto de
25 pontos; nenhum balanceamento altera silenciosamente a ficha do Mago.

## Arena

16 casas visíveis (1–16, internamente 0–15). Mago fixo na casa 1; primeiro inimigo
na 16, demais nas casas anteriores livres. Um inimigo ativo avança no máximo uma
casa por oportunidade até ficar no próprio alcance e nunca entra em casa
ocupada. Boneco continua imóvel e passivo. Alcance é a diferença entre casas:
da 1 à 16 são 15. Ataque fora do alcance não move o Mago nem causa dano.

Combate e atributos permanecem no CombatEngine. Vida determina resistência,
dano determina dano-base, iniciativa desempata a ordem de atuação, velocidade
define o intervalo de 16 - velocidade passos de 0,2 segundo e alcance determina
as casas atingíveis. O tutorial mostra uma distribuição válida de 25 pontos
para o Boneco distante, sem substituir o código do usuário.

Fundo original de floresta pixel art, com chão horizontal; sprites sobre o chão,
casas discretas numeradas, alcance destacado e ficha legível sobre painéis.
A apresentação se adapta ao retângulo atual da arena.

## Blocos

Uma barra compacta de setas conectadas, sem separar áreas de preparação e ação.
Todos os blocos podem ser selecionados, editados e arrastados. As declarações
continuam formando o programa executado uma vez; chamadas de ação repetem em
ciclo, na ordem em que aparecem na mesma barra. Mudar a posição de uma definição
durante a pausa não recria personagens em uma batalha já iniciada.

A alternativa de impor dependências entre declarações e chamadas mudaria também
o subconjunto de linguagem aceito. A implementação segue a recomendação de
preservar a semântica aprovada e unificar a manipulação visual. Ordem e conteúdos
persistem, com leitura dos saves v3 anteriores.

## Implementação e validação

1. Testar extremos, alcance, imobilidade do Mago e ocupação das casas; atualizar
   o motor e os testes antigos cujas posições iniciais mudaram.
2. Testar valores vindos do código e orçamento, incluindo dano e intervalo reais.
3. Testar arraste entre qualquer par de blocos e persistência; unificar a barra.
4. Integrar fundo via Editor e alinhar pés, casas e projéteis; atualizar tutorial.
5. Executar EditMode/PlayMode e inspecionar a cena em resoluções distintas.

## Arte

Gerada com a ferramenta integrada image_gen. Prompt: floresta lateral original
em pixel art para PrograMago, camadas de troncos e luz verde suave, chão de grama
horizontal, fundo dessaturado para contraste com personagens, sem personagens,
interface, grade, texto ou marcas. Asset final: Resources/Arena/ForestArena.png.

## Validação concluída em 20/09/2026

- EditMode: 271 testes aprovados. PlayMode: 41 aprovados.
- Reproduzidos antes da correção: aceitação de 16 inimigos; posição incorreta
  do Boneco após redimensionamento; fundo reduzido pela escala do Canvas.
- Prefab Mago possui CombatFootAnchor alinhado ao primeiro pixel opaco dos pés,
  mantendo o sprite original. HUD de preparação apresenta atributos em três linhas.
- Inspeção visual em 1920×1080 e 1366×768. Capturas locais em TestResults:
  arena-range-one.png, arena-ranged-projectile.png e arena-1366.png.
- Com alcance 1, Mago ficou na casa 1 e Boneco na 16 com 10 de vida;
  mensagem explicou distância 15 e como voltar à edição. Com dano 3 e alcance 15,
  o projétil atravessou a arena e o Boneco ficou com 7 de vida no primeiro ataque.
- Testes de código cobrem atributos, orçamento e parâmetros em ordem diferente;
  nenhuma ficha é substituída silenciosamente. Balanceamento permanece pendente.
