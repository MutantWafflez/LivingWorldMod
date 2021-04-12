using Microsoft.Xna.Framework.Audio;
using Terraria.ModLoader;

namespace LivingWorldMod.Audio.Sounds
{
    public class SpiderSacBurst : ModSound
    {
        public override SoundEffectInstance PlaySound(ref SoundEffectInstance soundInstance, float volume, float pan, SoundType type)
        {
            soundInstance = sound.CreateInstance();
            soundInstance.Volume = volume * 0.33f;
            soundInstance.Pan = pan;
            return soundInstance;
        }
    }
}