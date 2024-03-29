﻿using RimWorld;
using Verse;
using RimWorld.Planet;

namespace EBSGFramework
{
    public class BiomeGene : HediffAdder
    {
        public override void PostAdd()
        {
            base.PostAdd();
            if (Extension == null)
            {
                Log.Error(def + " needs the EBSG extension to properly function. Deleting gene to excess errors.");
                pawn.genes.RemoveGene(this);
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
        }

        public override void Tick()
        {
            if (pawn.IsHashIntervalTick(100) & !pawn.Dead)
            {
                bool biomeFlag = false;
                if (pawn.Map != null)
                {
                    if (pawn.Map.Biome != null)
                    {
                        biomeFlag = CheckBiome(pawn.Map.Biome);
                    }
                    if (!biomeFlag || !Extension.biomeOverridesWater)
                    {
                        if (!Extension.hediffsWhileRaining.NullOrEmpty() || !Extension.needOffsetsPerHourWhileRaining.NullOrEmpty())
                        {
                            if (pawn.Map.weatherManager.RainRate > Extension.minimumRainAmount) RainingStuff();
                            else EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileRaining);
                        }

                        if (Extension.waterSatisfiedByRain && pawn.Map.weatherManager.RainRate > Extension.minimumRainAmount) WaterStuff();
                        else if (EBSGUtilities.CheckNearbyWater(pawn, Extension.waterTilesNeeded, out int waterCount, Extension.maxWaterDistance)) WaterStuff();
                        else NoWaterStuff();
                    }
                    else
                    {
                        EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInWater);
                        EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileOutOfWater);
                    }
                }
                else if (pawn.GetCaravan() != null)
                {
                    Caravan caravan = pawn.GetCaravan();
                    if (caravan.Biome != null) CheckBiome(caravan.Biome);
                    EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInWater);
                    EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileOutOfWater);
                }
                else
                {
                    NeutralBiome();
                    EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInWater);
                    EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileOutOfWater);
                }
            }
        }

        public bool CheckBiome(BiomeDef biome)
        {
            if (!Extension.abysmalBiomes.NullOrEmpty() && Extension.abysmalBiomes.Contains(biome))
            {
                AbysmalBiome();
                return true;
            }
            if (!Extension.terribleBiomes.NullOrEmpty() && Extension.terribleBiomes.Contains(biome))
            {
                TerribleBiome();
                return true;
            }
            if (!Extension.badBiomes.NullOrEmpty() && Extension.badBiomes.Contains(biome))
            {
                BadBiome();
                return true;
            }
            if (!Extension.goodBiomes.NullOrEmpty() && Extension.goodBiomes.Contains(pawn.Map.Biome))
            {
                GoodBiome();
                return true;
            }
            if (!Extension.greatBiomes.NullOrEmpty() && Extension.greatBiomes.Contains(biome))
            {
                GreatBiome();
                return true;
            }
            if (!Extension.amazingBiomes.NullOrEmpty() && Extension.amazingBiomes.Contains(biome))
            {
                AmazingBiome();
                return true;
            }
            NeutralBiome();
            return false;
        }

        public void AmazingBiome()
        {
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInGreatBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInGoodBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInBadBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInTerribleBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInAbysmalBiome);

            EBSGUtilities.ApplyHediffs(pawn, null, Extension.hediffsWhileInAmazingBiome);
            EBSGUtilities.HandleNeedOffsets(pawn, Extension.needOffsetsPerHourInAmazingBiome, true, 100, true);
        }

        public void GreatBiome()
        {
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInAmazingBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInGoodBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInBadBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInTerribleBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInAbysmalBiome);

            EBSGUtilities.ApplyHediffs(pawn, null, Extension.hediffsWhileInGreatBiome);
            EBSGUtilities.HandleNeedOffsets(pawn, Extension.needOffsetsPerHourInGreatBiome, true, 100, true);
        }

        public void GoodBiome()
        {
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInAmazingBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInGreatBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInBadBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInTerribleBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInAbysmalBiome);

            EBSGUtilities.ApplyHediffs(pawn, null, Extension.hediffsWhileInGoodBiome);
            EBSGUtilities.HandleNeedOffsets(pawn, Extension.needOffsetsPerHourInGoodBiome, true, 100, true);
        }

        public void BadBiome()
        {
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInAmazingBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInGreatBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInGoodBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInTerribleBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInAbysmalBiome);

            EBSGUtilities.ApplyHediffs(pawn, null, Extension.hediffsWhileInBadBiome);
            EBSGUtilities.HandleNeedOffsets(pawn, Extension.needOffsetsPerHourInBadBiome, true, 100, true);
        }

        public void TerribleBiome()
        {
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInAmazingBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInGreatBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInGoodBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInBadBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInAbysmalBiome);

            EBSGUtilities.ApplyHediffs(pawn, null, Extension.hediffsWhileInTerribleBiome);
            EBSGUtilities.HandleNeedOffsets(pawn, Extension.needOffsetsPerHourInTerribleBiome, true, 100, true);
        }

        public void AbysmalBiome()
        {
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInAmazingBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInGreatBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInGoodBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInBadBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInTerribleBiome);

            EBSGUtilities.ApplyHediffs(pawn, null, Extension.hediffsWhileInAbysmalBiome);
            EBSGUtilities.HandleNeedOffsets(pawn, Extension.needOffsetsPerHourInAbysmalBiome, true, 100, true);
        }

        public void NeutralBiome()
        {
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInAmazingBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInGreatBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInGoodBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInBadBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInTerribleBiome);
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInAbysmalBiome);
        }

        public void WaterStuff()
        {
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileOutOfWater);
            EBSGUtilities.ApplyHediffs(pawn, null, Extension.hediffsWhileInWater);
            EBSGUtilities.HandleNeedOffsets(pawn, Extension.needOffsetsPerHourInWater, true, 100, true);
        }

        public void NoWaterStuff()
        {
            EBSGUtilities.RemoveHediffs(pawn, null, Extension.hediffsWhileInWater);
            EBSGUtilities.ApplyHediffs(pawn, null, Extension.hediffsWhileOutOfWater);
            EBSGUtilities.HandleNeedOffsets(pawn, Extension.needOffsetsPerHourNotInWater, true, 100, true);
        }

        public void RainingStuff()
        {
            EBSGUtilities.ApplyHediffs(pawn, null, Extension.hediffsWhileRaining);
            EBSGUtilities.HandleNeedOffsets(pawn, Extension.needOffsetsPerHourWhileRaining, true, 100, true);
        }
    }
}
