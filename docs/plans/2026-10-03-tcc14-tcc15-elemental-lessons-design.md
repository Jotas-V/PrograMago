# TCC-14/TCC-15 — integração de herança e sobrescrita

Desenho aprovado pelo usuário em 03/10/2026: começar com Piromante e Golem.

- Etapa 7: acrescentar public class Piromante extends Mago ao documento Mago e combater o Boneco com a forma de fogo do único personagem.
- Etapa 8: sobrescrever lancarMagia no Piromante com @Override e super; substituir a instância do Boneco pela ficha do Golem de Gelo (12 de vida, gelo) e usar essa instância na chamada da magia.
- A forma usada nas duas atividades é Piromante, explicitamente ensinada no tutorial. Não escolher a forma automaticamente com base no alvo. A estratégia condicional fica para a etapa 9.
- Reutilizar os validadores existentes; rejeitar ausência do Piromante, sobrescrita em outra classe ou inimigo que não corresponda à atividade antes de iniciar combate/aplicar Ajustes.
- Manter edição livre, uma única entidade Mago, atributos e preparação existentes. Não alterar dano, movimento, sprites ou clipes.
- Ampliar a progressão até a etapa 8 e reutilizar a migração por prefixo de saves v4; a etapa 9 permanece bloqueada.
- Testar progressão, compilação da forma fixa e percurso PlayMode completo com diagnóstico, fogo contra gelo, vitória e recarga.

## Validação e resultado

- As etapas 7 e 8 estão liberadas após a etapa de magia; a etapa 9 continua bloqueada.
- Os cinco testes iniciais de compilação/progressão foram observados falhando antes da implementação e passaram depois. O percurso PlayMode foi observado falhando no bloqueio da etapa 7 e passou após a integração.
- O percurso integrado cobre herança inválida, correção, forma Piromante, um único ator Mago, fogo eficaz contra o Golem, vitória e recarga. A seleção do bloco é restaurada depois de renderizar a lição.
- Save v4 com seis atividades concluídas migra para oito atividades disponíveis sem fabricar conclusão das novas etapas.
- EditMode completo: 367/369 aprovados. PlayMode completo: 44/45 aprovados. Permanecem apenas as falhas anteriores MagoPrefab_UsesProjectPixelArtWithPointFiltering, Scene_PreparingPresentationAgainReusesAuthoredObjects e Arena_BackdropFillsFrameEvenUnderScaledParent.
- Relatórios locais: C:/Users/Pichau/Documents/TCC/PrograMago/tmp/tcc14-15-dev-editmode.xml e tcc14-15-dev-playmode.xml.
- Descrições da TCC-14 e TCC-15 alinhadas ao recorte aprovado, com estados preservados. A TCC-14 não foi concluída antecipadamente: os modelos de Hidromante e Eletromante existem, mas o uso na estratégia e o restante da jornada continuam nas próximas integrações.
- Execução dos testes no checkout dev em batch. O checkout principal aberto na interface do Unity e suas alterações locais de arte não foram modificados.
