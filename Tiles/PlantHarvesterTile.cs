using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace PlantHarvester.Tiles
{
    public enum PlantStage: byte
    {
		Planted,
		Growing,
		Grown
    }

    class PlantHarvesterTile : ModTile
    {
		public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileContainer[Type] = true;
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1200;
            Main.tileValue[Type] = 500;

            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
            TileObjectData.newTile.HookCheck = new PlacementHook(new Func<int, int, int, int, int, int>(Chest.FindEmptyChest), -1, 0, true);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(new Func<int, int, int, int, int, int>(Chest.AfterPlacement_Hook), -1, 0, false);
            TileObjectData.newTile.AnchorInvalidTiles = new[] { 127 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);

			ModTranslation name = CreateMapEntryName();
            name.SetDefault("Plant Harvester Chest");
            AddMapEntry(new Color(200, 200, 200), name, MapChestName);

            name = CreateMapEntryName(Name + "_Locked"); // With multiple map entries, you need unique translation keys.
            name.SetDefault("Locked Example Chest");
            AddMapEntry(new Color(0, 141, 63), name, MapChestName);
            disableSmartCursor = true;
            adjTiles = new int[] { TileID.PlanterBox };
            chest = "Example Chest";
            chestDrop = ItemType<Items.Placeable.PlantHarvesterPlaceable>();
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(i * 16, j * 16, 32, 32, ItemType<Items.Placeable.PlantHarvesterPlaceable>());
			Chest.DestroyChest(i, j);
		}

		public override void RandomUpdate(int i, int j)
		{
			if (GetStage(i, j) == PlantStage.Grown)
            {
                int chestIndex = GetChestIndex(i, j);
                Chest chest = Main.chest[chestIndex];

				AddItemToChest(chest, ItemID.Daybloom);
				ChangeGrowthStage(i, j, -2);
            }
            else
            {
				ChangeGrowthStage(i, j, 1);
			}
		}

		private void AddItemToChest(Chest chest, short itemID)
        {
			if (chest.item.Any(x => x.netID == itemID && x.stack < x.maxStack))
			{
				chest.item.Where(x => x.netID == itemID && x.stack < x.maxStack)
							.FirstOrDefault()
							.stack++;
			}
			else if (chest.item.Any(x => x.type == ItemID.None))
			{
				chest.item.Where(x => x.type == ItemID.None)
							.FirstOrDefault()
							.SetDefaults(itemID);
			}
		}

		private PlantStage GetStage(int i, int j)
        {
			return (PlantStage)Math.Floor((decimal)((Main.tile[i, j].frameX / 18) / 2));
		}

        private static int GetChestIndex(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            int left = i;
            int top = j;
            if (tile.frameX % 36 != 0)
            {
                left--;
            }
            if (tile.frameY != 0)
            {
                top--;
            }
            int chestIndex = Chest.FindChest(left, top);
            return chestIndex;
        }

        private IEnumerable<Tile> GetRelatedTiles(int i, int j)
        {
			var relatedTiles = new List<Tile>();
			Tile curTile = Main.tile[i, j];
			Tile leftTile = Main.tile[i - 1, j];
			int topY = j - curTile.frameY / 18 % 2;


			bool isRightSideFrame = leftTile.type == curTile.type
									&& (leftTile.frameX / 18) == ((curTile.frameX / 18) - 1)
									&& (leftTile.frameX / 18) % 2 == 0;

			relatedTiles.Add(Main.tile[i, topY]);
			relatedTiles.Add(Main.tile[i, topY + 1]);

			if (isRightSideFrame)
            {
				relatedTiles.Add(Main.tile[i - 1, topY]);
				relatedTiles.Add(Main.tile[i - 1, topY + 1]);
			}
			else
            {
				relatedTiles.Add(Main.tile[i + 1, topY]);
				relatedTiles.Add(Main.tile[i + 1, topY + 1]);
			}

			return relatedTiles;
        }

		private void ChangeGrowthStage(int i, int j, short stages)
        {
			short frameAdjustment = 36;
			var tiles = GetRelatedTiles(i, j);
			foreach (Tile tile in tiles)
			{
				tile.frameX += (short)(frameAdjustment * stages);
			}
		}

        public override ushort GetMapOption(int i, int j) => (ushort)(Main.tile[i, j].frameX / 54);

        public override bool HasSmartInteract() => true;

        public override bool IsLockedChest(int i, int j) => Main.tile[i, j].frameX / 36 == 1;

        public override bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual)
        {
            if (Main.dayTime)
                return false;
            dustType = this.dustType;
            return true;
        }

        public string MapChestName(string name, int i, int j)
        {
            int left = i;
            int top = j;
            Tile tile = Main.tile[i, j];
            if (tile.frameX % 36 != 0)
            {
                left--;
            }
            if (tile.frameY != 0)
            {
                top--;
            }
            int chest = Chest.FindChest(left, top);

            if (chest < 0)
            {
                return Language.GetTextValue("LegacyChestType.0");
            }
            else if (Main.chest[chest].name == "")
            {
                return name;
            }
            else
            {
                return name + ": " + Main.chest[chest].name;
            }
        }

		public override bool NewRightClick(int i, int j)
		{
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			Main.mouseRightRelease = false;
			int left = i;
			int top = j;
			if (tile.frameX % 36 != 0)
			{
				left--;
			}
			if (tile.frameY != 0)
			{
				top--;
			}
			if (player.sign >= 0)
			{
				Main.PlaySound(SoundID.MenuClose);
				player.sign = -1;
				Main.editSign = false;
				Main.npcChatText = "";
			}
			if (Main.editChest)
			{
				Main.PlaySound(SoundID.MenuTick);
				Main.editChest = false;
				Main.npcChatText = "";
			}
			if (player.editedChestName)
			{
				NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f, 0f, 0f, 0, 0, 0);
				player.editedChestName = false;
			}
			bool isLocked = IsLockedChest(left, top);
			if (Main.netMode == NetmodeID.MultiplayerClient && !isLocked)
			{
				if (left == player.chestX && top == player.chestY && player.chest >= 0)
				{
					player.chest = -1;
					Recipe.FindRecipes();
					Main.PlaySound(SoundID.MenuClose);
				}
				else
				{
					NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, (float)top, 0f, 0f, 0, 0, 0);
					Main.stackSplit = 600;
				}
			}
			else
			{
				int chest = Chest.FindChest(left, top);
				if (chest >= 0)
				{
					Main.stackSplit = 600;
					if (chest == player.chest)
					{
						player.chest = -1;
						Main.PlaySound(SoundID.MenuClose);
					}
					else
					{
						player.chest = chest;
						Main.playerInventory = true;
						Main.recBigList = false;
						player.chestX = left;
						player.chestY = top;
						Main.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
					}
					Recipe.FindRecipes();
				}
			}
			return true;
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			int left = i;
			int top = j;
			if (tile.frameX % 36 != 0)
			{
				left--;
			}
			if (tile.frameY != 0)
			{
				top--;
			}
			int chest = Chest.FindChest(left, top);
			player.showItemIcon2 = -1;
			if (chest < 0)
			{
				player.showItemIconText = Language.GetTextValue("LegacyChestType.0");
			}
			else
			{
				player.showItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : "Example Chest";
				if (player.showItemIconText == "Example Chest")
				{
					player.showItemIcon2 = ItemType<Items.Placeable.PlantHarvesterPlaceable>();
					player.showItemIconText = "";
				}
			}
			player.noThrow = 2;
			player.showItemIcon = true;
		}

		public override void MouseOverFar(int i, int j)
		{
			MouseOver(i, j);
			Player player = Main.LocalPlayer;
			if (player.showItemIconText == "")
			{
				player.showItemIcon = false;
				player.showItemIcon2 = 0;
			}
		}
	}
}
