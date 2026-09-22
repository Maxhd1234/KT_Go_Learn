using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using HarmonyLib;
using RimWorld;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Windows;
using Verse;
using Verse.AI;
using Verse.Noise;



namespace KT_Go_Learn
{

    public class GoLearn : ModSettings
    {
        public float learningCap = 0.9f;
        public bool onlyAllowWantedLearning = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref learningCap, "learningCap", 0.9f);
            Scribe_Values.Look(ref onlyAllowWantedLearning, "onlyAllowWantedLearning", false);
        }
    }

    public class FloatMenuOptionProvider_Learn : FloatMenuOptionProvider
    {
        protected override bool Drafted => false;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;
        GoLearn settings = LoadedModManager.GetMod<GoLearnMod>().GetSettings<GoLearn>();
        public override IEnumerable<FloatMenuOption> GetOptions(FloatMenuContext context)
        {
            Pawn pawn = context.FirstSelectedPawn;

            if (!CanLearn(pawn))
            {
                yield break;
            }

            foreach (Thing thing in context.ClickedThings)
            {
                // Radiotalking
                if (thing.def == ThingDefOf.CommsConsole)
                {
                    FloatMenuOption floatMenuOption = SoonNeedsBasicNeed(pawn, context);
                    if (!pawn.learning.ActiveLearningDesires.Contains(DefDatabase<LearningDesireDef>.GetNamed("Radiotalking")) && settings.onlyAllowWantedLearning)
                    {
                        yield return new FloatMenuOption("KT_Go_Learn_Unable".Translate("KT_Go_Learn_Unable_Radiotalking".Translate()), null);
                    }

                    else if (floatMenuOption == null)
                    {
                        JobDef jobDef = DefDatabase<JobDef>.GetNamed("Radiotalking");
                        Job learnJob = JobMaker.MakeJob(jobDef, thing);
                        learnJob.playerForced = true;
                        floatMenuOption = null;
                        yield return new FloatMenuOption("KT_Go_Learn_Radiotalking".Translate(),
                            delegate
                            {
                                pawn.jobs.ClearQueuedJobs();
                                pawn.jobs.TryTakeOrderedJob(learnJob);
                            }, iconThing: thing, iconColor: Color.white);
                    }
                    else if (floatMenuOption != null)
                    {
                        yield return floatMenuOption;
                    }

                }

                // Lessontaking
                if (thing.def == ThingDefOf.SchoolDesk)
                {
                    FloatMenuOption floatMenuOption = SoonNeedsBasicNeed(pawn, context);
                    if (!pawn.learning.ActiveLearningDesires.Contains(DefDatabase<LearningDesireDef>.GetNamed("Lessontaking")) && settings.onlyAllowWantedLearning)
                    {
                        yield return new FloatMenuOption("KT_Go_Learn_Unable".Translate("KT_Go_Learn_Unable_Lessontaking".Translate()), null);
                    }
                    else if (floatMenuOption == null)
                    {
                        Pawn potentialTeacher = SchoolUtility.FindTeacher(pawn);
                        if (potentialTeacher != null)
                        {
                            Job learnJob = JobMaker.MakeJob(JobDefOf.Lessontaking, thing);
                            learnJob.playerForced = true;
                            floatMenuOption = null;
                            yield return new FloatMenuOption("KT_Go_Learn_Lessontaking".Translate(),
                                delegate
                                {
                                    pawn.jobs.ClearQueuedJobs();
                                    pawn.jobs.TryTakeOrderedJob(learnJob);
                                }, iconThing: thing, iconColor: Color.white);
                        }
                        else
                        {
                            yield return new FloatMenuOption("KT_Go_Learn_Unable".Translate("KT_Go_Learn_Unable_Teacher"), null);
                        }
                    }
                    else if (floatMenuOption != null)
                    {
                        yield return floatMenuOption;
                    }
                }
            }

            if (context.ClickedCell.GetFirstBuilding(pawn.Map) == null || IsGoodDestinationFor(context.ClickedCell, context.FirstSelectedPawn, careAboutDanger: true) || !context.ClickedCell.GetThingList(pawn.Map).Any(t => t.def.passability == Traversability.Impassable))
            {

                // Skydreaming
                if (IsValidTargettoSkydream(context))
                {
                    FloatMenuOption floatMenuOption = SoonNeedsBasicNeed(pawn, context);
                    if (!pawn.learning.ActiveLearningDesires.Contains(DefDatabase<LearningDesireDef>.GetNamed("Skydreaming")) && settings.onlyAllowWantedLearning)
                    {
                        yield return new FloatMenuOption("KT_Go_Learn_Unable".Translate("KT_Go_Learn_Unable_Skydreaming".Translate()), null);

                    }
                    else if (floatMenuOption == null)
                    {
                        JobDef skyDef = DefDatabase<JobDef>.GetNamed("Skydreaming");
                        Job job = JobMaker.MakeJob(skyDef, context.ClickedCell);
                        job.playerForced = true;
                        floatMenuOption = null;
                        yield return new FloatMenuOption("KT_Go_Learn_Skydreaming".Translate(),
                            delegate
                            {
                                pawn.jobs.ClearQueuedJobs();
                                pawn.jobs.TryTakeOrderedJob(job);
                            }, MenuOptionPriority.High);
                    }
                    else if (floatMenuOption != null)
                    {
                        yield return floatMenuOption;
                    }
                }

                // NatureRunning
                if (IsValidTargettoSkydream(context))
                {
                    FloatMenuOption floatMenuOption = SoonNeedsBasicNeed(pawn, context);
                    if (!pawn.learning.ActiveLearningDesires.Contains(DefDatabase<LearningDesireDef>.GetNamed("NatureRunning")) && settings.onlyAllowWantedLearning)
                    {
                        yield return new FloatMenuOption("KT_Go_Learn_Unable".Translate("KT_Go_Learn_Unable_NatureRunning".Translate()), null);

                    }
                    else if (floatMenuOption == null)
                    {
                        JobDef jobdef = DefDatabase<JobDef>.GetNamed("NatureRunning");
                        Job job = JobMaker.MakeJob(jobdef, context.ClickedCell);
                        job.playerForced = true;
                        job.locomotionUrgency = LocomotionUrgency.Sprint;
                        job.SetTarget(TargetIndex.B, context.ClickedCell);
                        floatMenuOption = null;
                        yield return new FloatMenuOption("KT_Go_Learn_NatureRunning".Translate(),
                            delegate
                            {
                                pawn.jobs.ClearQueuedJobs();
                                pawn.jobs.TryTakeOrderedJob(job);
                            }, MenuOptionPriority.High);
                    }
                    else if (floatMenuOption != null)
                    {
                        yield return floatMenuOption;
                    }
                }
                // Floordrawing
                if (CanDrawOn(context))
                {
                    FloatMenuOption floatMenuOption = SoonNeedsBasicNeed(pawn, context);
                    if (!pawn.learning.ActiveLearningDesires.Contains(DefDatabase<LearningDesireDef>.GetNamed("Floordrawing")) && settings.onlyAllowWantedLearning)
                    {
                        yield return new FloatMenuOption("KT_Go_Learn_Unable".Translate("KT_Go_Learn_Unable_Floordrawing".Translate()), null);

                    }
                    else if (floatMenuOption == null)
                    {
                        JobDef floorDef = DefDatabase<JobDef>.GetNamed("Floordrawing");
                        Job job = JobMaker.MakeJob(floorDef, context.ClickedCell, context.ClickedCell);
                        job.playerForced = true;
                        floatMenuOption = null;
                        yield return new FloatMenuOption("KT_Go_Learn_Floordrawing".Translate(),
                            delegate
                            {
                                pawn.jobs.ClearQueuedJobs();
                                pawn.jobs.TryTakeOrderedJob(job);
                            }, MenuOptionPriority.High);
                    }
                    else if (floatMenuOption != null)
                    {
                        yield return floatMenuOption;
                    }
                }
            }

            // Workwatching
            if (context.ClickedPawns.Count > 0 && LearningGiver_Workwatching.ChildCanLearnFromAdultJob(pawn, context.ClickedPawns[0]))
            {

                FloatMenuOption floatMenuOption = SoonNeedsBasicNeed(pawn, context);
                if (!pawn.learning.ActiveLearningDesires.Contains(DefDatabase<LearningDesireDef>.GetNamed("Workwatching")) && settings.onlyAllowWantedLearning)
                {
                    yield return new FloatMenuOption("KT_Go_Learn_Unable".Translate("KT_Go_Learn_Unable_Workwatching".Translate()), null);
                }
                else if (floatMenuOption == null)
                {
                    JobDef workDef = DefDatabase<JobDef>.GetNamed("Workwatching");
                    Job job = JobMaker.MakeJob(workDef, context.ClickedPawns[0]);
                    job.playerForced = true;
                    floatMenuOption = null;
                    string pawnNameFull = context.ClickedPawns[0].Name.ToString();

                    yield return new FloatMenuOption("KT_Go_Learn_Workwatching".Translate(Regex.Match(pawnNameFull, @"'([^']+)'").Groups[1].Value),
                        delegate
                        {
                            pawn.jobs.ClearQueuedJobs();
                            pawn.jobs.TryTakeOrderedJob(job);
                        }, iconThing: context.ClickedPawns[0], iconColor: Color.white);
                }
                else if (floatMenuOption != null)
                {
                    yield return floatMenuOption;
                }

            }
        }

        public bool IsValidTargettoSkydream(FloatMenuContext context)
        {
            Pawn searcher = context.FirstSelectedPawn;
            IntVec3 c = context.ClickedCell;
            Map map = searcher.Map;
            if (c.Roofed(map) || c.GetTerrain(map).avoidWander || !IsGoodDestinationFor(c, searcher, careAboutDanger: true))
            {
                return false;
            }
            else
                return true;
        }
        public bool CanDrawOn(FloatMenuContext context)
        {
            IntVec3 clickedCell = context.ClickedCell;
            Map map = context.map;
            if (FilthMaker.CanMakeFilth(clickedCell, map, ThingDefOf.Filth_Floordrawing, FilthSourceFlags.Pawn) && clickedCell.GetFirstBuilding(context.map) == null)
            {
                return true;
            }
            return false;
        }
        public bool CanLearn(Pawn child)
        {
            if (child.needs?.learning?.CurLevel < settings.learningCap && child.DevelopmentalStage == DevelopmentalStage.Child)
            {
                return true;
            }
            else
                return false;
        }

        public bool IsGoodDestinationFor(IntVec3 c, Pawn pawn, bool careAboutDanger)
        {
            Map map = pawn.Map;
            if (!c.Standable(map))
            {
                return false;
            }
            if (careAboutDanger && c.GetTerrain(map).dangerous)
            {
                return false;
            }
            if (!c.WalkableBy(map, pawn))
            {
                return false;
            }

            if (c.IsForbidden(pawn))
            {
                return false;
            }
            if (careAboutDanger && c.GetDangerFor(pawn, map) == Danger.Deadly)
            {
                return false;
            }
            if (careAboutDanger && PawnUtility.KnownDangerAt(c, pawn.Map, pawn))
            {
                return false;
            }
            if (careAboutDanger && c.VacuumConcernTo(pawn))
            {
                return false;
            }
            return true;
        }

        public FloatMenuOption SoonNeedsBasicNeed(Pawn pawn, FloatMenuContext context)
        {
            if (pawn.needs.rest.CurLevel < 0.28f)
            {
                return new FloatMenuOption("KT_Go_Learn_Unable".Translate("KT_Go_Learn_Unable_Tired".Translate()), null);
            }
            if (pawn.needs.food.CurLevel < 0.25f)
            {
                return new FloatMenuOption("KT_Go_Learn_Unable".Translate("KT_Go_Learn_Unable_Hungry".Translate()), null);
            }
            if (pawn.needs.learning.CurLevel >= settings.learningCap)
            {
                return new FloatMenuOption("KT_Go_Learn_Unable".Translate("KT_Go_Learn_Unable_Satisfied".Translate()), null);
            }
            else
                return null;
        }

        [HarmonyPatch(typeof(NatureRunningUtility), nameof(NatureRunningUtility.TryFindNatureInterestTarget))]
        public static class Patch_ForceNatureRunningStart
        {
            static void Postfix(Pawn searcher, ref LocalTargetInfo interestTarget, ref bool __result)
            {
                if (searcher == null)
                    return;
                    
                Job curJob = searcher.CurJob;
                if (curJob != null && curJob.def.defName == "NatureRunning" && curJob.playerForced
                    && curJob.GetTarget(TargetIndex.B).IsValid)
                {
                    interestTarget = curJob.GetTarget(TargetIndex.B);
                    __result = true;
                    curJob.SetTarget(TargetIndex.B, LocalTargetInfo.Invalid); 
                }
            }
        }

        [HarmonyPatch(typeof(Pawn_TimetableTracker), "CurrentAssignment", MethodType.Getter)]
        [HarmonyPriority(Priority.Last)]
        public static class Patch_CurrentAssignment
        {
            public static void Postfix(Pawn_TimetableTracker __instance, ref TimeAssignmentDef __result)
            {
                var pawnField = typeof(Pawn_TimetableTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
                Pawn pawn = (Pawn)pawnField?.GetValue(__instance);
                if (pawn.CurJob == null)
                    return;
                if (__result == TimeAssignmentDefOf.Work)
                {
                    if (pawn != null && pawn.IsColonistPlayerControlled && pawn.CurJob.playerForced)
                    {
                        JobDef jobDef = pawn.CurJob?.def;
                        if (jobDef == JobDefOf.Lessontaking ||
                            jobDef == DefDatabase<JobDef>.GetNamed("Workwatching") ||
                            jobDef == DefDatabase<JobDef>.GetNamed("Floordrawing") ||
                            jobDef == DefDatabase<JobDef>.GetNamed("Skydreaming") ||
                            jobDef == DefDatabase<JobDef>.GetNamed("Radiotalking") ||
                            jobDef == DefDatabase<JobDef>.GetNamed("NatureRunning"))
                        {
                            __result = TimeAssignmentDefOf.Joy;
                        }
                    }
                }
            }
        }
    }
}