using RimWorld;
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
                            else pawn.RemoveHediffs(null, Extension.hediffsWhileRaining);
                        }

                        if (Extension.waterSatisfiedByRain && pawn.Map.weatherManager.RainRate > Extension.minimumRainAmount) WaterStuff();
                        else if (EBSGUtilities.CheckNearbyWater(pawn, Extension.waterTilesNeeded, out int waterCount, Extension.maxWaterDistance)) WaterStuff();
                        else NoWaterStuff();
                    }
                    else
                    {
                        pawn.RemoveHediffs(null, Extension.hediffsWhileInWater);
                        pawn.RemoveHediffs(null, Extension.hediffsWhileOutOfWater);
                    }
                }
                else if (pawn.GetCaravan() != null)
                {
                    Caravan caravan = pawn.GetCaravan();
                    if (caravan.Biome != null) CheckBiome(caravan.Biome);
                    pawn.RemoveHediffs(null, Extension.hediffsWhileInWater);
                    pawn.RemoveHediffs(null, Extension.hediffsWhileOutOfWater);
                }
                else
                {
                    NeutralBiome();
                    pawn.RemoveHediffs(null, Extension.hediffsWhileInWater);
                    pawn.RemoveHediffs(null, Extension.hediffsWhileOutOfWater);
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
            pawn.RemoveHediffs(null, Extension.hediffsWhileInGreatBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInGoodBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInBadBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInTerribleBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInAbysmalBiome);

            pawn.ApplyHediffs(null, Extension.hediffsWhileInAmazingBiome);
            pawn.HandleNeedOffsets(Extension.needOffsetsPerHourInAmazingBiome, true, 100, true);
        }

        public void GreatBiome()
        {
            pawn.RemoveHediffs(null, Extension.hediffsWhileInAmazingBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInGoodBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInBadBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInTerribleBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInAbysmalBiome);

            pawn.ApplyHediffs(null, Extension.hediffsWhileInGreatBiome);
            pawn.HandleNeedOffsets(Extension.needOffsetsPerHourInGreatBiome, true, 100, true);
        }

        public void GoodBiome()
        {
            pawn.RemoveHediffs(null, Extension.hediffsWhileInAmazingBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInGreatBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInBadBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInTerribleBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInAbysmalBiome);

            pawn.ApplyHediffs(null, Extension.hediffsWhileInGoodBiome);
            pawn.HandleNeedOffsets(Extension.needOffsetsPerHourInGoodBiome, true, 100, true);
        }

        public void BadBiome()
        {
            pawn.RemoveHediffs(null, Extension.hediffsWhileInAmazingBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInGreatBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInGoodBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInTerribleBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInAbysmalBiome);

            pawn.ApplyHediffs(null, Extension.hediffsWhileInBadBiome);
            pawn.HandleNeedOffsets(Extension.needOffsetsPerHourInBadBiome, true, 100, true);
        }

        public void TerribleBiome()
        {
            pawn.RemoveHediffs(null, Extension.hediffsWhileInAmazingBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInGreatBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInGoodBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInBadBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInAbysmalBiome);

            pawn.ApplyHediffs(null, Extension.hediffsWhileInTerribleBiome);
            pawn.HandleNeedOffsets(Extension.needOffsetsPerHourInTerribleBiome, true, 100, true);
        }

        public void AbysmalBiome()
        {
            pawn.RemoveHediffs(null, Extension.hediffsWhileInAmazingBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInGreatBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInGoodBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInBadBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInTerribleBiome);

            pawn.ApplyHediffs(null, Extension.hediffsWhileInAbysmalBiome);
            pawn.HandleNeedOffsets(Extension.needOffsetsPerHourInAbysmalBiome, true, 100, true);
        }

        public void NeutralBiome()
        {
            pawn.RemoveHediffs(null, Extension.hediffsWhileInAmazingBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInGreatBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInGoodBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInBadBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInTerribleBiome);
            pawn.RemoveHediffs(null, Extension.hediffsWhileInAbysmalBiome);
        }

        public void WaterStuff()
        {
            pawn.RemoveHediffs(null, Extension.hediffsWhileOutOfWater);
            pawn.ApplyHediffs(null, Extension.hediffsWhileInWater);
            pawn.HandleNeedOffsets(Extension.needOffsetsPerHourInWater, true, 100, true);
        }

        public void NoWaterStuff()
        {
            pawn.RemoveHediffs(null, Extension.hediffsWhileInWater);
            pawn.ApplyHediffs(null, Extension.hediffsWhileOutOfWater);
            pawn.HandleNeedOffsets(Extension.needOffsetsPerHourNotInWater, true, 100, true);
        }

        public void RainingStuff()
        {
            pawn.ApplyHediffs(null, Extension.hediffsWhileRaining);
            pawn.HandleNeedOffsets(Extension.needOffsetsPerHourWhileRaining, true, 100, true);
        }
    }
}
