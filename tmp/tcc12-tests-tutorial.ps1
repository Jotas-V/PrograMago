$ErrorActionPreference='Stop'
$p='Assets/PrograMago/Tests/EditMode/Domain/CombatEngineTests.cs'
$c=[IO.File]::ReadAllText((Join-Path $PWD $p))
$c=$c.Replace('CreateEngine(8, 3, 4,','CreateEngine(8, 3, 15,').Replace('CreateEngine(8, 2, 4,','CreateEngine(8, 2, 15,').Replace('CreateEngine(15, 15, 4,','CreateEngine(15, 15, 15,')
$c=$c.Replace('engine.Enemies[0].Position, Is.EqualTo(4)','engine.Enemies[0].Position, Is.EqualTo(15)')
$c=$c.Replace('Tick_OutOfRangeMovesInsteadOfDamaging','Tick_OutOfRangeStaysInPlaceWithoutDamaging').Replace('action.Kind, Is.EqualTo(CombatEventKind.Move)','action.Kind, Is.EqualTo(CombatEventKind.OutOfRange)').Replace('engine.WizardPosition, Is.EqualTo(1)','engine.WizardPosition, Is.Zero')
$c=$c.Replace('new CombatWizard(8, 3, 6,','new CombatWizard(8, 3, 15,').Replace('engine.Tick().Target, Is.EqualTo("first")','engine.Tick().Target, Is.EqualTo("second")')
$c=$c.Replace('tick < 100 && engine.Outcome','tick < 300 && engine.Outcome')
[IO.File]::WriteAllText((Join-Path $PWD $p),$c)
$p='Assets/PrograMago/Tests/PlayMode/Integration/GameplaySceneTests.cs'
$c=[IO.File]::ReadAllText((Join-Path $PWD $p))
$c=$c.Replace('PrepareCombatMago(8, 3, 4, 10, 5)','PrepareCombatMago(1, 3, 15, 5, 1)').Replace('tick < 100 &&','tick < 300 &&')
$c=$c.Replace('new Mago(8, 7, 4, 4, 2)','new Mago(4, 3, 15, 1, 2)')
$c=$c.Replace('Is.EqualTo("Atacar")','Does.Contain("Atacar")')
$c=$c.Replace('FindSceneComponent<TMP_Text>("CombatAction1Label").text,'+"`r`n"+'                Does.Contain("Atacar")','FindSceneComponent<TMP_Text>("CombatAction3Label").text,'+"`r`n"+'                Does.Contain("Atacar")')
$c=$c.Replace('FindSceneComponent<TMP_Text>("CombatAction1Label").text,'+"`n"+'                Does.Contain("Atacar")','FindSceneComponent<TMP_Text>("CombatAction3Label").text,'+"`n"+'                Does.Contain("Atacar")')
$c=$c.Replace('saved.combatBlocks[0], Does.StartWith','saved.combatBlocks[3], Does.StartWith').Replace('FindSceneComponent<TMP_Text>("CombatAction1Label").text, Does.Contain("3 comandos")','FindSceneComponent<TMP_Text>("CombatAction4Label").text, Does.Contain("3 comandos")')
$c=$c.Replace('new Vector2(0.1f, 0.85f)','new Vector2(0.03125f, 0.79f)')
$c=$c.Replace('            Assert.That(wizard.localPosition, Is.EqualTo(Vector3.zero));','            Assert.That(arenaCamera.WorldToViewportPoint(wizard.position).x, Is.LessThan(0.08f));')
[IO.File]::WriteAllText((Join-Path $PWD $p),$c)
$p='Assets/PrograMago/Runtime/Unity/GameplayBootstrapper.Tutorial.cs'
$c=[IO.File]::ReadAllText((Join-Path $PWD $p))
$c=$c.Replace('1/6 · Preserve seu Mago','1/6 · Ajuste seu Mago')
$first=@'
            "<b>Objetivo: derrotar o Boneco na casa 16.</b>\n\nO Mago fica na casa 1 e atira de longe. O Boneco não anda nem ataca. São <b>15 casas de distância</b>: use alcance 15 para acertá-lo.\n\nOs cinco atributos vêm do seu código, com até <b>25 pontos</b> no total. Exemplo válido:\n• vida = 4\n• dano = 3\n• alcance = 15\n• iniciativa = 1\n• velocidadeAtaque = 2\n\nAjuste os valores de new Mago(...) seguindo a ordem dos parâmetros do <b>seu construtor</b>. Não apague a classe.\n\nUse um bloco vazio para escrever Inimigo nas próximas etapas.",
'@
$c=[regex]::Replace($c,'(?m)^            "<b>Objetivo:.*$', $first)
$last=@'
            "<b>Todos os blocos podem ser movidos na mesma sequência.</b>\n\nClique para editar; segure e arraste para ordenar. Mago e Inimigo preparam os objetos uma vez. As chamadas seguintes se repetem em ciclo, na ordem da barra:\n\n1. analisarAlvo();\n2. selecionarMagia();\n3. lancarMagia();\n\nUse + para adicionar ações e − para remover a ação selecionada. Cada bloco pode conter várias chamadas.\n\nO Mago fica parado. Só há dano se o inimigo estiver no alcance; as casas azuis mostram a distância coberta.\n\nDurante a luta, <b>pause para reordenar</b>. Para mudar atributos, <b>segure R por 5 segundos</b>, edite o Mago e clique em Batalhar novamente."
'@
$c=[regex]::Replace($c,'(?m)^            "<b>As setas de ação.*$', $last)
$c=$c.Replace('bloco de preparação vazio','bloco vazio').Replace('Mago completo no bloco 1','Bloco Mago completo').Replace('Inimigo completo no bloco 2','Bloco Inimigo completo')
[IO.File]::WriteAllText((Join-Path $PWD $p),$c)
