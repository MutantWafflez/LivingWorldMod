using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

/// <summary>
///     Module for Town NPCs that deal with drawing related tasks.
/// </summary>
public sealed class TownNPCSpriteModule (NPC npc, TownGlobalNPC globalNPC, Texture2D blinkTexture) : TownNPCModule(npc, globalNPC) {
    private const int GivingAnimationDuration = (int)(LWMUtils.RealLifeSecond * 1.5f);
    private const int EyelidClosedDuration = 15;

    private readonly HashSet<Texture2D> _drawSet = [];

    private int _blinkTimer;

    private int _givingTimer;
    private int _givingItemType;

    public bool IsBlinking {
        get;
        private set;
    }

    public bool IsGiving {
        get;
        private set;
    }

    public override void Update() {
        _drawSet.Clear();

        if (Main.netMode != NetmodeID.Server) {
            UpdateHead();
        }
    }

    public void RequestOverlay(Texture2D request) {
        if (request is null || Main.netMode == NetmodeID.Server) {
            return;
        }

        _drawSet.Add(request);
    }

    public void RequestGiving(int givingItemType = -1) {
        IsGiving = true;
        _givingTimer = GivingAnimationDuration;
        _givingItemType = givingItemType;
    }

    public void RequestBlink(int duration = EyelidClosedDuration) {
        IsBlinking = true;
        _blinkTimer = duration;
    }

    public void DrawNPCOverlays(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Asset<Texture2D> npcAsset = TownNPCProfiles.Instance.GetProfile(npc, out ITownNPCProfile profile) ? profile.GetTextureNPCShouldUse(npc) : TextureAssets.Npc[npc.type];

        int frameWidth = npcAsset.Width();
        int frameHeight = npcAsset.Height() / Main.npcFrameCount[npc.type];

        Vector2 halfSize = new(frameWidth / 2, frameHeight / 2);
        Vector2 drawPos = new(
            npc.position.X - screenPos.X + npc.width / 2 - frameWidth * npc.scale / 2f + halfSize.X * npc.scale,
            npc.position.Y - screenPos.Y + npc.height - frameHeight * npc.scale + 4f + halfSize.Y * npc.scale + Main.NPCAddHeight(npc) /*+ num35*/ + npc.gfxOffY
        );

        drawColor = npc.GetNPCColorTintedByBuffs(drawColor);
        foreach (Texture2D texture in _drawSet) {
            spriteBatch.Draw(
                texture,
                drawPos,
                null,
                npc.color == default(Color) ? npc.GetAlpha(drawColor) : npc.GetColor(drawColor),
                npc.rotation,
                halfSize,
                npc.scale,
                npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0
            );
        }
    }

    public void FrameNPC(int frameHeight) {
        if (!IsGiving) {
            return;
        }

        if (--_givingTimer <= 0) {
            IsGiving = false;
            _givingTimer = 0;
            _givingItemType = -1;
            return;
        }

        int nonAttackFrameCount = Main.npcFrameCount[npc.type] - NPCID.Sets.AttackFrameCount[npc.type];

        const int animationHalf = GivingAnimationDuration / 2;
        int frame = (_givingTimer < animationHalf ? _givingTimer : animationHalf - _givingTimer % animationHalf) switch {
            >= 10 and < 16 => nonAttackFrameCount - 5,
            >= 16 => nonAttackFrameCount - 4,
            _ => 0
        };

        npc.frame.Y = frameHeight * frame;
    }

    private void UpdateHead() {
        if (!IsBlinking) {
            if (--_blinkTimer > 0) {
                return;
            }

            RequestBlink();
        }
        else if (--_blinkTimer <= 0) {
            IsBlinking = false;
            _blinkTimer = Main.rand.Next(180, 360);
        }

        if (IsBlinking) {
            RequestOverlay(blinkTexture);
        }
    }
}