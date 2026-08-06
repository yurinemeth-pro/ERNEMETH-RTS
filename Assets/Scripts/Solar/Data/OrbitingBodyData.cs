public abstract class OrbitingBodyData : CelestialBodyData
{
    public float orbitalRadiusAU = 1f; // distância até o CORPO QUE ELE ORBITA (não necessariamente o Sol)
    public float orbitalPeriodDays = 365f;
    public float startingAngleDegrees = 0f;
}