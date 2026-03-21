using System;
using System.Collections.Generic;
using System.Text;
using Klei;
using UnityEngine;

namespace ControlledMorale
{
    // Bottle/meter diagnostics — filter player.log by Tag; set Enabled = false to silence.
    public static class AlcoholBreweryBottleDebug
    {
        public const string Tag = "[AlcoholBrewery.BottleDebug]";
        public static bool Enabled = true;
        public static bool FatSim1000Snapshot = true;
        public const int MaxCharsPerUnityLogLine = 12000;

        public static void Log(string message)
        {
            if (!Enabled)
                return;
            string timePrefix = "";
            try
            {
                if (GameClock.Instance != null)
                    timePrefix = $"GameTime={GameClock.Instance.GetTime():F2}s ";
            }
            catch
            {
                // ignore
            }
            Debug.Log($"{Tag} {timePrefix}UnityTime={Time.time:F3}s frame={Time.frameCount} timeScale={Time.timeScale} | {message}");
        }

        public static void LogFat(string title, string body)
        {
            if (!Enabled)
                return;
            Log($"═══ {title} ═══ begin");
            if (string.IsNullOrEmpty(body))
            {
                Log($"(empty body for {title})");
                return;
            }
            for (int i = 0; i < body.Length; i += MaxCharsPerUnityLogLine)
            {
                int len = Math.Min(MaxCharsPerUnityLogLine, body.Length - i);
                Debug.Log($"{Tag} [{title} part {i / MaxCharsPerUnityLogLine + 1}]\n{body.Substring(i, len)}");
            }
            Log($"═══ {title} ═══ end");
        }
    }

    public class AlcoholBrewery : ComplexFabricator
    {
        private Controller.Instance smi;
        private bool blockProduction;

        public override void OnSpawn()
        {
            base.OnSpawn();
            Debug.Log("[AlcoholBrewery] OnSpawn — v3 bottler-style-pickup-workable");
            AlcoholBreweryBottleDebug.Log(
                $"OnSpawn building={gameObject.name} pos={transform.position} timeScale={Time.timeScale}");
            var vanillaFabSm = GetComponent<ComplexFabricatorSM>();
            if (vanillaFabSm != null)
                UnityEngine.Object.Destroy(vanillaFabSm);
            smi = new Controller.Instance(this);
            smi.StartSM();
            if (outStorage != null)
                outStorage.OnStorageChange += BottleDebugOnOutStorage;
            if (inStorage != null)
                inStorage.OnStorageChange += BottleDebugOnInStorage;
            smi.BottleDebugSnapshot("OnSpawn after StartSM + storage hooks");
        }

        public override void OnCleanUp()
        {
            if (outStorage != null)
                outStorage.OnStorageChange -= BottleDebugOnOutStorage;
            if (inStorage != null)
                inStorage.OnStorageChange -= BottleDebugOnInStorage;
            smi?.StopSM("OnCleanUp");
            base.OnCleanUp();
        }

        public void BottleDebugSnapshotFromExternal(string context)
        {
            smi?.BottleDebugSnapshot(context);
        }

        public void RefreshBottleMeterFromStorage()
        {
            smi?.UpdateBottleMeter();
        }

        public void EjectOutputIfIncompatibleWithRecipe(ComplexRecipe recipe)
        {
            if (recipe?.results == null || outStorage == null || outStorage.MassStored() <= 0f)
                return;
            var outputTags = new HashSet<Tag>();
            foreach (var r in recipe.results)
            {
                if (r.material.IsValid)
                    outputTags.Add(r.material);
            }
            if (outputTags.Count == 0)
                return;
            bool incompatible = false;
            foreach (GameObject go in outStorage.items)
            {
                if (go == null)
                    continue;
                var kpid = go.GetComponent<KPrefabID>();
                if (kpid == null)
                    continue;
                if (!outputTags.Contains(kpid.PrefabID()))
                {
                    incompatible = true;
                    break;
                }
            }
            if (!incompatible)
                return;
            AlcoholBreweryBottleDebug.Log(
                $"EjectOutputIfIncompatibleWithRecipe: recipe={recipe.id} — ejecting outStorage as pickupables (blocked new queue vs held product)");
            bool prev = outStorage.allowItemRemoval;
            outStorage.allowItemRemoval = true;
            var snapshot = new List<GameObject>(outStorage.items);
            int cell = Grid.PosToCell(this);
            Vector3 dropPos = Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore) + outputOffset;
            foreach (GameObject go in snapshot)
            {
                if (go == null)
                    continue;
                GameObject dropped = outStorage.Drop(go, true);
                if (dropped != null)
                    dropped.transform.SetPosition(dropPos);
            }
            outStorage.allowItemRemoval = prev;
            SetQueueDirty();
            RefreshBottleMeterFromStorage();
        }

        private void BottleDebugOnOutStorage(GameObject _)
        {
            AlcoholBreweryBottleDebug.Log("HOOK outStorage.OnStorageChange fired");
            smi?.BottleDebugSnapshot("hook outStorage.OnStorageChange");
        }

        private void BottleDebugOnInStorage(GameObject _)
        {
            AlcoholBreweryBottleDebug.Log("HOOK inStorage.OnStorageChange fired");
            smi?.BottleDebugSnapshot("hook inStorage.OnStorageChange");
        }

        internal static PrimaryElement GetFirstPrimaryElementFromStorage(Storage storage)
        {
            if (storage == null)
                return null;
            for (int i = 0; i < storage.items.Count; i++)
            {
                GameObject go = storage.items[i];
                if (go == null)
                    continue;
                PrimaryElement pe = go.GetComponent<PrimaryElement>();
                if (pe != null)
                    return pe;
            }
            return null;
        }

        internal static PrimaryElement GetBestLiquidPrimaryElementForBottleMeter(Storage storage)
        {
            if (storage == null || storage.MassStored() <= 0f)
                return null;
            PrimaryElement best = null;
            float bestMass = -1f;
            for (int i = 0; i < storage.items.Count; i++)
            {
                GameObject go = storage.items[i];
                if (go == null)
                    continue;
                PrimaryElement pe = go.GetComponent<PrimaryElement>();
                if (pe == null || pe.Mass <= 0f)
                    continue;
                Element el = pe.Element;
                if (el == null || !el.IsLiquid)
                    continue;
                if (pe.Mass > bestMass)
                {
                    bestMass = pe.Mass;
                    best = pe;
                }
            }
            return best ?? GetFirstPrimaryElementFromStorage(storage);
        }

        public PrimaryElement GetFirstPrimaryElementForMeterVisual()
        {
            if (outStorage == null || outStorage.MassStored() <= 0f)
                return null;
            return GetBestLiquidPrimaryElementForBottleMeter(outStorage);
        }

        public override void Sim1000ms(float dt)
        {
            if (AlcoholBreweryBottleDebug.Enabled && AlcoholBreweryBottleDebug.FatSim1000Snapshot && smi != null)
                smi.BottleDebugSnapshot("Sim1000ms FatSim1000Snapshot (per-building, very verbose)");
            if (blockProduction)
                return;
            base.Sim1000ms(dt);
        }

        private class Controller : GameStateMachine<Controller, Controller.Instance, AlcoholBrewery>
        {
            private const string AnimOn = "on";
            private const string AnimReady = "ready";
            private const string AnimWorkingPre = "working_pre";
            private const string AnimWorkingLoop = "working";
            private const string AnimWorkingPost = "working_post";

#pragma warning disable CS0649 // State fields wired by GameStateMachine when building the graph
            public State nonoperational;
            public OperationalStates operational;

            public class OperationalStates : State
            {
                public State idle;
                public State awaitingProduction;
                public State working_pre;
                public State working_during;
                public State working_post;
                public State output_waiting;
            }
#pragma warning restore CS0649

            public override void InitializeStates(out BaseState default_state)
            {
                default_state = nonoperational;

                nonoperational
                    .PlayAnim("off")
                    .TagTransition(GameTags.Operational, operational, false);

                operational
                    .EnterTransition(operational.output_waiting, smi => smi.master.outStorage.MassStored() > 0f)
                    .DefaultState(operational.idle)
                    .TagTransition(GameTags.Operational, nonoperational, true);

                operational.idle
                    .Enter(smi =>
                    {
                        smi.master.blockProduction = false;
                        smi.EnsureMeterActive();
                        smi.UpdateBottleMeter();
                        smi.PlayMainBuildingAnim("off", KAnim.PlayMode.Once);
                        smi.BottleDebugSnapshot("SM operational.idle Enter");
                    })
                    .Transition(operational.awaitingProduction,
                        smi => smi.master.CurrentWorkingOrder != null, UpdateRate.SIM_200ms)
                    .EventTransition(GameHashes.FabricatorOrderStarted, operational.awaitingProduction);

                operational.awaitingProduction
                    .Enter(smi =>
                    {
                        smi.EnsureMeterActive();
                        smi.UpdateBottleMeter();
                        smi.PlayMainBuildingAnim(AnimOn, KAnim.PlayMode.Once);
                        smi.BottleDebugSnapshot("SM operational.awaitingProduction Enter");
                    })
                    .EventTransition(GameHashes.FabricatorOrderCompleted, operational.working_post)
                    .OnAnimQueueComplete(operational.working_pre);

                operational.working_pre
                    .Enter(smi =>
                    {
                        smi.EnsureMeterActive();
                        smi.UpdateBottleMeter();
                        smi.PlayMainBuildingAnim(AnimWorkingPre, KAnim.PlayMode.Once);
                        smi.BottleDebugSnapshot("SM operational.working_pre Enter");
                    })
                    .EventTransition(GameHashes.FabricatorOrderCompleted, operational.working_post)
                    .OnAnimQueueComplete(operational.working_during);

                operational.working_during
                    .Enter(smi =>
                    {
                        smi.EnsureMeterActive();
                        smi.UpdateBottleMeter();
                        smi.PlayMainBuildingAnim(AnimWorkingLoop, KAnim.PlayMode.Loop);
                        smi.BottleDebugSnapshot("SM operational.working_during Enter");
                    })
                    .EventTransition(GameHashes.FabricatorOrderCompleted, operational.working_post);

                operational.working_post
                    .Enter(smi =>
                    {
                        smi.master.blockProduction = true;
                        smi.EnsureMeterActive();
                        smi.UpdateBottleMeter();
                        smi.PlayMainBuildingAnim(AnimWorkingPost, KAnim.PlayMode.Once);
                        smi.BottleDebugSnapshot("SM operational.working_post Enter");
                    })
                    .OnAnimQueueComplete(operational.output_waiting);

                operational.output_waiting
                    .Enter(smi =>
                    {
                        if (smi.master.outStorage.MassStored() <= 0f)
                        {
                            AlcoholBreweryBottleDebug.Log("SM operational.output_waiting Enter → outStorage empty, GoTo idle");
                            smi.GoTo(smi.sm.operational.idle);
                            return;
                        }
                        smi.master.blockProduction = true;
                        smi.EnsureMeterActive();
                        smi.UpdateBottleMeter();
                        smi.PlayMainBuildingAnim(AnimReady, KAnim.PlayMode.Loop);
                        smi.ConfigurePickupables(true);
                        smi.BottleDebugSnapshot("SM operational.output_waiting Enter");
                    })
                    .Exit(smi => smi.ConfigurePickupables(false))
                    .EventTransition(GameHashes.OnStorageChange, operational.idle,
                        smi => smi.master.outStorage.MassStored() <= 0f);
            }

            public new class Instance : GameStateMachine<Controller, Instance, AlcoholBrewery, object>.GameInstance
            {
                private static readonly string[] MeterHiddenBuildingSymbols =
                {
                    "bottle",
                    "substance_tinter",
                    "substance_tinter_cap"
                };

                public MeterController meter { get; private set; }
                private readonly KBatchedAnimController buildingAnim;

                public Instance(AlcoholBrewery master) : base(master)
                {
                    buildingAnim = master.GetComponent<KBatchedAnimController>();
                    meter = new MeterController(buildingAnim, "bottle", "off", Meter.Offset.UserSpecified,
                        Grid.SceneLayer.BuildingFront, MeterHiddenBuildingSymbols);
                    BottleDebugSnapshot("Instance ctor after meter init");
                }

                public void BottleDebugSnapshot(string context)
                {
                    if (!AlcoholBreweryBottleDebug.Enabled)
                        return;
                    try
                    {
                        AlcoholBreweryBottleDebug.LogFat(context, BuildFatSnapshotReport(context));
                    }
                    catch (Exception e)
                    {
                        AlcoholBreweryBottleDebug.Log($"{context} | snapshot exception: {e}\n{e.StackTrace}");
                    }
                }

                private static string TransformPath(Transform t)
                {
                    if (t == null)
                        return "?";
                    var parts = new System.Collections.Generic.List<string>();
                    while (t != null)
                    {
                        parts.Add(t.name);
                        t = t.parent;
                    }
                    parts.Reverse();
                    return string.Join("/", parts.ToArray());
                }

                private string BuildFatSnapshotReport(string context)
                {
                    var m = master;
                    var sb = new StringBuilder(4096);
                    sb.AppendLine($"context={context}");
                    sb.AppendLine($"building GO={m.gameObject.name} pos={m.transform.position} rot={m.transform.rotation.eulerAngles}");
                    sb.AppendLine($"blockProduction={m.blockProduction} duplicantOperated={m.duplicantOperated}");
                    try
                    {
                        var cur = m.CurrentWorkingOrder;
                        if (cur != null)
                            sb.AppendLine($"CurrentWorkingOrder id={cur.id} OrderProgress={m.OrderProgress}");
                        else
                            sb.AppendLine("CurrentWorkingOrder=(null)");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"CurrentWorkingOrder read failed: {ex.Message}");
                    }

                    void AppendKbac(string label, KBatchedAnimController kbac)
                    {
                        if (kbac == null)
                        {
                            sb.AppendLine($"{label}: null");
                            return;
                        }
                        sb.AppendLine($"{label}: name={kbac.gameObject.name} path={TransformPath(kbac.transform)}");
                        sb.AppendLine($"  enabled={kbac.enabled} sceneLayer={kbac.sceneLayer} FlipX={kbac.FlipX} FlipY={kbac.FlipY}");
                        sb.AppendLine($"  currentAnim={kbac.currentAnim} offset={kbac.Offset}");
                        if (kbac.AnimFiles != null)
                        {
                            sb.AppendLine($"  AnimFiles.Length={kbac.AnimFiles.Length}");
                            for (int i = 0; i < kbac.AnimFiles.Length; i++)
                                sb.AppendLine($"    [{i}]={(kbac.AnimFiles[i] != null ? kbac.AnimFiles[i].name : "null")}");
                        }
                        else
                            sb.AppendLine("  AnimFiles=null");
                    }

                    AppendKbac("building KBAC", buildingAnim);
                    if (buildingAnim != null)
                    {
                        foreach (var sym in MeterHiddenBuildingSymbols)
                        {
                            bool vis = buildingAnim.GetSymbolVisiblity(new KAnimHashedString(sym));
                            sb.AppendLine($"  building symbol '{sym}' visible={vis}");
                        }
                    }

                    if (meter?.meterController != null)
                    {
                        AppendKbac("meter KBAC", meter.meterController);
                        sb.AppendLine($"meter GO activeSelf={meter.gameObject.activeSelf} activeInHierarchy={meter.gameObject.activeInHierarchy}");
                        sb.AppendLine($"meter GO pos={meter.gameObject.transform.position} localPos={meter.gameObject.transform.localPosition}");
                    }
                    else
                        sb.AppendLine("meter or meter.meterController: null");

                    sb.AppendLine("--- KBatchedAnimControllers under building root (all) ---");
                    var allKbac = m.GetComponentsInChildren<KBatchedAnimController>(true);
                    sb.AppendLine($"count={allKbac.Length}");
                    for (int i = 0; i < allKbac.Length; i++)
                    {
                        var k = allKbac[i];
                        sb.AppendLine($"  [{i}] {TransformPath(k.transform)} anim={k.currentAnim} enabled={k.enabled} sceneLayer={k.sceneLayer}");
                    }

                    void AppendStorage(string label, Storage st)
                    {
                        if (st == null)
                        {
                            sb.AppendLine($"{label}: null");
                            return;
                        }
                        sb.AppendLine($"{label}: mass={st.MassStored():F3} kg capacity={st.capacityKg} allowRemoval={st.allowItemRemoval} items={st.items.Count}");
                        for (int i = 0; i < st.items.Count; i++)
                        {
                            var go = st.items[i];
                            if (go == null)
                            {
                                sb.AppendLine($"  [{i}] null");
                                continue;
                            }
                            var kpid = go.GetComponent<KPrefabID>();
                            var pe = go.GetComponent<PrimaryElement>();
                            sb.AppendLine($"  [{i}] prefab={kpid?.PrefabID()} mass={pe?.Mass ?? -1f} elem={pe?.ElementID}");
                            var pup = go.GetComponent<Pickupable>();
                            if (pup != null)
                                sb.AppendLine($"       Pickupable targetWorkable={(pup.targetWorkable != null ? pup.targetWorkable.GetType().Name : "null")} Reserved={pup.ReservedAmount}");
                        }
                    }

                    AppendStorage("inStorage", m.inStorage);
                    AppendStorage("outStorage", m.outStorage);

                    var pickup = m.GetComponent<AlcoholBreweryPickupWorkable>();
                    sb.AppendLine($"AlcoholBreweryPickupWorkable={(pickup != null ? "present" : "missing")}");

                    return sb.ToString();
                }

                private void SetBuildingBottleSymbolsVisible(bool visible)
                {
                    if (buildingAnim == null)
                        return;
                    foreach (var s in MeterHiddenBuildingSymbols)
                        buildingAnim.SetSymbolVisiblity(new KAnimHashedString(s), visible);
                }

                public void EnsureMeterActive()
                {
                    if (meter?.gameObject != null)
                        meter.gameObject.SetActive(true);
                    if (meter?.meterController != null)
                        meter.meterController.enabled = true;
                    SetBuildingBottleSymbolsVisible(false);
                }

                public void RestoreMeterForBottleFill()
                {
                    EnsureMeterActive();
                    UpdateBottleMeter();
                }

                public void PlayMainBuildingAnim(string anim, KAnim.PlayMode mode)
                {
                    if (buildingAnim != null)
                    {
                        buildingAnim.Play(new HashedString(anim), mode, 1f, 0f);
                        AlcoholBreweryBottleDebug.Log($"PlayMainBuildingAnim clip={anim} mode={mode}");
                    }
                }

                public void QueueMainBuildingAnim(string anim, KAnim.PlayMode mode)
                {
                    if (buildingAnim == null)
                        return;
                    string before = buildingAnim.CurrentAnim?.name ?? "(null)";
                    buildingAnim.Queue(new HashedString(anim), mode, 1f, 0f);
                    string after = buildingAnim.CurrentAnim?.name ?? "(null)";
                    AlcoholBreweryBottleDebug.Log(
                        $"QueueMainBuildingAnim queued={anim} mode={mode} curAnimBefore={before} curAnimAfter={after}");
                    if (AlcoholBreweryBottleDebug.Enabled)
                        BottleDebugSnapshot($"after QueueMainBuildingAnim {anim}");
                }

                public void ConfigurePickupables(bool entering)
                {
                    var outStor = master.outStorage;
                    var pickupWorkable = master.GetComponent<AlcoholBreweryPickupWorkable>();
                    AlcoholBreweryBottleDebug.Log(
                        $"ConfigurePickupables entering={entering} outItems={outStor?.items?.Count ?? -1} mass={(outStor != null ? outStor.MassStored() : -1f):F3}");
                    outStor.allowItemRemoval = entering;
                    foreach (GameObject go in outStor.items)
                    {
                        if (go == null) continue;
                        Pickupable p = go.GetComponent<Pickupable>();
                        if (p == null) continue;
                        if (entering)
                        {
                            p.targetWorkable = pickupWorkable;
                            p.SetOffsets(new CellOffset[] { new CellOffset(0, 0) });
                            p.OnReservationsChanged = (Action<Pickupable, bool, Pickupable.Reservation>)
                                Delegate.Combine(p.OnReservationsChanged,
                                    new Action<Pickupable, bool, Pickupable.Reservation>(pickupWorkable.OnReservationsChanged));
                        }
                        else
                        {
                            p.targetWorkable = p;
                            p.SetOffsetTable(OffsetGroups.InvertedStandardTable);
                            p.OnReservationsChanged = (Action<Pickupable, bool, Pickupable.Reservation>)
                                Delegate.Remove(p.OnReservationsChanged,
                                    new Action<Pickupable, bool, Pickupable.Reservation>(pickupWorkable.OnReservationsChanged));
                            FetchableMonitor.Instance fmi = p.GetSMI<FetchableMonitor.Instance>();
                            if (fmi != null)
                                fmi.SetForceUnfetchable(false);
                        }
                        go.Trigger(-778359855, outStor);
                    }
                }

                public void UpdateBottleMeter()
                {
                    if (meter == null)
                        return;
                    PrimaryElement first = master.GetFirstPrimaryElementForMeterVisual();
                    AlcoholBreweryBottleDebug.Log(
                        $"UpdateBottleMeter hasOutProduct={first != null} elem={(first != null ? first.ElementID.ToString() : "none")} mass={(first != null ? first.Mass.ToString("F3") : "-")}");
                    if (first == null)
                        return;
                    Substance sub = first.Element.substance;
                    if (sub == null)
                        return;
                    if (sub.anims != null && sub.anims.Length > 0)
                        meter.meterController.SwapAnims(sub.anims);
                    else if (sub.anim != null)
                        meter.meterController.SwapAnims(new[] { sub.anim });
                    else
                        return;
                    meter.meterController.Play(OreSizeVisualizerComponents.GetAnimForMass(first.Mass),
                        KAnim.PlayMode.Paused, 1f, 0f);
                    Color32 colour = sub.colour;
                    colour.a = byte.MaxValue;
                    meter.SetSymbolTint(new KAnimHashedString("meter_fill"), colour);
                    meter.SetSymbolTint(new KAnimHashedString("water1"), colour);
                    meter.SetSymbolTint(new KAnimHashedString("substance_tinter"), colour);
                    meter.SetSymbolTint(new KAnimHashedString("substance_tinter_cap"), colour);
                    SetBuildingBottleSymbolsVisible(false);
                    if (AlcoholBreweryBottleDebug.Enabled)
                        BottleDebugSnapshot("after UpdateBottleMeter");
                }
            }
        }
    }

    public class AlcoholBreweryPickupWorkable : Workable
    {
        public Storage storage;
        private MeterController workerMeter;

        public override void OnPrefabInit()
        {
            base.OnPrefabInit();
            overrideAnims = new KAnimFile[] { Assets.GetAnim("anim_interacts_bottler_kanim") };
            workAnims = new HashedString[] { "pick_up" };
            workingPstComplete = null;
            workingPstFailed = null;
            synchronizeAnims = true;
            SetOffsets(new CellOffset[] { new CellOffset(0, 0) });
            SetWorkTime(overrideAnims[0].GetData().GetAnim("pick_up").totalTime);
            resetProgressOnStop = true;
            showProgressBar = false;
        }

        public override void OnStartWork(WorkerBase worker)
        {
            base.OnStartWork(worker);
            AlcoholBreweryBottleDebug.Log(
                $"PickupWorkable.OnStartWork worker={worker?.name} storage mass={(storage != null ? storage.MassStored() : -1f):F3} items={storage?.items?.Count ?? -1}");
            PrimaryElement pe = null;
            if (worker != null && worker.GetStartWorkInfo() is Pickupable.PickupableStartWorkInfo swi &&
                swi.originalPickupable != null)
                pe = swi.originalPickupable.GetComponent<PrimaryElement>();
            if (pe == null)
                pe = AlcoholBrewery.GetBestLiquidPrimaryElementForBottleMeter(storage);
            CreateBottleProxyObject(worker, pe);
        }

        public override void OnCompleteWork(WorkerBase worker)
        {
            AlcoholBreweryBottleDebug.Log($"PickupWorkable.OnCompleteWork worker={worker?.name}");
            Storage workerStorage = worker.GetComponent<Storage>();
            var workInfo = (Pickupable.PickupableStartWorkInfo)worker.GetStartWorkInfo();
            if (workInfo.amount > 0f)
                storage.TransferMass(workerStorage, workInfo.originalPickupable.KPrefabID.PrefabID(), workInfo.amount, false, false, false);
            GameObject transferred = workerStorage.FindFirst(workInfo.originalPickupable.KPrefabID.PrefabID());
            if (transferred != null)
            {
                Pickupable p = transferred.GetComponent<Pickupable>();
                p.targetWorkable = p;
                FetchableMonitor.Instance fmi = p.GetSMI<FetchableMonitor.Instance>();
                if (fmi != null)
                    fmi.SetForceUnfetchable(false);
                workInfo.setResultCb(transferred);
            }
            else
            {
                workInfo.setResultCb(null);
            }
            GetComponent<AlcoholBrewery>()?.BottleDebugSnapshotFromExternal("PickupWorkable.OnCompleteWork after transfer");
            base.OnCompleteWork(worker);
            GetComponent<AlcoholBrewery>()?.RefreshBottleMeterFromStorage();
        }

        public override void OnStopWork(WorkerBase worker)
        {
            AlcoholBreweryBottleDebug.Log($"PickupWorkable.OnStopWork worker={worker?.name}");
            base.OnStopWork(worker);
            CleanupBottleProxyObject();
            GetComponent<AlcoholBrewery>()?.RefreshBottleMeterFromStorage();
        }

        public override void OnAbortWork(WorkerBase worker)
        {
            AlcoholBreweryBottleDebug.Log($"PickupWorkable.OnAbortWork worker={worker?.name}");
            base.OnAbortWork(worker);
            GetAnimController().Play("ready", KAnim.PlayMode.Once, 1f, 0f);
        }

        public override void OnForcedCleanUp()
        {
            if (worker != null)
            {
                ChoreDriver cd = worker.GetComponent<ChoreDriver>();
                if (cd != null)
                    cd.StopChore();
                else
                    worker.StopWork();
            }
            if (workerMeter != null)
                CleanupBottleProxyObject();
            base.OnForcedCleanUp();
        }

        private void CreateBottleProxyObject(WorkerBase worker, PrimaryElement pe)
        {
            if (workerMeter != null)
                CleanupBottleProxyObject();
            if (pe == null)
            {
                AlcoholBreweryBottleDebug.Log("CreateBottleProxyObject: no PrimaryElement — skip proxy");
                return;
            }
            AlcoholBreweryBottleDebug.Log(
                $"CreateBottleProxyObject on dupe chest element={pe.ElementID} mass={pe.Mass:F3} (second bottle overlay during pick_up)");
            workerMeter = new MeterController(worker.GetComponent<KBatchedAnimController>(),
                "snapto_chest", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer,
                new string[] { "snapto_chest" });
            Substance sub = pe.Element.substance;
            if (sub.anims != null && sub.anims.Length > 0)
                workerMeter.meterController.SwapAnims(sub.anims);
            else if (sub.anim != null)
                workerMeter.meterController.SwapAnims(new[] { sub.anim });
            else
            {
                AlcoholBreweryBottleDebug.Log("CreateBottleProxyObject: substance has no anim bank — skip chest proxy");
                CleanupBottleProxyObject();
                return;
            }
            workerMeter.meterController.Play("empty", KAnim.PlayMode.Paused, 1f, 0f);
            Color32 colour = pe.Element.substance.colour;
            colour.a = byte.MaxValue;
            workerMeter.SetSymbolTint(new KAnimHashedString("meter_fill"), colour);
            workerMeter.SetSymbolTint(new KAnimHashedString("water1"), colour);
            workerMeter.SetSymbolTint(new KAnimHashedString("substance_tinter"), colour);
            workerMeter.SetSymbolTint(new KAnimHashedString("substance_tinter_cap"), colour);
            GetComponent<AlcoholBrewery>()?.BottleDebugSnapshotFromExternal("after CreateBottleProxy on dupe");
        }

        public void OnReservationsChanged(Pickupable _ignore, bool _ignore2, Pickupable.Reservation _ignore3)
        {
            bool forceUnfetchable = false;
            foreach (GameObject go in storage.items)
            {
                if (go != null && go.GetComponent<Pickupable>()?.ReservedAmount > 0f)
                {
                    forceUnfetchable = true;
                    break;
                }
            }
            foreach (GameObject go in storage.items)
            {
                FetchableMonitor.Instance fmi = go.GetSMI<FetchableMonitor.Instance>();
                if (fmi != null)
                    fmi.SetForceUnfetchable(forceUnfetchable);
            }
        }

        private void CleanupBottleProxyObject()
        {
            if (workerMeter != null && !workerMeter.gameObject.IsNullOrDestroyed())
            {
                AlcoholBreweryBottleDebug.Log("CleanupBottleProxyObject: removing dupe chest meter proxy");
                workerMeter.Unlink();
                workerMeter.gameObject.DeleteObject();
            }
            workerMeter = null;
        }
    }
}
