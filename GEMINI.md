# AvenueXR Game Jam — Guida Tecnica e Architettura (GEMINI.md)

Documento di riferimento per l'architettura, le convenzioni e le linee guida del codebase per agenti AI e sviluppatori.

---

## 1. Panoramica del Progetto

- **Nome**: AvenueXR Game Jam (Politecnico di Torino) — *VR Trash Sorter* (PC Flat-screen Porting)
- **Genere**: First-Person Narrative Decision-Making / Job Simulator distopico
- **Contesto**: Creato in 72 ore durante la game jam per il progetto Avenue / XrAvenue, convertito in versione PC Desktop.
- **Concept narrativo**: Il giocatore impersona un addetto allo smistamento rifiuti sotto un regime totalitario. Dalla sua postazione (una finestra verso la città e 3 cestini meccanici a manovella) riceve oggetti portati da cittadini/NPC. Lo smistamento (o il rifiuto/restituzione/getto dalla finestra) influenza il mondo esterno (inquinamento, proteste, repressione) e determina il destino del protagonista attraverso bivi morali e finali multipli.
- **Stile visivo**: Low-poly ispirato all'estetica PS1 / schermi retro, con shader di pixelazione a schermo intero.

---

## 2. Tech Stack & Versioni

| Componente | Versione / Tecnologia | Dettagli |
|---|---|---|
| **Unity Engine** | `6000.6.2f1` (Unity 6.6.2f1) | API Physics aggiornate (`linearVelocity`) |
| **Render Pipeline** | Universal Render Pipeline (URP) `17.4.0` | API **Render Graph** abilitate per post-processing |
| **Platform Target** | PC Standalone (Windows Flat-screen) | Controlli Mouse & Tastiera in prima persona |
| **Player System** | `AvenueXR.Player` | WASD, Mouse Look, HeadBob slider, Crosshair Raycast |
| **Architettura Eventi** | **Butter** (`dev.nicklaj.butter`) | Event-driven tramite ScriptableObject |
| **Tweening / UI Anim** | DOTween + PrimeTween | Animazioni fumetti, UI e transizioni |
| **UI** | TextMeshPro (TMP) World Space + Runtime Crosshair | Popup 3D integrati nella scena |

---

## 3. Architettura del Software

Il progetto segue un pattern **Event-Driven basato su ScriptableObject** utilizzando la libreria **Butter**. Nessun manager possiede riferimenti diretti e accoppiati agli altri manager: la comunicazione avviene attraverso canali di eventi indipendenti.

```
                         ┌─────────────────────────────────┐
                         │       GameStateManager          │
                         │   (Progressione & Bivi Morali)  │
                         └───────────────┬─────────────────┘
                                         │ onDayStart / onDayEnd
                                         ▼
     ┌──────────────────────┐  dialoghi ┌────────────────────────────────┐
     │   DialogueManager    │◄──────────┤      WasteDeliveryManager      │
     │ (Fumetti, Typewriter)│           │   (Orchestratore della giornata)│
     └──────────────────────┘           └───────┬──────────────┬─────────┘
                                                │              │
                   ┌────────────────────────────┘              └──────────────────────────┐
                   ▼                                                                      ▼
       ┌────────────────────────┐                                            ┌──────────────────────┐
       │     NPCController      │                                            │  WasteObjectSpawner  │
       │ (Movimento e Waypoint) │                                            │   (Spawning Prefab)  │
       └────────────────────────┘                                            └──────────┬───────────┘
                                                                                        │
                                                                                        ▼
       ┌────────────────────────┐           Delta rotazione                  ┌──────────────────────┐
       │    XRPhysicalCrank     ├───────────────────────────────────────────►│      BinCrusher      │
       │  (Manovella fisica XR) │                                            │  (Smaciullatore SFX) │
       └────────────────────────┘                                            └──────────┬───────────┘
                                                                                        │ onWasteSorted
                                                                                        ▼
                                                                             ┌──────────────────────┐
                                                                             │     WasteBin         │
                                                                             │  (Cestini / Logica)  │
                                                                             └──────────────────────┘
```

### Scene Setup & Additive Loading
Il gioco si avvia dalla scena `Init` via [`SceneLoader`](file:///Assets/_Project/Scripts/Core/SceneLoader.cs), che carica additivamente:
1. `MainScene`: Contiene la logica di gioco, il player rig (`XR Origin`), i manager e i cestini.
2. `Env`: Contiene la geometria dell'ambiente (stanza, esterno città, illuminazione e skybox).
3. La scena `Init` viene scaricata al termine del caricamento.

---

## 4. Mappa dei Sistemi e Componenti (`Assets/_Project/Scripts/`)

### Core (`AvenueXR.Core`)

| Script | Ruolo e Funzionamento |
|---|---|
| [`GameStateManager`](file:///Assets/_Project/Scripts/Core/GameStateManager.cs) | Gestisce il ciclo dei giorni. Tiene traccia di `currentDailyRebellionPoints`. Al termine del giorno valuta `rebellionThreshold` per instradare verso `nextDayRebel` o `nextDayObedient`, o attiva `onFinaleReached` se `isFinale == true`. |
| [`WasteDeliveryManager`](file:///Assets/_Project/Scripts/Core/WasteDeliveryManager.cs) | Macchina a stati sequenziale dello step giornaliero: chiama l'NPC con l'oggetto (`NPCController.DeliverObject`), genera l'oggetto fisico (`WasteObjectSpawner`), avvia i dialoghi NPC/Boss (`DialogueManager`), attende la risoluzione dell'oggetto e l'uscita dell'NPC. |
| [`DayData`](file:///Assets/_Project/Scripts/Core/DayData.cs) | ScriptableObject che definisce: intro/outro, lista step di consegna (`WasteDeliveryStep`), bivi, soglia ribellione, livello inquinamento (`worldPollutionLevel`), audio del giorno e configurazioni per finali. |
| [`DialogueData`](file:///Assets/_Project/Scripts/Core/DialogueData.cs) | ScriptableObject contenente battute (`DialogueLine`), speaker (Boss o Npc), tempi di pausa e clip vocali specifiche. |
| [`DialogueManager`](file:///Assets/_Project/Scripts/Core/DialogueManager.cs) | Gestore della coda dialoghi. Apre/chiude i fumetti [`WorldDialoguePopup`](file:///Assets/_Project/Scripts/Tutorial/WorldDialoguePopUp.cs), accende/spegne la TV tramite `onTVStateChanged` e sincronizza le voci dell'[`AudioManager`](file:///Assets/_Project/Scripts/Core/AudioManager.cs). |
| [`NPCController`](file:///Assets/_Project/Scripts/Core/NPCController.cs) | Movimento NPC basato su waypoint (`Transform`). Macchina a stati: `Idle` -> `Walking` -> `Interacting` -> `Walking` (ritorno) -> `Idle`. Ritorna solo dopo la chiamata a `CompleteInteraction()`. |
| [`NPCVisualManager`](file:///Assets/_Project/Scripts/Core/NPCVisualManager.cs) | Alterna a rotazione i modelli visivi degli NPC dalla lista `npcModels` e ne sincronizza gli `Animator` (`isWalking`, `isInteracting`). |
| [`NPCHandBinder`](file:///Assets/_Project/Scripts/Core/NPCHandBinder.cs) | Clona l'oggetto nella mano dell'NPC durante il cammino, rimuovendone colliders, Rigidbody e componenti di gameplay per evitare collisioni indesiderate. |
| [`XRPhysicalCrank`](file:///Assets/_Project/Scripts/Core/XRPhysicalCrank.cs) | Manovella fisica VR derivata da `XRGrabInteractable`. Calcola la rotazione angolare continua della mano proiettata sull'asse locale (`rotationAxis`), accumula i gradi e notifica `OnRotationDelta`. Modula il pitch dell'audio in base alla velocità. |
| [`BinCrusher`](file:///Assets/_Project/Scripts/Core/BinCrusher.cs) | Gestore della pressa meccanica di ogni singolo cestino. Ascolta `OnRotationDelta` della relativa manovella: richiede 360° di rotazione (`rotationNeeded`), attiva particelle (`WasteParticleData`), suona lo SFX di distruzione (`WasteAudioData`), distrugge l'oggetto e pubblica `onWasteSorted`. |
| [`BeltManager`](file:///Assets/_Project/Scripts/Core/BeltManager.cs) | Ruota due rulli visivi in versi opposti seguendo i movimenti della manovella. |
| [`WasteBin`](file:///Assets/_Project/Scripts/Core/WasteBin.cs) | Trigger del cestino. Ignora gli oggetti ancora impugnati dal giocatore (`interactable.isSelected`). Blocca la fisica dell'oggetto, valuta se è corretto o se è il `isRejectBin`, assegna punti ribellione se necessario e invia a `BinCrusher`. |
| [`WasteReturnZone`](file:///Assets/_Project/Scripts/Core/WasteReturnZone.cs) | Trigger posizionato presso la scrivania/finestra per restituire l'oggetto all'NPC (atto di ribellione). |
| [`WindowZone`](file:///Assets/_Project/Scripts/Core/WindowZone.cs) | Trigger per gettare oggetti fuori dalla finestra (ribellione per oggetti speciali). |
| [`WasteItem`](file:///Assets/_Project/Scripts/Core/WasteItem.cs) | Oggetto fisico: memorizza il tipo (`WasteType`), suona audio al grab/drop e si teletrasporta a `_respawnPosition` se cade al di sotto di `killYThreshold`. |
| [`WasteObjectSpawner`](file:///Assets/_Project/Scripts/Core/WasteObjectSpawner.cs) | Seleziona ed istanzia prefab casuali per categoria (`WasteType`) sul tavolo, assicurandosi di riattivare fisica e collider. |
| [`CityPollutionManager`](file:///Assets/_Project/Scripts/Core/CityPollutionManager.cs) | Attiva/disattiva oggetti 3D nella città visibile dalla finestra a seconda del livello di inquinamento del giorno corrente o del finale. |
| [`MoneyManager`](file:///Assets/_Project/Scripts/Core/MoneyManager.cs) | Calcola il saldo del giocatore: premia lo smistamento corretto (+10$) e penalizza gli errori (-5$). Aggiorna un display World Space. |
| [`TVController`](file:///Assets/_Project/Scripts/Core/TVController.cs) | Controlla l'animatore dello schermo della TV (accesa quando parla il Boss, spenta quando parla l'NPC). |
| [`DayEndFader`](file:///Assets/_Project/Scripts/Core/DayEndFader.cs) | Gestisce il fade to black tramite Animator a fine giornata e lo resetta a inizio giorno. |
| [`FinaleUIManager`](file:///Assets/_Project/Scripts/Core/FinaleUIManager.cs) | Mostra la schermata finale (titolo e descrizione) sfruttando il componente `WorldDialoguePopup`. |
| [`FinaleGrabTrigger`](file:///Assets/_Project/Scripts/Core/FinaleGrabTrigger.cs) | Trigger associato a oggetti di fine gioco (es. pistola) che scatenano la conclusione non appena afferrati. |
| [`AudioManager`](file:///Assets/_Project/Scripts/Core/AudioManager.cs) | Gestisce sorgenti globali (musica, ambient, SFX) e balbettio procedurale per le voci (loop con pitch randomizzato). |
| [`FootstepManager`](file:///Assets/_Project/Scripts/Core/FootstepManager.cs) & [`NPCFootstepManager`](file:///Assets/_Project/Scripts/Core/NPCFootstepManager.cs) | Emettono suoni di passi a intervalli di distanza percorsa (`stepDistance`). |
| [`FlowDebugger`](file:///Assets/_Project/Scripts/Core/FlowDebugger.cs) | Strumento fondamentale con comandi `[ContextMenu]` per testare tutto il loop (smistamento, bivi ribelli, skip dialoghi, fine giorno) senza visore VR. |

### Player PC (`AvenueXR.Player`)

| Script | Ruolo e Funzionamento |
|---|---|
| [`PlayerController`](file:///Assets/_Project/Scripts/Player/PlayerController.cs) | Movimento in prima persona basato su `CharacterController` (WASD + Shift per scattare), Mouse Look con pitch clamp e gestione del lock del cursore (`CursorLockMode.Locked`). |
| [`HeadBobController`](file:///Assets/_Project/Scripts/Player/HeadBobController.cs) | Oscillazione procedurale sinusoidale della telecamera durante il cammino. Intensità e frequenza controllabili tramite slider in Inspector (`bobAmount`, `bobFrequency`). |
| [`PlayerInteraction`](file:///Assets/_Project/Scripts/Player/PlayerInteraction.cs) | Raycast centrale dal mirino (distanza max `reachDistance`). Gestisce: Click Sinistro/`E` per raccogliere e depositare oggetti (`WasteItem`), Click Destro per lanciare (`throwForce`), e Click Sinistro/`E`/trascinamento mouse per azionare la manovella (`XRPhysicalCrank`). |
| [`CrosshairUI`](file:///Assets/_Project/Scripts/Player/CrosshairUI.cs) | Mirino a schermo dinamico in runtime OnGUI: cambia dimensione e colore quando si punta un oggetto o una manovella interagibile. |

### Shaders & Post-Processing (`Assets/_Project/Shaders/Pixelate/`)
- [`PixelateRendererFeature.cs`](file:///Assets/_Project/Shaders/Pixelate/PixelateRendererFeature.cs): `ScriptableRendererFeature` URP compatibile con Unity 6 Render Graph.
- [`PixelateRenderPass.cs`](file:///Assets/_Project/Shaders/Pixelate/PixelateRenderPass.cs): Esegue la blit pass sul material con parametri di scala pixel (`_PixelateX`, `_PixelateY`).
- [`Pixelate.shader`](file:///Assets/_Project/Shaders/Pixelate/Pixelate.shader): Shader HLSL ottimizzato per VR Single Pass Stereo (`UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX`, `SAMPLE_TEXTURE2D_X`), che esegue un campionamento singolo al centro del macropixel per minimizzare l'impatto sul frame rate.

---

## 5. Catalogo Eventi Butter (ScriptableObject Events)

Tutti gli eventi sono asset `.asset` in `Assets/_Project/Events/` e implementano il pattern Observer di Butter:

| Event Asset | Tipo | Mittente Principale | Ricevitori |
|---|---|---|---|
| `OnDaYStart` | `DayDataEvent` | `GameStateManager` | `WasteDeliveryManager`, `DialogueManager`, `CityPollutionManager`, `DayEndFader`, `AudioManager` |
| `OnDayEnd` | `GameEvent` | `WasteDeliveryManager` / `FinaleGrabTrigger` | `GameStateManager`, `DayEndFader`, `AudioManager` |
| `OnFinaleReached` | `DayDataEvent` | `GameStateManager` | `FinaleUIManager`, `CityPollutionManager`, `DayEndFader`, `AudioManager` |
| `OnDialogueStart` | `DialogueDataEvent` | `WasteDeliveryManager` | `DialogueManager` |
| `OnDialogueFinished` | `GameEvent` | `DialogueManager` | `WasteDeliveryManager` |
| `OnWasteDelivered` | `WasteTypeEvent` | `WasteDeliveryManager` | Sistemi audio / visuali |
| `OnWasteSorted` | `WasteTypeEvent` | `BinCrusher`, `WindowZone` | `WasteDeliveryManager`, `AudioManager` |
| `OnWasteReturned` | `WasteTypeEvent` | `WasteReturnZone` | `WasteDeliveryManager` |
| `OnMoralChoiceMade` | `BoolEvent` | `WasteBin`, `WasteReturnZone`, `WindowZone` | `GameStateManager` (`true` = ribellione, `false` = obbedienza) |
| `OnSortingResult` | `BoolEvent` | `BinCrusher` | `MoneyManager` (`true` = corretto, `false` = errato) |
| `OnTvStateChange` | `BoolEvent` | `DialogueManager` | `TVController` |
| `OnBossVoiceTriggered` | `AudioClipEvent` | `DialogueManager` | `AudioManager` |
| `OnNpcVoiceTriggered` | `AudioClipEvent` | `DialogueManager` | `AudioManager` |

---

## 6. Tipi di Rifiuto e Logica Morale (`WasteType`)

I tipi di rifiuti definiti nell'enum [`WasteType`](file:///Assets/_Project/Scripts/Core/WasteType.cs) sono:
- **Standard**: `Paper`, `Plastic`, `Glass`, `Metal`, `Organic`
- **Speciali / Narrativi**: `Moral` (es. sacche di sangue), `Gore` (parti umane), `Bomb` (ordigno di cospirazione), `Card`, `Baby`, `Box`

### Regole Morali:
- Cestinare un oggetto speciale (`Moral`, `Gore`, `Bomb`, `Baby`) nei normali bidoni è considerato **Obbedienza** (`onMoralChoiceMade.Raise(false)`).
- Mettere l'oggetto nel **Reject Bin**, restituirlo all'NPC (`WasteReturnZone`) o lanciarlo dalla finestra (`WindowZone`) è considerato **Ribellione** (`onMoralChoiceMade.Raise(true)`).
- Punti Ribellione: ogni azione di ribellione incrementa `currentDailyRebellionPoints`. Se a fine giorno `points >= day.rebellionThreshold`, si sblocca il ramo ribelle (`nextDayRebel`).

---

## 7. Regole e Convenzioni per lo Sviluppo

### A. Compatibilità Unity 6 & URP
1. **Fisica**: NON usare `Rigidbody.velocity` (deprecato in Unity 6). Usare sempre `Rigidbody.linearVelocity` e `Rigidbody.angularVelocity`.
2. **Ricerca Oggetti**: NON usare `FindObjectOfType<T>()`. Usare `FindFirstObjectByType<T>()` o `FindAnyObjectByType<T>()`.
3. **URP Render Graph**: Qualsiasi modifica alle pass post-processing deve utilizzare `RecordRenderGraph(RenderGraph, ContextContainer)` e i parametri `RenderGraphUtils`. Evitare chiamate a `Debug.Log` dentro `RecordRenderGraph` poiché vengono eseguite ogni frame.

### B. Gestione degli Eventi Butter
1. **Registrazione Listener**: Iscrivere sempre i listener in `OnEnable()` e disiscriverli **tassativamente** in `OnDisable()`.
   ```csharp
   void OnEnable()
   {
       if (myEvent != null) myEvent.RegisterListener(HandleEvent);
   }
   void OnDisable()
   {
       if (myEvent != null) myEvent.DeregisterListener(HandleEvent);
   }
   ```
2. **Eventi senza parametri**: Butter usa il tipo `Unit` per gli eventi void (`GameEvent`). Nel delegate usare `(Unit unit) => ...` o `_ => ...`.

### C. Interazioni XR e Fisica degli Oggetti
1. **Oggetti in mano**: Quando un oggetto è trasportato da un NPC o preso in carico da un cestino, disabilitare i `Collider` e impostare `rb.isKinematic = true` per evitare conflitti con la mano del player o con i trigger di scena.
2. **Interactables**: Per verificare se il giocatore sta tenendo un oggetto in mano, controllare `XRGrabInteractable.isSelected`.
3. **Namespace XR**: Utilizzare `UnityEngine.XR.Interaction.Toolkit.Interactables` per gli interactable (es. `XRGrabInteractable`).

### D. Testing Senza Visore VR
- Utilizzare [`FlowDebugger`](file:///Assets/_Project/Scripts/Core/FlowDebugger.cs) tramite il menu tasto destro (Context Menu) nel componente durante il Play Mode per simulare le interazioni di gioco (smistamenti, dialoghi, bivi ribelli).

---

## 8. Workflow per Aggiunte Comuni

### Come aggiungere un nuovo Giorno:
1. In Unity: `Create > AvenueXR > Day Data` (es. `Day5_Rebellion.asset`).
2. Configurare `dayLabel`, dialoghi di intro/outro, audio del giorno e livello inquinamento (`worldPollutionLevel`).
3. Popolare la lista `deliveries`: per ogni step specificare `type` e gli eventuali dialoghi Boss/NPC.
4. Collegare `nextDayObedient` e `nextDayRebel` e la soglia `rebellionThreshold`.
5. Se è un finale, attivare `isFinale = true`, assegnare `endingTitle`, `endingDescription`, `endingSoundClip` e opzionalmente `specialFinaleObjectPrefab`.

### Come aggiungere un nuovo Tipo di Oggetto:
1. Aggiungere il valore all'enum [`WasteType`](file:///Assets/_Project/Scripts/Core/WasteType.cs).
2. Aggiungere i prefabs corrispondenti in [`WasteObjectSpawner`](file:///Assets/_Project/Scripts/Core/WasteObjectSpawner.cs).
3. Configurare lo SFX di distruzione in `WasteAudioData.asset`.
4. Configurare le particelle di distruzione in `WasteParticleData.asset`.
5. Se necessario, aggiungere la regola di gestione morale in [`WasteBin`](file:///Assets/_Project/Scripts/Core/WasteBin.cs) o [`WindowZone`](file:///Assets/_Project/Scripts/Core/WindowZone.cs).
