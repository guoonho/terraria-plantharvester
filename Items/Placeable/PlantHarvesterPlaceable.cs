using PlantHarvester.Tiles;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace PlantHarvester.Items.Placeable
{
    class PlantHarvesterPlaceable : ModItem
    {
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("A plant box that auto-harvests crops into a chest linked by wire.");
		}

		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.DayBloomPlanterBox);
			item.createTile = TileType<PlantHarvesterTile>();
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);

			recipe.AddIngredient(ItemID.DirtBlock, 1);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
