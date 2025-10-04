using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Classes;
using LivingWorldMod.Content.TownNPCRevitalization.DataStructures.Enums;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.BaseTypes.NPCs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Configs;
using LivingWorldMod.Content.TownNPCRevitalization.Globals.Hooks;
using LivingWorldMod.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace LivingWorldMod.Content.TownNPCRevitalization.Globals.TownNPCModules;

/// <summary>
///     Module that handles various aspects of the social-ness of Town NPCs. Involves code for the player talking to this NPC, and for the flavor animations of "talking" to other Town NPCs.
/// </summary>
public sealed class TownNPCChatModule : TownNPCModule, IUpdateSleep {
    private sealed class ChatModuleGlobalEmote : GlobalEmoteBubble {
        public override bool PreDraw(EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects) =>
            // Only draw emotes when player is zoomed OUT far enough
            !(emoteBubble.anchor.entity is NPC npc && npc.TryGetGlobalNPC(out TownNPCChatModule chatModule) && chatModule.IsChattingToEntityInBackground && HasMinimumZoomToSeeWords());
    }

    private const int DefaultChatBubbleDuration = LWMUtils.RealLifeSecond * 5;

    /// <summary>
    ///     The denominator of the fractional chance that a NPC who otherwise can chat will
    ///     chat each tick.
    /// </summary>
    private const int ChitChatChanceDenominator = 75;

    private static LocalizedTextGroup _chatTemplateGroup;
    private static LocalizedTextGroup _npcNameGroup;
    private static LocalizedTextGroup _nounGroup;
    private static LocalizedTextGroup _adjectiveGroup;
    private static LocalizedTextGroup _locationGroup;
    private static LocalizedTextGroup _itemNameGroup;

    // public readonly ForgetfulArray<string> chatHistory = new(50);

    private string _currentSentence;
    private int _chatBubbleDuration;
    private int _chatCooldown;
    private int _chatReceptionCooldown;

    public override int UpdatePriority => -1;

    /// <summary>
    ///     Whether this NPC is currently being talked to by a player.
    /// </summary>
    public bool IsChattingWithPlayerDirectly {
        get;
        private set;
    }

    /// <summary>
    ///     Whether this NPC is currently chatting to another entity in the background, i.e with the chat bubbles.
    /// </summary>
    public bool IsChattingToEntityInBackground => _currentSentence is not null;

    /// <summary>
    ///     Whether this NPC is currently talking to any entity at all.
    /// </summary>
    public bool IsSpeaking => IsChattingWithPlayerDirectly || IsChattingToEntityInBackground;

    private static bool HasMinimumZoomToSeeWords() {
        Vector2 currentZoom = Main.GameViewMatrix.Zoom;
        Vector2 zoomRequired = new (ModContent.GetInstance<RevitalizationConfigClient>().minimumZoomToSeeSmallTalk);

        return currentZoom.X >= zoomRequired.X && currentZoom.Y >= zoomRequired.Y;
    }

    public override void SetStaticDefaults() {
        _chatTemplateGroup = new LocalizedTextGroup(Lang.CreateDialogFilter("Mods.LivingWorldMod.InterTownNPCChat."));
        _npcNameGroup = new LocalizedTextGroup(Lang.CreateDialogFilter("NPCName."));
        _nounGroup = new LocalizedTextGroup(Lang.CreateDialogFilter("RandomWorldName_Noun."));
        _adjectiveGroup = new LocalizedTextGroup(Lang.CreateDialogFilter("RandomWorldName_Adjective."));
        _locationGroup = new LocalizedTextGroup(Lang.CreateDialogFilter("RandomWorldName_Location."));
        _itemNameGroup = new LocalizedTextGroup(Lang.CreateDialogFilter("ItemName."));
    }

    public override void UpdateModule() {
        // Adapted vanilla code
        IsChattingWithPlayerDirectly = false;
        for (int i = 0; i < Main.maxPlayers; i++) {
            Player player = Main.player[i];

            if (!player.active || player.talkNPC != NPC.whoAmI) {
                continue;
            }

            IsChattingWithPlayerDirectly = true;

            NPC.direction = player.position.X + player.width / 2f < NPC.position.X + NPC.width / 2f ? -1 : 1;
            NPC.GetGlobalNPC<TownNPCPathfinderModule>().PausePathfind();
        }
        // End of adapted code

        if (Main.netMode == NetmodeID.Server) {
            return;
        }

        if (--_chatReceptionCooldown <= 0) {
            _chatReceptionCooldown = 0;
        }

        if (_currentSentence is not null) {
            IUpdateTownNPCSmallTalk.Invoke(NPC, --_chatBubbleDuration);

            if (_chatBubbleDuration > 0) {
                return;
            }

            _currentSentence = null;
            _chatBubbleDuration = 0;
            _chatCooldown = Main.rand.Next(LWMUtils.RealLifeSecond * 3, LWMUtils.RealLifeSecond * 5);
        }

        if (--_chatCooldown <= 0) {
            _chatCooldown = 0;
        }
        else {
            return;
        }

        TownNPCChatModule otherChatModule = null;
        if (IsSpeaking
            || !Main.rand.NextBool(ChitChatChanceDenominator)
            || LWMUtils.GetFirstNPC(otherNPC =>
                NPC != otherNPC
                && otherNPC.TryGetGlobalNPC(out otherChatModule)
                && !otherChatModule.IsSpeaking
                && NPC.Center.Distance(otherNPC.Center) <= 100f
                && Collision.CanHit(NPC.Center, 0, 0, otherNPC.Center, 0, 0)
            ) is not { } chatRecipient
            || chatRecipient.GetGlobalNPC<TownNPCChatModule>()._chatReceptionCooldown > 0
        ) {
            return;
        }

        LocalizedText chatTemplate = _chatTemplateGroup.RandomText;
        var chatSubstitutions = new {
            SpeakingNPC = NPC.GivenOrTypeName,
            RandomNPCName = _npcNameGroup.RandomText,
            RandomNPCNameTwo = _npcNameGroup.RandomText,
            ChatRecipient = chatRecipient.GivenOrTypeName,
            Noun = _nounGroup.RandomText.Value.ToLower(),
            Adjective = _adjectiveGroup.RandomText.Value.ToLower(),
            Location = _locationGroup.RandomText,
            RandomItemName = _itemNameGroup.RandomText,
            RandomPlayer = Main.rand.Next(LWMUtils.GetAllPlayers(_ => true)).name
        };

        _currentSentence = chatTemplate.FormatWith(chatSubstitutions);
        _chatBubbleDuration = DefaultChatBubbleDuration;
        EmoteBubble.NewBubbleNPC(new WorldUIAnchor(NPC), DefaultChatBubbleDuration, new WorldUIAnchor(chatRecipient));
        otherChatModule._chatReceptionCooldown = _chatBubbleDuration + LWMUtils.RealLifeSecond;

        // chatHistory.Add(_currentSentence);
    }

    // TODO: Re-write chat bubble drawing
    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (!IsChattingToEntityInBackground || !HasMinimumZoomToSeeWords()) {
            return;
        }

        DynamicSpriteFont font = FontAssets.MouseText.Value;
        const float maxWidth = 200f;
        Vector2 textScale = new(0.45f);
        float fadeAlpha = MathHelper.Clamp(_chatBubbleDuration / (float)LWMUtils.RealLifeSecond, 0f, 1f);

        Vector2 textSize = ChatManager.GetStringSize(font, _currentSentence, textScale, maxWidth);
        Vector2 textDrawPos = NPC.Top - screenPos + new Vector2(-textSize.X / 2f, -textSize.Y);

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
            new Vector2(NPC.Center.X - screenPos.X - chatBottomTexture.Width / 2f, bottomBorderRect.Y),
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

    /// <summary>
    ///     Disables this NPC from chatting with other NPCs for the specified duration, in ticks.
    /// </summary>
    /// <remarks>
    ///     This only prevents new chats from occuring, and won't cancel chats that have already started.
    /// </remarks>
    public void DisableChatting(int duration) {
        _chatCooldown = duration;
    }

    /// <summary>
    ///     Disables the ability for other NPCs to chat with this NPC for the specified duration, in ticks.
    /// </summary>
    public void DisableChatReception(int duration) {
        _chatReceptionCooldown = duration;
    }

    public void UpdateSleep(NPC npc, Vector2? drawOffset, NPCRestType restType) {
        if (Main.netMode == NetmodeID.Server) {
            return;
        }

        DisableChatting(LWMUtils.RealLifeSecond);
        DisableChatReception(LWMUtils.RealLifeSecond);
    }
}