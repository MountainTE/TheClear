using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.DataStructures;


namespace TheClear;

[ApiVersion(2, 1)]
public class Plugin : TerrariaPlugin
{
    public override string Name => "TheClear";
    public override string Author => "DXYL";
    public override string Description => "None";
    public override Version Version => new Version(1, 0);

    public Plugin(Main game) : base(game)
    {
    }

    public override void Initialize()
    {
        Commands.ChatCommands.Add(new Command("TCR.clear", clear, "tclear", "tc"));
        Commands.ChatCommands.Add(new Command("TCR.clear", build, "tbuild", "tb"));
        Commands.ChatCommands.Add(new Command("TCR.clear", builder, "tbuilder", "tbr"));
    }

    private void builder(CommandArgs args)
    {
        var site = args.Parameters.ConvertAll(int.Parse);
        ITile tile = Main.tile[site[0], site[1]];
        TSPlayer.All.SendMessage($"{tile.leftSlope()}",Color.Red);
    }
    
    private void clear(CommandArgs args)
    {
        TSPlayer.All.SendMessage($"{args.Player.TPlayer.Center}",Color.Red);
        if (args.Parameters[0] == null)
        {
            args.Player.SendMessage("示例/tc 100 100 200 200\n意为清理坐标(100,100) 至(200,200的区域)哦~~~", Color.Red);
        }
        if (args.Parameters.Count > 4||args.Parameters.Count<4)
        {
            args.Player.SendMessage("示例/tc 100 100 200 200\n意为清理坐标(100,100) 至(200,200的区域)", Color.Red);
            return;
        }
        

        if (args.Parameters.Count == 4)
        {
            var site = args.Parameters.ConvertAll(int.Parse);
            int x1 = site[0];
            int y1 = site[1];
            int x2 = site[2];
            int y2 = site[3];
            if (x1 == x2 && y1 == y2)
            {
                args.Player.SendMessage("这是一个点", Color.Red);
            }
            int minX = Math.Min(x1, x2);
            int maxX = Math.Max(x1, x2);
            int minY = Math.Min(y1, y2);
            int maxY = Math.Max(y1, y2);
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    // 获取图格
                    ITile tile = Main.tile[x, y];
                    if (tile == null)
                        continue;
                    if (tile.active())
                    {
                        tile.active(false);
                        tile.type = 0;
                        tile.frameX = 0;
                        tile.frameY = 0;
                    }
                    
                    if (tile.wall >(ushort)0)
                    {
                        tile.wall = 0;
                    }

                    // 清除电线、执行器等
                    if (tile.wire() || tile.wire2() || tile.wire3() || tile.wire4())
                    {
                        tile.wire(false);
                        tile.wire2(false);
                        tile.wire3(false);
                        tile.wire4(false);
                    }
                    
                    // 同步给所有玩家
                    NetMessage.SendTileSquare(-1, x, y, 1);
                }
            }
        }
    }
    
    private void build(CommandArgs args)
    {

        if (args.Parameters.Count > 5||args.Parameters.Count<5)
        {
            args.Player.SendMessage("示例/tb L 100 200 50 1\n意为清理从玩家左侧脚底下,如果输入R则为右侧\n至(100,200)的区域" +
                                    "\n并搭建间隔为50格的倾斜平台\n最后的那个1表示启用平台倾斜\n如果最后一个是0那么久不倾斜)", Color.Red);
            return;
        }
        
        int counter = 0;
        foreach (var VARIABLE in args.Parameters)
        {
            switch (counter)
            {
                case 0:
                    foreach (var c in VARIABLE)
                    {
                        if (char.IsDigit(c))
                        {
                            args.Player.SendMessage("你所输入的一号位不是代表左右的字母", Color.Red);
                            return;
                        }
                        else if (char.IsLetter(c))
                        {
                        }
                    }
                    break;
                case 1:
                    foreach (var c in VARIABLE)
                    {
                        if (char.IsDigit(c))
                        {
                        }
                        else if (char.IsLetter(c))
                        {
                            args.Player.SendMessage("你所输入的二号位不是代表X轴的数字", Color.Red);
                            return;
                        }
                    }
                    break;
                case 2:
                    foreach (var c in VARIABLE)
                    {
                        if (char.IsDigit(c))
                        {
                        }
                        else if (char.IsLetter(c))
                        {
                            args.Player.SendMessage("你所输入的三号位不是代表Y轴的数字", Color.Red);
                            return;
                        }
                    }
                    break;
                case 3:
                    foreach (var c in VARIABLE)
                    {
                        if (char.IsDigit(c))
                        {
                        }
                        else if (char.IsLetter(c))
                        {
                            args.Player.SendMessage("你所输入的四号位不是代表平台间隔的数字", Color.Red);
                            return;
                        }
                    }
                    break;
                case 4:
                    foreach (var c in VARIABLE)
                    {
                        if (char.IsDigit(c))
                        {
                        }
                        else if (char.IsLetter(c))
                        {
                            args.Player.SendMessage("你所输入的五号位不是代表平台倾斜的数字", Color.Red);
                            return;
                        }
                    }
                    break;
            }
            counter++;
        }
        
        if (args.Parameters.Count == 5)
        {
            var site = args.Parameters.Skip(1).ToList().ConvertAll(int.Parse);
            int x1 = args.Player.TileX;
            int y1 = args.Player.TileY;
            int x2 = site[0];
            int y2 = site[1];
            int mX = 0;
            int mxX = 0;
            int mY = 0;
            int mxY = 0;
            int barrage = 0;
            int types = 0;
            int minX = 0;
            int maxX = 0;
            int minY = 0;
            int maxY = 0;
            if (x1 == x2 && y1 == y2)
            {
                args.Player.SendMessage("这是一个点", Color.Red);
            }

            if (args.Parameters[0] == "L" || args.Parameters[0] == "l")
            { 
                mX = x1;
                mxX = x1 - x2;
                mY = y1;
                mxY = y1 - y2;
                minX = Math.Min(mX, mxX);
                maxX = Math.Max(mX, mxX); 
                minY = Math.Min(mY, mxY);
                maxY = Math.Max(mY, mxY);
            }
            if (args.Parameters[0] == "R" || args.Parameters[0] == "r")
            {
                mX = x1;
                mxX = x1 + x2;
                mY = y1;
                mxY = y1 - y2;
                minX = Math.Min(mX, mxX);
                maxX = Math.Max(mX, mxX); 
                minY = Math.Min(mY, mxY);
                maxY = Math.Max(mY, mxY);
            }
            if (site[2] == null || site[2] == 0)
            {
                barrage = 60;
            }
            if (site[2] != null && site[2] != 0)
            {
                barrage = site[2];
            }
            if (site[3] != 1 && site[3] != 0)
            {
                args.Player.SendMessage($"倾斜只为1或0", Color.Red);
                return;
            }
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    ITile tile = Main.tile[x, y];
                    switch (tile.type)
                    {
                        case 41:
                            args.Player.SendMessage($"检测到保护方块,退出", Color.Red);
                            return;
                        case 43:
                            args.Player.SendMessage($"检测到保护方块,退出", Color.Red);
                            return;
                        case 44:
                            args.Player.SendMessage($"检测到保护方块,退出", Color.Red);
                            return;
                        case 226:
                            args.Player.SendMessage($"检测到保护方块,退出", Color.Red);
                            return;
                    }
                }
            }

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    // 获取图格
                    ITile tile = Main.tile[x, y];
                    if (tile == null)
                        continue;

                    if (tile.active())
                    {
                        tile.active(false);
                        tile.type = 0;
                        tile.frameX = 0;
                        tile.frameY = 0;
                    }

                    if (tile.wall > (ushort)0)
                    {
                        tile.wall = 0;
                    }

                    // 清除电线、执行器等
                    if (tile.wire() || tile.wire2() || tile.wire3() || tile.wire4())
                    {
                        tile.wire(false);
                        tile.wire2(false);
                        tile.wire3(false);
                        tile.wire4(false);
                    }

                    // 同步给所有玩家
                    NetMessage.SendTileSquare(-1, x, y, 1);
                }
            }
            
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y+=barrage)
                {
                    WorldGen.PlaceTile(x, y, 19, false, true, -1, 6);
                    TSPlayer.All.SendTileRect((short) x, (short) y, 1, 1);
                    ITile tile = Main.tile[x, y];
                    tile.slope((byte)site[3]);
                    NetMessage.SendTileSquare(-1, x, y, 1);
                }
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            //移除所有由本插件添加的所有指令
            var asm = Assembly.GetExecutingAssembly();
            Commands.ChatCommands.RemoveAll(c => c.CommandDelegate.Method?.DeclaringType?.Assembly == asm);
        }
        base.Dispose(disposing);
    }
}