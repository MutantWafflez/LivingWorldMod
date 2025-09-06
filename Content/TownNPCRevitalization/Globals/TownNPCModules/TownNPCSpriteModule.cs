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
using Terraria.GameContent.UI;
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

    public const int GivingAnimationDuration = (int)(LWMUtils.RealLifeSecond * 1.5f);
    private const int EyelidClosedDuration = 15;
    private const int TalkDuration = 8;

    private const int TalkOverlayIndex = 0;
    private const int EyelidOverlayIndex = 1;

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
        _frameYOverride = NoFrameYOverride;
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

        UpdateFlavorAnimations(frameWidth, frameHeight, defaultOrigin);
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

        if (frameOverride is { } @override) {
            RequestFrameOverride(@override);
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

    private void UpdateFlavorAnimations(int frameWidth, int frameHeight, Vector2 defaultOrigin) {
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

    private void TownNPCFindFrame(int num) {
        int num236 = NPC.isLikeATownNPC ? NPCID.Sets.ExtraFramesCount[NPC.type] : 0;
        bool flag11 = NPCID.Sets.IsTownSlime[NPC.type];
        if (false && !Main.dedServ && TownNPCProfiles.Instance.GetProfile(NPC, out ITownNPCProfile profile)) {
            Asset<Texture2D> textureNPCShouldUse = profile.GetTextureNPCShouldUse(NPC);
            if (textureNPCShouldUse.IsLoaded) {
                num = textureNPCShouldUse.Height() / Main.npcFrameCount[NPC.type];
                NPC.frame.Width = textureNPCShouldUse.Width();
                NPC.frame.Height = num;
            }
        }

        if (NPC.velocity.Y == 0f) {
            if (NPC.direction == 1) {
                NPC.spriteDirection = 1;
            }

            if (NPC.direction == -1) {
                NPC.spriteDirection = -1;
            }

            if (NPCID.Sets.IsTownSlime[NPC.type]) {
                NPC.spriteDirection *= -1;
            }

            int num237 = Main.npcFrameCount[NPC.type] - NPCID.Sets.AttackFrameCount[NPC.type];
            if (NPC.ai[0] == 23f) {
                NPC.frameCounter += 1.0;
                int num238 = NPC.frame.Y / num;
                int num17 = num237 - num238;
                if ((uint)(num17 - 1) > 1u && (uint)(num17 - 4) > 1u && num238 != 0) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }

                int num239 = 0;
                num239 = !(NPC.frameCounter < 6.0) ? num237 - 4 : num237 - 5;
                if (NPC.ai[1] < 6f) {
                    num239 = num237 - 5;
                }

                NPC.frame.Y = num * num239;
            }
            else if (NPC.ai[0] >= 20f && NPC.ai[0] <= 22f) {
                int num240 = NPC.frame.Y / num;
                switch ((int)NPC.ai[0]) {
                    case 20:
                        if (flag11) {
                            if (NPC.ai[1] > 30f && (num240 < 8 || num240 > 13)) {
                                num240 = 8;
                            }

                            if (num240 > 0) {
                                NPC.frameCounter += 1.0;
                            }

                            if (NPC.frameCounter >= 12.0) {
                                NPC.frameCounter = 0.0;
                                num240++;
                                if (num240 > 13 && NPC.ai[1] > 30f) {
                                    num240 = 8;
                                }

                                if (num240 > 13) {
                                    num240 = 0;
                                }
                            }
                        }

                        if (NPC.type == 656) {
                            if (NPC.ai[1] > 30f && (num240 < 7 || num240 > 9)) {
                                num240 = 7;
                            }

                            if (num240 > 0) {
                                NPC.frameCounter += 1.0;
                            }

                            if (NPC.frameCounter > 4.0) {
                                NPC.frameCounter = 0.0;
                                num240++;
                                if (num240 > 8 && NPC.ai[1] > 30f) {
                                    num240 = 8;
                                }

                                if (num240 > 9) {
                                    num240 = 0;
                                }
                            }
                        }

                        if (NPC.type == 637) {
                            if (NPC.ai[1] > 30f && (num240 < 10 || num240 > 16)) {
                                num240 = 10;
                            }

                            if (num240 > 0) {
                                NPC.frameCounter += 1.0;
                            }

                            if (NPC.frameCounter > 4.0) {
                                NPC.frameCounter = 0.0;
                                num240++;
                                if (num240 > 13 && NPC.ai[1] > 30f) {
                                    num240 = 13;
                                }

                                if (num240 > 16) {
                                    num240 = 0;
                                }
                            }
                        }

                        if (NPC.type != 638) {
                            break;
                        }

                        if (NPC.ai[1] > 30f && (num240 < 23 || num240 > 27)) {
                            num240 = 23;
                        }

                        if (num240 > 0) {
                            NPC.frameCounter += 1.0;
                        }

                        if (NPC.frameCounter > 4.0) {
                            NPC.frameCounter = 0.0;
                            num240++;
                            if (num240 > 26 && NPC.ai[1] > 30f) {
                                num240 = 24;
                            }

                            if (num240 > 27) {
                                num240 = 0;
                            }
                        }

                        break;
                    case 21:
                        if (NPC.type == 656) {
                            if (NPC.ai[1] > 30f && (num240 < 10 || num240 > 16)) {
                                num240 = 10;
                            }

                            if (num240 > 0) {
                                NPC.frameCounter += 1.0;
                            }

                            if (NPC.frameCounter > 4.0) {
                                NPC.frameCounter = 0.0;
                                num240++;
                                if (num240 > 13 && NPC.ai[1] > 30f) {
                                    num240 = 13;
                                }

                                if (num240 > 16) {
                                    num240 = 0;
                                }
                            }
                        }

                        if (NPC.type == 637) {
                            if (NPC.ai[1] > 30f && (num240 < 17 || num240 > 21)) {
                                num240 = 17;
                            }

                            if (num240 > 0) {
                                NPC.frameCounter += 1.0;
                            }

                            if (NPC.frameCounter > 4.0) {
                                NPC.frameCounter = 0.0;
                                num240++;
                                if (num240 > 19 && NPC.ai[1] > 30f) {
                                    num240 = 19;
                                }

                                if (num240 > 21) {
                                    num240 = 0;
                                }
                            }
                        }

                        if (NPC.type != 638) {
                            break;
                        }

                        if (NPC.ai[1] > 30f && (num240 < 17 || num240 > 22)) {
                            num240 = 17;
                        }

                        if (num240 > 0) {
                            NPC.frameCounter += 1.0;
                        }

                        if (NPC.frameCounter > 4.0) {
                            NPC.frameCounter = 0.0;
                            num240++;
                            if (num240 > 21 && NPC.ai[1] > 30f) {
                                num240 = 18;
                            }

                            if (num240 > 22) {
                                num240 = 0;
                            }
                        }

                        break;
                    case 22:
                        if (NPC.type == 656) {
                            int num241 = Main.npcFrameCount[NPC.type];
                            if (NPC.ai[1] > 40f && (num240 < 17 || num240 >= num241)) {
                                num240 = 17;
                            }

                            if (num240 > 0) {
                                NPC.frameCounter += 1.0;
                            }

                            if (NPC.frameCounter > 4.0) {
                                NPC.frameCounter = 0.0;
                                num240++;
                                if (num240 > 20 && NPC.ai[1] > 40f) {
                                    num240 = 19;
                                }

                                if (num240 >= num241) {
                                    num240 = 0;
                                }
                            }
                        }

                        if (NPC.type != 637) {
                            break;
                        }

                        if (NPC.ai[1] > 30f && (num240 < 17 || num240 > 27)) {
                            num240 = 17;
                        }

                        if (num240 > 0) {
                            NPC.frameCounter += 1.0;
                        }

                        if (NPC.frameCounter > 4.0) {
                            NPC.frameCounter = 0.0;
                            num240++;
                            if (num240 > 27) {
                                num240 = !(NPC.ai[1] <= 30f) ? 22 : 20;
                            }
                            else if (NPC.ai[1] <= 30f && num240 == 22) {
                                num240 = 0;
                            }
                            else if (NPC.ai[1] > 30f && num240 > 19 && num240 < 22) {
                                num240 = 22;
                            }
                        }

                        break;
                }

                NPC.frame.Y = num240 * num;
            }
            else if (NPC.ai[0] == 2f) {
                NPC.frameCounter += 1.0;
                if (NPC.frame.Y / num == num237 - 1 && NPC.frameCounter >= 5.0) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }
                else if (NPC.frame.Y / num == 0 && NPC.frameCounter >= 40.0) {
                    NPC.frame.Y = num * (num237 - 1);
                    NPC.frameCounter = 0.0;
                }
                else if (NPC.frame.Y != 0 && NPC.frame.Y != num * (num237 - 1)) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }
            }
            else if (NPC.ai[0] == 11f) {
                NPC.frameCounter += 1.0;
                if (NPC.frame.Y / num == num237 - 1 && NPC.frameCounter >= 50.0) {
                    if (NPC.frameCounter == 50.0) {
                        int num242 = Main.rand.Next(4);
                        for (int m = 0; m < 3 + num242; m++) {
                            int num243 = Dust.NewDust(NPC.Center + Vector2.UnitX * -NPC.direction * 8f - Vector2.One * 5f + Vector2.UnitY * 8f, 3, 6, 216, -NPC.direction, 1f);
                            Dust dust = Main.dust[num243];
                            dust.velocity /= 2f;
                            Main.dust[num243].scale = 0.8f;
                        }

                        if (Main.rand.Next(30) == 0) {
                            int num244 = Gore.NewGore(new EntitySource_Misc(""), NPC.Center + Vector2.UnitX * -NPC.direction * 8f, Vector2.Zero, Main.rand.Next(580, 583));
                            Gore gore = Main.gore[num244];
                            gore.velocity /= 2f;
                            Main.gore[num244].velocity.Y = Math.Abs(Main.gore[num244].velocity.Y);
                            Main.gore[num244].velocity.X = (0f - Math.Abs(Main.gore[num244].velocity.X)) * NPC.direction;
                        }
                    }

                    if (NPC.frameCounter >= 100.0 && Main.rand.Next(20) == 0) {
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0.0;
                    }
                }
                else if (NPC.frame.Y / num == 0 && NPC.frameCounter >= 20.0) {
                    NPC.frame.Y = num * (num237 - 1);
                    NPC.frameCounter = 0.0;
                    EmoteBubble.NewBubble(89, new WorldUIAnchor(NPC), 90);
                }
                else if (NPC.frame.Y != 0 && NPC.frame.Y != num * (num237 - 1)) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }
            }
            else if (NPC.ai[0] == 5f) {
                NPC.frame.Y = num * (num237 - 3);
                if (NPC.type == 637) {
                    NPC.frame.Y = num * 19;
                }

                NPC.frameCounter = 0.0;
            }
            else if (NPC.ai[0] == 6f) {
                NPC.frameCounter += 1.0;
                int num245 = NPC.frame.Y / num;
                int num17 = num237 - num245;
                if ((uint)(num17 - 1) > 1u && (uint)(num17 - 4) > 1u && num245 != 0) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }

                int num246 = 0;
                num246 = !(NPC.frameCounter < 10.0)
                    ? NPC.frameCounter < 16.0 ? num237 - 5 :
                    NPC.frameCounter < 46.0 ? num237 - 4 :
                    NPC.frameCounter < 60.0 ? num237 - 5 :
                    !(NPC.frameCounter < 66.0) ? NPC.frameCounter < 72.0 ? num237 - 5 :
                    NPC.frameCounter < 102.0 ? num237 - 4 :
                    NPC.frameCounter < 108.0 ? num237 - 5 :
                    !(NPC.frameCounter < 114.0) ? NPC.frameCounter < 120.0 ? num237 - 5 :
                    NPC.frameCounter < 150.0 ? num237 - 4 :
                    NPC.frameCounter < 156.0 ? num237 - 5 :
                    !(NPC.frameCounter < 162.0) ? NPC.frameCounter < 168.0
                        ? num237 - 5
                        : NPC.frameCounter < 198.0
                            ? num237 - 4
                            : NPC.frameCounter < 204.0
                                ? num237 - 5
                                : !(NPC.frameCounter < 210.0)
                                    ? NPC.frameCounter < 216.0 ? num237 - 5 :
                                    NPC.frameCounter < 246.0 ? num237 - 4 :
                                    NPC.frameCounter < 252.0 ? num237 - 5 :
                                    !(NPC.frameCounter < 258.0) ? NPC.frameCounter < 264.0 ? num237 - 5 : NPC.frameCounter < 294.0 ? num237 - 4 : NPC.frameCounter < 300.0 ? num237 - 5 : 0 : 0
                                    : 0 : 0 : 0 : 0
                    : 0;
                if (num246 == num237 - 4 && num245 == num237 - 5) {
                    Vector2 vector4 = NPC.Center + new Vector2(10 * NPC.direction, -4f);
                    for (int n = 0; n < 8; n++) {
                        int num247 = Main.rand.Next(139, 143);
                        int num248 = Dust.NewDust(vector4, 0, 0, num247, NPC.velocity.X + NPC.direction, NPC.velocity.Y - 2.5f, 0, default(Color), 1.2f);
                        Main.dust[num248].velocity.X += NPC.direction * 1.5f;
                        Dust dust = Main.dust[num248];
                        dust.position -= new Vector2(4f);
                        dust = Main.dust[num248];
                        dust.velocity *= 2f;
                        Main.dust[num248].scale = 0.7f + Main.rand.NextFloat() * 0.3f;
                    }
                }

                NPC.frame.Y = num * num246;
                if (NPC.frameCounter >= 300.0) {
                    NPC.frameCounter = 0.0;
                }
            }
            else if ((NPC.ai[0] == 7f || NPC.ai[0] == 19f) && !NPCID.Sets.IsTownPet[NPC.type]) {
                NPC.frameCounter += 1.0;
                int num249 = NPC.frame.Y / num;
                int num17 = num237 - num249;
                if ((uint)(num17 - 1) > 1u && (uint)(num17 - 4) > 1u && num249 != 0) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }

                int num250 = 0;
                if (NPC.frameCounter < 16.0) {
                    num250 = 0;
                }
                else if (NPC.frameCounter == 16.0) {
                    EmoteBubble.NewBubbleNPC(new WorldUIAnchor(NPC), 112);
                }
                else if (NPC.frameCounter < 128.0) {
                    num250 = NPC.frameCounter % 16.0 < 8.0 ? num237 - 2 : 0;
                }
                else if (NPC.frameCounter < 160.0) {
                    num250 = 0;
                }
                else if (NPC.frameCounter != 160.0) {
                    num250 = NPC.frameCounter < 220.0 ? NPC.frameCounter % 12.0 < 6.0 ? num237 - 2 : 0 : 0;
                }
                else {
                    EmoteBubble.NewBubbleNPC(new WorldUIAnchor(NPC), 60);
                }

                NPC.frame.Y = num * num250;
                if (NPC.frameCounter >= 220.0) {
                    NPC.frameCounter = 0.0;
                }
            }
            else if (NPC.ai[0] == 9f) {
                NPC.frameCounter += 1.0;
                int num251 = NPC.frame.Y / num;
                int num17 = num237 - num251;
                if ((uint)(num17 - 1) > 1u && (uint)(num17 - 4) > 1u && num251 != 0) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }

                int num252 = 0;
                num252 = !(NPC.frameCounter < 10.0) ? !(NPC.frameCounter < 16.0) ? num237 - 4 : num237 - 5 : 0;
                if (NPC.ai[1] < 16f) {
                    num252 = num237 - 5;
                }

                if (NPC.ai[1] < 10f) {
                    num252 = 0;
                }

                NPC.frame.Y = num * num252;
            }
            else if (NPC.ai[0] == 18f) {
                NPC.frameCounter += 1.0;
                int num253 = NPC.frame.Y / num;
                int num17 = num237 - num253;
                if ((uint)(num17 - 1) > 1u && (uint)(num17 - 4) > 1u && num253 != 0) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }

                int num254 = 0;
                if (NPC.frameCounter < 10.0) {
                    num254 = 0;
                }
                else if (NPC.frameCounter < 16.0) {
                    num254 = num237 - 1;
                }
                else {
                    num254 = num237 - 2;
                }

                if (NPC.ai[1] < 16f) {
                    num254 = num237 - 1;
                }

                if (NPC.ai[1] < 10f) {
                    num254 = 0;
                }

                num254 = Main.npcFrameCount[NPC.type] - 2;
                NPC.frame.Y = num * num254;
            }
            else if (NPC.ai[0] == 10f || NPC.ai[0] == 13f) {
                NPC.frameCounter += 1.0;
                int num255 = NPC.frame.Y / num;
                int num17 = num255 - num237;
                if ((uint)num17 > 3u && num255 != 0) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }

                int num256 = 10;
                int num257 = 6;
                if (NPC.type == 633) {
                    num256 = 0;
                    num257 = 2;
                }

                int num258 = 0;
                num258 = !(NPC.frameCounter < num256)
                    ? NPC.frameCounter < num256 + num257 ? num237 :
                    NPC.frameCounter < num256 + num257 * 2 ? num237 + 1 :
                    NPC.frameCounter < num256 + num257 * 3 ? num237 + 2 :
                    NPC.frameCounter < num256 + num257 * 4 ? num237 + 3 : 0
                    : 0;
                NPC.frame.Y = num * num258;
            }
            else if (NPC.ai[0] == 15f) {
                NPC.frameCounter += 1.0;
                int num259 = NPC.frame.Y / num;
                int num17 = num259 - num237;
                if ((uint)num17 > 3u && num259 != 0) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }

                float num260 = NPC.ai[1] / NPCID.Sets.AttackTime[NPC.type];
                int num261 = 0;
                num261 = num260 > 0.65f ? num237 : num260 > 0.5f ? num237 + 1 : num260 > 0.35f ? num237 + 2 : num260 > 0f ? num237 + 3 : 0;
                NPC.frame.Y = num * num261;
            }
            else if (NPC.ai[0] == 25f) {
                NPC.frame.Y = num;
            }
            else if (NPC.ai[0] == 12f) {
                NPC.frameCounter += 1.0;
                int num262 = NPC.frame.Y / num;
                int num17 = num262 - num237;
                if ((uint)num17 > 4u && num262 != 0) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }

                int num263 = num237 + NPC.GetShootingFrame(NPC.ai[2]);
                NPC.frame.Y = num * num263;
            }
            else if (NPC.ai[0] == 14f || NPC.ai[0] == 24f) {
                NPC.frameCounter += 1.0;
                int num264 = NPC.frame.Y / num;
                int num17 = num264 - num237;
                if ((uint)num17 > 1u && num264 != 0) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }

                int num265 = 12;
                int num266 = NPC.frameCounter % num265 * 2.0 < num265 ? num237 : num237 + 1;
                NPC.frame.Y = num * num266;
                if (NPC.ai[0] == 24f) {
                    if (NPC.frameCounter == 60.0) {
                        EmoteBubble.NewBubble(87, new WorldUIAnchor(NPC), 60);
                    }

                    if (NPC.frameCounter == 150.0) {
                        EmoteBubble.NewBubble(3, new WorldUIAnchor(NPC), 90);
                    }

                    if (NPC.frameCounter >= 240.0) {
                        NPC.frame.Y = 0;
                    }
                }
            }
            else if (NPC.ai[0] == 1001f) {
                NPC.frame.Y = num * (num237 - 1);
                NPC.frameCounter = 0.0;
            }
            else if (NPC.CanTalk && (NPC.ai[0] == 3f || NPC.ai[0] == 4f)) {
                NPC.frameCounter += 1.0;
                int num267 = NPC.frame.Y / num;
                int num17 = num237 - num267;
                if ((uint)(num17 - 1) > 1u && (uint)(num17 - 4) > 1u && num267 != 0) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }

                bool flag12 = NPC.ai[0] == 3f;
                int num268 = 0;
                int num269 = 0;
                int num270 = -1;
                int num271 = -1;
                if (NPC.frameCounter < 10.0) {
                    num268 = 0;
                }
                else if (NPC.frameCounter < 16.0) {
                    num268 = num237 - 5;
                }
                else if (NPC.frameCounter < 46.0) {
                    num268 = num237 - 4;
                }
                else if (NPC.frameCounter < 60.0) {
                    num268 = num237 - 5;
                }
                else if (NPC.frameCounter < 216.0) {
                    num268 = 0;
                }
                else if (NPC.frameCounter == 216.0 && Main.netMode != 1) {
                    num270 = 70;
                }
                else if (NPC.frameCounter < 286.0) {
                    num268 = NPC.frameCounter % 12.0 < 6.0 ? num237 - 2 : 0;
                }
                else if (NPC.frameCounter < 320.0) {
                    num268 = 0;
                }
                else if (NPC.frameCounter != 320.0 || Main.netMode == 1) {
                    num268 = NPC.frameCounter < 420.0 ? NPC.frameCounter % 16.0 < 8.0 ? num237 - 2 : 0 : 0;
                }
                else {
                    num270 = 100;
                }

                if (NPC.frameCounter < 70.0) {
                    num269 = 0;
                }
                else if (NPC.frameCounter != 70.0 || Main.netMode == 1) {
                    num269 = NPC.frameCounter < 160.0 ? NPC.frameCounter % 16.0 < 8.0 ? num237 - 2 : 0 :
                        NPC.frameCounter < 166.0 ? num237 - 5 :
                        NPC.frameCounter < 186.0 ? num237 - 4 :
                        NPC.frameCounter < 200.0 ? num237 - 5 :
                        !(NPC.frameCounter < 320.0) ? NPC.frameCounter < 326.0 ? num237 - 1 : 0 : 0;
                }
                else {
                    num271 = 90;
                }

                if (flag12) {
                    NPC nPC = Main.npc[(int)NPC.ai[2]];
                    if (num270 != -1) {
                        EmoteBubble.NewBubbleNPC(new WorldUIAnchor(NPC), num270, new WorldUIAnchor(nPC));
                    }

                    if (num271 != -1 && nPC.CanTalk) {
                        EmoteBubble.NewBubbleNPC(new WorldUIAnchor(nPC), num271, new WorldUIAnchor(NPC));
                    }
                }

                NPC.frame.Y = num * (flag12 ? num268 : num269);
                if (NPC.frameCounter >= 420.0) {
                    NPC.frameCounter = 0.0;
                }
            }
            else if (NPC.CanTalk && (NPC.ai[0] == 16f || NPC.ai[0] == 17f)) {
                NPC.frameCounter += 1.0;
                int num272 = NPC.frame.Y / num;
                int num17 = num237 - num272;
                if ((uint)(num17 - 1) > 1u && (uint)(num17 - 4) > 1u && num272 != 0) {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }

                bool flag13 = NPC.ai[0] == 16f;
                int num273 = 0;
                int num274 = -1;
                if (NPC.frameCounter < 10.0) {
                    num273 = 0;
                }
                else if (NPC.frameCounter < 16.0) {
                    num273 = num237 - 5;
                }
                else if (NPC.frameCounter < 22.0) {
                    num273 = num237 - 4;
                }
                else if (NPC.frameCounter < 28.0) {
                    num273 = num237 - 5;
                }
                else if (NPC.frameCounter < 34.0) {
                    num273 = num237 - 4;
                }
                else if (NPC.frameCounter < 40.0) {
                    num273 = num237 - 5;
                }
                else if (NPC.frameCounter == 40.0 && Main.netMode != 1) {
                    num274 = 45;
                }
                else if (NPC.frameCounter < 70.0) {
                    num273 = num237 - 4;
                }
                else if (NPC.frameCounter < 76.0) {
                    num273 = num237 - 5;
                }
                else if (NPC.frameCounter < 82.0) {
                    num273 = num237 - 4;
                }
                else if (NPC.frameCounter < 88.0) {
                    num273 = num237 - 5;
                }
                else if (NPC.frameCounter < 94.0) {
                    num273 = num237 - 4;
                }
                else if (NPC.frameCounter < 100.0) {
                    num273 = num237 - 5;
                }
                else if (NPC.frameCounter == 100.0 && Main.netMode != 1) {
                    num274 = 45;
                }
                else if (NPC.frameCounter < 130.0) {
                    num273 = num237 - 4;
                }
                else if (NPC.frameCounter < 136.0) {
                    num273 = num237 - 5;
                }
                else if (NPC.frameCounter < 142.0) {
                    num273 = num237 - 4;
                }
                else if (NPC.frameCounter < 148.0) {
                    num273 = num237 - 5;
                }
                else if (NPC.frameCounter < 154.0) {
                    num273 = num237 - 4;
                }
                else if (NPC.frameCounter < 160.0) {
                    num273 = num237 - 5;
                }
                else if (NPC.frameCounter != 160.0 || Main.netMode == 1) {
                    num273 = NPC.frameCounter < 220.0 ? num237 - 4 : NPC.frameCounter < 226.0 ? num237 - 5 : 0;
                }
                else {
                    num274 = 75;
                }

                if (flag13 && num274 != -1) {
                    int num275 = (int)NPC.localAI[2];
                    int num276 = (int)NPC.localAI[3];
                    int num277 = (int)Main.npc[(int)NPC.ai[2]].localAI[3];
                    int num278 = (int)Main.npc[(int)NPC.ai[2]].localAI[2];
                    int num279 = 3 - num275 - num276;
                    int num280 = 0;
                    if (NPC.frameCounter == 40.0) {
                        num280 = 1;
                    }

                    if (NPC.frameCounter == 100.0) {
                        num280 = 2;
                    }

                    if (NPC.frameCounter == 160.0) {
                        num280 = 3;
                    }

                    int num281 = 3 - num280;
                    int num282 = -1;
                    int num283 = 0;
                    while (num282 < 0) {
                        num17 = num283 + 1;
                        num283 = num17;
                        if (num17 >= 100) {
                            break;
                        }

                        num282 = Main.rand.Next(2);
                        if (num282 == 0 && num278 >= num276) {
                            num282 = -1;
                        }

                        if (num282 == 1 && num277 >= num275) {
                            num282 = -1;
                        }

                        if (num282 == -1 && num281 <= num279) {
                            num282 = 2;
                        }
                    }

                    if (num282 == 0) {
                        Main.npc[(int)NPC.ai[2]].localAI[3] += 1f;
                        num277++;
                    }

                    if (num282 == 1) {
                        Main.npc[(int)NPC.ai[2]].localAI[2] += 1f;
                        num278++;
                    }

                    int num284 = Utils.SelectRandom(Main.rand, 38, 37, 36);
                    int num285 = num284;
                    switch (num282) {
                        case 0:
                            switch (num284) {
                                case 38:
                                    num285 = 37;
                                    break;
                                case 37:
                                    num285 = 36;
                                    break;
                                case 36:
                                    num285 = 38;
                                    break;
                            }

                            break;
                        case 1:
                            switch (num284) {
                                case 38:
                                    num285 = 36;
                                    break;
                                case 37:
                                    num285 = 38;
                                    break;
                                case 36:
                                    num285 = 37;
                                    break;
                            }

                            break;
                    }

                    if (num281 == 0) {
                        if (num277 >= 2) {
                            num284 -= 3;
                        }

                        if (num278 >= 2) {
                            num285 -= 3;
                        }
                    }

                    EmoteBubble.NewBubble(num284, new WorldUIAnchor(NPC), num274);
                    EmoteBubble.NewBubble(num285, new WorldUIAnchor(Main.npc[(int)NPC.ai[2]]), num274);
                }

                NPC.frame.Y = num * (flag13 ? num273 : num273);
                if (NPC.frameCounter >= 420.0) {
                    NPC.frameCounter = 0.0;
                }
            }
            else if (NPC.velocity.X == 0f) {
                if (NPC.type == 638) {
                    int num286 = NPC.frame.Y / num;
                    if (num286 > 7) {
                        num286 = 0;
                    }

                    NPC.frameCounter += 1.0;
                    if (NPC.frameCounter > 4.0) {
                        NPC.frameCounter = 0.0;
                        num286++;
                        if (num286 > 7) {
                            num286 = 0;
                        }
                    }

                    NPC.frame.Y = num286 * num;
                }
                else if (NPC.type == 140 || NPC.type == 489) {
                    NPC.frame.Y = num;
                    NPC.frameCounter = 0.0;
                }
                else {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0.0;
                }
            }
            else {
                int num287 = 6;
                if (NPC.type == 632) {
                    num287 = 12;
                }

                if (NPC.type == 534) {
                    num287 = 12;
                }

                if (NPC.type == 638) {
                    num287 = 12;
                }

                if (NPC.type == 656) {
                    num287 = 12;
                }

                if (flag11) {
                    num287 = 12;
                }

                if (NPC.type == 489) {
                    num287 = 8;
                    NPC.frameCounter += Math.Abs(NPC.velocity.X) * 1f;
                    NPC.frameCounter += 0.5;
                }
                else {
                    NPC.frameCounter += Math.Abs(NPC.velocity.X) * 2f;
                    NPC.frameCounter += 1.0;
                }

                if (NPC.type == 462) {
                    num287 = 9;
                }

                int num288 = num * 2;
                if (NPC.type == 638) {
                    num288 = num * 9;
                }

                if (NPC.type == 656) {
                    num288 = num;
                }

                if (flag11) {
                    num288 = num;
                }

                if (NPC.frame.Y < num288) {
                    NPC.frame.Y = num288;
                }

                if (NPC.frameCounter > num287) {
                    NPC.frame.Y += num;
                    NPC.frameCounter = 0.0;
                }

                if (NPC.frame.Y / num >= Main.npcFrameCount[NPC.type] - num236) {
                    NPC.frame.Y = num288;
                }
            }

            return;
        }

        if (NPC.type == 462) {
            NPC.frameCounter = 0.0;
            if (NPC.velocity.Y < 0f) {
                NPC.frame.Y = num;
            }
            else {
                NPC.frame.Y = num * 2;
            }

            return;
        }

        if (flag11) {
            NPC.spriteDirection = -NPC.direction;
            int num289 = NPC.frame.Y / num;
            if (NPC.velocity.Y < 0f) {
                if (num289 < 2 || num289 > 3) {
                    num289 = 2;
                    NPC.frameCounter = -1.0;
                }

                if ((NPC.frameCounter += 1.0) >= 4.0) {
                    NPC.frameCounter = 0.0;
                    num289++;
                    if (num289 >= 3) {
                        num289 = 3;
                    }
                }

                NPC.frame.Y = num289 * num;
            }
            else if (NPC.velocity.Y > 0f) {
                if (num289 < 3 || num289 > 6) {
                    num289 = 3;
                    NPC.frameCounter = -1.0;
                }

                if ((NPC.frameCounter += 1.0) >= 4.0) {
                    NPC.frameCounter = 0.0;
                    num289++;
                    if (num289 >= 6) {
                        num289 = 6;
                    }
                }

                NPC.frame.Y = num289 * num;
            }
        }
        else {
            NPC.frameCounter = 0.0;
            NPC.frame.Y = num;
        }

        if (NPC.type == 489
            || NPC.type == 21
            || NPC.type == 31
            || NPC.type == 294
            || NPC.type == 326
            || NPC.type == 295
            || NPC.type == 296
            || NPC.type == 44
            || NPC.type == 77
            || NPC.type == 120
            || NPC.type == 140
            || NPC.type == 159
            || NPC.type == 167
            || NPC.type == 197
            || NPC.type == 201
            || NPC.type == 202) {
            NPC.frame.Y = 0;
        }

        if (NPC.type == 638) {
            NPC.frame.Y = num * 8;
        }
    }
}