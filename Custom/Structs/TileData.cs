using LivingWorldMod.Content.Tiles.DebugTiles;
using LivingWorldMod.Content.Walls.DebugWalls;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LivingWorldMod.Custom.Structs {

    /// <summary>
    /// Struct that holds Tile Data, mainly usage being for structures. Used for generating
    /// pre-determined structures.
    /// </summary>
    public readonly struct TileData : TagSerializable {
        /* Relevant Tile Fields/Properties:
            tile.type;
            IsActivated
            IsHalfBlock;
            FrameNumber;
            frameX;
            frameY;
            tile.Slope;
            tile.Color;
            tile.IsActuated;
            tile.HasActuator;
            tile.RedWire;
            tile.BlueWire;
            tile.GreenWire;
            tile.YellowWire;
            tile.LiquidType;
            tile.LiquidAmount;
            tile.wall;
            tile.WallColor;
            tile.WallFrameNumber;
            ile.WallFrameX;
            tile.WallFrameY;
            */

        public static readonly Func<TagCompound, TileData> DESERIALIZER = Deserialize;

        public readonly int type;

        public readonly bool isActivated;

        public readonly bool isHalfBlock;

        public readonly int frameNumber;

        public readonly int frameX;

        public readonly int frameY;

        public readonly int slopeType;

        public readonly int color;

        public readonly bool isActuated;

        public readonly bool hasActuator;

        public readonly bool hasRedWire;

        public readonly bool hasBlueWire;

        public readonly bool hasGreenWire;

        public readonly bool hasYellowWire;

        public readonly int liquidType;

        public readonly int liquidAmount;

        public readonly int wallType;

        public readonly int wallColor;

        public readonly int wallFrame;

        public readonly int wallFrameX;

        public readonly int wallFrameY;

        public readonly string modName;

        public readonly string modTileName;

        public readonly string modWallName;

        public TileData(Tile tile) {
            type = tile.IsActive ? tile.type : -1;
#if DEBUG
            type = tile.type == ModContent.TileType<SkipTile>() ? -2 : type;
#endif
            isActivated = tile.IsActive;
            isHalfBlock = tile.IsHalfBlock;
            frameNumber = tile.FrameNumber;
            frameX = tile.frameX;
            frameY = tile.frameY;
            slopeType = (int)tile.Slope;
            color = tile.Color;
            isActuated = tile.IsActuated;
            hasActuator = tile.HasActuator;
            hasRedWire = tile.RedWire;
            hasBlueWire = tile.BlueWire;
            hasGreenWire = tile.GreenWire;
            hasYellowWire = tile.YellowWire;
            liquidType = tile.LiquidType;
            liquidAmount = tile.LiquidAmount;
#if DEBUG
            wallType = tile.wall == ModContent.WallType<SkipWall>() ? -1 : tile.wall;
#else
            wallType = tile.wall;
#endif
            wallColor = tile.WallColor;
            wallFrame = tile.WallFrameNumber;
            wallFrameX = tile.WallFrameX;
            wallFrameY = tile.WallFrameY;

            ModTile modTile = ModContent.GetModTile(type);
            ModWall modWall = ModContent.GetModWall(wallType);
            modName = modTile?.Mod.Name;
            modTileName = modTile?.Name;
            modWallName = modWall?.Name;
        }

        public TileData(int type, bool isActivated, bool isHalfBlock, int frameNumber, int frameX, int frameY, int slopeType, int color, bool isActuated, bool hasActuator,
            bool hasRedWire, bool hasBlueWire, bool hasGreenWire, bool hasYellowWire, int liquidType, int liquidAmount, int wallType, int wallColor, int wallFrame,
            int wallFrameX, int wallFrameY) {
            this.type = isActivated ? type : -1;
#if DEBUG
            this.type = type == ModContent.TileType<SkipTile>() ? -2 : this.type;
#endif
            this.isActivated = isActivated;
            this.isHalfBlock = isHalfBlock;
            this.frameNumber = frameNumber;
            this.frameX = frameX;
            this.frameY = frameY;
            this.slopeType = slopeType;
            this.color = color;
            this.isActuated = isActuated;
            this.hasActuator = hasActuator;
            this.hasRedWire = hasRedWire;
            this.hasBlueWire = hasBlueWire;
            this.hasGreenWire = hasGreenWire;
            this.hasYellowWire = hasYellowWire;
            this.liquidType = liquidType;
            this.liquidAmount = liquidAmount;
#if DEBUG
            this.wallType = wallType == ModContent.WallType<SkipWall>() ? -1 : wallType;
#else
            this.wallType = wallType;
#endif
            this.wallColor = wallColor;
            this.wallFrame = wallFrame;
            this.wallFrameX = wallFrameX;
            this.wallFrameY = wallFrameY;

            ModTile modTile = ModContent.GetModTile(this.type);
            ModWall modWall = ModContent.GetModWall(this.wallType);
            modName = modTile?.Mod.Name;
            modTileName = modTile?.Name;
            modWallName = modWall?.Name;
        }

        public TileData(int type, bool isActivated, bool isHalfBlock, int frameNumber, int frameX, int frameY, int slopeType, int color, bool isActuated, bool hasActuator,
            bool hasRedWire, bool hasBlueWire, bool hasGreenWire, bool hasYellowWire, int liquidType, int liquidAmount, int wallType, int wallColor, int wallFrame,
            int wallFrameX, int wallFrameY, string modName, string modTileName, string modWallName) {
            this.type = isActivated ? type : -1;
#if DEBUG
            this.type = type == ModContent.TileType<SkipTile>() ? -2 : this.type;
#endif
            this.isActivated = isActivated;
            this.isHalfBlock = isHalfBlock;
            this.frameNumber = frameNumber;
            this.frameX = frameX;
            this.frameY = frameY;
            this.slopeType = slopeType;
            this.color = color;
            this.isActuated = isActuated;
            this.hasActuator = hasActuator;
            this.hasRedWire = hasRedWire;
            this.hasBlueWire = hasBlueWire;
            this.hasGreenWire = hasGreenWire;
            this.hasYellowWire = hasYellowWire;
            this.liquidType = liquidType;
            this.liquidAmount = liquidAmount;
#if DEBUG
            this.wallType = wallType == ModContent.WallType<SkipWall>() ? -1 : wallType;
#else
            this.wallType = wallType;
#endif
            this.wallColor = wallColor;
            this.wallFrame = wallFrame;
            this.wallFrameX = wallFrameX;
            this.wallFrameY = wallFrameY;
            this.modName = modName;
            this.modTileName = modTileName;
            this.modWallName = modWallName;
        }

        public static TileData Deserialize(TagCompound tag) {
            return new TileData(
                tag.GetInt(nameof(type)),
                tag.GetBool(nameof(isActivated)),
                tag.GetBool(nameof(isHalfBlock)),
                tag.GetInt(nameof(frameNumber)),
                tag.GetInt(nameof(frameX)),
                tag.GetInt(nameof(frameY)),
                tag.GetInt(nameof(slopeType)),
                tag.GetInt(nameof(color)),
                tag.GetBool(nameof(isActuated)),
                tag.GetBool(nameof(hasActuator)),
                tag.GetBool(nameof(hasRedWire)),
                tag.GetBool(nameof(hasBlueWire)),
                tag.GetBool(nameof(hasGreenWire)),
                tag.GetBool(nameof(hasYellowWire)),
                tag.GetInt(nameof(liquidType)),
                tag.GetInt(nameof(liquidAmount)),
                tag.GetInt(nameof(wallType)),
                tag.GetInt(nameof(wallColor)),
                tag.GetInt(nameof(wallFrame)),
                tag.GetInt(nameof(wallFrameX)),
                tag.GetInt(nameof(wallFrameY)),
                tag.GetString(nameof(modName)),
                tag.GetString(nameof(modTileName)),
                tag.GetString(nameof(modWallName))
            );
        }

        public TagCompound SerializeData() {
            return new TagCompound() {
                {nameof(type), type},
                {nameof(isActivated), isActivated},
                {nameof(isHalfBlock), isHalfBlock},
                {nameof(frameNumber), frameNumber},
                {nameof(frameX), frameX},
                {nameof(frameY), frameY},
                {nameof(slopeType), slopeType},
                {nameof(color), color},
                {nameof(isActuated), isActuated},
                {nameof(hasActuator), hasActuator},
                {nameof(hasRedWire), hasRedWire},
                {nameof(hasBlueWire), hasBlueWire},
                {nameof(hasGreenWire), hasGreenWire},
                {nameof(hasYellowWire), hasYellowWire},
                {nameof(liquidType), liquidType},
                {nameof(liquidAmount), liquidAmount},
                {nameof(wallType), wallType},
                {nameof(wallColor), wallColor},
                {nameof(wallFrame), wallFrame},
                {nameof(wallFrameX), wallFrameX},
                {nameof(wallFrameY), wallFrameY},
                {nameof(modName), modName},
                {nameof(modTileName), modTileName},
                {nameof(modWallName), modWallName}
            };
        }
    }
}