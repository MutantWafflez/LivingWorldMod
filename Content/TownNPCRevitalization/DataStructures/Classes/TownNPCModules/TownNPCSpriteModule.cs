using System.Collections.Generic;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Structs;
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
    private const int GivingAnimationDuration = (int)(LWMUtils.RealLifeSecond * 1.5f);
    private const int EyelidClosedDuration = 15;
    private const int TalkDuration = 8;

    private const int TalkTextureIndex = 0;
    private const int EyelidTextureIndex = 1;

    private readonly List<TownNPCDrawData> _drawRequests = [];

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

        for (int i = 0; i < _drawRequests.Count; i++) {
            TownNPCDrawData drawRequest = _drawRequests[i];
            if (drawRequest.drawDuration-- <= 0) {
                _drawRequests.RemoveAt(i--);
            }
            else {
                _drawRequests[i] = drawRequest;
            }
        }

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
    public void RequestDraw(in TownNPCDrawData request) {
        _drawRequests.Add(request);
    }

    /// <summary>
    ///     Offsets the base draw position of the NPC, and any subsequent layers/overlays.
    /// </summary>
    public void OffsetDrawPosition(Vector2 drawOffset) {
        _drawOffset = drawOffset;
    }

    public void DrawNPC(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Asset<Texture2D> npcAsset = TownNPCProfiles.Instance.GetProfile(npc, out ITownNPCProfile profile) ? profile.GetTextureNPCShouldUse(npc) : TextureAssets.Npc[npc.type];
        int frameWidth = npcAsset.Width();
        int frameHeight = npcAsset.Height() / Main.npcFrameCount[npc.type];
        Vector2 halfSize = new(frameWidth / 2, frameHeight / 2);

        float npcAddHeight = Main.NPCAddHeight(npc);
        Vector2 drawPos = new (
            npc.position.X - screenPos.X + npc.width / 2 - frameWidth * npc.scale / 2f + halfSize.X * npc.scale + _drawOffset.X,
            npc.position.Y - screenPos.Y + npc.height - frameHeight * npc.scale + 4f + halfSize.Y * npc.scale + npcAddHeight /*+ num35*/ + npc.gfxOffY + _drawOffset.Y
        );

        drawColor = npc.GetNPCColorTintedByBuffs(drawColor);
        Color shiftedDrawColor = npc.color == default(Color) ? npc.GetAlpha(drawColor) : npc.GetColor(drawColor);
        SpriteEffects spriteEffects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
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

        Main.instance.DrawNPCExtras(npc, true, npcAddHeight, 0f, shiftedDrawColor, halfSize, spriteEffects, screenPos);
        (defaultDrawData with { texture = npcAsset.Value, sourceRect = npc.frame }).Draw(spriteBatch);
        Main.instance.DrawNPCExtras(npc, false, npcAddHeight, 0f, shiftedDrawColor, halfSize, spriteEffects, screenPos);

        foreach (TownNPCDrawData drawRequest in _drawRequests) {
            ref readonly DrawData drawData = ref drawRequest.drawData;
            (drawData with { position = drawData.position + defaultDrawData.position }).Draw(spriteBatch);
        }
    }

    public void FrameNPC(int frameHeight) {
        if (_frameYOverride >= 0) {
            npc.frame.Y = frameHeight * _frameYOverride;
        }
    }

    private DrawData GetOverlayDrawData(int overlayIndex) => new (
        TownNPCDataSystem.spriteOverlayProfiles[npc.type].GetCurrentSpriteOverlay(npc, overlayIndex),
        Vector2.Zero,
        Color.White
    );

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
            RequestDraw(GetOverlayDrawData(EyelidTextureIndex));
        }

        if (IsTalking) {
            RequestDraw(GetOverlayDrawData(TalkTextureIndex));
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
        Texture2D value = TextureAssets.Projectile[ProjectileID.MechanicWrench].Value;
        if (npc.townNpcVariationIndex == 1) {
            value = TextureAssets.Extra[263].Value;
        }

        Vector2 offset = Vector2.Zero;
        offset -= new Vector2(value.Width, value.Height / Main.npcFrameCount[npc.type]) * npc.scale / 2f;
        offset += new Vector2(0f, y);
        offset += new Vector2(-npc.spriteDirection * 2, -2f);
        RequestDraw(new DrawData(value, offset, Color.White));
    }
}