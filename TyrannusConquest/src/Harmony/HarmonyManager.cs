using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using static Ele.TyrannusConquest.ModConstants;
using HarmonyLib;
using Vintagestory.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Common.Database;
using Vintagestory.API.Config;

namespace Ele.TyrannusConquest;

public class HarmonyManager : ModSystem
{
    private ICoreAPI _api;
    private ICoreClientAPI _capi;
    private ICoreServerAPI _sapi;
    private Harmony _harmony;
    

    public override double ExecuteOrder() => 0.03;
    public override void Start(ICoreAPI api)
    {
        _api = api;
        _harmony = new Harmony(harmonyID);
        _harmony?.PatchAll();
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ChunkMapLayer), "GenerateChunkImage")]
    public virtual bool GenerateChunkImage__Prefix(ref IWorldChunk[] ___chunksTmp, 
        ChunkMapLayer __instance, bool __result, Vec2i chunkPos, IMapChunk mc, 
        ref int[] tintedImage, ref string failureCode, bool colorAccurate = false)
    {
        BlockPos tmpPos = new BlockPos(0);
        Vec2i localpos = new Vec2i();
        const int chunksize = GlobalConstants.ChunkSize;

        // Prefetch chunks
        for (int cy = 0; cy < ___chunksTmp.Length; cy++)
        {
            ___chunksTmp[cy] = _sapi.World.BlockAccessor.GetChunk(chunkPos.X, cy, chunkPos.Y);
            if (___chunksTmp[cy] == null || !(___chunksTmp[cy] as IClientChunk).LoadedFromServer) return false;
        }

        // Prefetch map chunks
        IMapChunk[] mapChunks = new IMapChunk[]
        {
            _sapi.World.BlockAccessor.GetMapChunk(chunkPos.X - 1, chunkPos.Y - 1),
            _sapi.World.BlockAccessor.GetMapChunk(chunkPos.X - 1, chunkPos.Y),
            _sapi.World.BlockAccessor.GetMapChunk(chunkPos.X, chunkPos.Y - 1)
        };

        byte[] shadowMap = new byte[tintedImage.Length];
        for (int i = 0; i < shadowMap.Length; i++) shadowMap[i] = 128;

        for (int i = 0; i < tintedImage.Length; i++)
        {
            int y = mc.RainHeightMap[i];
            int cy = y / chunksize;
            if (cy >= ___chunksTmp.Length) continue;

            MapUtil.PosInt2d(i, chunksize, localpos);
            int lx = localpos.X;
            int lz = localpos.Y;

            float b = 1;
            int leftTop, rightTop, leftBot;

            IMapChunk leftTopMapChunk = mc;
            IMapChunk rightTopMapChunk = mc;
            IMapChunk leftBotMapChunk = mc;

            int topX = lx - 1;
            int botX = lx;
            int leftZ = lz - 1;
            int rightZ = lz;

            if (topX < 0 && leftZ < 0)
            {
                leftTopMapChunk = mapChunks[0];
                rightTopMapChunk = mapChunks[1];
                leftBotMapChunk = mapChunks[2];
            }
            else
            {
                if (topX < 0)
                {
                    leftTopMapChunk = mapChunks[1];
                    rightTopMapChunk = mapChunks[1];
                }
                if (leftZ < 0)
                {
                    leftTopMapChunk = mapChunks[2];
                    leftBotMapChunk = mapChunks[2];
                }
            }

            topX = GameMath.Mod(topX, chunksize);
            leftZ = GameMath.Mod(leftZ, chunksize);

            leftTop = leftTopMapChunk == null ? 0 : (y - leftTopMapChunk.RainHeightMap[leftZ * chunksize + topX]);
            rightTop = rightTopMapChunk == null ? 0 : (y - rightTopMapChunk.RainHeightMap[rightZ * chunksize + topX]);
            leftBot = leftBotMapChunk == null ? 0 : (y - leftBotMapChunk.RainHeightMap[leftZ * chunksize + botX]);

            float slopedir = (Math.Sign(leftTop) + Math.Sign(rightTop) + Math.Sign(leftBot));
            float steepness = Math.Max(Math.Max(Math.Abs(leftTop), Math.Abs(rightTop)), Math.Abs(leftBot));

            int blockId = ___chunksTmp[cy].UnpackAndReadBlock(MapUtil.Index3d(lx, y % chunksize, lz, chunksize, chunksize), BlockLayersAccess.FluidOrSolid);
            Block block = _api.World.Blocks[blockId];

            if (slopedir > 0) b = 1.08f + Math.Min(0.5f, steepness / 10f) / 1.25f;
            if (slopedir < 0) b = 0.92f - Math.Min(0.5f, steepness / 10f) / 1.25f;

            if (block.BlockMaterial == EnumBlockMaterial.Snow && !colorAccurate)
            {
                y--;
                cy = y / chunksize;
                blockId = ___chunksTmp[cy].UnpackAndReadBlock(MapUtil.Index3d(localpos.X, y % chunksize, localpos.Y, chunksize, chunksize), BlockLayersAccess.FluidOrSolid);
                block = _api.World.Blocks[blockId];
            }
            tmpPos.Set(chunksize * chunkPos.X + localpos.X, y, chunksize * chunkPos.Y + localpos.Y);

            if (colorAccurate)
            {
                int avgCol = block.GetColor(_capi, tmpPos);
                int rndCol = block.GetRandomColor(_capi, tmpPos, BlockFacing.UP, GameMath.MurmurHash3Mod(tmpPos.X, tmpPos.Y, tmpPos.Z, 30));
                // Why the eff is r and b flipped
                rndCol = ((rndCol & 0xff) << 16) | (((rndCol >> 8) & 0xff) << 8) | (((rndCol >> 16) & 0xff) << 0);

                // Add a bit of randomness to each pixel
                int col = ColorUtil.ColorOverlay(avgCol, avgCol, 0.6f); //avgCol, rndCol, 0.6f

                tintedImage[i] = rndCol;
                shadowMap[i] = (byte)(shadowMap[i] * b);
            }
            else
            {

                if (isLake(block))
                {
                    // Water
                    IWorldChunk lChunk = ___chunksTmp[cy];
                    IWorldChunk rChunk = ___chunksTmp[cy];
                    IWorldChunk tChunk = ___chunksTmp[cy];
                    IWorldChunk bChunk = ___chunksTmp[cy];

                    int leftX = localpos.X - 1;
                    int rightX = localpos.X + 1;
                    int topY = localpos.Y - 1;
                    int bottomY = localpos.Y + 1;

                    if (leftX < 0)
                    {
                        lChunk = _capi.World.BlockAccessor.GetChunk(chunkPos.X - 1, cy, chunkPos.Y);
                    }
                    if (rightX >= chunksize)
                    {
                        rChunk = _capi.World.BlockAccessor.GetChunk(chunkPos.X + 1, cy, chunkPos.Y);
                    }
                    if (topY < 0)
                    {
                        tChunk = _capi.World.BlockAccessor.GetChunk(chunkPos.X, cy, chunkPos.Y - 1);
                    }
                    if (bottomY >= chunksize)
                    {
                        bChunk = _capi.World.BlockAccessor.GetChunk(chunkPos.X, cy, chunkPos.Y + 1);
                    }

                    if (lChunk != null && rChunk != null && tChunk != null && bChunk != null)
                    {
                        leftX = GameMath.Mod(leftX, chunksize);
                        rightX = GameMath.Mod(rightX, chunksize);
                        topY = GameMath.Mod(topY, chunksize);
                        bottomY = GameMath.Mod(bottomY, chunksize);

                        Block lBlock = _api.World.Blocks[lChunk.UnpackAndReadBlock(MapUtil.Index3d(leftX, y % chunksize, localpos.Y, chunksize, chunksize), BlockLayersAccess.FluidOrSolid)];
                        Block rBlock = _api.World.Blocks[rChunk.UnpackAndReadBlock(MapUtil.Index3d(rightX, y % chunksize, localpos.Y, chunksize, chunksize), BlockLayersAccess.FluidOrSolid)];
                        Block tBlock = _api.World.Blocks[tChunk.UnpackAndReadBlock(MapUtil.Index3d(localpos.X, y % chunksize, topY, chunksize, chunksize), BlockLayersAccess.FluidOrSolid)];
                        Block bBlock = _api.World.Blocks[bChunk.UnpackAndReadBlock(MapUtil.Index3d(localpos.X, y % chunksize, bottomY, chunksize, chunksize), BlockLayersAccess.FluidOrSolid)];

                        if (isLake(lBlock) && isLake(rBlock) && isLake(tBlock) && isLake(bBlock))
                        {
                            tintedImage[i] = getColor(block, localpos.X, y, localpos.Y);
                        }
                        else
                        {
                            tintedImage[i] = __instance.colorsByCode["glacier"];
                        }
                    }
                    else
                    {
                        // Default to water until chunks are loaded.
                        tintedImage[i] = getColor(block, localpos.X, y, localpos.Y);
                    }
                }
                else
                {
                    shadowMap[i] = (byte)(shadowMap[i] * b);
                    tintedImage[i] = getColor(block, localpos.X, y, localpos.Y);
                }
            }



        }


        byte[] bla = new byte[shadowMap.Length];
        for (int i = 0; i < bla.Length; i++) bla[i] = shadowMap[i];

        BlurTool.Blur(shadowMap, 32, 32, 2);
        float sharpen = 1.0f;

        for (int i = 0; i < shadowMap.Length; i++)
        {
            float b = ((int)((shadowMap[i] / 128f - 1f) * 5)) / 5f;
            b += (((bla[i] / 128f - 1f) * 5) % 1) / 5f;

            tintedImage[i] = ColorUtil.ColorMultiply3Clamped(tintedImage[i], b * sharpen + 1f) | 255 << 24;
        }

        for (int cy = 0; cy < ___chunksTmp.Length; cy++) ___chunksTmp[cy] = null;

        return false;
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(ChunkMapLayer), "isLake")]
    public static bool isLake(Block block) =>
        throw new NotImplementedException("Reverse patch not applied yet.");

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(ChunkMapLayer), "getColor")]
    public int getColor(Block block, int x, int y1, int y2) =>
        throw new NotImplementedException("Reverse patch not applied yet.");
    public override void StartServerSide(ICoreServerAPI sapi)
    {
        base.StartServerSide(sapi);
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        base.StartClientSide(capi);
        _capi = capi;
    }

    public override void Dispose()
    {
        _harmony?.UnpatchAll(harmonyID);
    }
}