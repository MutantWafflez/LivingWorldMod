using LivingWorldMod.Content.Villages.Globals.BaseTypes.NPCs;
using LivingWorldMod.DataStructures.Classes;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.UI;

namespace LivingWorldMod.Content.Villages.UI.VillagerHousing;

/// <summary>
/// Element that shows a specific villager in the housing menu. An instance of this is created
/// per villager that exists in the world of a given type when the player is in the housing menu.
/// </summary>
public class UIHousingVillagerDisplay : UIElement {
    /// <summary>
    /// Whether or not this villager is currently selected.
    /// </summary>
    public bool IsSelected => myVillager.NPC.whoAmI == Main.instance.mouseNPCIndex;

    /// <summary>
    /// Whether or not this NPC is "allowed" to be housed, which is to say whether or not the
    /// village that the villager belongs to likes the player.
    /// </summary>
    //TODO: Swap back to commented expression when Reputation system is re-implemented
    public bool IsAllowed => true; //myVillager.RelationshipStatus >= VillagerRelationship.Like;

    /// <summary>
    /// The villager instance that this element is displaying.
    /// </summary>
    public Villager myVillager;

    public UIHousingVillagerDisplay(Villager villager) {
        myVillager = villager;

        // Very arbitrary number, I know, but this is about the smallest we can get the
        // width/height while keeping 3 objects in a row in the grid
        Width.Set(50.284f, 0f);
        Height.Set(50.284f, 0f);
    }

    public override void LeftClick(UIMouseEvent evt) {
        //Prevent any interaction if the villagers do not like the player
        if (!IsAllowed) {
            return;
        }

        //Change the mouse type properly
        if (IsSelected) {
            Main.instance.SetMouseNPC(-1, -1);
        }
        else {
            //Our IL edit in NPCHousingPatches.cs handles the drawing here.
            Main.instance.SetMouseNPC(myVillager.NPC.whoAmI, myVillager.Type);
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
    }

    public override void Update(GameTime gameTime) {
        if (!ContainsPoint(Main.MouseScreen) || myVillager is null) {
            return;
        }

        Main.LocalPlayer.mouseInterface = true;
        Main.instance.MouseText(IsAllowed ? myVillager.NPC.GivenName : "UI.VillagerHousing.VillagerTypeLocked".Localized(myVillager.VillagerType.ToString()).Value);
    }

    protected override void DrawChildren(SpriteBatch spriteBatch) {
        base.DrawChildren(spriteBatch);

        spriteBatch.Draw(TextureAssets.InventoryBack11.Value,
            GetDimensions().ToRectangle(),
            null,
            !IsAllowed ? Color.Gray : Color.White,
            0f,
            default(Vector2),
            SpriteEffects.None,
            0f);


        LayeredDrawObject drawObject = myVillager.drawObject;
        Rectangle textureDrawRegion = new(0, 0, drawObject.GetLayerFrameWidth(), drawObject.GetLayerFrameHeight(0, 0, Main.npcFrameCount[myVillager.Type]));

        drawObject.Draw(
            spriteBatch,
            new DrawData(
                null,
                GetDimensions().Center(),
                textureDrawRegion,
                IsSelected ? Color.Yellow : Color.White,
                0f,
                new Vector2(textureDrawRegion.Width / 2f, textureDrawRegion.Height / 2f * 1.15f),
                0.75f,
                SpriteEffects.None
            ),
            myVillager.DrawIndices
        );

        //Draw lock icon if not "allowed" (player is not high enough rep)
        if (!IsAllowed) {
            int lockSize = 22;

            spriteBatch.Draw(TextureAssets.HbLock[0].Value,
                GetDimensions().Center(),
                new Rectangle(0, 0, lockSize, lockSize),
                Color.White,
                0f,
                new Vector2(lockSize / 2f, lockSize / 2f),
                1.25f,
                SpriteEffects.None,
                0f);
        }
    }
}