﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP;
using UnityEngine;

namespace RP0
{
    public abstract class ModuleTooling : PartModule, IPartCostModifier
    {
        [KSPField]
        public string toolingType = "TankStarting";

        [KSPField]
        public string toolingName = "Tool Tank";

        [KSPField]
        public float untooledMultiplier = 10f;

        [KSPField]
        public float finalToolingCostMultiplier = 1f;

        [KSPField]
        // d^2, d^1, 1
        public Vector3 diameterToolingCost = new Vector3(6000f, 12000f, 500f);

        [KSPField]
        // d^2, d^1, l^1, 1
        public Vector4 lengthToolingCost = new Vector4(500f, 2000f, 200f, 100f);

        protected BaseEvent tEvent;

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Tool Tank")]
        public virtual void ToolingEvent()
        {
            if (IsUnlocked())
                return;

            float toolingCost = GetToolingCost();
            bool canAfford = true;
            if (!HighLogic.CurrentGame.Parameters.Difficulty.BypassEntryPurchaseAfterResearch)
            {
                if (Funding.Instance.Funds < toolingCost)
                    canAfford = false;
            }
            else
                toolingCost = 0f;

            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new MultiOptionDialog(
                            "Tooling has not yet been set up for this part. It will cost " + toolingCost.ToString("N0") + " funds.",
                            "ModuleManager",
                            HighLogic.UISkin,
                            new Rect(0.5f, 0.5f, 150f, 60f),
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIVerticalLayout(
                                new DialogGUIFlexibleSpace(),
                                new DialogGUIButton(canAfford ? "Purchase Tooling" : "Can't Afford",
                                    () =>
                                    {
                                        if (canAfford)
                                        {
                                            Funding.Instance.AddFunds(-toolingCost, TransactionReasons.RnDPartPurchase);
                                            PurchaseTooling();
                                            Events["ToolingEvent"].guiActiveEditor = false;
                                        }
                                    }, 140.0f, 30.0f, true),
                                new DialogGUIButton("Close", () => { }, 140.0f, 30.0f, true)
                                )),
                        false,
                        HighLogic.UISkin);
        }

        public abstract float GetToolingCost();

        public abstract void PurchaseTooling();

        public abstract bool IsUnlocked();

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            tEvent = Events["ToolingEvent"];

            tEvent.guiActiveEditor = IsUnlocked();
            tEvent.guiName = toolingName;
        }

        public virtual float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            if (!HighLogic.LoadedSceneIsEditor)
                return 0f;

            if (IsUnlocked())
            {
                tEvent.guiActiveEditor = false;
                return 0f;
            }
            tEvent.guiActiveEditor = true;

            /*float cost = part.partInfo.cost;
            float baseCost = cost;

            for (int i = part.Modules.Count; i-- > 0;)
            {
                PartModule m = part.Modules[i];
                if (m is ModuleTooling)
                    continue;

                IPartCostModifier c = m as IPartCostModifier;
                if (c == null)
                    continue;

                cost += c.GetModuleCost(baseCost, ModifierStagingSituation.CURRENT);
            }

            return cost * untooledMultiplier;*/

            return GetToolingCost() * ((1f / 50f) * 0.25f) * untooledMultiplier;
        }

        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }
    }
}
