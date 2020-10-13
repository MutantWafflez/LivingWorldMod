//Taken from https://github.com/tModLoader/tModLoader/blob/master/ExampleMod/UI/VanillaItemSlotWrapper.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace LivingWorldMod.UI.Elements
{
	class VanillaItemSlotWrapper : UIElement
	{
		internal Item Item;
		private readonly int _context;
		private readonly float _scale;
		internal Func<Item, bool> ValidItemFunc;

		public VanillaItemSlotWrapper(int context = ItemSlot.Context.ChestItem, float scale = 1f)
		{
			_context = context;
			_scale = scale;
			Item = new Item();
			Item.SetDefaults(0);

			Width.Set(Main.inventoryBack9Texture.Width * scale, 0f);
			Height.Set(Main.inventoryBack9Texture.Height * scale, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (!HarpyShrineUIState.showUI) return;

			float oldScale = Main.inventoryScale;
			Main.inventoryScale = _scale;
			Vector2 position = GetDimensions().ToRectangle().TopLeft();

			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface)
			{
				Main.LocalPlayer.mouseInterface = true;
				if (ValidItemFunc == null || ValidItemFunc(Main.mouseItem))
				{
					// Handle handles all the click and hover actions based on the context.
					ItemSlot.Handle(ref Item, _context);
				}
			}
			// Draw draws the slot itself and Item. Depending on context, the color will change, as will drawing other things like stack counts.
			ItemSlot.Draw(spriteBatch, ref Item, _context, position);
			Main.inventoryScale = oldScale;
		}
	}
}