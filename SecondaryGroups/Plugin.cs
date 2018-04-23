using System;
using System.Diagnostics;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using SecondaryGroups.Extensions;

namespace SecondaryGroups
{
  [ApiVersion(2, 1)]
  public class Plugin : TerrariaPlugin
  {
    public Plugin(Main game) : base(game)
    {
    }
    public override string Name => "SecondaryGroups";
    public override string Author => "Newy";
    public override string Description => "XenForo style secondary groups!";
    public override Version Version => typeof(Plugin).Assembly.GetName().Version;

    public override void Initialize()
    {
      Database.Connect();
      PlayerHooks.PlayerPermission += OnPlayerPermission;

      ServerApi.Hooks.ServerChat.Register(this, OnServerChat);

      /*PlayerHooks.PlayerItembanPermission += OnItemban;
      PlayerHooks.PlayerProjbanPermission += OnProjban;

      PlayerHooks.PlayerTilebanPermission += OnTileban;
      */

      TShockAPI.Commands.ChatCommands.Add(
        new Command("secondarygroups.modify", CommandRouter, "sgroup", "secgroup")
      );
    }

        /*private static void OnItemban(PlayerItembanPermissionEventArgs e)
        {
          if (e.Player.User == null) return;

          var data = GroupData.Get(e.Player.User);

          if (data == null) return;

          if (data.Groups.Any(g => e.BannedItem.AllowedGroups.Contains(g.Name)))
            e.Handled = true;
        }

        private static void OnProjban(PlayerProjbanPermissionEventArgs e)
        {
          if (e.Player.User == null) return;

          var data = GroupData.Get(e.Player.User);

          if (data == null) return;

          if (data.Groups.Any(g => e.BannedProjectile.AllowedGroups.Contains(g.Name)))
            e.Handled = true;
        }

        private static void OnTileban(PlayerTilebanPermissionEventArgs e)
        {
          if (e.Player.User == null) return;

          var data = GroupData.Get(e.Player.User);

          if (data == null) return;

          if (data.Groups.Any(g => e.BannedTile.AllowedGroups.Contains(g.Name)))
            e.Handled = true;
        }
        */
    
    //Code grabbed from USF
    private static void OnServerChat(ServerChatEventArgs e)
    {
            if (e.Handled)
                return;

            var player = TShock.Players[e.Who];

            if (player == null || !player.IsLoggedIn)
                return;

            if (!player.HasPermission(TShockAPI.Permissions.canchat) || player.mute)
                return;

            if (e.Text.StartsWith(TShock.Config.CommandSpecifier) ||
                e.Text.StartsWith(TShock.Config.CommandSilentSpecifier))
                return;

            var data = GroupData.Get(player.User);
            if (data == null) return;

            var prefix = data.Groups.LastOrDefault().Prefix ?? player.Group.Prefix;
            //var group = data.Groups.LastOrDefault() ?? player.Group;
            var chatColor = data.Groups.LastOrDefault().ChatColor?.ParseColor() ?? player.Group.ChatColor.ParseColor();

            var message = string.Format(TShock.Config.ChatFormat, player.Group.Name, prefix, player.Name, player.Group.Suffix,
                e.Text);
            TSPlayer.All.SendMessage(message, chatColor);
            TSPlayer.Server.SendMessage(message, chatColor);
            TShock.Log.Info($"Broadcast: {message}");

            e.Handled = true;
    }

    private static void CommandRouter(CommandArgs args)
    {
      if (args.Parameters.Count < 2)
      {
        args.Player.SendErrorMessage("Invalid usage! Usage: /sgroup <add/delete/list> <user> <group>");
        return;
      }

      var input = args.Parameters[0].ToLowerInvariant();
      args.Parameters.RemoveAt(0);

      try
      {
        switch (input)
        {
          case "add":
            Commands.AddCommand(args);
            break;

          case "del":
          case "delete":
            Commands.RemoveCommand(args);
            break;

          case "list":
            Commands.ListCommand(args);
            break;

          default:
            args.Player.SendErrorMessage("Invalid usage! Usage: /sgroup <add/delete/list> <user> <group>");
            return;
        }
      }
      catch (Exception e)
      {
        args.Player.SendErrorMessage(e.Message);
      }
    }

    private static void OnPlayerPermission(PlayerPermissionEventArgs e)
    {
#if DEBUG
      var sv = Stopwatch.StartNew();
#endif

      if (e.Player.User == null) return;

      var data = GroupData.Get(e.Player.User);

      if (data == null) return;

      if (data.Permissions.Contains(e.Permission))
        e.Handled = true;

#if DEBUG
      sv.Stop();

      if (sv.ElapsedMilliseconds > 5)
        Console.WriteLine("Permission check spent:" + sv.ElapsedMilliseconds);
#endif
    }

    protected override void Dispose(bool disposing)
    {
      PlayerHooks.PlayerPermission -= OnPlayerPermission;

      /*PlayerHooks.PlayerTilebanPermission -= OnTileban;
      PlayerHooks.PlayerProjbanPermission -= OnProjban;

      PlayerHooks.PlayerTilebanPermission -= OnTileban;
      */
      base.Dispose(disposing);
    }
  }
}