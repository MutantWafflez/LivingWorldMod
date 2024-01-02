using LivingWorldMod.Common.GlobalNPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace LivingWorldMod.Custom.Classes;

public sealed class TownNPCChatModule : TownNPCModule {
    private const int DefaultChatBubbleDuration = (int)(Utilities.Utilities.RealLifeSecond * 5d);

    /// <summary>
    /// The denominator of the fractional chance that a NPC who otherwise can chat will
    /// chat each tick.
    /// </summary>
    private const int ChitChatChanceDenominator = 75;

    /// <summary>
    /// Whether this NPC is currently being talked to by a
    /// player.
    /// </summary>
    public bool IsChattingToPlayer {
        get;
        private set;
    }

    /// <summary>
    /// Whether this NPC is currently chatting to another NPC.
    /// </summary>
    public bool IsChattingToNPC => _currentSentence is not null;

    /// <summary>
    /// Whether this NPC is currently talking to any entity at all.
    /// </summary>
    public bool IsSpeaking => IsChattingToPlayer || IsChattingToNPC;

    private static string GenerateRandomNPCName => Language.SelectRandom(Lang.CreateDialogFilter("NPCName.")).Value;

    public readonly ForgetfulArray<string> chatHistory = new(50);
    private readonly Texture2D _talkTexture;

    private string _currentSentence;
    private int _chatBubbleDuration;
    private int _chatCooldown;

    public TownNPCChatModule(NPC npc, Texture2D talkTexture) : base(npc) {
        _talkTexture = talkTexture;
    }

    public override void Update() {
        // Adapted vanilla code
        IsChattingToPlayer = false;
        for (int i = 0; i < Main.maxPlayers; i++) {
            Player player = Main.player[i];

            if (!player.active || player.talkNPC != npc.whoAmI) {
                continue;
            }

            IsChattingToPlayer = true;

            npc.direction = player.position.X + player.width / 2f < npc.position.X + npc.width / 2f ? -1 : 1;
        }
        // End of adapted code

        if (_currentSentence is not null) {
            // Every other 8 ticks while talking, add the draw call
            if (--_chatBubbleDuration % 16 <= 8) {
                GlobalNPC.SpriteModule.AddDrawRequest(_talkTexture);
            }

            if (_chatBubbleDuration > 0) {
                return;
            }

            _currentSentence = null;
            _chatBubbleDuration = 0;
            _chatCooldown = Main.rand.Next((int)(Utilities.Utilities.RealLifeSecond * 3d), (int)(Utilities.Utilities.RealLifeSecond * 5d));
        }

        if (--_chatCooldown <= 0) {
            _chatCooldown = 0;
        }
        else {
            return;
        }

        if (IsSpeaking
            || !Main.rand.NextBool(ChitChatChanceDenominator)
            || Utilities.Utilities.GetFirstNPC(otherNPC =>
                npc != otherNPC &&
                otherNPC.TryGetGlobalNPC(out TownGlobalNPC otherGlobalNPC)
                && !otherGlobalNPC.ChatModule.IsSpeaking
                && npc.Center.Distance(otherNPC.Center) <= 100f
                && Collision.CanHit(npc.Center, 0, 0, otherNPC.Center, 0, 0)
            ) is not { } chatRecipient
           ) {
            return;
        }

        LocalizedText chatTemplate = Language.SelectRandom(Lang.CreateDialogFilter("Mods.LivingWorldMod.InterTownNPCChat."));
        var chatSubstitutions = new {
            SpeakingNPC = npc.GivenOrTypeName,
            RandomNPCName = GenerateRandomNPCName,
            RandomNPCNameTwo = GenerateRandomNPCName,
            ChatRecipient = chatRecipient.GivenOrTypeName,
            Noun = Language.SelectRandom(Lang.CreateDialogFilter("RandomWorldName_Noun.")).Value.ToLower(),
            Adjective = Language.SelectRandom(Lang.CreateDialogFilter("RandomWorldName_Adjective.")).Value.ToLower(),
            Location = Language.SelectRandom(Lang.CreateDialogFilter("RandomWorldName_Location.")).Value,
            RandomItemName = Language.SelectRandom(Lang.CreateDialogFilter("ItemName.")).Value,
            RandomPlayer = Main.rand.Next(Utilities.Utilities.GetAllPlayers(_ => true)).name
        };

        _currentSentence = chatTemplate.FormatWith(chatSubstitutions);
        _chatBubbleDuration = DefaultChatBubbleDuration;

        chatHistory.Add(_currentSentence);
    }

    public void DoChatDrawing(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (!IsChattingToNPC) {
            return;
        }

        DynamicSpriteFont font = FontAssets.MouseText.Value;
        const float maxWidth = 200f;
        Vector2 textScale = new(0.45f);
        float fadeAlpha = MathHelper.Clamp(_chatBubbleDuration / (float)Utilities.Utilities.RealLifeSecond, 0f, 1f);

        Vector2 textSize = ChatManager.GetStringSize(font, _currentSentence, textScale, maxWidth);
        Vector2 textDrawPos = npc.Top - screenPos + new Vector2(-textSize.X / 2f, -textSize.Y);

        // Manual panel drawing, since we have to deal with variable text sizes. Not perfect, but it's better than nothing
        const int panelPadding = 6;
        const int cornerPixelSize = 2;
        Texture2D chatBottomTexture = ModContent.Request<Texture2D>($"{LWM.SpritePath}UI/TownNPCChats/Chat_Bottom").Value;

        Color topLeftBorderColor = new(71, 83, 156);
        Rectangle topBorderRect = new((int)(textDrawPos.X - panelPadding + cornerPixelSize), (int)(textDrawPos.Y - panelPadding), (int)textSize.X + panelPadding * 2, cornerPixelSize);
        spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            topBorderRect,
            null,
            topLeftBorderColor * fadeAlpha
        );
        Rectangle leftBorderRect = new((int)(textDrawPos.X - panelPadding), (int)(textDrawPos.Y - panelPadding + cornerPixelSize), cornerPixelSize, (int)textSize.Y + panelPadding - 16);
        spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            leftBorderRect,
            null,
            topLeftBorderColor * fadeAlpha
        );

        Color bottomRightBorderColor = new(34, 34, 96);
        Rectangle bottomBorderRect = topBorderRect with { Y = topBorderRect.Y + leftBorderRect.Height + cornerPixelSize };
        spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            bottomBorderRect,
            null,
            bottomRightBorderColor * fadeAlpha
        );
        Rectangle rightBorderRect = leftBorderRect with { X = topBorderRect.X + topBorderRect.Width };
        spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            rightBorderRect,
            null,
            bottomRightBorderColor * fadeAlpha
        );

        Color innerLayerOneColor = new(180, 203, 220);
        Rectangle innerLayerOneRect = new(topBorderRect.X, leftBorderRect.Y, topBorderRect.Width, leftBorderRect.Height);
        spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            innerLayerOneRect,
            null,
            innerLayerOneColor * fadeAlpha
        );
        Color innerLayerTwoColor = new(231, 241, 244);
        Rectangle innerLayerTwoRect = innerLayerOneRect.Modified(cornerPixelSize, cornerPixelSize, cornerPixelSize * -2, cornerPixelSize * -2);
        spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            innerLayerTwoRect,
            null,
            innerLayerTwoColor * fadeAlpha
        );

        spriteBatch.Draw(
            chatBottomTexture,
            new Vector2(npc.Center.X - screenPos.X - chatBottomTexture.Width / 2f, bottomBorderRect.Y),
            null,
            Color.White * fadeAlpha
        );

        ChatManager.DrawColorCodedStringWithShadow(
            spriteBatch,
            font,
            _currentSentence,
            textDrawPos,
            Color.LightGray * fadeAlpha,
            0f,
            Vector2.Zero,
            textScale,
            maxWidth,
            1.05f
        );
    }
}