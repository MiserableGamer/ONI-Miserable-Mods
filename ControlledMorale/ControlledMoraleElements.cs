namespace ControlledMorale
{
    public static class ControlledMoraleElements
    {
        public const string BeerElementId = "ControlledMoraleBeer";
        public const string WineElementId = "ControlledMoraleWine";
        public const string SpiritsElementId = "ControlledMoraleSpirits";

        public static readonly SimHashes BeerHash = (SimHashes)global::Hash.SDBMLower(BeerElementId);
        public static readonly SimHashes WineHash = (SimHashes)global::Hash.SDBMLower(WineElementId);
        public static readonly SimHashes SpiritsHash = (SimHashes)global::Hash.SDBMLower(SpiritsElementId);
    }
}
