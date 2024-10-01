using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
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
public sealed class TownNPCSpriteModule (NPC npc, TownGlobalNPC globalNPC) : TownNPCModule(npc, globalNPC) {
    private const int GivingAnimationDuration = (int)(LWMUtils.RealLifeSecond * 1.5f);
    private const int EyelidClosedDuration = 15;
    private const int TalkDuration = 8;

    private const int TalkTextureIndex = 0;
    private const int EyelidTextureIndex = 1;

    public static IReadOnlyDictionary<int, TownNPCSpriteOverlayProfile> overlayProfiles;

    private readonly HashSet<int> _drawSet = [];

    private int _blinkTimer;
    private int _mouthOpenTimer;

    private int _givingTimer;
    private int _givingItemType;

    public bool AreEyesClosed {
        get;
        private set;
    }

    public bool IsGiving {
        get;
        private set;
    }

    public bool IsTalking {
        get;
        private set;
    }

    public override void Update() {
        _drawSet.Clear();

        if (Main.netMode != NetmodeID.Server) {
            UpdateHead();
        }
    }

    public void GiveItem(int givingItemType = -1) {
        IsGiving = true;
        _givingTimer = GivingAnimationDuration;
        _givingItemType = givingItemType;
    }

    public void CloseEyes(int duration = EyelidClosedDuration) {
        AreEyesClosed = true;
        _blinkTimer = duration;
    }

    public void DoTalk(int duration = TalkDuration) {
        IsTalking = true;
        _mouthOpenTimer = duration;
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
        foreach (int textureIndex in _drawSet) {
            TownNPCSpriteOverlay currentOverlay = overlayProfiles[npc.type].GetCurrentSpriteOverlay(npc, textureIndex);
            spriteBatch.Draw(
                currentOverlay.Texture,
                drawPos + currentOverlay.PositionInFrame.ToVector2(),
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
        if (!AreEyesClosed) {
            if (--_blinkTimer <= 0) {
                CloseEyes();
            }
        }
        else if (--_blinkTimer <= 0) {
            AreEyesClosed = false;
            _blinkTimer = Main.rand.Next(180, 360);
        }

        if (IsTalking && --_mouthOpenTimer <= 0) {
            IsTalking = false;
            _mouthOpenTimer = 0;
        }

        if (AreEyesClosed) {
            _drawSet.Add(EyelidTextureIndex);
        }

        if (IsTalking) {
            _drawSet.Add(TalkTextureIndex);
        }
    }
}