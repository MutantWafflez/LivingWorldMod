using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs.TownNPCModules;

/// <summary>
///     Module for Town NPCs that deal with drawing related tasks.
/// </summary>
public sealed class TownNPCSpriteModule : TownNPCModule {
    /// <summary>
    ///     Small wrapper class for <see cref="UnlockableNPCEntryIcon" /> that wraps the <see cref="UnlockableNPCEntryIcon.Update" /> method so that we can add <see cref="TownNPCSpriteModule.UpdateModule" />
    ///     calls to the end of it, allowing for the Bestiary to actually draw NPCs with all the tweaks.
    /// </summary>
    private sealed class UnlockableTownNPCEntryIcon(UnlockableNPCEntryIcon icon) : IEntryIcon {
        public void Update(BestiaryUICollectionInfo providedInfo, Rectangle hitbox, EntryIconDrawSettings settings) {
            icon.Update(providedInfo, hitbox, settings);

            icon._npcCache.GetGlobalNPC<TownNPCSpriteModule>().UpdateModule();
        }

        public void Draw(BestiaryUICollectionInfo providedInfo, SpriteBatch spriteBatch, EntryIconDrawSettings settings) {
            icon.Draw(providedInfo, spriteBatch, settings);
        }

        public bool GetUnlockState(BestiaryUICollectionInfo providedInfo) => icon.GetUnlockState(providedInfo);

        public string GetHoverText(BestiaryUICollectionInfo providedInfo) => icon.GetHoverText(providedInfo);

        public IEntryIcon CreateClone() => new UnlockableTownNPCEntryIcon((UnlockableNPCEntryIcon)icon.CreateClone());
    }

    /// <summary>
    ///     Small helper record that holds data on some helpful parameters for usage with drawing Town NPCs.
    /// </summary>
    private record TownNPCDrawParameters(Asset<Texture2D> NPCAsset, int FrameWidth, int FrameHeight, Vector2 HalfSize, float NPCAddHeight, SpriteEffects SpriteEffects);

    public const int GivingAnimationDuration = (int)(LWMUtils.RealLifeSecond * 1.5f);
    private const int EyelidClosedDuration = 15;
    private const int TalkDuration = 8;

    private const int TalkTextureIndex = 0;
    private const int EyelidTextureIndex = 1;

    /// <summary>
    ///     The value that <see cref="_frameYOverride" /> is set to when there is currently no frame override occuring for this NPC.
    /// </summary>
    private const int NoFrameYOverride = -1;

    private readonly List<TownNPCDrawRequest> _drawRequests = [];

    private int _blinkTimer;
    private int _mouthOpenTimer;

    private int _givingTimer;
    private int _givingItemType;

    private int _frameYOverride = NoFrameYOverride;

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

    public override int UpdatePriority => -2;

    private TownNPCDrawParameters DrawParameters {
        get {
            Asset<Texture2D> npcAsset = TownNPCProfiles.Instance.GetProfile(NPC, out ITownNPCProfile profile) ? profile.GetTextureNPCShouldUse(NPC) : TextureAssets.Npc[NPC.type];
            int frameWidth = npcAsset.Width();
            int frameHeight = npcAsset.Height() / Main.npcFrameCount[NPC.type];
            Vector2 halfSize = new(frameWidth / 2, frameHeight / 2);
            float npcAddHeight = Main.NPCAddHeight(NPC);
            SpriteEffects spriteEffects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            return new TownNPCDrawParameters(npcAsset, frameWidth, frameHeight, halfSize, npcAddHeight, spriteEffects);
        }
    }

    public override void UpdateModule() {
        if (Main.netMode == NetmodeID.Server) {
            return;
        }

        _drawOffset = Vector2.Zero;
        _frameYOverride = NoFrameYOverride;
        _drawRequests.Clear();

        (Asset<Texture2D> npcAsset, int frameWidth, int frameHeight, Vector2 halfSize, float npcAddHeight, SpriteEffects spriteEffects) = DrawParameters;

        // Method Gaslighting
        // See RevitalizationNPCPatches.cs: TL;DR is the method is patched so that all sprite-batch calls are re-routed back to here (the sprite module) and we control the drawing
        for (int i = 0; i < 2; i++) {
            Main.instance.DrawNPCExtras(NPC, i == 0, npcAddHeight, 0, Color.White, halfSize, spriteEffects, Vector2.Zero);
        }

        // This is the request to actually draw the NPC itself
        RequestDraw(new TownNPCDrawRequest(npcAsset.Value, Vector2.Zero, NPC.frame));

        UpdateFlavorAnimations(frameWidth, frameHeight);
        if (NPC.type == NPCID.Mechanic) {
            DrawMechanicWrench();
        }
    }

    public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Icon = new UnlockableTownNPCEntryIcon((UnlockableNPCEntryIcon)bestiaryEntry.Icon);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        (Asset<Texture2D> _, int frameWidth, int frameHeight, Vector2 halfSize, float npcAddHeight, SpriteEffects spriteEffects) = DrawParameters;
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

        return false;
    }

    public override void FindFrame(NPC npc, int frameHeight) {
        if (_frameYOverride >= 0) {
            npc.frame.Y = frameHeight * _frameYOverride;
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

    private TownNPCSpriteOverlay GetOverlay(int overlayIndex) => TownNPCDataSystem.spriteOverlayProfiles[NPC.type].GetCurrentSpriteOverlay(NPC, overlayIndex);

    private void UpdateFlavorAnimations(int frameWidth, int frameHeight) {
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

        if (IsGiving) {
            int nonAttackFrameCount = Main.npcFrameCount[NPC.type] - NPCID.Sets.AttackFrameCount[NPC.type];
            const int animationHalf = GivingAnimationDuration / 2;
            RequestFrameOverride(
                (uint)((_givingTimer <= animationHalf ? _givingTimer : animationHalf - _givingTimer % (animationHalf + 1)) switch {
                    >= 10 and < 16 => nonAttackFrameCount - 5,
                    >= 16 => nonAttackFrameCount - 4,
                    _ => 0
                })
            );

            if (--_givingTimer <= 0) {
                IsGiving = false;
                _givingTimer = 0;
                _givingItemType = -1;
            }
        }

        int currentYFrame = _frameYOverride == NoFrameYOverride ? NPC.frame.Y / frameHeight : _frameYOverride;

        if (AreEyesClosed) {
            TownNPCSpriteOverlay overlay = GetOverlay(EyelidTextureIndex);

            Vector2 adjustedDrawOffset = new (NPC.spriteDirection == -1 ? overlay.DefaultDrawOffset.X : frameWidth - overlay.DefaultDrawOffset.X - overlay.Texture.Width, overlay.DefaultDrawOffset.Y);

            RequestDraw(new TownNPCDrawRequest(overlay.Texture, adjustedDrawOffset));
        }

        if (IsTalking) {
            TownNPCSpriteOverlay overlay = GetOverlay(TalkTextureIndex);

            Vector2 adjustedDrawOffset = new (NPC.spriteDirection == -1 ? overlay.DefaultDrawOffset.X : frameWidth - overlay.DefaultDrawOffset.X - overlay.Texture.Width, overlay.DefaultDrawOffset.Y);

            RequestDraw(new TownNPCDrawRequest(overlay.Texture, adjustedDrawOffset));
        }
    }

    private void DrawMechanicWrench() {
        // Adapted vanilla code
        if (NPC.localAI[0] != 0f) {
            return;
        }

        int offsetIndex = 0;
        if (NPC.frame.Y > 56) {
            offsetIndex += 4;
        }

        offsetIndex += NPC.frame.Y / 56;
        if (offsetIndex >= Main.OffsetsPlayerHeadgear.Length) {
            offsetIndex = 0;
        }

        Main.instance.LoadProjectile(ProjectileID.MechanicWrench);
        Texture2D wrenchTexture = TextureAssets.Projectile[ProjectileID.MechanicWrench].Value;
        if (NPC.townNpcVariationIndex == 1) {
            wrenchTexture = TextureAssets.Extra[ExtrasID.ShimmeredMechanicWrench].Value;
        }

        Vector2 wrenchCenterOffset = new Vector2(wrenchTexture.Width, wrenchTexture.Height) * NPC.scale / 2f;
        RequestDraw(new TownNPCDrawRequest(wrenchTexture, new Vector2(0, Main.OffsetsPlayerHeadgear[offsetIndex].Y), Origin: wrenchCenterOffset, DrawLayer: -1));
    }
}