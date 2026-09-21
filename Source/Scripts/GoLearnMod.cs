using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace KT_Go_Learn
{
    public class GoLearnMod : Mod
    {
        private static GoLearnMod _instance;

        public static GoLearnMod Instance => _instance;

        private readonly GoLearn settings;

        public GoLearn Settings => settings;

        public GoLearnMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<GoLearn>();
            var harmony = new Harmony("KT.Go_Learn");
            harmony.PatchAll();
            _instance = this;
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            float learningCapInternal = settings.learningCap;
            learningCapInternal = listingStandard.SliderLabeled(
                "Learning threshold: " + learningCapInternal.ToString("P0"),
                learningCapInternal,
                0f,
                1f
            );
            listingStandard.CheckboxLabeled("Only allow desired learning type", ref settings.onlyAllowWantedLearning, "Only allow forcing of learning when the clicked action is the childs wanted learning desire");

            settings.learningCap = (float)Math.Round(learningCapInternal, 2);

            if (!Mathf.Approximately(learningCapInternal, settings.learningCap))
            {
                settings.learningCap = learningCapInternal;
            }

            listingStandard.End();
        }

        public override string SettingsCategory()
        {
            return "Go F****** Learn";
        }
    }

}
