# TCC-42 — animações das quatro formas do mago

Aplicação da direção aprovada na TCC-40. Branch existente: `codex/TCC-12`. Integração inicial e interface aprovadas comitadas em `61271ca`. A configuração ativa voltou aos seis quadros do mago, mantendo velocidade 0,30, após o usuário considerar as oito poses repetitivas e pouco fluidas. O experimento retirado está em [TCC-42-Eight-Frame-Animations.md](TCC-42-Eight-Frame-Animations.md).

## Integração inicial de seis quadros (histórico)

Os doze clipes de Idle, Walk e Attack usam seis sprites distintos, preservando os quatro originais e intercalando os dois novos. Idle/Walk seguem 0,4,1,2,5,3; Attack segue 0,4,1,5,2,3. A amostragem passou de 8 para 12 fps para conservar ciclos de 0,5 s; a velocidade de Animator está em 0,30 (ciclo efetivo de aproximadamente 1,67 s). Idle e Walk continuam em loop; Attack permanece sem loop.

Foram corrigidas linhas YAML concatenadas nas quatro folhas adicionais. Os GUIDs únicos já presentes no checkout foram preservados. O PPU adicional usa `PPU original * 512 / 362`; filtro Point, sem mipmaps e sem compressão. Os controllers/overrides existentes já referenciam os clipes editados.

## Revisão visual no Editor

Unity 6000.3.11f1 aberto: renderizados os 72 quadros pelo Animator em Play Mode e conferidas as quatro folhas de revisão. Capturas reais da MainScene em 1920×1080 e 1440×1080, para Idle, Walk e Attack. A Game View foi ajustada temporariamente às resoluções, sem esticar a imagem de outra proporção. Objetos temporários foram removidos ao sair do Play Mode; tamanho anterior da Game View restaurado e MainScene deixada aberta, sem alterações pendentes na cena.

Correções realizadas pela API do Sprite Editor após verificar as capacidades do importer:

- Recortes de Walk excluem os fragmentos do ataque que invadiam a linha vizinha.
- Recortes de Attack incluem o topo de cajado/magia que ultrapassava a grade e excluem o personagem da coluna vizinha; alguns recortes de ataque foram ampliados horizontalmente.
- Ajustados pivôs adicionais para igualar a linha de apoio dos pés à dos sprites originais. Recortes não precisam ter dimensões idênticas: o alinhamento visual é dado pelo pivô, sem incluir partes de outra célula.
- O `CombatFootAnchor` do prefab Mago passou de -0,5611765 para 8/300 unidades, compatível com o pivô inferior e a margem dos pés. Isso corrigiu o mago flutuando na arena.
- Fontes PNG e quatro frames originais de cada sequência preservados. Arena, UI, inimigos, projéteis e balanceamento não receberam alterações.

Verificadas em runtime, nas quatro formas: Walking=true chega a Walk, Walking=false retorna a Idle, trigger Attack chega a Attack e o fim do ataque retorna a Idle, respeitando os blends do controller.

**Ressalva visual:** os desenhos adicionais ainda diferem em proporção/silhueta dos quatro originais, especialmente no Mago base. O ajuste dos recortes e pivôs resolve artefatos e apoio dos pés, mas não torna os desenhos geometricamente uniformes. A suavidade/consistência artística ainda não está aprovada. Pequenas partículas que já aparecem nos frames originais foram preservadas.

## Validação

- Unity importou e executou a integração dos clipes com código 0.
- Cinco testes dos magos passaram: doze clipes, referências resolvidas, controllers, pivôs, importação, duração e âncora do prefab. O novo teste da âncora reproduziu a falha antes da correção.
- Suíte EditMode após revisão: 310/312. As mesmas duas falhas pré-existentes de `GameplaySceneAssetTests` permanecem: expectativa antiga do caminho do sprite do prefab e duplicação de objetos ao preparar novamente a apresentação. No trabalho anterior, ambas foram reproduzidas com os clipes antigos.
- Não apareceram erros de compilação ou de gameplay durante a revisão. O Pipeline registrou um timeout de captura ao entrar em Play Mode; a repetição após a transição concluiu com sucesso.

Capturas e resultados estão em `tmp/tcc42-visual-review/`: quatro folhas `*-cycles.png`, seis capturas `tcc42-review-{idle,walk,attack}-{1920,1440}.png` e `editmode-results.json`.

A integração dos clipes pode ser reaplicada pelo menu **PrograMago > Art > Integrate TCC-42 Wizard Animations** (`TCC42WizardAnimationBuilder.Build`), mantendo os recortes/pivôs gravados nos importers. Inimigos, projéteis e UI continuam como etapas futuras; TCC-42 permanece In Progress.

## Ajuste posterior de velocidade e cenário

As quatro formas agora usam Animator.speed=0,30, após o pedido de uma nova redução. A verificação anterior das transições Idle/Walk/Attack e retorno ao Idle em Play Mode foi realizada em 0,60.

O fundo de floresta agora mantém a proporção da textura pela altura da arena e repete blocos espelhados até cobrir a largura necessária, ocultando os excedentes. Vento, folhas e vaga-lume também mantêm escala uniforme. Folhas seguem o mesmo grupo de vento sempre da esquerda para a direita, com deslocamento de 4% da largura atrás dele. Cada passagem dura 6,5–8,5 s, com pausa de 1,8–3,5 s e entrada/saída suaves.

Revisão na Game View real em 1920×1080 e 1440×1080: floresta sem lacunas, árvores sem alongamento e efeitos discretos acompanhando a composição. Os sprites do cenário podem ser mantidos; não foi necessário redesenhar PNGs. A repetição espelhada é perceptível, mas não apresenta corte abrupto. Esta conclusão se refere ao cenário; a ressalva artística dos quadros adicionais do mago acima permanece.

Três testes de apresentação passaram após reproduzir as falhas antes das correções. Suíte completa: 313/315; permanecem as mesmas duas falhas de GameplaySceneAssetTests já registradas. Capturas finais em `tmp/tcc42-scenery-review/tcc42-scenery-final-1920-motion.png` e `tcc42-scenery-final-1440.png`; resultado completo em `tmp/tcc42-visual-review/scenery-editmode-results.json`. Capturas usam quatro atores temporários para comparar formas; eles são removidos ao sair de Play Mode.

A versão aprovada da interface e a integração anterior foram comitadas em 61271ca. Os ajustes posteriores de controles e velocidade estão descritos em TCC-42-Controls-And-Cadence.md.
