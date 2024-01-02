using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace LivingWorldMod.Custom.Classes;

/// <summary>
/// Module for Town NPCs that deal with drawing related tasks.
/// </summary>
public sealed class TownNPCSpriteModule : TownNPCModule {
    private const int EyelidClosedDuration = 15;
    private readonly Texture2D _blinkTexture;

    private readonly HashSet<Texture2D> _drawSet;

    private bool _isBlinking;
    private int _blinkTimer;

    public TownNPCSpriteModule(NPC npc, Texture2D blinkTexture) : base(npc) {
        _blinkTexture = blinkTexture;
        _drawSet = new HashSet<Texture2D>();
    }

    public override void Update() {
        _drawSet.Clear();

        if (Main.netMode != NetmodeID.Server) {
            UpdateHead();
        }
    }

    public void AddDrawRequest(Texture2D request) {
        _drawSet.Add(request);
    }

    public void RequestBlink(int duration = EyelidClosedDuration) {
        _isBlinking = true;
        _blinkTimer = duration;
    }

    public void DrawOntoNPC(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (!TownNPCProfiles.Instance.GetProfile(npc, out ITownNPCProfile profile)) {
            return;
        }
        Asset<Texture2D> npcAsset = profile.GetTextureNPCShouldUse(npc);

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

    private void UpdateHead() {
        if (!_isBlinking) {
            if (--_blinkTimer > 0) {
                return;
            }

            RequestBlink();
        }
        else if (--_blinkTimer <= 0) {
            _isBlinking = false;
            _blinkTimer = Main.rand.Next(180, 360);
        }

        if (_isBlinking) {
            AddDrawRequest(_blinkTexture);
        }
    }
}