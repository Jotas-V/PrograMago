# TCC-42 — revisão da interface de pergaminho

Direção escolhida pelo usuário: livro/pergaminho simples em pixel art, poucos detalhes e espaço amplo para texto. O print rejeitou os painéis anteriores das estatísticas, da vitória e da área inferior. A aprovação anterior do cenário se limitava à floresta e aos efeitos; não abrangia a interface.

Substituídas as aplicações das molduras ornamentadas e do solo inferior por papel claro, tinta escura e botões marrons com ação principal verde. As bordas são geometria uGUI editável (PaperPanelBorder), sem bitmap esticado; os PNGs antigos continuam no projeto. Editor, tutorial, estatísticas, controles, vitória e derrota compartilham a mesma paleta. Não foram redesenhados os sprites aprovados da arena, magos, inimigos ou partículas.

O cartão de vitória agora se ancora entre 15%–85% da largura e 19%–81% da altura. Título, conquista, revisão e botão possuem áreas internas independentes, com margens e ajuste de texto. Fundo preto translúcido captura os cliques. A barra inferior mantém sua máscara com alpha branco e showMaskGraphic=false, exibindo controles sem desenhar o retângulo da máscara.

A aparência foi persistida na MainScene, além de aplicada no início do runtime. Velocidade do mago definida em 0,40 para todas as formas (ciclo efetivo de 1,25 s). Sem commit.

Revisão na Game View real: 1920×1080 e 1440×1080, com vitória aberta e fechada. Capturas em tmp/tcc42-paper-review/tcc42-paper-{workspace,victory}-final-{1920,1440}.png. Capturas foram feitas antes do ajuste final do backdrop para preto neutro; apenas o sombreado externo mudou levemente. Game View original restaurada e Play Mode encerrado.

Dois novos testes de contenção do cartão e de seus textos falharam com a composição anterior e passaram após a correção. Suíte final: 315/317. Permanecem as falhas anteriores de GameplaySceneAssetTests: caminho antigo do sprite do mago e preparação não idempotente da cena (159 objetos antes, 163 após). Resultado em tmp/tcc42-paper-tests.json. A primeira tentativa de testes foi interrompida porque o Editor estava em Play Mode; a suíte final foi executada corretamente fora de Play Mode.

A implementação está pronta para avaliação visual do usuário; a qualidade artística definitiva não é declarada aprovada apenas pelos testes de layout.