using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace LivingWorldMod.UI
{
    internal class HarpyShrineItemSlot : UIElement
    {
		internal Item Item;
		private readonly int _context;
		private readonly float _scale;
		internal Func<Item, bool> ValidItemFunc;

		public HarpyShrineItemSlot(int context = ItemSlot.Context.BankItem, float scale = 1f)
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
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = _scale;
			Rectangle rectangle = GetDimensions().ToRectangle();

			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface)
			{
				Main.LocalPlayer.mouseInterface = true;
				if (ValidItemFunc == null || ValidItemFunc(Main.mouseItem))
				{
					ItemSlot.Handle(ref Item, _context);
				}
			}
			ItemSlot.Draw(spriteBatch, ref Item, _context, rectangle.TopLeft());
			Main.inventoryScale = oldScale;
		}
	}
}
