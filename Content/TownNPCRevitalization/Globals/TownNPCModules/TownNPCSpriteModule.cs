using System;
using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Records;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Configs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Patches;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Systems;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader.Config;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

/// <summary>
///     Module for Town NPCs that deal with drawing related tasks.
/// </summary>
public sealed class TownNPCSpriteModule : TownNPCModule, IUpdateSleep, IUpdateTownNPCSmallTalk {
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

    private const int EyelidClosedDuration = 15;
    private const int TalkDuration = 8;

    private const int TalkOverlayIndex = 0;
    private const int EyelidOverlayIndex = 1;

    private readonly List<TownNPCDrawRequest> _drawRequests = [];

    private int _blinkTimer;
    private int _mouthOpenTimer;

    private Vector2 _drawOffset;

    public bool AreEyesClosed {
        get;
        private set;
    }

    public bool IsTalking {
        get;
        private set;
    }

    public override int UpdatePriority => -2;

    private bool IsDrawOverhaulDisabled => ModContent.GetInstance<RevitalizationConfigClient>().disabledDrawOverhauls.Contains(new NPCDefinition(NPC.type));

    private TownNPCDrawParameters DrawParameters {
        get {
            Asset<Texture2D> npcAsset = TownNPCProfiles.Instance.GetProfile(NPC, out ITownNPCProfile profile) ? profile.GetTextureNPCShouldUse(NPC) : TextureAssets.Npc[NPC.type];
            if (!npcAsset.IsLoaded) {
                npcAsset.ForceLoadAsset(NPC.ModNPC is { } modNPC ? modNPC.Mod.Name : "Terraria");
            }

            int frameWidth = npcAsset.Width();
            int frameHeight = npcAsset.Height() / Main.npcFrameCount[NPC.type];
            Vector2 halfSize = new(frameWidth / 2, frameHeight / 2);
            float npcAddHeight = Main.NPCAddHeight(NPC);
            SpriteEffects spriteEffects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            return new TownNPCDrawParameters(npcAsset, frameWidth, frameHeight, halfSize, npcAddHeight, spriteEffects);
        }
    }

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => TownGlobalNPC.IsValidFullTownNPC(entity, lateInstantiation);

    public override void UpdateModule() {
        _drawOffset = Vector2.Zero;
        _drawRequests.Clear();

        if (Main.netMode == NetmodeID.Server || IsDrawOverhaulDisabled) {
            return;
        }

        (Asset<Texture2D> npcAsset, int frameWidth, int frameHeight, Vector2 halfSize, float npcAddHeight, SpriteEffects spriteEffects) = DrawParameters;

        // Method Gaslighting
        // See RevitalizationNPCPatches.cs: TL;DR is the method is patched so that all sprite-batch calls are re-routed back to here (the sprite module) and we control the drawing
        for (int i = 0; i < 2; i++) {
            RevitalizationNPCPatches.AddExtrasToNPCDrawing(false, NPC, i == 0, npcAddHeight, 0, Color.White, halfSize, spriteEffects, Vector2.Zero);
        }

        // This is the request to actually draw the NPC itself
        Vector2 defaultOrigin = halfSize * NPC.scale;
        RequestDraw(new TownNPCDrawRequest(npcAsset.Value, Vector2.Zero, defaultOrigin, NPC.frame));

        AddFlavorOverlays(frameWidth, frameHeight, defaultOrigin);
        if (NPC.type == NPCID.Mechanic) {
            DrawMechanicWrench();
        }
    }

    public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Icon = new UnlockableTownNPCEntryIcon((UnlockableNPCEntryIcon)bestiaryEntry.Icon);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (IsDrawOverhaulDisabled) {
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }

        (Asset<Texture2D> _, int frameWidth, int frameHeight, Vector2 _, float npcAddHeight, SpriteEffects spriteEffects) = DrawParameters;
        Vector2 drawPos = new (
            npc.position.X + npc.width / 2 - frameWidth * npc.scale / 2f + /*halfSize.X * npc.scale*/ + _drawOffset.X,
            npc.position.Y + npc.height - frameHeight * npc.scale + 4f + /*halfSize.Y * npc.scale*/ + npcAddHeight /*+ num35*/ + npc.gfxOffY + _drawOffset.Y
        );

        drawColor = npc.GetNPCColorTintedByBuffs(drawColor);
        Color shiftedDrawColor = npc.color == default(Color) ? npc.GetAlpha(drawColor) : npc.GetColor(drawColor);
        DrawData defaultDrawData = new (
            null,
            drawPos,
            null,
            shiftedDrawColor,
            npc.rotation,
            Vector2.Zero,
            npc.scale,
            spriteEffects
        );

        foreach (TownNPCDrawRequest request in _drawRequests) {
            request.UnionWithDrawData(defaultDrawData, screenPos).Draw(spriteBatch);
        }

        return false;
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
    ///     Adds a draw request for drawing over the Town NPC. Note that the position given in the request will be drawn relative to the draw position of the NPC.
    ///     Only functions in SP/MP. Does nothing on the server.
    /// </summary>
    public void RequestDraw(in TownNPCDrawRequest request) {
        if (Main.netMode == NetmodeID.Server) {
            return;
        }

        _drawRequests.Add(request);
        _drawRequests.Sort();
    }

    /// <summary>
    ///     Offsets the base draw position of the NPC, and any subsequent layers/overlays.
    /// </summary>
    public void OffsetDrawPosition(Vector2 drawOffset) {
        _drawOffset = drawOffset;
    }

    public void UpdateSleep(NPC npc, Vector2? drawOffset, uint? frameOverride) {
        if (Main.netMode == NetmodeID.Server) {
            return;
        }

        if (drawOffset is { } offset) {
            OffsetDrawPosition(offset);
        }

        CloseEyes();

        TownNPCDrawRequest drawRequest = npc.GetGlobalNPC<TownNPCSleepModule>().SleepSpriteDrawData;
        RequestDraw(drawRequest);
    }

    public void UpdateTownNPCSmallTalk(NPC npc, int remainingTicks) {
        if (remainingTicks % 16 == 0) {
            DoTalk();
        }
    }

    private TownNPCSpriteOverlay GetOverlay(int overlayIndex) => TownNPCDataSystem.spriteOverlayProfiles[NPC.type].GetCurrentSpriteOverlay(NPC, overlayIndex);

    private void AddFlavorOverlays(int frameWidth, int frameHeight, Vector2 defaultOrigin) {
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

        int currentYFrame = NPC.frame.Y / frameHeight;
        (bool, int)[] overlayPairs = [(AreEyesClosed, EyelidOverlayIndex), (IsTalking, TalkOverlayIndex)];
        foreach ((bool isOverlayActive, int overlayIndex) in overlayPairs) {
            if (!isOverlayActive) {
                continue;
            }

            AddOverlay(overlayIndex, frameWidth, defaultOrigin, currentYFrame);
        }
    }

    private void AddOverlay(int overlayIndex, int frameWidth, Vector2 defaultOrigin, int currentYFrame) {
        TownNPCSpriteOverlay overlay = GetOverlay(overlayIndex);
        Vector2 adjustedDrawOffset = new (NPC.spriteDirection == -1 ? overlay.DefaultDrawOffset.X : frameWidth - overlay.DefaultDrawOffset.X - overlay.Texture.Width, overlay.DefaultDrawOffset.Y);
        adjustedDrawOffset += overlay.AdditionalFrameOffsets.TryGetValue(currentYFrame, out Vector2 frameOffset) ? frameOffset : Vector2.Zero;

        Vector2 overlayOrigin = new (
            (defaultOrigin.X - overlay.DefaultDrawOffset.X) * -NPC.spriteDirection + (NPC.spriteDirection == 1 ? overlay.Texture.Width : 0),
            defaultOrigin.Y - adjustedDrawOffset.Y
        );
        RequestDraw(new TownNPCDrawRequest(overlay.Texture, adjustedDrawOffset, overlayOrigin, DrawLayer: 1));
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
        RequestDraw(new TownNPCDrawRequest(wrenchTexture, new Vector2(0, Main.OffsetsPlayerHeadgear[offsetIndex].Y + wrenchTexture.Height / 2), wrenchCenterOffset, DrawLayer: -1));
    }

    // TODO: Get rid of this once pet animations are all fully implemented again
    private void TownNPCFindFrame(int frameHeight) {
        int extraFrameCount = NPCID.Sets.ExtraFramesCount[NPC.type];

        if (NPC.velocity.Y == 0f) {
            NPC.spriteDirection = NPC.direction switch {
                1 => 1,
                -1 => -1,
                _ => NPC.spriteDirection
            };

            int nonAttackFrameCount = Main.npcFrameCount[NPC.type] - NPCID.Sets.AttackFrameCount[NPC.type];
            switch (NPC.ai[0]) {
                // Town Pet Flavor Animation states
                case >= 20f and <= 22f: {
                    int frame = NPC.frame.Y / frameHeight;
                    switch ((int)NPC.ai[0]) {
                        case 20:
                            switch (NPC.type) {
                                case NPCID.TownBunny: {
                                    if (NPC.ai[1] > 30f && frame is < 7 or > 9) {
                                        frame = 7;
                                    }

                                    if (frame > 0) {
                                        NPC.frameCounter += 1.0;
                                    }

                                    if (NPC.frameCounter > 4.0) {
                                        NPC.frameCounter = 0.0;
                                        frame++;
                                        if (frame > 8 && NPC.ai[1] > 30f) {
                                            frame = 8;
                                        }

                                        if (frame > 9) {
                                            frame = 0;
                                        }
                                    }

                                    break;
                                }
                                case NPCID.TownCat: {
                                    if (NPC.ai[1] > 30f && frame is < 10 or > 16) {
                                        frame = 10;
                                    }

                                    if (frame > 0) {
                                        NPC.frameCounter += 1.0;
                                    }

                                    if (NPC.frameCounter > 4.0) {
                                        NPC.frameCounter = 0.0;
                                        frame++;
                                        if (frame > 13 && NPC.ai[1] > 30f) {
                                            frame = 13;
                                        }

                                        if (frame > 16) {
                                            frame = 0;
                                        }
                                    }

                                    break;
                                }
                            }

                            if (NPC.type != NPCID.TownDog) {
                                break;
                            }

                            if (NPC.ai[1] > 30f && frame is < 23 or > 27) {
                                frame = 23;
                            }

                            if (frame > 0) {
                                NPC.frameCounter += 1.0;
                            }

                            if (NPC.frameCounter > 4.0) {
                                NPC.frameCounter = 0.0;
                                frame++;
                                if (frame > 26 && NPC.ai[1] > 30f) {
                                    frame = 24;
                                }

                                if (frame > 27) {
                                    frame = 0;
                                }
                            }

                            break;
                        case 21:
                            switch (NPC.type) {
                                case NPCID.TownBunny: {
                                    if (NPC.ai[1] > 30f && frame is < 10 or > 16) {
                                        frame = 10;
                                    }

                                    if (frame > 0) {
                                        NPC.frameCounter += 1.0;
                                    }

                                    if (NPC.frameCounter > 4.0) {
                                        NPC.frameCounter = 0.0;
                                        frame++;
                                        if (frame > 13 && NPC.ai[1] > 30f) {
                                            frame = 13;
                                        }

                                        if (frame > 16) {
                                            frame = 0;
                                        }
                                    }

                                    break;
                                }
                                case NPCID.TownCat: {
                                    if (NPC.ai[1] > 30f && frame is < 17 or > 21) {
                                        frame = 17;
                                    }

                                    if (frame > 0) {
                                        NPC.frameCounter += 1.0;
                                    }

                                    if (NPC.frameCounter > 4.0) {
                                        NPC.frameCounter = 0.0;
                                        frame++;
                                        if (frame > 19 && NPC.ai[1] > 30f) {
                                            frame = 19;
                                        }

                                        if (frame > 21) {
                                            frame = 0;
                                        }
                                    }

                                    break;
                                }
                            }

                            if (NPC.type != NPCID.TownDog) {
                                break;
                            }

                            if (NPC.ai[1] > 30f && frame is < 17 or > 22) {
                                frame = 17;
                            }

                            if (frame > 0) {
                                NPC.frameCounter += 1.0;
                            }

                            if (NPC.frameCounter > 4.0) {
                                NPC.frameCounter = 0.0;
                                frame++;
                                if (frame > 21 && NPC.ai[1] > 30f) {
                                    frame = 18;
                                }

                                if (frame > 22) {
                                    frame = 0;
                                }
                            }

                            break;
                        case 22:
                            if (NPC.type == NPCID.TownBunny) {
                                int frameCount = Main.npcFrameCount[NPC.type];
                                if (NPC.ai[1] > 40f && (frame < 17 || frame >= frameCount)) {
                                    frame = 17;
                                }

                                if (frame > 0) {
                                    NPC.frameCounter += 1.0;
                                }

                                if (NPC.frameCounter > 4.0) {
                                    NPC.frameCounter = 0.0;
                                    frame++;
                                    if (frame > 20 && NPC.ai[1] > 40f) {
                                        frame = 19;
                                    }

                                    if (frame >= frameCount) {
                                        frame = 0;
                                    }
                                }
                            }

                            if (NPC.type != NPCID.TownCat) {
                                break;
                            }

                            if (NPC.ai[1] > 30f && frame is < 17 or > 27) {
                                frame = 17;
                            }

                            if (frame > 0) {
                                NPC.frameCounter += 1.0;
                            }

                            if (NPC.frameCounter > 4.0) {
                                NPC.frameCounter = 0.0;
                                frame++;
                                if (frame > 27) {
                                    frame = !(NPC.ai[1] <= 30f) ? 22 : 20;
                                }
                                else {
                                    frame = NPC.ai[1] switch {
                                        <= 30f when frame == 22 => 0,
                                        > 30f when frame is > 19 and < 22 => 22,
                                        _ => frame
                                    };
                                }
                            }

                            break;
                    }

                    NPC.frame.Y = frame * frameHeight;
                    break;
                }
                // Throw attack state
                case 10f:
                // Nurse heal state
                case 13f: {
                    NPC.frameCounter += 1.0;
                    int frame = NPC.frame.Y / frameHeight;
                    int framesAwayFromTotal = frame - nonAttackFrameCount;
                    if ((uint)framesAwayFromTotal > 3u && frame != 0) {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0.0;
                    }

                    int frameDuration1 = 10;
                    int frameDuration2 = 6;
                    if (NPC.type == NPCID.BestiaryGirl) {
                        frameDuration1 = 0;
                        frameDuration2 = 2;
                    }

                    int nextFrame;
                    if (!(NPC.frameCounter < frameDuration1)) {
                        if (NPC.frameCounter < frameDuration1 + frameDuration2) {
                            nextFrame = nonAttackFrameCount;
                        }
                        else {
                            if (NPC.frameCounter < frameDuration1 + frameDuration2 * 2) {
                                nextFrame = nonAttackFrameCount + 1;
                            }
                            else {
                                if (NPC.frameCounter < frameDuration1 + frameDuration2 * 3) {
                                    nextFrame = nonAttackFrameCount + 2;
                                }
                                else {
                                    if (NPC.frameCounter < frameDuration1 + frameDuration2 * 4) {
                                        nextFrame = nonAttackFrameCount + 3;
                                    }
                                    else {
                                        nextFrame = 0;
                                    }
                                }
                            }
                        }
                    }
                    else {
                        nextFrame = 0;
                    }

                    NPC.frame.Y = frameHeight * nextFrame;
                    break;
                }
                // Melee attack state
                case 15f: {
                    NPC.frameCounter += 1.0;
                    int frame = NPC.frame.Y / frameHeight;
                    int framesAwayFromTotal = frame - nonAttackFrameCount;
                    if ((uint)framesAwayFromTotal > 3u && frame != 0) {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0.0;
                    }

                    float attackProgress = NPC.ai[1] / NPCID.Sets.AttackTime[NPC.type];
                    int nextFrame = attackProgress switch {
                        > 0.65f => nonAttackFrameCount,
                        > 0.5f => nonAttackFrameCount + 1,
                        > 0.35f => nonAttackFrameCount + 2,
                        > 0f => nonAttackFrameCount + 3,
                        _ => 0
                    };

                    NPC.frame.Y = frameHeight * nextFrame;
                    break;
                }
                // Shimmer transform state
                case 25f:
                    NPC.frame.Y = frameHeight;
                    break;
                // Firearm attack state
                case 12f: {
                    NPC.frameCounter += 1.0;
                    int frame = NPC.frame.Y / frameHeight;
                    int framesAwayFromTotal = frame - nonAttackFrameCount;
                    if ((uint)framesAwayFromTotal > 4u && frame != 0) {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0.0;
                    }

                    NPC.frame.Y = frameHeight * (nonAttackFrameCount + NPC.GetShootingFrame(NPC.ai[2]));
                    break;
                }
                // Magic attack state
                case 14f: {
                    NPC.frameCounter += 1.0;
                    int frame = NPC.frame.Y / frameHeight;
                    int framesAwayFromTotal = frame - nonAttackFrameCount;
                    if ((uint)framesAwayFromTotal > 1u && frame != 0) {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0.0;
                    }

                    const int timerCap = 12;
                    NPC.frame.Y = frameHeight * (NPC.frameCounter % timerCap * 2.0 < timerCap ? nonAttackFrameCount : nonAttackFrameCount + 1);
                    break;
                }
                default: {
                    if (NPC.velocity.X == 0f) {
                        switch (NPC.type) {
                            case NPCID.TownDog: {
                                int frame = NPC.frame.Y / frameHeight;
                                if (frame > 7) {
                                    frame = 0;
                                }

                                NPC.frameCounter += 1.0;
                                if (NPC.frameCounter > 4.0) {
                                    NPC.frameCounter = 0.0;
                                    frame++;
                                    if (frame > 7) {
                                        frame = 0;
                                    }
                                }

                                NPC.frame.Y = frame * frameHeight;
                                break;
                            }
                            default:
                                NPC.frame.Y = 0;
                                NPC.frameCounter = 0.0;
                                break;
                        }
                    }
                    else {
                        int frameDuration = NPC.type switch {
                            NPCID.TownDog or NPCID.TownBunny => 12,
                            _ => 6
                        };

                        NPC.frameCounter += Math.Abs(NPC.velocity.X) * 2f + 1f;

                        int frameY = NPC.type switch {
                            NPCID.TownDog => frameHeight * 9,
                            NPCID.TownBunny => frameHeight,
                            _ => frameHeight * 2
                        };

                        if (NPC.frame.Y < frameY) {
                            NPC.frame.Y = frameY;
                        }

                        if (NPC.frameCounter > frameDuration) {
                            NPC.frame.Y += frameHeight;
                            NPC.frameCounter = 0.0;
                        }

                        if (NPC.frame.Y / frameHeight >= Main.npcFrameCount[NPC.type] - extraFrameCount) {
                            NPC.frame.Y = frameY;
                        }
                    }

                    break;
                }
            }

            return;
        }

        NPC.frameCounter = 0.0;
        NPC.frame.Y = frameHeight;

        NPC.frame.Y = NPC.type switch {
            NPCID.TownDog => frameHeight * 8,
            _ => NPC.frame.Y
        };
    }
}