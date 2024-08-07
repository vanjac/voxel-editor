// Source: Google Translate
// Additional resources:
// https://georeference.org/forum/t150812.62
// https://steamcommunity.com/app/519860/discussions/6/1744512496199584988
// https://www.construct.net/en/forum/construct-3/portugues-pt-br-14/100-today-132770
public class PortugueseStrings : GUIStringSet {
    // World -> Mundo
    // Paint -> Tinta
    // Material -> Material
    // Overlay -> Sobreposição
    // Custom Texture -> Textura Personalizada
    // Bevel -> Chanfro
    // Edge -> Aresta
    // Object -> Objeto
    // Tag -> Tag
    // Pivot -> Pivô
    // Substance -> Substância
    // Behavior -> Comportamento
    // Sensor -> Sensor
    // On/Off -> Ligado/Desligado
    // Activator -> Ativador
    // Target -> Alvo
    // Input -> Entrada
    // Player -> Jogador
    // Health -> Saúde
    // Face -> Face

    public override string Yes =>
        "Sim";
    public override string No =>
        "Não";
    public override string Done =>
        "Feito";
    public override string Close =>
        "Fechar";
    public override string AreYouSure =>
        "Tem certeza?";

    public override string LoadingWorld =>
        "Carregando mundo...";

    public override string WelcomeMessage =>
        "Bem vindo ao N-Space\nSeguir o tutorial é recomendado!";
    public override string StartTutorial =>
        "Tutorial";
    public override string CreateNewWorld =>
        "Novo Mundo";
    public override string IndoorWorld =>
        "Interior";
    public override string FloatingWorld =>
        "Flutuante";
    public override string PlayWorld =>
        "Jogue";
    public override string RenameWorld =>
        "Renomear";
    public override string CopyWorld =>
        "Copiar";
    public override string DeleteWorld =>
        "Excluir";
    public override string ShareWorld =>
        "Compartilhar";
    public override string WorldNamePrompt =>
        "Digite o novo nome mundial...";
    public override string WorldRenamePrompt(string oldName) =>
        $"Digite o novo nome para {oldName}";
    public override string WorldDeleteConfirm(string name) =>
        $"Tem certeza de que deseja excluir {name}?";
    public override string ErrorCreatingWorld =>
        "Erro ao criar arquivo mundial";
    public override string ErrorSpecialCharacter =>
        "Esse nome contém um caractere especial que não é permitido.";
    public override string ErrorPeriodName =>
        "O nome não pode começar com ponto final.";
    public override string ErrorWorldAlreadyExists =>
        "Já existe um mundo com esse nome.";

    public override string OpenHelp =>
        "Ajuda";
    public override string OpenAbout =>
        "Sobre";
    public override string OpenWebsite =>
        "Site";
    public override string OpenVideos =>
        "Vídeos";
    public override string Donate =>
        "Doar";
    public override string Language =>
        "Idioma";
    public override string LanguageAuto =>
        "<i>Automático</i>";

    public override string ImportWorldNamePrompt =>
        "Digite o nome do mundo importado...";
    public override string ImportingFile =>
        "Importando arquivo...";
    public override string ImportWorldError =>
        "Erro ao importar o mundo";

    public override string CreateObjectTitle =>
        "Criar";
    public override string OpenWorldProperties =>
        "Mundo";
    public override string SelectSubmenu =>
        "Selecione...";
    public override string SelectDraw =>
        "Desenhar";
    public override string SelectWithPaint =>
        "Com Tinta";
    public override string SelectFillPaint =>
        "Preencher";
    public override string SelectWithTag =>
        "Com Tag";
    public override string OpenBevel =>
        "Chanfrar";
    public override string RevertChanges =>
        "Reverter";
    public override string ConfirmRevertWorldChanges =>
        "Desfazer todas as alterações desde que o mundo foi aberto?";
    public override string SelectWithTagTitle =>
        "Selecione por tag";
    public override string SelectWithPaintInstruction =>
        "Toque para escolher a tinta...";
    public override string SelectFillPaintInstruction =>
        "Toque para preencher a tinta...";
    public override string CreateSubstanceInstruction =>
        "Empurre ou puxe para criar uma substância";
    public override string DrawSelectInstruction =>
        "Toque e arraste para selecionar";
    public override string EntityPickNone =>
        "Nada";
    public override string PickObjectInstruction =>
        "Selecione um objeto...";
    public override string PickObjectCount(int count) =>
        $"{count} objetos selecionados";

    public override string SensorsDetect =>
        "Deteco";
    public override string SensorsLogic =>
        "Lógica";
    public override string BehaviorsMotion =>
        "Mover";
    public override string BehaviorsGraphics =>
        "Gráficos";
    public override string BehaviorsLife =>
        "Vida";
    public override string BehaviorsPhysics =>
        "Física";
    public override string BehaviorsSound =>
        "Som";

    public override string BevelSelectEdgesInstruction =>
        "Selecione arestas para chanfrar...";
    public override string BevelHeader =>
        "Chanfrar:";
    public override string BevelNoSelection =>
        "(nenhum selecionado)";
    public override string BevelShapeHeader =>
        "Forma:";
    public override string BevelSizeHeader =>
        "Tamanho:";

    public override string ImportFile =>
        "Importar arquivo";
    public override string ImportFromWorldHeader =>
        "Ou importar de um mundo...";
    public override string NoDataInWorld(string type) =>
        $"O mundo não contém arquivos X.";

    public override string FilterSpecificObject =>
        "Objeto específico";
    public override string FilterTags =>
        "Tags";
    public override string FilterTagsTitle =>
        "Filtrar por tags";
    public override string FilterActiveBehavior =>
        "Comportamento"; // shortened for space
    public override string FilterActiveBehaviorTitle =>
        "Filtrar por comportamento ativo";
    public override string FilterNothing =>
        "Nada";
    public override string FilterWithTag(string tag) =>
        $"Com etiqueta {tag}";
    public override string FilterMultipleTags(string tags) =>
        $"Tags {tags}";

    public override string TargetAny =>
        "Qualquer";
    public override string TargetWorld =>
        "Mundo";
    public override string TargetLocal =>
        "Local";
    public override string TargetLocalDirection(string dir) =>
        $"{dir} local";
    public override string TargetPickObject =>
        "Selecionar objeto...";
    public override string TargetRandom =>
        "Aleatório";
    public override string Center =>
        "Centro";
    public override string North =>
        "Norte";
    public override string South =>
        "Sul";
    public override string East =>
        "Leste";
    public override string West =>
        "Oeste";
    public override string Up =>
        "Cima";
    public override string Down =>
        "Baixo";
    public override string Top =>
        "Cima";
    public override string Bottom =>
        "Baixo";
    public override string NorthLetter =>
        "N";
    public override string SouthLetter =>
        "S";
    public override string EastLetter =>
        "L";
    public override string WestLetter =>
        "O";

    public override string CustomTextureCategory =>
        "PERSONALIZADO";
    public override string MaterialImportFromWorld =>
        "Importar do mundo...";
    public override string MaterialColorHeader =>
        "Ajustar cor";
    public override string ColorTintMode =>
        "Tingir";
    public override string ColorPaintMode =>
        "Pintar";
    public override string CustomTextureDeleteConfirm =>
        "Tem certeza de que deseja excluir esta textura personalizada?";
    public override string NoCustomMaterialsInWorld =>
        "O mundo não contém texturas personalizadas para materiais.";
    public override string NoCustomOverlaysInWorld =>
        "O mundo não contém texturas personalizadas para sobreposições.";

    public override string PaintMaterial =>
        "Material";
    public override string PaintOverlay =>
        "Sobreposição";

    public override string PropertiesDifferent =>
        "diferente";
    public override string CloneEntity =>
        "Clonar";
    public override string CloneInstruction =>
        "Toque para colocar o clone";
    public override string DeleteEntity =>
        "Excluir";
    public override string ChangeSensor =>
        "Alterar Sensor";
    public override string AddBehavior =>
        "Comportamento"; // already has + icon
    public override string RemoveBehavior =>
        "Remover";
    public override string OtherBehaviorsPlaceholder =>
        "mais comportamentos...";
    public override string SensorName(string name) =>
        $"Sensor {name}";
    public override string BehaviorName(string name) =>
        $"{name}"; // removed for space
    public override string NoSensor =>
        "Sem Sensor";
    public override string TargetEntity(string name) =>
        $"Alvo:  {name}";
    public override string EntityActivators =>
        "Ativadores";

    public override string EntityRefNone =>
        "Nada";
    public override string EntityRefSelf =>
        "Auto";
    public override string EntityRefTarget =>
        "Alvo";
    public override string EntityRefActivator =>
        "Ativador";
    public override string RangeSeparator =>
        "a";
    public override string ChangeProperty(string name) =>
        $"Alterar {name}";
    public override string SelectProperty(string name) =>
        $"Selecione {name}";
    public override string SensorConditionHeader =>
        "Quando o sensor está:";
    public override string SensorOn =>
        "Lig"; // abbreviated
    public override string SensorOff =>
        "Deslig";
    public override string SensorBoth =>
        "Ambos";
    public override string WhenSensorIsOn =>
        "Quando sensor ligado";
    public override string FilterByTitle =>
        "Filtrar por...";
    public override string Camera =>
        "Câmera";
    public override string InputsHeader =>
        "Entradas:";
    public override string AddInput =>
        "Entrada"; // already has + icon

    public override string WorldWarningsHeader =>
        "Houve alguns problemas com a leitura do mundo:";
    public override string UnknownSaveError =>
        "Ocorreu um erro ao salvar o arquivo. Por favor, envie-me um e-mail sobre isso e inclua uma captura de tela desta mensagem. chroma@chroma.zone\n\n";
    public override string UnknownReadError =>
        "Ocorreu um erro ao ler o arquivo.";

    public override string HelpMenuTitle =>
        "Ajudar";
    public override string HelpTutorials =>
        "Tutoriais";
    public override string HelpDemoWorlds =>
        "Demos"; // Demonstrações (too long)
    public override string TutorialWorldName(string name) =>
        $"Tutorial - {name}";
    public override string DemoWorldName(string name) =>
        $"Demo - {name}";
    public override string TutorialIntro =>
        "Introdução";
    public override string TutorialPainting =>
        "Pintar";
    public override string TutorialBevels =>
        "Chanfros";
    public override string TutorialSubstances =>
        "Substâncias";
    public override string TutorialObjects =>
        "Objetos";
    public override string TutorialTips =>
        "Dicas e Atalhos";
    public override string TutorialAdvancedGameLogic1 =>
        "Lógica do Jogo 1 (Inglês)"; // not translated yet
    public override string TutorialAdvancedGameLogic2 =>
        "Lógica do Jogo 2 (Inglês)"; // not translated yet
    public override string DemoDoors =>
        "Portas";
    public override string DemoHovercraft =>
        "Hovercraft";
    public override string DemoAI =>
        "IA De Personagem";
    public override string DemoPlatforms =>
        "Jogo de Plataforma";
    public override string DemoShapes =>
        "Formas";
    public override string DemoLogic =>
        "Lógica";
    public override string DemoImpossibleHallway =>
        "Corredor Impossível";
    public override string DemoConveyor =>
        "Transportadora";
    public override string DemoBallPit =>
        "Piscina de bolinhas";

    public override string HealthCounterPrefix =>
        "Saúde: ";
    public override string ScoreCounterPrefix =>
        "Pontuação: ";
    public override string YouDied =>
        "você morreu :(";
    public override string ResumeGame =>
        "Retomar";
    public override string RestartGame =>
        "Reiniciar";
    public override string OpenEditor =>
        "Editor";
    public override string CloseGame =>
        "Fechar";

    public override string PropTag =>
        "Tag";
    public override string PropTarget =>
        "Alvo";
    public override string PropCondition =>
        "Condição";
    public override string PropXRay =>
        "Raio X?";
    public override string PropHealth =>
        "Saúde";
    public override string PropPivot =>
        "Pivô";
    public override string PropSky =>
        "Céu";
    public override string PropAmbientLightIntensity =>
        "Luz ambiente";
    public override string PropSunIntensity =>
        "Intensidade do sol";
    public override string PropSunColor =>
        "Cor do sol";
    public override string PropSunPitch =>
        "Inclinação do sol";
    public override string PropSunYaw =>
        "Guinada do sol";
    public override string PropShadows =>
        "Sombras";
    public override string PropReflections =>
        "Reflexões";
    public override string PropFog =>
        "Nevoeiro";
    public override string PropFogDensity =>
        "Densidade do nevoeiro";
    public override string PropFogColor =>
        "Cor do nevoeiro";
    public override string PropBase =>
        "Base";
    public override string PropTexture =>
        "Textura";
    public override string PropSize =>
        "Tamanho";
    public override string PropPixelFilter =>
        "Filtro";
    public override string PropFilter =>
        "Filtro";
    public override string PropFootstepSounds =>
        "Sons de passos?";
    public override string PropScoreIs =>
        "A pontuação é";
    public override string PropThreshold =>
        "Limiar";
    public override string PropInput =>
        "Entrada";
    public override string PropOffTime =>
        "Tempo desligar";
    public override string PropOnTime =>
        "Tempo ligar"; // ?
    public override string PropStartOn =>
        "Iniciar ativado?";
    public override string PropMaxDistance =>
        "Distância máx";
    public override string PropInputs =>
        "Entradas";
    public override string PropDistance =>
        "Distância";
    public override string PropMinVelocity =>
        "Velocidade min";
    public override string PropMinAngularVelocity =>
        "Vel. angular min";
    public override string PropDirection =>
        "Direção";
    public override string PropOffInput =>
        "Entrada deslig";
    public override string PropOnInput =>
        "Entrada lig";
    public override string PropThrowSpeed =>
        "Força arremesso";
    public override string PropThrowAngle =>
        "Ângulo arremesso";
    public override string PropDensity =>
        "Densidade";
    public override string PropMode =>
        "Modo";
    public override string PropIgnoreMass =>
        "Ignorar massa?";
    public override string PropStopObjectFirst =>
        "Parar o objeto?";
    public override string PropStrength =>
        "Força";
    public override string PropToward =>
        "Na direção";
    public override string PropColor =>
        "Cor";
    public override string PropAmount =>
        "Quantia";
    public override string PropRate =>
        "Taxa";
    public override string PropKeepWithin =>
        "Mantenha entre";
    public override string PropSpeed =>
        "Velocidade";
    public override string PropFacing =>
        "Voltado para";
    public override string PropAlignment =>
        "Alinhamento";
    public override string PropIntensity =>
        "Intensidade";
    public override string PropShadowsEnable =>
        "Sombras?";
    public override string PropFront =>
        "Frente";
    public override string PropYawPitch =>
        "Guinada|Arremesso";
    public override string PropParent =>
        "Pai";
    public override string PropFollowRotation =>
        "Seguir rotação?";
    public override string PropGravityEnable =>
        "Gravity?";
    public override string PropRange =>
        "Alcance";
    public override string PropRealTime =>
        "Tempo real?";
    public override string PropScaleFactor =>
        "Fator";
    public override string PropSound =>
        "Som";
    public override string PropPlayMode =>
        "Modo de reprodução";
    public override string PropVolume =>
        "Volume";
    public override string PropFadeIn =>
        "Surgindo";
    public override string PropFadeOut =>
        "Desaparece";
    public override string PropSpatialMode =>
        "Modo espacial";
    public override string PropFadeDistance =>
        "Distância";
    public override string PropAxis =>
        "Eixo";
    public override string PropTo =>
        "Para";
    public override string PropRelativeTo =>
        "Relativo a";

    public override string NoneName =>
        "Nada";
    public override string AnythingName =>
        "Qualquer";
    public override string ObjectName =>
        "Objeto";
    public override string ObjectDesc =>
        "Um objeto não feito de blocos";
    public override string SubstanceName =>
        "Substância";
    public override string SubstanceDesc =>
        "Uma entidade feita de blocos";
    public override string SubstanceLongDesc =>
        "O ponto pivô é usado como centro de rotação/escala e como ponto alvo para outros comportamentos.";
    public override string WorldName =>
        "Mundo";
    public override string WorldDesc =>
        "Propriedades que afetam o mundo inteiro";
    public override string CustomTextureName =>
        "Textura";
    public override string CustomTextureDesc =>
        "Uma imagem de textura personalizada para materiais ou sobreposições";
    public override string BallName =>
        "Bola";
    public override string BallDesc =>
        "Uma esfera que pode ser pintada";
    public override string BallLongDesc =>
        "Use o botão de pintura para alterar a cor/material";
    public override string PlayerName =>
        "Jogador";
    public override string PlayerDesc =>
        "O personagem que você controla no jogo";

    public override string CheckScoreName =>
        "Pontuação"; // same as Score behavior
    public override string CheckScoreDesc =>
        "Ativo quando a pontuação está igual ou acima/abaixo de um limite";
    public override string DelayName =>
        "Atraso";
    public override string DelayDesc =>
        "Adicionar atraso à ativação ou desativação da entrada";
    public override string DelayLongDesc =>
@"Se a <b>Entrada</b> estiver ligada por mais tempo do que o <b>Tempo ligar</b>, o sensor será ligado. Se a entrada estiver desligada por mais tempo do que o <b>Tempo desligar</b>, o sensor será desligado. Se a entrada ligar/desligar mais rápido que o tempo de ligar/desligar, nada acontece.

Ativadores: os ativadores da Entrada, adicionados e removidos com atraso";
    public override string InCameraName =>
        "Na Câmera";
    public override string InCameraDesc =>
        "Ativo quando o jogador olha para o objeto";
    public override string InCameraLongDesc =>
@"Ativa enquanto o jogador estiver olhando na direção do objeto, mesmo que esteja obscurecido. Não é ativado se o objeto não tiver comportamento Visível.

Ativador: o jogador";
    public override string ThresholdName =>
        "Limiar";
    public override string ThresholdDesc =>
        "Ativo quando um certo número de outros objetos estão ativos";
    public override string ThresholdLongDesc =>
@"Soma os valores de todas as <b>Entradas</b>. Se uma entrada estiver ativada e definida como <b>+1</b>, isso adiciona 1 ao total. Se uma entrada estiver ativada e definida como <b>-1</b>, isso subtrai 1 do total. O sensor liga se o total estiver igual ou acima do <b>Limiar</b>.

Ativadores: os ativadores combinados de todas as entradas Positivas menos os ativadores das entradas Negativas";
    public override string InRangeName =>
        "No Alcance";
    public override string InRangeDesc =>
        "Detectar objetos dentro de alguma distância";
    public override string InRangeLongDesc =>
        "Ativadores: todos os objetos ao alcance";
    public override string MotionName =>
        "Movimento";
    public override string MotionDesc =>
        "Detectar movimento acima de alguma velocidade";
    public override string MotionLongDesc =>
        "É ativado quando o objeto está se movendo mais rápido que a <b>Velocidade mínima</b> na direção especificada e girando em torno de qualquer eixo mais rápido que a <b>Velocidade angular mínima</b> (graus por segundo).";
    public override string PulseName =>
        "Pulso";
    public override string PulseDesc =>
        "Ligar e desligar continuamente";
    public override string PulseLongDesc =>
        "<b>Entrada</b> é opcional. Quando conectado, controla se o pulso está ativo. Quando a entrada é desligada, o pulso completa um ciclo completo e então para.";
    public override string RandomPulseName =>
        "Cintilação"; // flicker
    public override string RandomPulseDesc =>
        "Ligue e desligue em um padrão aleatório";
    public override string RandomPulseLongDesc =>
        "Alterna entre ligar/desligar usando tempos aleatórios selecionados dentro dos intervalos. Útil para comportamento imprevisível, luzes tremeluzentes, etc.";
    public override string TapName =>
        "Aperte";
    public override string TapDesc =>
        "Detectar jogador tocando no objeto";
    public override string TapLongDesc =>
@"O objeto precisa ser Sólido para detectar um toque, mas não precisa ser Visível.

Ativador: o jogador";
    public override string ToggleName =>
        "Alternar";
    public override string ToggleDesc =>
        "Uma entrada para ligar, uma entrada para desligar, caso contrário, segure";
    public override string ToggleLongDesc =>
@"Se ambas as entradas forem ligadas simultaneamente, o sensor alterna entre ligado/desligado.

Ativadores: os ativadores da <b>Entrada Ligado</b>, congelados quando é ligado";
    public override string TouchName =>
        "Toque";
    public override string TouchDesc =>
        "Ativo ao tocar outro objeto";
    public override string TouchLongDesc =>
@"•  <b>Filtro:</b> O objeto ou tipos de objeto que ativam o sensor.
•  <b>Velocidade min:</b> O objeto deve entrar com esta velocidade relativa para ativar.
•  <b>Direção:</b> O objeto deve entrar nesta direção para ativar.

Ativadores: todos os objetos em colisão correspondentes ao filtro

ERRO: Dois objetos que tenham comportamentos Sólidos, mas não comportamentos Físicos, não detectarão uma colisão.";

    public override string CarryableName =>
        "Transportável";
    public override string CarryableDesc =>
        "Permitir que o jogador pegue/solte/arremesse";
    public override string CarryableLongDesc =>
@"Toque para pegar o objeto, toque novamente para lançar. Aumentar o <b>Ângulo de arremesso</b> faz com que o objeto seja lançado em arco.

Requer Física";
    public override string CharacterName =>
        "Personagem";
    public override string CharacterDesc =>
        "Aplique a gravidade, mas mantenha-se em pé";
    public override string CharacterLongDesc =>
@"Esta é uma alternativa ao comportamento Física. Os objetos terão gravidade, mas não poderão tombar. Quando usado com o comportamento Mover, os objetos cairão no chão em vez de flutuarem.

<b>Densidade</b> afeta a massa do objeto, proporcional ao seu volume.";
    public override string CloneName =>
        "Clone";
    public override string CloneDesc =>
        "Crie uma cópia do objeto";
    public override string CloneLongDesc =>
@"Um novo clone é criado imediatamente quando o comportamento é ativado. O clone começará com a saúde original do objeto. Sensores que filtram um objeto específico também serão ativados para qualquer um de seus clones.

•  <b>Para:</b> Local de destino para clone
•  <b>Relativo a:</b> Localização de origem opcional. Se especificado, o clone será deslocado do objeto original pela diferença entre o destino e a origem.";
    public override string ForceName =>
        "Força";
    public override string ForceDesc =>
        "Aplicar força instantânea ou contínua";
    public override string ForceLongDesc =>
@"Funciona apenas para objetos com comportamento Física.

•  O modo ""Impulse"" fará com que um impulso instantâneo seja aplicado quando o comportamento for ativado.
•  O modo ""Continuous"" fará com que a força seja aplicada continuamente enquanto o comportamento estiver ativo.
•  <b>Ignorar massa</b> dimensiona a força para compensar a massa do objeto.
•  <b>Parar o objeto</b> interromperá qualquer movimento existente antes de aplicar a força.";
    public override string HaloName =>
        "Halo";
    public override string HaloDesc =>
        "Efeito brilhante";
    public override string HaloLongDesc =>
        "Halo aparece no ponto pivô das substâncias";
    public override string HurtHealName =>
        "Ferir/Curar";
    public override string HurtHealDesc =>
        "Perder/ganhar saúde; abaixo de 0, o objeto morre";
    public override string HurtHealLongDesc =>
@"•  <b>Quantia:</b> Mudança na saúde. O positivo cura, o negativo dói.
•  <b>Taxa:</b> Segundos entre feridas/curas sucessivas. 0 significa que a saúde só mudará uma vez quando o comportamento for ativado.
•  <b>Mantenha entre:</b> A saúde só mudará se estiver dentro dessa faixa e nunca sairá dessa faixa.";
    public override string JoystickName =>
        "Controle";
    public override string JoystickDesc =>
        "Controle o movimento com o joystick";
    public override string LightName =>
        "Luz";
    public override string LightDesc =>
        "Fonte de luz no centro do objeto";
    public override string LightLongDesc =>
        "A luz se origina do ponto pivô das substâncias";
    public override string LookAtName =>
        "Olhe Para";
    public override string LookAtDesc =>
        "Apontar em uma direção ou em direção ao objeto";
    public override string LookAtLongDesc =>
@"•  <b>Velocidade</b> é a velocidade angular máxima em graus por segundo.
•  <b>Frente</b> é o lado do objeto que será apontado para o alvo.
•  <b>Guinada:</b> Ativa a rotação esquerda-direita.
•  <b>Arremesso:</b> Permite rotação para cima e para baixo. Ambos podem ser usados ao mesmo tempo.
As substâncias girarão em torno de seu ponto pivô.";
    public override string MoveName =>
        "Mover";
    public override string MoveDesc =>
        "Mover-se em uma direção ou em direção a um objeto";
    public override string MoveLongDesc =>
@"Quando usado com comportamentos Sólidos e Físicos, o objeto não poderá passar por outros objetos.
Quando usado com comportamentos Sólido e Personagem, o objeto também será afetado pela gravidade.
Aumente a <b>Densidade</b> do comportamento da Física/Personagem para aumentar a força de empurrão do objeto.";
    public override string MoveWithName =>
        "Mover Com";
    public override string MoveWithDesc =>
        "Acompanhe o movimento de outro objeto";
    public override string MoveWithLongDesc =>
        "ERRO: Este comportamento impedirá que os comportamentos Mover funcionem.";
    public override string PhysicsName =>
        "Física";
    public override string PhysicsDesc =>
        "Mova-se de acordo com as leis da física";
    public override string PhysicsLongDesc =>
        "<b>Densidade</b> afeta a massa do objeto, proporcional ao seu volume.";
    public override string ReflectorName =>
        "Refletor";
    public override string ReflectorDesc =>
        "Adicione reflexos mais realistas à área";
    public override string ReflectorLongDesc =>
@"Captura uma imagem da área circundante e a utiliza para simular reflexos.
•  Superfícies dentro do <b>Alcance</b> de distância são afetadas
•  <b>Intensidade</b> controla o brilho dos reflexos
•  Quando <b>Tempo real</b> estiver marcado, os reflexos serão atualizados continuamente (caro!)";
    public override string ScaleName =>
        "Escala";
    public override string ScaleDesc =>
        "Alterar o tamanho ao longo de cada eixo";
    public override string ScaleLongDesc =>
        "As substâncias são dimensionadas em torno de seu ponto pivô. Para objetos físicos, a massa <i>não</i> mudará.";
    public override string ScoreName =>
        "Pontuação";
    public override string ScoreDesc =>
        "Adicionar ou subtrair da pontuação do jogador";
    public override string SolidName =>
        "Sólido";
    public override string SolidDesc =>
        "Bloqueie e colida com outros objetos";
    public override string SoundName =>
        "Som";
    public override string SoundDesc =>
        "Reproduzir um som";
    public override string SoundLongDesc =>
@"• O modo ""1shot"" reproduz o som inteiro toda vez que o comportamento está ativo. Várias cópias podem ser reproduzidas ao mesmo tempo. Os fades não têm efeito.
• No modo ""Bkgnd"" o som está sempre tocando, mas é silenciado quando o comportamento está inativo.

Formatos suportados: MP3, WAV, OGG, AIF, XM, IT";
    public override string Sound3DName =>
        "Som 3D";
    public override string Sound3DDesc =>
        "Reproduzir um som no espaço 3D";
    public override string Sound3DLongDesc =>
@"•  No modo ""Point"", o panorâmica estéreo será usado para fazer com que o som pareça ser emitido pelo objeto.
•  No modo ""Ambient"" o som parecerá envolver o player.
•  <b>Distância:</b> O som diminuirá com a distância dentro desta faixa -- além da distância máxima, ele se tornará inaudível.

Consulte comportamento Som para documentação adicional.";
    public override string SpinName =>
        "Rodar";
    public override string SpinDesc =>
        "Girar continuamente";
    public override string SpinLongDesc =>
@"•  <b>Velocidade</b> está em graus por segundo. <b>Eixo</b> especifica o eixo de rotação.
As substâncias girarão em torno de seu ponto pivô.";
    public override string TeleportName =>
        "Teleporte";
    public override string TeleportDesc =>
        "Teletransporte-se instantaneamente para outro local";
    public override string TeleportLongDesc =>
@"•  <b>Para</b> Local de destino para teletransportar
•  <b>Relativo a:</b> Localização de origem opcional. Se especificado, em vez de ir diretamente ao destino, o objeto será deslocado pela diferença entre o destino e a origem.";
    public override string VisibleName =>
        "Visível";
    public override string VisibleDesc =>
        "O objeto é visível no jogo";
    public override string WaterName =>
        "Água";
    public override string WaterDesc =>
        "Simular física de flutuabilidade";
    public override string WaterLongDesc =>
        "A água não deveria ser Sólida e não deveria ter Física. Este comportamento controla apenas a física da água, não a aparência.";

    public override string SolidSubstanceName =>
        "Substância Sólida";
    public override string SolidSubstanceDesc =>
        "Um bloco sólido e opaco por padrão";
    public override string WaterSubstanceDesc =>
        "Um bloco de água onde você pode nadar";
    public override string TriggerName =>
        "Acionar";
    public override string TriggerDesc =>
        "Bloco invisível e não sólido com sensor de toque";
    public override string GlassName =>
        "Vidro";
    public override string GlassDesc =>
        "Bloco sólido de vidro";
    public override string EmptyName =>
        "Vazio";
    public override string EmptyDesc =>
        "Objeto bola invisível";
    public override string LightObjectDesc =>
        "Fonte de luz centrada em um ponto";

    public override string TutorialWelcome =>
        "Bem-vindo! Este é um breve tutorial que irá guiá-lo através do aplicativo. Você pode acessar este tutorial e outros a qualquer momento. Pressione a seta para a direita para continuar.";
    public override string TutorialRoom =>
        "Neste momento você está olhando para o interior de uma sala. Duas paredes estão escondidas para que você possa ver o interior. O jogador está no centro.";
    public override string TutorialOrbit =>
        "Navegação: use dois dedos para girar e aperte para ampliar. <i>Tente olhar ao redor da sala.</i> (o tutorial avançará quando você concluir isso)";
    public override string TutorialPan =>
        "<i>Use três dedos para deslocar.</i> (Se isso não funcionar no seu telefone, tente tocar no botão no canto inferior direito para alternar o modo panorâmico/girar.)";
    public override string TutorialSelectFace =>
        "<i>Toque com um dedo para selecionar uma única face de um bloco.</i>";
    public override string TutorialPull(string axisName) =>
        $"<i>Puxe a seta {axisName} em direção ao centro da sala para puxar o bloco para fora.</i>";
    public override string TutorialAxisX =>
        "Vermelha";
    public override string TutorialAxisY =>
        "Verde";
    public override string TutorialAxisZ =>
        "Azul";
    public override string TutorialPush =>
        "<i>Agora selecione uma face diferente e empurre-a para longe do centro da sala.</i>";
    public override string TutorialPushHint(string axisName) =>
        $"<i>(dica: use a seta {axisName})</i>";
    public override string TutorialColumn =>
        "<i>Agora selecione uma face diferente e puxe-a em direção ao centro da sala. Continue puxando até chegar ao outro lado.</i>";
    public override string TutorialBox =>
        "Toque e arraste para selecionar um grupo de faces em um retângulo ou caixa. <i>Tente isso algumas vezes.</i>";
    public override string TutorialWall =>
        "<i>Toque duas vezes para selecionar uma parede inteira.</i>";
    public override string TutorialSculpt =>
        "Ao selecionar faces e empurrá-los/puxá-los, você pode esculpir o mundo.";
    public override string TutorialButtons =>
        "Esses botões aparecem na parte superior da tela, com base no contexto.";
    public override string TutorialButtonsResource =>
        "Tutorials/toolbar_buttons_portuguese";
    public override string TutorialHelpMenu =>
        "Isso é o suficiente para começar! Você pode acessar mais tutoriais escolhendo Ajuda no menu.";
    public override string TutorialLinks =>
        "Confira também os tutoriais em vídeo no YouTube e no subreddit. Existem links no menu principal.";
    public override string TutorialPaintButton =>
        "<i>Selecione algumas faces e toque no ícone do rolo de pintura para abrir o o painel de tinta.</i>";
    public override string TutorialPaintReopen =>
        "<i>Reabra o painel de tinta. (Selecione algumas faces e toque no ícone do rolo de pintura</i>";
    public override string TutorialPaintPanel =>
        "Você pode usar o painel de tinta para pintar as faces selecionadas com <i>materiais</i> e <i>sobreposições</i>.";
    public override string TutorialPaintCategories =>
        "Escolha qualquer uma das categorias para procurar uma textura. Em seguida, toque no botão Cor para alterar sua cor.";
    public override string TutorialPaintLayers =>
        "Uma tinta é composta de duas partes: um material opaco e uma sobreposição transparente. Use as abas para alternar entre as duas partes.";
    public override string TutorialPaintTransform =>
        "Use estes botões para girar e espelhar a tinta.";
    public override string TutorialPaintSky =>
        "O material \"Sky\" é especial: no jogo é uma janela desobstruída para o céu. Num mundo interior, esta é a única forma de ver o céu.";
    public override string TutorialBevelButton =>
        "<i>Toque no botão de menu e escolha \"Chanfrar\" para abrir o modo chanfrado.</i>";
    public override string TutorialBevelReopen =>
        "<i>Reabra o modo chanfro. (Toque no botão de menu e escolha \"Chanfrar\")</i>";
    public override string TutorialBevelSelect =>
        "Em vez de selecionar faces, agora você pode selecionar arestas. <i>Toque e arraste para selecionar.</i>";
    public override string TutorialBevelShape =>
        "<i>Toque em uma forma de chanfro na lista para chanfrar a aresta.</i>";
    public override string TutorialBevelSize =>
        "<i>Toque em um tamanho para alterar o tamanho do chanfro.</i>";
    public override string TutorialBevelDoubleTap =>
        "<i>Toque duas vezes em uma aresta para selecionar todas as arestas contíguas.</i>";
    public override string TutorialBevelExit =>
        "<i>Quando terminar, toque no botão de seleção para sair.</i>";
    public override string TutorialSubstanceIntro =>
        "Neste tutorial você construirá uma plataforma móvel usando uma <i>Substância.</i> Substâncias são objetos independentes que podem se mover e responder à interação.";
    public override string TutorialSubstancePit =>
        "Primeiro construa um buraco que seja muito largo e profundo para ser atravessado saltando.";
    public override string TutorialSubstanceCreateButton =>
        "Agora adicionaremos uma substância que se tornará uma plataforma móvel. <i>Selecione uma fileira de faces em um lado do poço e toque no botão do cubo.</i>";
    public override string TutorialSubstanceSolid =>
        "<i>Escolha \"Substância Sólida\".</i>";
    public override string TutorialSubstancePull =>
        "<i>Puxe para fora para construir uma plataforma.</i>";
    public override string TutorialSubstanceBehaviors =>
        "As substâncias são controladas pelos seus <i>Comportamentos</i>. Esta substância tem comportamentos <i>Visível</i> e <i>Sólido</i> que a tornam visível e sólida no jogo.";
    public override string TutorialSubstanceReselect =>
        "<i>Toque na substância para selecioná-la novamente.</i>";
    public override string TutorialSubstanceMoveBehavior =>
        "<i>Tente adicionar um comportamento Mover à plataforma.</i> Observe que os comportamentos são organizados em diversas categorias.";
    public override string TutorialSubstanceEditDirection =>
        "O comportamento Mover fará com que esta substância se mova para o Norte a uma velocidade constante. <i>Toque na direção para editá-la.</i>";
    public override string TutorialSubstanceSetDirection =>
        "<i>Faça-o se mover em direção ao outro lado do poço (olhe para a rosa dos ventos para orientação)</i>";
    public override string TutorialSubstancePlayTest =>
        "<i>Tente jogar o seu jogo.</i> A plataforma se moverá em uma direção para sempre. Precisamos fazê-lo mudar de direção no final do poço.";
    public override string TutorialSubstanceOnOff =>
        "As substâncias têm dois estados, Ligado e Desligado. Os comportamentos podem ser configurados para ficarem ativos apenas no estado Ligado ou Desligado.";
    public override string TutorialSubstanceMoveOpposite =>
        "<i>Adicione um segundo comportamento Mover, que move a plataforma na direção oposta.</i>";
    public override string TutorialSubstanceBehaviorConditions =>
        "<i>Agora ative um comportamento apenas no estado Desligado e outro no estado Ligado.</i> (a substância começará Desligada).";
    public override string TutorialSubstanceSensor =>
        "O estado ligado/desligado de uma substância é controlado por um sensor. <i>Dê à plataforma um sensor Pulso. (na aba Lógica)</i> Isso fará com que ele ligue/desligue repetidamente.";
    public override string TutorialSubstanceTime =>
        "<i>Agora ajuste o tempo que o sensor passa no estado ligado e desligado para fazer a plataforma se mover por toda a distância do poço.</i>";
    public override string TutorialSubstancePlayFinal =>
        "<i>Jogue agora.</i> Se você construiu tudo corretamente, a plataforma deverá se mover pelo poço e voltar repetidamente.";
    public override string TutorialSubstanceNext =>
        "Em seguida, experimente o tutorial <i>Objetos</i> para aprender sobre outro tipo de elemento interativo.";
    public override string TutorialObjectSelect =>
        "<i>Selecione uma face e toque no botão cubo.</i>";
    public override string TutorialObjectCreate =>
        "<i>Escolha a aba Objeto e escolha Bola.</i>";
    public override string TutorialObjectExplain =>
        "Você acabou de criar um objeto bola. Assim como as substâncias, você pode dar aos objetos comportamentos e sensores para adicionar interatividade.";
    public override string TutorialObjectReselect =>
        "<i>Toque na bola para selecioná-la novamente.</i>";
    public override string TutorialObjectPaint =>
        "<i>Experimente pintar a bola.</i>";
    public override string TutorialObjectAddMove =>
        "<i>Adicione um comportamento Mover à bola.</i>";
    public override string TutorialObjectFollowPlayer =>
        "<i>Edite o comportamento Mover para fazer a bola seguir o jogador.</i>";
    public override string TutorialObjectPlayTest =>
        "<i>Tente jogar o seu jogo.</i> Em seguida, faremos com que a bola machuque você quando você tocá-la.";
    public override string TutorialObjectSensor =>
        "<i>Dê à bola um sensor de Toque.</i>";
    public override string TutorialObjectTouchPlayer =>
        "<i>Agora configure o sensor Toque para que ele só ligue ao tocar no player.</i>";
    public override string TutorialObjectAddTargetedBehavior =>
        "<i>Toque em Adicionar comportamento. No menu de comportamento, toque no botão \"Alvo\" e selecione o jogador como alvo. Em seguida, escolha Ferir/Curar na aba \"Vida\".</i>";
    public override string TutorialObjectIncorrectTarget =>
        "Você não definiu o alvo para o jogador. Remova o comportamento e tente novamente.";
    public override string TutorialObjectTargetExplain =>
        "Por padrão, Ferir/Curar fere o objeto ao qual está anexado (a bola). Ao definir um Alvo, fizemos com que ele atuasse sobre um objeto diferente (o jogador).";
    public override string TutorialObjectHurtOn =>
        "<i>Defina Ferir/Curar para ativar quando o sensor estiver ligado.</i> Mesmo que tenha como alvo o jogador, ele usará o sensor da bola para ligar/desligar.";
    public override string TutorialObjectHurtRate =>
        "<i>Defina a taxa de Ferir/Curar como 1 para machucar repetidamente (a cada 1 segundo) enquanto você estiver tocando a bola.</i>";
    public override string TutorialObjectPlayFinal =>
        "<i>Jogue e tente evitar a morte!</i> Você pode alterar a velocidade da bola e a quantidade de dano para ajustar a dificuldade.";
    public override string TutorialObjectCharacterBehavior =>
        "Se você construir alguns obstáculos, notará que a bola pode flutuar e passar pelas paredes. <i>Adicione um comportamento Personagem para corrigir isso. (na aba Física)</i>";
    public override string TutorialObjectNext =>
        "Leia o tutorial <i>Lógica do Jogo</i> para aprender como adicionar interatividade mais complexa aos jogos.";
    public override string TutorialTipsMessage =>
@"•  Toque duas vezes para selecionar uma parede inteira. A seleção será limitada pelas faces já selecionadas.
•  Toque três vezes em um rosto para selecionar <i>todos</i> os rostos conectados a ele. A seleção será limitada pelas faces já selecionadas.
•  Toque três vezes em uma substância para selecionar toda a substância.
•  Marque a caixa ""Raio X"" de uma substância para torná-la transparente apenas no editor. Isso permite que você veja por trás e faça zoom.
•  O painel de tinta mantém atalhos para as cinco tintas mais recentes. Para ""copiar"" uma tinta para outra face, selecione a face de origem, abra e feche o painel de tinta, selecione as faces de destino e use o atalho de tinta recente.
•  Deslizar faces lateralmente ao longo de uma parede move suas tintas, deixando um rastro atrás delas.
•  Verifique a seção ""Selecionar"" no menu para obter atalhos úteis para selecionar faces e objetos.
•  Você pode selecionar vários objetos/substâncias para editar todas as suas propriedades de uma só vez.";
}
