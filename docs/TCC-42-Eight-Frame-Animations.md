# TCC-42 — oito quadros por animação

**Estado atual: experimento retirado das animações ativas após a revisão do usuário.** As poses aparentavam repetição e não trouxeram a fluidez esperada. Restaurados os clipes anteriores: seis quadros nas quatro formas do mago e quatro nos inimigos, com velocidade 0,30. Interface e controles mantidos. Folhas novas preservadas como material de estudo, sem referências nos clipes ativos; backup dos clipes experimentais em `tmp/tcc42-rejected-eight-clips/`. O restante deste documento registra o experimento, não a configuração ativa.

Na branch existente `codex/TCC-12`, adicionados dois desenhos intermediários para Idle, Walk e Attack de Mago, Piromante, Hidromante e Eletromante: 24 desenhos novos. Preparados também 24 intermediários para BonecoTreinamento, ElementalFogo, GolemGelo e SlimeAquatico, integrando os dois quadros adicionais desses inimigos que já existiam. Os 24 clipes agora têm oito sprites resolvidos e distintos.

As oito folhas `*-InbetweenFrames.png` usam as folhas originais e adicionais correspondentes como referência. Os 16 PNGs anteriores foram preservados byte a byte, verificado por SHA-256. Quadros originais e os seis quadros já integrados dos magos mantêm sua ordem relativa.

Escolhidos oito nesta etapa: aumento de 33% sobre os seis do mago, com duas novas poses por ação. Dez permitiriam intervalos menores, mas exigiriam mais quatro intermediários por ação e não resolveriam automaticamente diferenças de proporção. A aprovação da suavidade artística final permanece com o usuário.

Idle/Walk: `0,4,1,6,2,5,3,7`; Attack: `0,4,1,6,5,2,7,3`. Amostragem em 16 fps, mantendo clipes de 0,5 s. Animator.speed=0,30: aproximadamente 1,67 s por ciclo e 0,208 s por desenho. Idle/Walk usam loop; Attack retorna a Idle pelo controller existente.

Importação pelo Sprite Editor Data Provider, verificando capacidades antes da edição. PPU proporcional à grade original, filtro Point, sem mipmaps/compressão. Pivôs alinhados à base dos pés. Recortes adicionais dos inimigos corrigidos para excluir partes das poses vizinhas e incluir o topo do ataque, preservando GUIDs. O builder existente reaplica os 24 clipes pelo menu de integração TCC-42.

## Validação

- Nove testes de assets passaram: referências, duração, loop, controllers e ordem dos originais. O teste atualizado dos magos reproduziu seis quadros antes da integração.
- Suíte completa: 321/323. Duas falhas anteriores de GameplaySceneAssetTests permanecem: caminho antigo Mago-pixel.png e contagem 159/163 ao preparar novamente a apresentação.
- Play Mode: 192 poses renderizadas; Walk, Idle, Attack após o blend e retorno a Idle passaram nos oito controllers em 0,30.
- Personagens reais da MainScene (mago e boneco): velocidade 0,30, transições e espera pelo término do ataque confirmadas.
- Oito folhas de revisão pelo Animator e captura real da arena em 1920×1080. Objetos temporários removidos ao sair de Play Mode; Game View anterior restaurada.

Resultados: `tmp/tcc42-eight-all-tests.json`, `tmp/tcc42-eight-runtime.json`, `tmp/tcc42-eight-live-runtime.json`, `tmp/tcc42-eight-frames-report.json`. Capturas: `tmp/tcc42-eight-review/`.

Ainda existem diferenças de silhueta/proporção entre folhas originais e adicionais, especialmente no mago base e no golem. O alinhamento dos pés corrige a posição, sem eliminar essas diferenças artísticas. Pequenos efeitos que já invadem células nas folhas originais permanecem. Os controllers dos outros três inimigos estão preparados nos assets; a MainScene atual instancia o controller animado do boneco. Esta etapa não altera a seleção dos inimigos em gameplay.

O commit da interface aprovada continua sendo `61271ca`. Os ajustes posteriores de controles/velocidade e o retorno aos clipes anteriores entram na entrega seguinte. As folhas de oito quadros retiradas permanecem apenas locais para estudo, fora dessa entrega. TCC-42 continua In Progress.
