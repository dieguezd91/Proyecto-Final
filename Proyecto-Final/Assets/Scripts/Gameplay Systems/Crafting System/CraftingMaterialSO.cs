using UnityEngine;

public enum MaterialType
{
    None = 0,
    SpectralCrystal,       // CristalEspectral
    WindwalkerEssence,     // EsenciaVendaval
    VoltaicCore,           // NucleoVoltaico
    EternalEmber,          // BrasaEterna
    StellarFragment,       // FragmentoEstelar
    LunarEssence,          // EsenciaLunar
    CrystallizedTears,     // LagrimasCristalizadas
    FlameberryFruit,       // BayasFlamigeras
    AstralRoots,           // RaicesAstrales
    VoltaicPollen,         // PolenVoltaico
    FrostSpores,           // EsporasGelidas
    EtherealTendrils,       // ZarcillosEtereos
    HouseHealingPotion,
    BossHeart,
    Gold
}

public enum RarityEnum
{
    None,
    common,
    uncommon,
    rare
}

[CreateAssetMenu(fileName = "New Material Data", menuName = "Crafting/Material Data")]
public class CraftingMaterialSO : ScriptableObject
{
    public string materialName;
    public Sprite materialIcon;
    public MaterialType materialType;
    public string materialDescription;
    public RarityEnum rarity;
    public bool isDropped;
    public bool isProduct;
    public ElementEnum element;
}