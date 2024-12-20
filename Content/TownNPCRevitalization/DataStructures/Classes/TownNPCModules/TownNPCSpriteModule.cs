﻿using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes.TownNPCModules;

/// <summary>
///     Module for Town NPCs that deal with drawing related tasks.
/// </summary>
public sealed class TownNPCSpriteModule (NPC npc, TownGlobalNPC globalNPC) : TownNPCModule(npc, globalNPC) {
    /// <summary>
    ///     Small helper record that holds data on some helpful parameters for usage with drawing Town NPCs.
    /// </summary>
    private record TownNPCDrawParameters(Asset<Texture2D> NPCAsset, int FrameWidth, int FrameHeight, Vector2 HalfSize, float NPCAddHeight, SpriteEffects SpriteEffects);

    private const int GivingAnimationDuration = (int)(LWMUtils.RealLifeSecond * 1.5f);
    private const int EyelidClosedDuration = 15;
    private const int TalkDuration = 8;

    private const int TalkTextureIndex = 0;
    private const int EyelidTextureIndex = 1;

    private readonly List<TownNPCDrawRequest> _drawRequests = [];

    private int _blinkTimer;
    private int _mouthOpenTimer;

    private int _givingTimer;
    private int _givingItemType;

    private int _frameYOverride;

    private Vector2 _drawOffset;

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
        if (Main.netMode == NetmodeID.Server) {
            return;
        }

        _drawOffset = Vector2.Zero;
        _frameYOverride = -1;
        _drawRequests.Clear();

        (Asset<Texture2D> npcAsset, int _, int _, Vector2 halfSize, float npcAddHeight, SpriteEffects spriteEffects) = GetDrawParameters();

        // Method Gaslighting
        // See NPCDrawPatches.cs: TL;DR is the method is patched so that all sprite-batch calls are re-routed back to here (the sprite module) and we control the drawing
        for (int i = 0; i < 2; i++) {
            Main.instance.DrawNPCExtras(npc, i == 0, npcAddHeight, 0, Color.White, halfSize, spriteEffects, Vector2.Zero);
        }

        // This is the request to actually draw the NPC itself
        RequestDraw(new TownNPCDrawRequest(npcAsset.Value, Vector2.Zero, npc.frame));

        UpdateFlavorAnimations();
        if (npc.type == NPCID.Mechanic) {
            DrawMechanicWrench();
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

    /// <summary>
    ///     Requests a Y Frame override for the NPC. The request is only accepted if there is no override already occuring.
    /// </summary>
    public void RequestFrameOverride(uint newFrameY) {
        if (_frameYOverride == -1) {
            _frameYOverride = (int)newFrameY;
        }
    }

    /// <summary>
    ///     Adds a draw request for drawing over the Town NPC. Note that the position given in the request will be drawn relative to the draw position of the NPC.
    /// </summary>
    public void RequestDraw(in TownNPCDrawRequest request) {
        _drawRequests.Add(request);
        _drawRequests.Sort();
    }

    /// <summary>
    ///     Offsets the base draw position of the NPC, and any subsequent layers/overlays.
    /// </summary>
    public void OffsetDrawPosition(Vector2 drawOffset) {
        _drawOffset = drawOffset;
    }

    public void DrawNPC(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        (Asset<Texture2D> _, int frameWidth, int frameHeight, Vector2 halfSize, float npcAddHeight, SpriteEffects spriteEffects) = GetDrawParameters();
        Vector2 drawPos = new (
            npc.position.X + npc.width / 2 - frameWidth * npc.scale / 2f + halfSize.X * npc.scale + _drawOffset.X,
            npc.position.Y + npc.height - frameHeight * npc.scale + 4f + halfSize.Y * npc.scale + npcAddHeight /*+ num35*/ + npc.gfxOffY + _drawOffset.Y
        );

        drawColor = npc.GetNPCColorTintedByBuffs(drawColor);
        Color shiftedDrawColor = npc.color == default(Color) ? npc.GetAlpha(drawColor) : npc.GetColor(drawColor);
        DrawData defaultDrawData = new (
            null,
            drawPos,
            null,
            shiftedDrawColor,
            npc.rotation,
            halfSize,
            npc.scale,
            spriteEffects
        );

        foreach (TownNPCDrawRequest request in _drawRequests) {
            request.UnionWithDrawData(defaultDrawData, screenPos).Draw(spriteBatch);
        }
    }

    public void FrameNPC(int frameHeight) {
        if (_frameYOverride >= 0) {
            npc.frame.Y = frameHeight * _frameYOverride;
        }
    }

    private TownNPCDrawParameters GetDrawParameters() {
        Asset<Texture2D> npcAsset = TownNPCProfiles.Instance.GetProfile(npc, out ITownNPCProfile profile) ? profile.GetTextureNPCShouldUse(npc) : TextureAssets.Npc[npc.type];
        int frameWidth = npcAsset.Width();
        int frameHeight = npcAsset.Height() / Main.npcFrameCount[npc.type];
        Vector2 halfSize = new(frameWidth / 2, frameHeight / 2);
        float npcAddHeight = Main.NPCAddHeight(npc);
        SpriteEffects spriteEffects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        return new TownNPCDrawParameters(npcAsset, frameWidth, frameHeight, halfSize, npcAddHeight, spriteEffects);
    }

    private Texture2D GetOverlayTexture(int overlayIndex) => TownNPCDataSystem.spriteOverlayProfiles[npc.type].GetCurrentSpriteOverlay(npc, overlayIndex);

    private void UpdateFlavorAnimations() {
        if (!AreEyesClosed) {
            if (--_blinkTimer <= 0) {
                CloseEyes();
            }
        }
        else if (--_blinkTimer <= 0) {
            AreEyesClosed = false;
            _blinkTimer = Main.rand.Next(LWMUtils.RealLifeSecond * 3, LWMUtils.RealLifeSecond * 6);
        }

        if (IsTalking && --_mouthOpenTimer <= 0) {
            IsTalking = false;
            _mouthOpenTimer = 0;
        }

        if (AreEyesClosed) {
            RequestDraw(new TownNPCDrawRequest(GetOverlayTexture(EyelidTextureIndex)));
        }

        if (IsTalking) {
            RequestDraw(new TownNPCDrawRequest(GetOverlayTexture(TalkTextureIndex)));
        }

        if (!IsGiving) {
            return;
        }

        int nonAttackFrameCount = Main.npcFrameCount[npc.type] - NPCID.Sets.AttackFrameCount[npc.type];
        const int animationHalf = GivingAnimationDuration / 2;
        RequestFrameOverride(
            (uint)((_givingTimer < animationHalf ? _givingTimer : animationHalf - _givingTimer % animationHalf) switch {
                >= 10 and < 16 => nonAttackFrameCount - 5,
                >= 16 => nonAttackFrameCount - 4,
                _ => 0
            })
        );

        if (--_givingTimer > 0) {
            return;
        }

        IsGiving = false;
        _givingTimer = 0;
        _givingItemType = -1;
    }

    private void DrawMechanicWrench() {
        // Adapted vanilla code
        if (npc.localAI[0] != 0f) {
            return;
        }

        int offsetIndex = 0;
        if (npc.frame.Y > 56) {
            offsetIndex += 4;
        }

        offsetIndex += npc.frame.Y / 56;
        if (offsetIndex >= Main.OffsetsPlayerHeadgear.Length) {
            offsetIndex = 0;
        }

        float y = Main.OffsetsPlayerHeadgear[offsetIndex].Y;
        Main.instance.LoadProjectile(ProjectileID.MechanicWrench);
        Texture2D wrenchTexture = TextureAssets.Projectile[ProjectileID.MechanicWrench].Value;
        if (npc.townNpcVariationIndex == 1) {
            wrenchTexture = TextureAssets.Extra[ExtrasID.ShimmeredMechanicWrench].Value;
        }

        Vector2 offset = -(new Vector2(wrenchTexture.Width, wrenchTexture.Height / Main.npcFrameCount[npc.type]) * npc.scale / 2f) + new Vector2(2f, -12f + y);
        RequestDraw(new TownNPCDrawRequest(wrenchTexture, offset, Origin: Vector2.Zero, DrawLayer: -1));
    }
}