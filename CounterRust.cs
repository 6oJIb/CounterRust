using Newtonsoft.Json;
using Oxide.Core.Plugins;
using Oxide.Ext.SimpleCUI;
using Oxide.Ext.SimpleCUI.Assets;
using Oxide.Game.Rust.Cui;
using Oxide.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal enum ReasonRoundEnd
{
    BombExploded,
    BombDefused,
    TimeIsUp,
    TeamRaidersDead,
    TeamDefendersDead
}

internal enum Team
{
    Raiders = -1,
    Defenders = 1
}

internal class UserInterface
{
    public BasePlayer owner;
    public CUI.Frame countdownFrame = new CUI.Frame();
    public CUI.Canvas spectatorCanvas = new CUI.Canvas();
    public CUI.Canvas roundTabCanvas = new CUI.Canvas();
    public CUI.Canvas roundResultCanvas = new CUI.Canvas();
    public CUI.TextLabel countdownText = new CUI.TextLabel();
    public Plugin ImageLibrary;
    public CUI.Canvas progressBarCanvas = new CUI.Canvas();
    public CUI.ImageLabel bombIcon = new CUI.ImageLabel();
    public Timer progerssBarTimerOnce;
    public Timer progerssBarTimerEvery;
    public const string raiderScoreText = "RaidersScore";
    public const string defendersScoreText = "DefendersScore";
    public const string avatarShadowName = "AvatarShadow";
    public const string killsTextName = "Kills";
    public const string deathTextName = "Deaths";

    public UserInterface(Plugin ImageLibrary, BasePlayer owner)
    {
        this.ImageLibrary = ImageLibrary;
        this.owner = owner;
    }

    public void CreateInterface(List<MatchMember> filtredMembers, int maxTeamSize)
    {
        List<BasePlayer> raiders = new List<BasePlayer>();
        List<BasePlayer> defenders = new List<BasePlayer>();
        foreach (MatchMember matchMember in filtredMembers)
        {
            if (matchMember.team == Team.Raiders)
                raiders.Add(matchMember.GetPlayer());
            else
                defenders.Add(matchMember.GetPlayer());
        }

        CUI.Canvas roundTabCanvas = new CUI.Canvas();
        roundTabCanvas.anchorPoint = new Vector2(0.5f, 1);
        roundTabCanvas.position = new Vector2(0, 945);
        roundTabCanvas.size = new Vector2(1920, 135);
        roundTabCanvas.parent = CUILayers.Under;
        roundTabCanvas.owner = owner;
        this.roundTabCanvas = roundTabCanvas;

        CUI.Frame roundTabFrame = new CUI.Frame();
        roundTabFrame.size = CUI.Transform.FromScale(1, 1);
        roundTabFrame.AddComponent(new CuiHorizontalLayoutGroupComponent
        {
            ChildAlignment = TextAnchor.LowerCenter,
            Padding = "0",  // or "10 10 10 10"
            Spacing = 4,
            ChildControlHeight = false,
            ChildControlWidth = false,
            ChildForceExpandHeight = false,
            ChildForceExpandWidth = false,
            ChildScaleHeight = false,
            ChildScaleWidth = false
        });
        roundTabFrame.color = CUI.Color.ProcentRGB(0, 0, 0, 0);
        roundTabFrame.parent = roundTabCanvas;

        int i;
        for (i = 0; i < maxTeamSize - raiders.Count; ++i)
            CreateEmptyAvatar();
        foreach (BasePlayer raider in raiders)
            CreateAvatar(raider);


        CUI.Frame backgroundFrame = new CUI.Frame();
        backgroundFrame.parent = roundTabFrame;
        backgroundFrame.size = CUI.Transform.FromOffset(110, 80);
        backgroundFrame.color = CUI.Color.HEX("252525", 0.95f);
        CUI.Frame countdownFrame = new CUI.Frame();
        countdownFrame.parent = backgroundFrame;
        countdownFrame.position = CUI.Transform.FromScale(0, 1);
        countdownFrame.size = CUI.Transform.FromOffset(110, 36);
        countdownFrame.anchor = CUI.Transform.FromScale(0, 1);
        countdownFrame.color = CUI.Color.HEX("1B1A15", 0.95f);
        this.countdownFrame = countdownFrame;
        CUI.TextLabel countdownText = new CUI.TextLabel();
        countdownText.parent = countdownFrame;
        countdownText.size = CUI.Transform.FromScale(1f, 1f);
        countdownText.fontSize = 20;
        countdownText.text = ":)";
        countdownText.align = TextAnchor.MiddleCenter;
        this.countdownText = countdownText;


        //ROUND RAIDERS
        CUI.Frame roundRaidersFrame = new CUI.Frame();
        roundRaidersFrame.parent = backgroundFrame;
        roundRaidersFrame.size = CUI.Transform.FromOffset(53, 40);
        roundRaidersFrame.color = CUI.Color.HEX("7A3A2D", 0.9f);
        CUI.ImageLabel roundRaidersShadow = new CUI.ImageLabel();
        roundRaidersShadow.parent = roundRaidersFrame;
        roundRaidersShadow.size = CUI.Transform.FromScale(1, 1);
        roundRaidersShadow.position = CUI.Transform.FromOffset(0, 1);
        roundRaidersShadow.png = SimpleCUIExt.GetPreloadImage(ImageLibrary, "shadow");
        roundRaidersShadow.color = CUI.Color.ProcentRGB(1, 1, 1, 0.75f);
        CUI.TextLabel roundRaidersText = new CUI.TextLabel();
        roundRaidersText.name = raiderScoreText;
        roundRaidersText.parent = roundRaidersShadow;
        roundRaidersText.size = roundRaidersShadow.size;
        roundRaidersText.position = CUI.Transform.zero;
        roundRaidersText.anchor = CUI.Transform.zero;
        roundRaidersText.fontSize = 20;
        roundRaidersText.align = TextAnchor.MiddleCenter;
        roundRaidersText.text = "0";

        //ROUND DEFRNDERS
        CUI.Frame roundDefendersFrame = new CUI.Frame();
        roundDefendersFrame.parent = backgroundFrame;
        roundDefendersFrame.anchor = CUI.Transform.FromScale(1, 0);
        roundDefendersFrame.size = CUI.Transform.FromOffset(53, 40);
        roundDefendersFrame.position = CUI.Transform.FromScale(1, 0);
        roundDefendersFrame.color = CUI.Color.HEX("244254", 0.9f);
        CUI.ImageLabel roundDefendersShadow = new CUI.ImageLabel();
        roundDefendersShadow.parent = roundDefendersFrame;
        roundDefendersShadow.position = CUI.Transform.FromOffset(0, 1);
        roundDefendersShadow.size = CUI.Transform.FromScale(1, 1);
        roundDefendersShadow.png = SimpleCUIExt.GetPreloadImage(ImageLibrary, "shadow");
        roundDefendersShadow.color = CUI.Color.ProcentRGB(1, 1, 1, 0.75f);
        CUI.TextLabel roundDefendersText = new CUI.TextLabel();
        roundDefendersText.name = defendersScoreText;
        roundDefendersText.parent = roundDefendersShadow;
        roundDefendersText.size = CUI.Transform.FromScale(1, 1);
        roundDefendersText.position = CUI.Transform.zero;
        roundDefendersText.anchor = CUI.Transform.zero;
        roundDefendersText.anchor = CUI.Transform.zero;
        roundDefendersText.fontSize = 20;
        roundDefendersText.align = TextAnchor.MiddleCenter;
        roundDefendersText.text = "0";

        foreach (BasePlayer defender in defenders)
            CreateAvatar(defender);
        for (i = 0; i < maxTeamSize - defenders.Count; ++i)
            CreateEmptyAvatar();

        roundTabCanvas.Draw();
    }

    public void CreateRoundResult(ReasonRoundEnd reason)
    {
        CUI.Color red = CUI.Color.HEX("D52528", 0.9f);
        CUI.Color blue = CUI.Color.HEX("114C99", 0.9f);
        CUI.Color grey = CUI.Color.HEX("252525", 0.9f);

        PlayerUtility.RunEffect(owner.GetNetworkPosition(), "assets/bundled/prefabs/fx/item_unlock.prefab", owner);

        CUI.Canvas canvas = new CUI.Canvas();
        canvas.anchorPoint = new Vector2(0.5f, 1f);
        canvas.size = new Vector2(500, 100);
        canvas.position = new Vector2(1920f / 2f, 795);
        canvas.anchor = CUI.Transform.FromScale(0.5f, 0);
        canvas.owner = owner;
        canvas.parent = CUILayers.Overall;

        CUI.Frame frame = new CUI.Frame();
        frame.sprite = CUISprites.Tiletex;
        frame.size = CUI.Transform.FromScale(1, 1);
        frame.color = CUI.Color.HEX("252525", 0.95f);
        frame.fadeOut = 0.5f;
        frame.parent = canvas;
        CUI.Frame line = new CUI.Frame();
        line.fadeOut = 0.5f;
        line.anchor = CUI.Transform.FromScale(0, 1);
        line.size = CUI.Transform.FromOffset(500, 20);
        line.color = CUI.Color.HEX("838383", 0.9f);
        line.parent = frame;
        CUI.TextLabel text = new CUI.TextLabel();
        text.parent = frame;
        text.fadeOut = 0.5f;
        text.size = CUI.Transform.FromScale(1, 1);
        text.fontSize = 20;
        text.align = TextAnchor.MiddleCenter;

        switch (reason)
        {
            case ReasonRoundEnd.TeamRaidersDead:
                line.color = blue;
                text.text = "Вся команда рейдеров мертва. Победила команда дефендеров";
                break;
            case ReasonRoundEnd.TeamDefendersDead:
                line.color = red;
                text.text = "Вся команда дефендеров мертва. Победила команда рейдеров";
                break;
            case ReasonRoundEnd.BombExploded:
                line.color = red;
                text.text = "Бомба была взорвана. Победила команда рейдеров";
                break;
            case ReasonRoundEnd.BombDefused:
                line.color = blue;
                text.text = "Бомба была обезврежена. Победила команда дефендеров";
                break;
            case ReasonRoundEnd.TimeIsUp:
                line.color = blue;
                text.text = "Время вышло. Победила команда дефендеров";
                break;
            default:
                line.color = grey;
                text.text = "Причина неизвестна";
                break;
        }
        canvas.Draw();
        text.Update();
        line.Update();
        roundResultCanvas = canvas;
    }

    public void CreateProgressBar(ref PluginTimers timer, float time)
    {
        CUI.Canvas canvas = new CUI.Canvas();
        canvas.owner = owner;
        canvas.anchorPoint = new Vector2(0.5f, 0f);
        canvas.position = new Vector2(1920f / 2f, 440f);
        canvas.anchor = CUI.Transform.FromScale(0.5f, 0);
        canvas.size = new Vector2(72f, 6f);
        canvas.parent = CUILayers.Under;
        canvas.fadeOut = 0.1f;

        //PROGRESS BAR
        CUI.Frame progressBarBackground = new CUI.Frame();
        progressBarBackground.parent = canvas;
        progressBarBackground.size = CUI.Transform.FromScale(1, 1);
        progressBarBackground.color = CUI.Color.ProcentRGB(1, 1, 1, 0.5f);
        progressBarBackground.fadeOut = 0.1f;
        CUI.Frame progressBarFrame = new CUI.Frame();
        progressBarFrame.parent = progressBarBackground;
        progressBarFrame.color = CUI.Color.ProcentRGB(1, 1, 1, 1);
        canvas.Draw();

        DestroyProgressBar();

        timer.Once(time, () =>
        {
            if (canvas.isAlive)
                canvas.Destroy();
        });

        const float tick = 0.02f;
        float x = 0;
        float inc = 1 / (time / tick);
        progressBarCanvas = canvas;
        progerssBarTimerEvery = timer.Every(tick, () =>
        {
            if (progressBarFrame == null) return;
            if (!progressBarFrame.isAlive) return;

            x += inc;
            if (x >= 1f - Mathf.Epsilon)
            {
                progressBarFrame.size = CUI.Transform.FromScale(1f, 1f);
                progressBarFrame.Update();
                return;
            }

            progressBarFrame.size = CUI.Transform.FromScale(x, 1f);
            progressBarFrame.Update();
        });
        progerssBarTimerOnce = timer.Once(time, DestroyProgressBar);
    }

    private bool TryFindShadow(CUI.ImageLabel avatar, out CUI.ImageLabel avatarShadow)
    {
        foreach (CUI.Element child in avatar.children)
        {
            if (child is CUI.ImageLabel && child.name == avatarShadowName)
            {
                avatarShadow = (CUI.ImageLabel)child;
                return true;
            }
        }
        avatarShadow = new CUI.ImageLabel();
        return false;
    }

    public void MakeAvatarDeath(BasePlayer died)
    {
        foreach (CUI.Element e in roundTabCanvas.GetDescendants().ToList())
        {
            if (e is CUI.ImageLabel && e.name == died.UserIDString)
            {
                CUI.ImageLabel avatar = (CUI.ImageLabel)e;

                if (TryFindShadow(avatar, out CUI.ImageLabel avatarShadow))
                {
                    CUI.TextLabel avatarNick = (CUI.TextLabel)avatarShadow.children[0];
                    avatar.color = CUI.Color.ProcentRGB(1, 1, 1, 0.4f);
                    avatarShadow.color = CUI.Color.ProcentRGB(0, 0, 0, 0.4f);
                    avatarNick.color = CUI.Color.ProcentRGB(1, 1, 1, 0.4f);
                    avatarNick.Update();
                    avatarShadow.Update();
                    avatar.Update();

                    break;
                }
                else Oxide.Core.Interface.Oxide.LogInfo("[CounterRust] Avater shadow is not found");

            }
        }
    }

    public void CrateBombIcon()
    {
        string whiteBombIcon = SimpleCUIExt.GetPreloadImage(ImageLibrary, "c4bomb_white");

        foreach (CUI.Element child in countdownFrame.children)
            child.Destroy();

        CUI.ImageLabel bombIcon = new CUI.ImageLabel();
        bombIcon.owner = countdownFrame.owner;
        bombIcon.parent = countdownFrame;
        bombIcon.size = CUI.Transform.FromScale(0.4f, 0.9f);
        bombIcon.anchor = CUI.Transform.FromScale(0.5f, 0.5f);
        bombIcon.position = CUI.Transform.FromScale(0.5f, 0.5f);
        bombIcon.png = whiteBombIcon;
        bombIcon.color = CUI.Color.HEX("#732020", 1);
        bombIcon.Draw();
        this.bombIcon = bombIcon;
    }

    public void MakeBombIconRed()
    {
        if (bombIcon.isAlive)
        {
            bombIcon.color = CUI.Color.HEX("732020", 1);
            bombIcon.Update();
        }
    }

    public void MakeBombIconGreen()
    {
        if (bombIcon.isAlive)
        {
            bombIcon.color = CUI.Color.HEX("4f9532", 1);
            bombIcon.Update();
        }
    }

    public void MakeBombIconWhiteRed()
    {
        if (bombIcon.isAlive)
        {
            bombIcon.color = CUI.Color.HEX("de4545", 1);
            bombIcon.Update();
        }
    }

    public void CreateExplosionIcon()
    {
        string png = SimpleCUIExt.GetPreloadImage(ImageLibrary, "explosion");

        foreach (CUI.Element child in countdownFrame.children)
            child.Destroy();

        CUI.ImageLabel explosionIcon = new CUI.ImageLabel();
        explosionIcon.owner = owner;
        explosionIcon.parent = countdownFrame;
        explosionIcon.size = CUI.Transform.FromScale(0.4f, 0.9f);
        explosionIcon.anchor = CUI.Transform.FromScale(0.5f, 0.5f);
        explosionIcon.position = CUI.Transform.FromScale(0.5f, 0.5f);
        explosionIcon.color = CUI.Color.ProcentRGB(1, 1, 1, 0.75f);
        explosionIcon.png = png;
        explosionIcon.Draw();
        //Oxide.Core.Interface.Oxide.LogInfo($"[onevsfive] ASDSDSD");
    }

    //public void MakeAvatersAlive()
    //{
    //    foreach (Frame plrRoundTab in data)
    //    {
    //        foreach (CUIBase e in plrRoundTab.children.ToList())
    //        {
    //            if (e is ImageLabel && IsMember(e.name))
    //            {
    //                ImageLabel avatar = (ImageLabel)e;
    //                ImageLabel avatarShadow = (ImageLabel)avatar.children[0];
    //                CUI.TextLabel avatarNick = (CUI.TextLabel)avatarShadow.children[0];
    //                avatar.color = CUI.Color.ProcentRGB(1, 1, 1, 1f);
    //                avatarShadow.color = CUI.Color.ProcentRGB(1, 1, 1, 1f);
    //                avatarNick.color = CUI.Color.ProcentRGB(1, 1, 1, 1f);
    //                avatarNick.UpdateCUI();
    //                avatarShadow.UpdateCUI();
    //                avatar.UpdateCUI();    
    //            }
    //        }
    //    }
    //}

    public void CreateSpectatorInterface(BasePlayer target)
    {
        CUI.Canvas canvas = new CUI.Canvas();
        canvas.anchorPoint = new Vector2(0.5f, 0);
        canvas.position = new Vector2(661, 123);
        canvas.size = new Vector2(570, 90);
        canvas.parent = CUILayers.Hud;
        canvas.needCursor = true;
        canvas.owner = owner;

        CUI.Frame backgroundFrame = new CUI.Frame();
        backgroundFrame.name = target.UserIDString;
        backgroundFrame.size = CUI.Transform.FromOffset(570, 90);
        backgroundFrame.parent = canvas;
        backgroundFrame.color = CUI.Color.HEX("1B1A15", 0.95f);

        CUI.ImageLabel avatar = new CUI.ImageLabel();
        avatar.size = CUI.Transform.FromOffset(80, 80);
        avatar.position = CUI.Transform.FromScale(0.5f, 0.5f);
        avatar.parent = backgroundFrame;
        avatar.anchor = CUI.Transform.FromScale(0.5f, 0.5f);
        avatar.png = SimpleCUIExt.GetPreloadImage(ImageLibrary, target.UserIDString);

        CUI.Frame info = new CUI.Frame();
        info.parent = avatar;
        info.anchor = CUI.Transform.FromScale(1, 0);
        info.size = CUI.Transform.FromOffset(151, 80);
        info.color = CUI.Color.ProcentRGB(0, 0, 0, 0);
        info.AddComponent(new CuiVerticalLayoutGroupComponent
        {
            ChildAlignment = TextAnchor.UpperRight,
            Spacing = 0,
            Padding = "0 0 4 0"
        });

        CUI.TextLabel nick = new CUI.TextLabel();
        nick.parent = info;
        nick.text = target.displayName;
        nick.fontSize = 10;
        nick.size = CUI.Transform.FromOffset(151, 20);
        nick.align = TextAnchor.MiddleRight;

        CUI.ImageLabel topAvatar = new CUI.ImageLabel();
        foreach (CUI.Element e in roundTabCanvas.GetDescendants().ToList())
        {
            if (e is CUI.ImageLabel && e.name == target.UserIDString)
            {
                topAvatar = (CUI.ImageLabel)e;
                break;
            }
        }
        foreach (CUI.Element child in topAvatar.children.ToList())
        {
            if (child.name == killsTextName)
            {
                CUI.TextLabel killsText = (CUI.TextLabel)child;
                CUI.TextLabel kills = new CUI.TextLabel();
                kills.owner = owner;
                kills.name = killsTextName;
                kills.parent = info;
                kills.text = "убийств: " + killsText.text;
                kills.fontSize = 8;
                kills.size = CUI.Transform.FromOffset(151, 20);
                kills.align = TextAnchor.MiddleRight;
            }
            if (child.name == deathTextName)
            {
                CUI.TextLabel deathsText = (CUI.TextLabel)child;
                CUI.TextLabel deaths = new CUI.TextLabel();
                deaths.owner = owner;
                deaths.name = deathTextName;
                deaths.parent = info;
                deaths.text = "смертей: " + deathsText.text;
                deaths.fontSize = 8;
                deaths.align = TextAnchor.MiddleRight;
                deaths.size = CUI.Transform.FromOffset(151, 20);
            }
        }

        CUI.Button arrowNext = new CUI.Button();
        //arrowNext.AddComponent(new CuiButtonComponent {
        //    Command = "hud.spectating.next",
        //    Sprite = "assets/icons/maparrow.png",
        //    Color = " 1 1 1 1",
        //    PressedColor = "1 1 1 0.75"
        //});
        arrowNext.parent = backgroundFrame;
        arrowNext.anchor = CUI.Transform.FromScale(1, 0);
        arrowNext.position = CUI.Transform.FromScale(1, 0);
        arrowNext.size = CUI.Transform.FromOffset(90, 90);
        arrowNext.rotation = 180;
        arrowNext.command = "hud.spectating.next";
        arrowNext.sprite = "assets/icons/maparrow.png";
        arrowNext.color = CUI.Color.ProcentRGB(1, 1, 1, 1);
        arrowNext.pressedColor = CUI.Color.ProcentRGB(1, 1, 1, 0.75f);


        CUI.Button arrowPrevios = new CUI.Button();
        arrowPrevios.parent = backgroundFrame;
        arrowPrevios.size = CUI.Transform.FromOffset(90, 90);
        arrowPrevios.command = "hud.spectating.previous";
        arrowPrevios.sprite = "assets/icons/maparrow.png";
        arrowPrevios.pressedColor = CUI.Color.ProcentRGB(1, 1, 1, 0.75f);
        arrowPrevios.color = CUI.Color.ProcentRGB(1, 1, 1, 1);
        //arrowPrevios.AddComponent(new CuiButtonComponent
        //{
        //    Command = "hud.spectating.previous",
        //    Sprite = "assets/icons/maparrow.png",
        //    Color = " 1 1 1 1",
        //    PressedColor = "1 1 1 0.75"
        //});

        canvas.Draw();
        spectatorCanvas = canvas;
    }

    public void SetTabScore(int raidersWins, int defendersWins)
    {
        foreach (CUI.Element e in roundTabCanvas.GetDescendants().ToList())
        {
            if (e.name == raiderScoreText)
            {
                CUI.TextLabel raidersScore = (CUI.TextLabel)e;
                raidersScore.text = raidersWins.ToString();
                raidersScore.Update();
            }
            if (e.name == defendersScoreText)
            {
                CUI.TextLabel defendersScore = (CUI.TextLabel)e;
                defendersScore.text = defendersWins.ToString();
                defendersScore.Update();
            }
        }
    }

    public void CreateEmptyAvatar()
    {
        CUI.ImageLabel avatar = new CUI.ImageLabel();
        avatar.owner = owner;
        avatar.name = "EmptyAvatar";
        avatar.parent = roundTabCanvas.children[0];
        avatar.size = CUI.Transform.FromOffset(80, 80);
        avatar.anchor = CUI.Transform.FromScale(0.5f, 0);
        avatar.color = CUI.Color.ProcentRGB(0, 0, 0, 0);
    }

    public void SetPlayerScore(MatchMember ownerScore)
    {
        //SCORE IN TAB
        foreach (CUI.Element e in roundTabCanvas.GetDescendants().ToList())
        {
            if (e is CUI.ImageLabel && e.name == ownerScore.userID.ToString())
            {
                CUI.ImageLabel avatar = (CUI.ImageLabel)e;

                foreach (CUI.Element child in avatar.children.ToList())
                {
                    if (child.name == killsTextName)
                    {
                        CUI.TextLabel killsText = (CUI.TextLabel)child;
                        killsText.text = ownerScore.kills.ToString();
                        killsText.Update();
                    }
                    if (child.name == deathTextName)
                    {
                        CUI.TextLabel deathsText = (CUI.TextLabel)child;
                        deathsText.text = ownerScore.deaths.ToString();
                        deathsText.Update();
                    }
                }
            }
        }
    }

    public void UpdateSpectatorPlayerScore(MatchMember ownerScore)
    {
        //SCORE IN SPECTATOR
        if (spectatorCanvas.isAlive)
        {
            foreach (CUI.Element child in spectatorCanvas.GetDescendants())
            {
                if (child is CUI.TextLabel && child.name == killsTextName)
                {
                    CUI.TextLabel killsText = (CUI.TextLabel)child;
                    killsText.text = "убийств: " + ownerScore.kills.ToString();
                    killsText.Update();
                }
                if (child is CUI.TextLabel && child.name == deathTextName)
                {
                    CUI.TextLabel deathsText = (CUI.TextLabel)child;
                    deathsText.text = "смертей: " + ownerScore.deaths.ToString();
                    deathsText.Update();
                }
            }
        }
    }

    public void CreateAvatar(BasePlayer target)
    {
        if (!PlayerUtility.IsOnline(target))
        {
            CreateEmptyAvatar();
            return;
        }

        CUI.ImageLabel avatar = new CUI.ImageLabel();
        avatar.name = target.UserIDString;
        avatar.owner = owner;
        avatar.parent = roundTabCanvas.children[0];
        avatar.size = CUI.Transform.FromOffset(80, 80);
        avatar.anchor = CUI.Transform.FromScale(0.5f, 0);
        avatar.png = SimpleCUIExt.GetPreloadImage(ImageLibrary, target.UserIDString);

        CUI.TextLabel kills = new CUI.TextLabel();
        kills.owner = owner;
        kills.parent = avatar;
        kills.align = TextAnchor.LowerLeft;
        kills.text = "0";
        kills.name = killsTextName;
        kills.size = CUI.Transform.FromScale(0.5f, 0.5f);
        kills.position = CUI.Transform.FromOffset(5, 5);
        kills.fontSize = 10;
        kills.outlineColor = CUI.Color.ProcentRGB(0, 0, 0, 1);
        kills.outlineSize = new Vector2(1f, 1f);
        kills.outlineParentAlpha = true;

        CUI.TextLabel deaths = new CUI.TextLabel();
        deaths.owner = owner;
        deaths.parent = avatar;
        deaths.align = TextAnchor.LowerRight;
        deaths.text = "0";
        deaths.name = deathTextName;
        deaths.position = CUI.Transform.FromOffset(75, 5);
        deaths.anchor = CUI.Transform.FromScale(1, 0);
        deaths.size = CUI.Transform.FromScale(0.5f, 0.5f);
        deaths.fontSize = 10;
        deaths.outlineColor = CUI.Color.ProcentRGB(0, 0, 0, 1);
        deaths.outlineSize = new Vector2(1f, 1f);
        deaths.outlineParentAlpha = true;

        CUI.ImageLabel avatarShadow = new CUI.ImageLabel();
        avatarShadow.name = avatarShadowName;
        avatarShadow.owner = owner;
        avatarShadow.parent = avatar;
        avatarShadow.size = CUI.Transform.FromScale(1, 0.7f);
        avatarShadow.anchor = CUI.Transform.FromScale(0, 0.99f);
        avatarShadow.png = SimpleCUIExt.GetPreloadImage(ImageLibrary, "shadow");
        avatarShadow.color = CUI.Color.ProcentRGB(0, 0, 0, 0.9f);

        CUI.TextLabel avatarNick = new CUI.TextLabel();
        avatarNick.owner = owner;
        avatarNick.parent = avatarShadow;
        avatarNick.anchor = CUI.Transform.FromScale(0, 1);
        avatarNick.position = CUI.Transform.FromScale(0, 1);
        avatarNick.size = CUI.Transform.FromScale(1, 1);
        avatarNick.text = target.displayName;
        avatarNick.align = TextAnchor.UpperCenter;
        avatarNick.fontSize = 10;
    }

    private string TranslateTime(int time)
    {
        int minutes = time / 60;
        int seconds = time % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }

    public void SetTime(int second)
    {
        countdownText.text = TranslateTime(second);
        if (second <= 10)
            countdownText.color = CUI.Color.HEX("CE3F27", 1);
        else
            countdownText.color = CUI.Color.ProcentRGB(1, 1, 1, 1);
        if (second <= 5 && second >= 1)
        {
            PlayerUtility.RunEffect(
                owner.transform.position,
                "assets/prefabs/tools/detonator/effects/attack.prefab",
                owner
            );
        }
        countdownText.Update();
    }

    public void DestroyInterface()
    {
        DestroySpectatorMenu();
        roundTabCanvas.Destroy();
        DestroyProgressBar();
        DestroyRoundResult();
    }

    public void DestroyRoundResult()
    {
        if (roundResultCanvas.isAlive)
            roundResultCanvas.Destroy();
    }

    public void DestroyProgressBar()
    {
        if (progressBarCanvas.isAlive)
        {
            Oxide.Core.Interface.Oxide.LogInfo("[CounterRust] Progress bar is destroyed");
            progressBarCanvas.Destroy();
        }
        progerssBarTimerEvery?.Destroy();
        progerssBarTimerEvery = null;
        progerssBarTimerOnce?.Destroy();
        progerssBarTimerOnce = null;
    }

    public void DestroySpectatorMenu()
    {
        if (spectatorCanvas.isAlive)
            spectatorCanvas.Destroy();
    }


}

internal class MatchMember
{
    public ulong userID;
    public int kills = 0;
    public int deaths = 0;
    public bool droppedOut = false;
    public bool disconnected = false;
    public Team team;
    public UserInterface userInterface;

    public MatchMember(ulong userID, Team team)
    {
        this.userID = userID;
        this.team = team;
    }

    public bool IsRaider() => team == Team.Raiders;

    public bool IsDefender() => team == Team.Defenders;

    public bool IsOnline() => PlayerUtility.IsOnline(GetPlayer());

    public BasePlayer GetPlayer() => BasePlayer.FindByID(userID);

    public void Heal()
    {
        if (IsOnline())
        {
            BasePlayer player = GetPlayer();
            player.Heal(player.MaxHealth());
            player.metabolism.ApplyChange(MetabolismAttribute.Type.Bleeding, 0, 0);
            player.SendNetworkUpdate();
        }
    }
}

internal class Match
{
    public bool isGoing = false;
    public int roundSwap;
    public int roundMax;
    public int roundCount;
    public Dictionary<Team, int> tabScore = new Dictionary<Team, int>() { { Team.Raiders, 0 }, { Team.Defenders, 0 } };
    public List<MatchMember> members = new List<MatchMember>();

    public Match(int roundSwap, int maxRound)
    {
        tabScore[Team.Raiders] = 0;
        tabScore[Team.Defenders] = 0;

        this.roundSwap = roundSwap;
        this.roundMax = maxRound;
    }

    public List<MatchMember> GetOnlineMembers()
    {
        List<MatchMember> memberList = new List<MatchMember>();
        foreach (MatchMember member in members)
        {
            if (member.IsOnline())
                memberList.Add(member);
        }
        return memberList;
    }

    public void Swap()
    {
        tabScore[Team.Raiders] = tabScore[Team.Raiders] + tabScore[Team.Defenders];
        tabScore[Team.Defenders] = tabScore[Team.Raiders] - tabScore[Team.Defenders];
        tabScore[Team.Raiders] = tabScore[Team.Raiders] - tabScore[Team.Defenders];
        foreach (MatchMember m in members)
            m.team = (Team)(-(int)m.team);
    }

    public void IncreaseWins(Team team) =>
        tabScore[team] = ++tabScore[team];

    public bool IsMember(ulong userID) =>
        members.Find(m => m.userID == userID) is MatchMember;

    public List<MatchMember> GetPlayersInTeam(Team team) =>
        members.Where(m => m.team == team).ToList();

    public void AddMember(ulong userID, Team team)
    {
        if (IsMember(userID))
            Oxide.Core.Interface.Oxide.LogInfo($"[onevsfive] Player [{userID}] already exist in MatchMember (Match->AddMember)");
        else
            members.Add(new MatchMember(userID, team));
    }

    public bool TryGetMatchMember(ulong userID, out MatchMember matchMember)
    {
        matchMember = null;
        foreach (MatchMember mb in members)
            if (mb.userID == userID)
                matchMember = mb;
        return matchMember != null;
    }

    public bool IsSwap() => (roundCount % roundSwap) == 0;

    public bool IsTie() => tabScore[Team.Raiders] == tabScore[Team.Defenders];

    public bool CreatePlant(Plugin ZoneManager, string id, string name, Vector3 pos, Vector3 size)
    {
        string ToXYZ(Vector3 v) => $"{v.x} {v.y} {v.z}";

        string[] args = { "id", id, "name", name, "location", ToXYZ(pos), "size", ToXYZ(size) };
        return ZoneManager.Call<bool>("CreateOrUpdateZone", id, args, pos);
    }
}

internal class Round
{
    public int countdown;
    public List<MatchMember> members = new List<MatchMember>();
    public bool isGoing = false;
    public bool isBombDefused = false;
    public bool isBombPlanted = false;
    public bool isBombExploded = false;
    public bool isTimeIsUp = false;
    public BasePlayer bombPlanter;
    public BasePlayer bombDefuser;
    public RFTimedExplosive bomb;
    public Timer timerOnce;
    public Timer timerEvery;

    public bool TryGetMatchMember(ulong userID, out MatchMember matchMember)
    {
        matchMember = GetMember(userID);
        return matchMember != null;
    }

    public MatchMember GetMember(ulong userID)
    {
        foreach (MatchMember member in members)
            if (member.userID == userID && member.IsOnline())
                return member;

        Oxide.Core.Interface.Oxide.LogInfo($"[onevsfive] Player {userID} not found in Round members");
        return null;
    }

    public bool IsTeamAlive(Team team) =>
        members.Any(m => m.team == team && !m.droppedOut);

    public bool IsMember(ulong userID) =>
        members.Find(m => m.userID == userID) is MatchMember;

    public List<BasePlayer> GetOnlinePlayers() =>
        GetOnlineMembers().Select(m => m.GetPlayer()).ToList();

    public List<MatchMember> GetOnlineMembers() =>
        members.FindAll(m => m.IsOnline());

    public List<BasePlayer> GetTeamPlayers(Team team) =>
       GetOnlineMembers().Where(m => m.team == team).Select(m => m.GetPlayer()).ToList();

    public int CountAliveInTeam(Team team) =>
        members.FindAll(m => m.team == team && !m.droppedOut).Count;

    public void ClearMembersFlags()
    {
        foreach (MatchMember member in members)
        {
            member.droppedOut = false;
            member.disconnected = false;
        }
    }

    public BasePlayer GetRandomBombPlanter()
    {
        List<BasePlayer> plrs = GetTeamPlayers(Team.Raiders);
        int index = UnityEngine.Random.Range(0, plrs.Count);

        if (plrs.Count == 0)
        {
            Oxide.Core.Interface.Oxide.LogInfo("[onevsfive] Team [Raiders] is empty");
            return null;
        }

        return plrs[index]; ;
    }
}

internal struct Circle3
{
    internal Vector3 center;
    internal float radius;

    internal Circle3(Vector3 centre, float radius)
    {
        this.center = centre; this.radius = radius;
    }

    internal Vector3 getRandomPointXZ()
    {
        Vector2 point = UnityEngine.Random.insideUnitCircle * this.radius;
        float x = this.center.x + point.x;
        float y = this.center.y;
        float z = this.center.z + point.y;

        return new Vector3(x, y, z);
    }
}

internal static class PlayerUtility
{
    public static void ShowTip(BasePlayer player, GameTip.Styles style, string text)
    {
        player.ShowToast(style, text);
    }

    public static void RunEffect(Vector3 position, string prefab, BasePlayer player)
    {
        var effect = new Effect();
        effect.Init(Effect.Type.Generic, position, Vector3.zero);
        effect.pooledString = prefab;

        if (player != null)
            EffectNetwork.Send(effect, player.net.connection);
        else
            EffectNetwork.Send(effect);
    }

    public static bool IsOnline(BasePlayer player)
    {
        if (player == null) return false;
        if (player.net == null) return false;
        if (!player.IsConnected) return false;
        if (player.IsDestroyed) return false;
        return true;
    }

    public static void GiveKit(Oxide.Game.Rust.Libraries.Rust rust, BasePlayer player, string kitName) =>
        rust.RunServerCommand("kit give", player.UserIDString, kitName);

    public static void AddItemToBelt(BasePlayer player, string itemShortName, int iAmount = 1, int iTargetPos = -1)
    {
        Item item = ItemManager.CreateByPartialName(itemShortName, iAmount);
        item.MoveToContainer(player.inventory.containerBelt, iTargetPos);
    }

    public static void ClearInventory(BasePlayer plr)
    {
        PlayerUtility.RemoveActiveItem(plr);
        plr.inventory.Strip();
    }

    public static void RemoveActiveItem(BasePlayer player)
    {
        Item item = player.GetActiveItem();
        if (item != null)
        {
            player.UpdateActiveItem(default);
            item.Remove();
        }
    }

    public static void Teleport(BasePlayer player, Vector3 newPosition, bool wakeUp = true)
    {
        if (!player.IsValid() && player.IsDead())
        {
            return;
        }
        if (Vector3.Distance(newPosition, Vector3.zero) < 5f)
        {
            return;
        }
        Vector3 oldPosition = player.transform.position;

        newPosition.y += 0.1f;
        player.PauseFlyHackDetection(5f);
        player.PauseSpeedHackDetection(5f);
        player.ApplyStallProtection(4f);
        player.UpdateActiveItem(default);
        player.EnsureDismounted();
        player.Server_CancelGesture();

        if (player.HasParent())
        {
            player.SetParent(null, true, true);
        }

        if (player.IsConnected)
        {
            player.StartSleeping();
            if (player.IsAdmin) player.RunOfflineMetabolism(state: false);
            player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);
            player.ClientRPC(RpcTarget.Player("StartLoading", player), arg1: true);
        }

        player.Teleport(newPosition);

        if (player.IsConnected)
        {
            if (!player.limitNetworking && !player.isInvisible)
            {
                player.UpdateNetworkGroup();
                player.SendNetworkUpdateImmediate();
            }

            player.ClearEntityQueue(null);
            //player.SendFullSnapshot();
            if (wakeUp) player.Invoke(() =>
            {
                if (player && player.IsConnected)
                {
                    if (player.limitNetworking || player.isInvisible) player.EndSleeping();
                    else player.EndSleeping();
                }
            }, 0.5f);
        }

        if (!player.limitNetworking && !player.isInvisible)
        {
            player.ForceUpdateTriggers();
        }
        Oxide.Core.Interface.Oxide.CallHook("OnPlayerTeleported", player, oldPosition, newPosition);
    }
}

internal static class TeamSpawns
{
    public static readonly Vector3 raiders = new Vector3(17.83f, -249.89f, 0.82f);
    public static readonly Vector3 defenders = new Vector3(285f, -249.89f, -2f);
}


namespace Oxide.Plugins
{
    [Info("CounterRust", "000", "1.0")]
    class CounterRust : RustPlugin
    {
        //Circle3 lobby = new Circle3(new Vector3(-391.21f, -249.73f, -27.74f), 3.49f);

        #region Fields
        //Requires: ZoneManager, ImageLibrary
        [PluginReference] private Plugin ZoneManager, ImageLibrary;
        Round round;
        Match match;

        const string bombShortPrefabName = "explosive.timed";
        const string explosionPrefabName = "assets/content/effects/explosions/explosion large.prefab";
        //const string explosionPrefabName = "assets/prefabs/tools/c4/effects/c4_explosion.prefab";
        //string bombPrefab = "assets/prefabs/tools/c4/explosive.timed.entity.prefab";    
        //string beepSound = "assets/prefabs/locks/keypad/effects/lock.code.unlock.prefab";
        const string deploySound = "assets/prefabs/tools/c4/effects/c4_stick.prefab";
        const string defusingSound1 = "assets/prefabs/npc/autoturret/effects/targetacquired.prefab";
        const string defusingSound2 = "assets/prefabs/locks/keypad/effects/lock.code.denied.prefab";
        const string plantingSound = "assets/prefabs/locks/keypad/effects/lock.code.unlock.prefab";
        const string beepSound = "assets/prefabs/locks/keypad/effects/lock.code.unlock.prefab";
        #endregion

        #region Config
        internal PluginConfig pluginConfig;
        internal class PluginConfig
        {
            [JsonProperty(PropertyName = "Auto mode")]
            public bool AutoMode = true;

            [JsonProperty(PropertyName = "Can join")]
            public bool CanJoin = true;

            [JsonProperty(PropertyName = "Raiders")]
            public List<string> Raiders = new List<string>();

            [JsonProperty(PropertyName = "Defenders")]
            public List<string> Defenders = new List<string>();

            [JsonProperty(PropertyName = "Wallhack player")]
            public string WallhackUserID = string.Empty;

            [JsonProperty(PropertyName = "Max team members")]
            public int MaxTeamSize = 5;

            [JsonProperty(PropertyName = "Death duration")]
            public int DeathDuration = 5;

            [JsonProperty(PropertyName = "Round-swap")]
            public int SwapRound = 12;

            [JsonProperty(PropertyName = "Round to win")]
            public int MaxRound = 13;

            [JsonProperty(PropertyName = "Round start delay")]
            public int RoundStartDelay = 5;

            [JsonProperty(PropertyName = "Round duration")]
            public int RoundDuration = 120;

            [JsonProperty(PropertyName = "Round end delay")]
            public int RoundEndDelay = 5;

            [JsonProperty(PropertyName = "Bomb max damage")]
            public float BombMaxDamage = 300;

            [JsonProperty(PropertyName = "Bomb explosion raduis")]
            public float BombExplosionRadius = 40;

            [JsonProperty(PropertyName = "Bomb plant time")]
            public int BombPlantTime = 5;

            [JsonProperty(PropertyName = "Bomb defuse time")]
            public int BombDefuseTime = 5;

            [JsonProperty(PropertyName = "Bomb lifetime")]
            public int BombLifetime = 50;
        }

        protected override void LoadDefaultConfig()
        {
            pluginConfig = new PluginConfig();
        }
        protected override void LoadConfig()
        {
            base.LoadConfig();
            pluginConfig = Config.ReadObject<PluginConfig>();
            SaveConfig();
        }
        protected override void SaveConfig() => Config.WriteObject(pluginConfig);

        internal PluginConfig ReadConfig()
        {
            LoadConfig();
            return pluginConfig;
        }
        #endregion


        private void Init()
        {
            PrintToChat("Plugin 1vs5 active");

            LoadConfig();

            match = new Match(pluginConfig.SwapRound, pluginConfig.MaxRound);
            round = new Round();
        }

        private void OnServerInitialized()
        {
            SimpleCUIExt.PreloadImage(ImageLibrary, "shadow", "https://i.postimg.cc/fR1FNzR9/5108f6fc4343c3641d8592d5293cd81c-(1).png");
            SimpleCUIExt.PreloadImage(ImageLibrary, "c4bomb_white", "https://i.postimg.cc/1zKjhsk0/c4bomb.png");
            SimpleCUIExt.PreloadImage(ImageLibrary, "explosion", "https://i.postimg.cc/HLrBFMcz/explosion.png");
            Puts(ImageLibrary == null ? "ImageLibrary: not loaded" : "ImageLibrary: loaded");
            Puts(ZoneManager == null ? "ZoneManager: not loaded" : "ZoneManager: loaded");
            rust.RunServerCommand("server.readcfg");
        }

        private void Unload()
        {
            if (match.isGoing)
                EndMatch();
        }

        #region Match


        //-----------------------------MAIN-----------------------------


        private void StartMatch()
        {
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                Puts($"{player} is on server");
            }
            timer.Once(0.1f, OutGameInfo);

            LoadConfig();

            match = new Match(pluginConfig.SwapRound, pluginConfig.MaxRound);
            match.isGoing = true;
            match.CreatePlant(ZoneManager, "zone_plantA", "plantA", new Vector3(174.99f, -249.61f, -135.02f), new Vector3(12.9f, 0.1f, 12.0f));
            match.CreatePlant(ZoneManager, "zone_plantB", "plantB", new Vector3(155.50f, -249.61f, 53.45f), new Vector3(12f, 0.1f, 12.0f));


            //plantA["id"] = "zone_plantA";
            //plantA["name"] = "plantA";
            //plantA["location"] = Vector3toZoneString();
            //plantA["size"] = Vector3toZoneString();

            //plantB["id"] = "zone_plantB";
            //plantB["name"] = "plantB";
            //plantB["location"] = Vector3toZoneString(new Vector3(155.50f, -249.61f, 53.45f));
            //plantB["size"] = Vector3toZoneString(new Vector3(12f, 0.1f, 12.0f));


            if (pluginConfig.AutoMode)
            {
                int i = 1;
                foreach (BasePlayer player in BasePlayer.activePlayerList.ToList())
                {
                    if (i % 2 == 0)
                        match.AddMember(player.userID, Team.Defenders);
                    else
                        match.AddMember(player.userID, Team.Raiders);
                    if (++i > pluginConfig.MaxTeamSize * 2) break;
                }
            }
            else
            {
                foreach (string userID in pluginConfig.Raiders)
                {
                    BasePlayer player = BasePlayer.Find(userID);
                    if (player != null)
                        match.AddMember(player.userID, Team.Raiders);
                }

                foreach (string userID in pluginConfig.Defenders)
                {
                    BasePlayer player = BasePlayer.Find(userID);
                    if (player != null)
                        match.AddMember(player.userID, Team.Defenders);
                }
            }

            StartRound();
        }

        private void StartRound()
        {
            match.roundCount++;

            if (match.roundCount > match.roundMax)
            {
                if (!match.IsTie())
                {
                    EndMatch();
                    return;
                }
                else
                {
                    if (Math.Abs(match.tabScore[Team.Raiders] - match.tabScore[Team.Defenders]) > 1)
                    {
                        EndMatch();
                        return;
                    }
                }
            }

            round = new Round();
            round.isGoing = true;
            LoadConfig();

            if (pluginConfig.CanJoin)
            {
                int i = 1;
                foreach (BasePlayer player in BasePlayer.activePlayerList.ToList())
                {
                    if (match.IsMember(player.userID)) continue;
                    if (i > pluginConfig.MaxTeamSize * 2) break;

                    int raidersCount = match.GetPlayersInTeam(Team.Raiders).Count;
                    int defendersCount = match.GetPlayersInTeam(Team.Defenders).Count;

                    if (defendersCount < raidersCount)
                        match.AddMember(player.userID, Team.Defenders);
                    else if (raidersCount < defendersCount)
                        match.AddMember(player.userID, Team.Raiders);
                    else
                    {
                        if (i % 2 == 0) match.AddMember(player.userID, Team.Raiders);
                        else match.AddMember(player.userID, Team.Defenders);
                    }
                    ++i;
                }
            }

            if (match.IsSwap())
                match.Swap();

            round.members = match.GetOnlineMembers();
            if (round.members.Count == 0)
            {
                Puts("Round members is zero. Match is ended");
                EndMatch();
                return;
            }
            List<BasePlayer> raiders = round.GetTeamPlayers(Team.Raiders);
            List<BasePlayer> defenders = round.GetTeamPlayers(Team.Defenders);
            round.ClearMembersFlags();

            Puts($"Round {match.roundCount} start");

            string raidersStr = string.Join(", ", raiders.Select(plr => plr.displayName));
            Puts($"Raiders: {raidersStr}");

            string defendersStr = string.Join(", ", defenders.Select(plr => plr.displayName));
            Puts($"Defenders: {defendersStr}");

            foreach (MatchMember member in round.GetOnlineMembers())
            {
                member.userInterface = new UserInterface(ImageLibrary, member.GetPlayer());
                member.userInterface.CreateInterface(SortMembers(round.GetOnlineMembers()), pluginConfig.MaxTeamSize);
                member.userInterface.SetTabScore(match.tabScore[Team.Raiders], match.tabScore[Team.Defenders]);
                foreach (MatchMember m in round.GetOnlineMembers())
                    member.userInterface.SetPlayerScore(m);

                BasePlayer player = member.GetPlayer();

                //if (player.UserIDString == pluginConfig.WallhackUserID && !player.IPlayer.HasPermission("adminesp.use"))
                //    player.IPlayer.GrantPermission("adminesp.use");
            }

            SpawnPlayers(raiders, TeamSpawns.raiders, "RaiderKit");
            SpawnPlayers(defenders, TeamSpawns.defenders, "DefenderKit");

            BasePlayer bombPlanter = round.GetRandomBombPlanter();
            if (bombPlanter != null)
            {
                //round.bombPlanter = bombPlanter;
                timer.Once(0.15f, () =>
                {
                    PlayerUtility.AddItemToBelt(bombPlanter, bombShortPrefabName);
                    Puts($"Bomb given [{bombPlanter.displayName}]");
                });
            }
            else
                Puts("Random bomb planter is null in startRound()=>getRandomPlayerInTeam()");

            CreateNewTeam(raiders);
            CreateNewTeam(defenders);

            // ЧУТЬ ДЕЛЕЯ ПОСЛЕ ТП
            round.timerOnce = timer.Once(0.5f, () =>
            {
                foreach (BaseNetworkable ent in BaseEntity.serverEntities.ToList())
                {
                    if (ent is Door)
                    {
                        Door door = (Door)ent;
                        if (door.IsOpen())
                            door.SetOpen(false);
                        door.UpdateNetworkGroup();
                        door.SendNetworkUpdateImmediate();
                    }
                }
                Timer freezeTimer = timer.Every(1f / 100f, () =>
                {
                    foreach (MatchMember member in round.GetOnlineMembers())
                    {
                        BasePlayer player = member.GetPlayer();
                        ForcePlayerPosition(player, player.transform.position);
                        member.Heal();
                    }
                });
                timer.Once(pluginConfig.RoundStartDelay, freezeTimer.Destroy);
                // ПОДГОТОВКА
                round.countdown = pluginConfig.RoundStartDelay;
                foreach (MatchMember member in round.GetOnlineMembers())
                    member.userInterface.SetTime(round.countdown);
                round.timerEvery = timer.Every(1f, () =>
                {
                    --round.countdown;
                    foreach (MatchMember member in round.GetOnlineMembers())
                        member.userInterface.SetTime(round.countdown);
                });



                round.timerOnce = timer.Once(pluginConfig.RoundStartDelay, () =>
                {
                    round.timerEvery.Destroy();
                    // ОСНОВНОЕ ВРЕМЯ
                    round.countdown = pluginConfig.RoundDuration;
                    foreach (MatchMember member in round.GetOnlineMembers())
                    {
                        member.userInterface.SetTime(round.countdown);
                        member.Heal();
                    }
                    round.timerEvery = timer.Every(1f, () =>
                    {
                        --round.countdown;
                        foreach (MatchMember member in round.GetOnlineMembers())
                            member.userInterface.SetTime(round.countdown);
                    });
                    round.timerOnce = timer.Once(pluginConfig.RoundDuration, () =>
                    {
                        round.timerEvery.Destroy();
                        round.isTimeIsUp = true;
                        CallRoundEnd(ReasonRoundEnd.TimeIsUp);
                    });
                });

            });
        }

        private void EndMatch()
        {
            if (round.timerOnce != null)
            {
                if (!round.timerOnce.Destroyed)
                {
                    round.timerOnce.Destroy();
                    round.timerEvery.Destroy();
                }
            }

            Puts($"Round {match.roundCount} end. Forced");
            Puts("End match");
            KillAll(round.GetOnlinePlayers());
            ClearRound();
            round = new Round();
            match = new Match(pluginConfig.SwapRound, pluginConfig.MaxRound);
        }

        private bool ShouldRoundEnd()
        {
            if (round.isBombDefused)
                return true;

            if (round.isBombExploded)
                return true;

            if (round.isTimeIsUp)
                return true;

            if (round.isBombPlanted && !round.IsTeamAlive(Team.Defenders))
                return true;

            if (round.isBombPlanted && round.IsTeamAlive(Team.Defenders))
                return false;

            return !round.IsTeamAlive(Team.Raiders) || !round.IsTeamAlive(Team.Defenders);
        }

        private void CallRoundEnd(ReasonRoundEnd reason)
        {
            if (!ShouldRoundEnd())
                return;
            if (!round.isGoing)
                return;

            round.isGoing = false;
            foreach (MatchMember matchMember in round.GetOnlineMembers())
                matchMember.userInterface.CreateRoundResult(reason);

            round.timerEvery.Destroy();
            round.timerOnce.Destroy();
            round.timerOnce = timer.Once(pluginConfig.RoundEndDelay, () =>
            {
                KillAll(round.GetOnlinePlayers());
                NextTick(() =>
                {
                    Puts($"Round {match.roundCount} end. {reason}");
                    ClearRound();
                    StartRound();
                });
            });

            switch (reason)
            {
                case ReasonRoundEnd.BombExploded:
                    match.IncreaseWins(Team.Raiders);
                    break;

                case ReasonRoundEnd.BombDefused:
                    match.IncreaseWins(Team.Defenders);
                    break;

                case ReasonRoundEnd.TimeIsUp:
                    match.IncreaseWins(Team.Defenders);
                    break;

                case ReasonRoundEnd.TeamRaidersDead:
                    match.IncreaseWins(Team.Defenders);
                    break;

                case ReasonRoundEnd.TeamDefendersDead:
                    match.IncreaseWins(Team.Raiders);
                    break;
            }
            foreach (MatchMember matchMember in round.GetOnlineMembers())
                matchMember.userInterface.SetTabScore(match.tabScore[Team.Raiders], match.tabScore[Team.Defenders]);
        }


        //-----------------------------HELPFUL-----------------------------


        private void OutGameInfo()
        {
            List<string> info = new List<string>()
            {
                $"Всего раундов {match.roundMax}. Смена сторон происходит каждые {match.roundSwap} раундов",
                "Нажмите кнопку 'E' в присяде на территории плента, чтобы обезвредить бомбу",
                "Нажмите 'E' в присяде на территории плента, чтобы установить бомбу",
            };
            PrintToChat(string.Join("\n", info));
        }

        private void ClearRound()
        {
            round.timerOnce?.Destroy();
            round.timerEvery?.Destroy();
            round.GetOnlineMembers().ForEach(m => m.userInterface.DestroyInterface());
            ClearArea();
            ClearAllTeams();
            //foreach (BasePlayer player in BasePlayer.activePlayerList.ToList())
            //{
            //    if (player.IPlayer.HasPermission("adminesp.use"))
            //        player.IPlayer.RevokePermission("adminesp.use");
            //}

            if (round.bomb != null)
                round.bomb.AdminKill();
        }

        private void ClearArea()
        {
            int items = 0; int corpses = 0;
            foreach (BaseNetworkable ent in BaseEntity.serverEntities.ToList())
            {
                if (ent is Door)
                {
                    Door door = (Door)ent;
                    if (door.IsOpen())
                        door.SetOpen(false);
                    door.UpdateNetworkGroup();
                    door.SendNetworkUpdateImmediate();
                }
                else if (ent is BuildingBlock)
                {
                    ent.KillAsMapEntity();
                }
                else if (ent is LootableCorpse || ent is PlayerCorpse)
                {
                    LootableCorpse corpse = ent as LootableCorpse;
                    List<ItemContainer> inventory = new List<ItemContainer>();
                    if (corpse != null)
                        corpse.GetAllInventories(inventory);

                    foreach (ItemContainer container in inventory)
                        container.Clear();

                    corpses++;
                    ent.KillAsMapEntity();
                }
                else if (ent is DroppedItem)
                {
                    items++;
                    ent.KillAsMapEntity();
                }
                else if (ent is RFTimedExplosive)
                {
                    items++;
                    ent.KillAsMapEntity();
                }
            }

            Puts($"Items removed: {items}. Corpses removed: {corpses}. All doors is closed");
        }

        private bool IsPlayerNearBomb(BasePlayer player)
        {
            if (round.bomb == null)
                return false;

            if (Vector3.Distance(player.transform.position, round.bomb.transform.position) <= 0.3f)
                return true;

            return false;
        }

        private void SpawnPlayers(IEnumerable<BasePlayer> players, Vector3 spawnpoint, string kitName)
        {
            Circle3 circle = new Circle3(spawnpoint, 7f);
            foreach (BasePlayer player in players)
            {
                if (player != null)
                {
                    if (player.IsDead())
                        player.Respawn();

                    PlayerUtility.Teleport(player, circle.getRandomPointXZ());

                    timer.Once(0.1f, () =>
                    {
                        PlayerUtility.RemoveActiveItem(player);
                        PlayerUtility.ClearInventory(player);
                        PlayerUtility.GiveKit(rust, player, kitName);
                    });
                }
                else
                    Puts($"One player in spawnPlayers() is null");
            }
        }

        private void KillAll(IEnumerable<BasePlayer> players)
        {
            foreach (BasePlayer player in players)
            {
                if (PlayerUtility.IsOnline(player))
                {
                    if (player.IsSpectating())
                        player.DieInstantly();

                    if (!player.IsDead())
                        player.DieInstantly();

                    NextTick(() => ForceRespawn(player));
                }
                else
                    Puts($"Player in KillAll() is null");
            }
        }

        private List<double> LinearCounter(double totalTime)
        {
            List<double> list = new List<double>();

            double start = 1.0;
            double minValue = 0.1;

            int N = (int)Math.Round(2 * totalTime / (start + minValue));

            double step = (start - minValue) / (N - 1);

            double sum = 0;
            double value = start;

            for (int tick = 1; tick <= N; tick++)
            {
                double v = (tick == N) ? totalTime - sum : value;
                sum += v;

                list.Add(v);

                value -= step;
            }

            return list;
        }

        private object CanLootPlayer(BasePlayer target, BasePlayer initiator)
        {
            PrintToChat("123");
            if (match.isGoing) return false;
            else return null;
        }

        private object OnItemPickup(Item item, BasePlayer player, WorldItem instance)
        {
            if (!match.isGoing) return null;
            if (item.info.shortname == "ammo.rifle") return null;
            if (item.info.shortname == "syringe.medical") return null;
            if (item.info.shortname == "bandage") return null;
            if (item.info.shortname == "explosive.timed")
            {
                if (round.TryGetMatchMember(player.userID, out MatchMember matchMember))
                {
                    if (matchMember.IsRaider())
                        return null;
                }
                else return false;
            }
            if (item.info.shortname == "rifle.ak") return null;
            return false;
        }


        #endregion Match

        #region Spectating     


        private bool CanBeSpectated(BasePlayer player)
            => player != null
            && !player.IsSpectating()
            && !player.IsDead()
            && !player.IsSleeping()
            && PlayerUtility.IsOnline(player);

        public enum ListDirection
        {
            Next = 1,
            Previous = -1
        }

        private bool TryFindSpectationTarget(IEnumerable<BasePlayer> team, ListDirection dir, BasePlayer spectatingTarget, out BasePlayer target)
        {
            target = null;
            List<BasePlayer> list = SortPlayers(team.ToList());
            int i;
            int startIndex = spectatingTarget != null ? list.IndexOf(spectatingTarget) : 0;
            if (startIndex < 0)
            {
                Puts("Start index in TryFindSpectating target is out array");
                return false;
            }

            if (spectatingTarget != null)
            {
                startIndex += (int)dir;
                if (startIndex >= list.Count)
                    startIndex = 0;
                else if (startIndex < 0)
                    startIndex = list.Count - 1;
            }

            for (i = 0; i < list.Count; ++i)
            {
                BasePlayer player = list[startIndex];
                startIndex += (int)dir;
                if (startIndex == list.Count) startIndex = 0;
                if (startIndex == -1) startIndex = list.Count - 1;

                if (player == null)
                    continue;
                if (!CanBeSpectated(player))
                    continue;
                if (round.GetMember(player.userID).droppedOut)
                    continue;

                target = player;
                return true;
            }
            return false;
        }

        public void TrySpectate(BasePlayer player, IEnumerable<BasePlayer> team, ListDirection instr, BasePlayer spectatingTarget)
        {
            if (player == null || player.IsDestroyed) return;
            if (TryFindSpectationTarget(team, instr, spectatingTarget, out BasePlayer target))
            {
                player.StartSpectating();
                player.UpdateSpectateTarget(target.userID);
                if (round.TryGetMatchMember(player.userID, out MatchMember member))
                {
                    if (member.userInterface.spectatorCanvas.isAlive)
                        member.userInterface.DestroySpectatorMenu();
                    member.userInterface.CreateSpectatorInterface(target);
                }
            }
            else
            {
                if (round.TryGetMatchMember(player.userID, out MatchMember member))
                {
                    if (member.userInterface.spectatorCanvas.isAlive)
                        member.userInterface.DestroySpectatorMenu();
                }
                NextTick(() => { ForceRespawn(player); });
            }
        }


        #endregion Spectating

        #region Team


        private bool OnTeamLeave(RelationshipManager.PlayerTeam pT, BasePlayer bP) => false;

        private bool OnTeamCreate(BasePlayer p) => false;

        private void RemovePlayerFromTeam(BasePlayer player)
        {
            List<RelationshipManager.PlayerTeam> teams = RelationshipManager.ServerInstance.teams.Values.ToList();
            foreach (var team in teams)
            {
                var membersCopy = team.members.ToList();
                foreach (ulong plrId in membersCopy)
                {
                    if (plrId == player.userID)
                        team.RemovePlayer(plrId);
                }
            }
        }

        private void ClearAllTeams()
        {
            List<RelationshipManager.PlayerTeam> teams = RelationshipManager.ServerInstance.teams.Values.ToList();
            foreach (var team in teams)
            {
                var membersCopy = team.members.ToList();
                foreach (ulong plrId in membersCopy)
                    team.RemovePlayer(plrId);
                team.Disband();
            }
        }

        private void CreateNewTeam(IEnumerable<BasePlayer> players)
        {
            RelationshipManager.PlayerTeam newTeam = RelationshipManager.ServerInstance.CreateTeam();
            foreach (BasePlayer plr in players)
                newTeam.AddPlayer(plr);
        }


        #endregion Team

        #region Helpers


        private List<MatchMember> SortMembers(IEnumerable<MatchMember> members) =>
            members.OrderBy(m => m.userID).ToList();

        private List<BasePlayer> SortPlayers(IEnumerable<BasePlayer> players)
        {
            List<BasePlayer> list = new List<BasePlayer>(players);
            list.Sort((a, b) => ((ulong)a.userID).CompareTo((ulong)b.userID));
            return list;
        }



        #endregion Helpers

        #region Bomb


        void OnExplosiveThrown(BasePlayer player, BaseEntity entity, ThrownWeapon item)
        {
            if (entity.ShortPrefabName == "explosive.timed.deployed" && match.isGoing)
            {
                entity.AdminKill();
                PlayerUtility.AddItemToBelt(player, bombShortPrefabName);
            }
        }

        bool IsPlayerInPlant(BasePlayer player)
            => IsPlayerInPlantA(player) || IsPlayerInPlantB(player);

        bool IsPlayerInPlantA(BasePlayer player)
            => ZoneManager.Call<bool>("IsPlayerInZone", "zone_plantA", player);

        bool IsPlayerInPlantB(BasePlayer player)
            => ZoneManager.Call<bool>("IsPlayerInZone", "zone_plantB", player);

        private void PlantBomb(BasePlayer player, int lifetime)
        {
            RaycastHit hit;
            Vector3 origin = player.eyes.position;
            Vector3 direction = Vector3.down;
            int mask = LayerMask.GetMask("Terrain", "World");
            string prefab = "assets/prefabs/tools/c4/explosive.timed.deployed.prefab";
            if (!Physics.Raycast(origin, direction, out hit, 10f, mask))
                return;

            BaseEntity entity = GameManager.server.CreateEntity(prefab, Vector3.zero);
            if (entity == null)
            {
                Puts("Failed to create bomb");
                return;
            }

            RFTimedExplosive bomb = entity as RFTimedExplosive;
            if (bomb == null)
            {
                Puts("Entity is not bomb");
                return;
            }

            BaseEntity foundation = GameManager.server.CreateEntity(
                "assets/prefabs/building core/foundation/foundation.prefab",
                hit.point - new Vector3(0, 4, 0)
            );
            if (entity == null)
            {
                Puts("Failed to crate foundation");
                return;
            }
            foundation.Spawn();
            bomb.Spawn();

            bomb.CancelInvoke(bomb.Explode);
            bomb.SetFlagLocal(BaseEntity.Flags.Reserved2, false);
            bomb.ServerPosition = hit.point;
            Vector3 rot = player.eyes.bodyRotation.eulerAngles;
            rot.x = 0f;
            Quaternion yawOnly = Quaternion.Euler(0f, rot.y, 0f);
            bomb.transform.localRotation = yawOnly * Quaternion.Euler(-90, 0, 0) * Quaternion.Euler(0, 0, -180);
            bomb.DoStick(bomb.ServerPosition, hit.normal, foundation, hit.collider);
            bomb.SendNetworkUpdate();

            Puts("Bomb planted");
            Effect.server.Run(deploySound, hit.point);

            round.bomb = bomb;
            round.isBombPlanted = true;
            round.timerOnce = timer.Once(lifetime, DetonateBomb);

            List<double> counter = new List<double>(LinearCounter(lifetime));

            void InvokeTimer(int step)
            {
                //if (isDestroyed) return;
                if (step > counter.Count)
                    return;

                double delay = counter[step - 1];
                round.timerEvery = timer.Once((float)delay, () =>
                {
                    //if (isDestroyed) return;
                    Effect.server.Run(beepSound, hit.point);
                    foreach (MatchMember member in round.GetOnlineMembers())
                        member.userInterface.MakeBombIconWhiteRed();

                    timer.Once(0.2f, () =>
                    {
                        foreach (MatchMember member in round.GetOnlineMembers())
                            member.userInterface.MakeBombIconRed();
                    });
                    InvokeTimer(step + 1);
                });
            }
            InvokeTimer(1);
        }

        private void DetonateBomb()
        {
            float f1(float x)
            {
                float dmg = (-1f * (pluginConfig.BombMaxDamage / pluginConfig.BombExplosionRadius) * x
                             + pluginConfig.BombMaxDamage);

                return Mathf.Max(dmg, 0f);
            }

            round.isBombExploded = true;
            Puts("Bomb has been detonated");
            CallRoundEnd(ReasonRoundEnd.BombExploded);
            foreach (BasePlayer player in BasePlayer.activePlayerList.ToList())
            {
                if (player == null || player.IsDead() || player.IsSpectating())
                    continue;

                RFTimedExplosive bomb = round.bomb;
                float distance = Vector3.Distance(player.transform.position, bomb.transform.position);
                if (distance <= pluginConfig.BombExplosionRadius)
                    player.Hurt(f1(distance), Rust.DamageType.Explosion, bomb, false);
            }

            round.GetOnlineMembers().ForEach(m => m.userInterface.CreateExplosionIcon());

            Effect.server.Run(explosionPrefabName, round.bomb.transform.position);
            if (round.bomb != null)
            {
                round.bomb.AdminKill();
                round.bomb = null;
            }
        }

        private void DefuseBomb()
        {
            Puts("Bomb defused!");
            round.bomb.SetFlagLocal(BaseEntity.Flags.Reserved1, true);
            round.bomb.SendNetworkUpdate();
            round.isBombDefused = true;
        }


        #endregion Bomb

        #region Plant and Defuse
        private bool CanBombInteract(BasePlayer player)
            => round.isGoing
            && player.IsOnGround()
            && IsPlayerInPlant(player)
            && player.IsDucked()
            && player.CanInteract();

        private bool CanPlant(BasePlayer player)
            => CanBombInteract(player)
            && IsBombInHands(player)
            && round.GetMember(player.userID).IsRaider();

        private bool CanDefuse(BasePlayer player)
            => CanBombInteract(player)
            && round.isBombPlanted
            && IsPlayerNearBomb(player)
            && round.GetMember(player.userID).IsDefender();

        private bool IsBombInHands(BasePlayer player)
        {
            Item item = player.GetActiveItem();
            if (item != null && item.info.shortname == "explosive.timed")
                return true;

            return false;
        }

        private Vector3 bombInteractionPos;
        private Timer tmrEvery;
        private Timer tmrOnce;
        private object OnPlayerInput(BasePlayer player, InputState input)
        {
            if (player == null || input == null || player.serverInput == null || !round.IsMember(player.userID))
                return null;

            MatchMember matchMember = round.GetMember(player.userID);

            if (input.WasJustPressed(BUTTON.USE))
            {
                if (CanPlant(player) && tmrEvery == null && round.bombPlanter == null && !round.isBombPlanted)
                {
                    float seconds = 0;
                    float tick = 0.2f;
                    tmrEvery = timer.Every(tick, () =>
                    {
                        Planting(seconds % 6 == 0);
                        seconds += tick * 10;
                        seconds = Mathf.Round(seconds);
                    });
                    tmrOnce = timer.Once(pluginConfig.BombPlantTime, PlantTimeOver);

                    round.bombPlanter = player;
                    matchMember.userInterface.CreateProgressBar(ref timer, pluginConfig.BombPlantTime);
                    bombInteractionPos = player.transform.position;
                    Puts($"Player [{player.ToString()}] start planting");
                }
                else if (CanDefuse(player) && tmrEvery == null && round.bombDefuser == null && round.isBombPlanted)
                {
                    float tick = 0.2f;
                    tmrEvery = timer.Every(tick, Defusing);
                    tmrOnce = timer.Once(pluginConfig.BombDefuseTime, DefuseTimeOver);

                    Effect.server.Run(defusingSound1, round.bomb.transform.position);
                    Effect.server.Run(defusingSound2, round.bomb.transform.position);

                    round.bombDefuser = player;
                    matchMember.userInterface.CreateProgressBar(ref timer, pluginConfig.BombDefuseTime);
                    bombInteractionPos = player.GetNetworkPosition();
                    Puts($"Player [{player.ToString()}] start defusing");
                }
            }
            if (input.WasJustReleased(BUTTON.USE))
            {

                if (tmrEvery != null && round.bombPlanter != null && !round.isBombPlanted)
                {
                    if (player.userID == round.bombPlanter.userID)
                        InteruptPlant();
                }
                else if (tmrEvery != null && round.bombDefuser != null && round.isBombPlanted)
                {
                    if (player.userID == round.bombDefuser.userID)
                        InteruptDefuse();
                }

            }

            return null;
        }
        private void DestroyTmr()
        {
            tmrEvery?.Destroy();
            tmrEvery = null;
            tmrOnce?.Destroy();
            tmrOnce = null;
        }

        private bool IsDifferentPos(Vector3 v1, Vector3 v2)
        {
            float a = 0.2f;
            Vector3 d = v1 - v2;
            bool b = Mathf.Abs(d.x) < a && Mathf.Abs(d.y) < a && Mathf.Abs(d.z) < a;
            return !b;
        }

        private void Planting(bool shouldBeepSound)
        {
            if (!CanPlant(round.bombPlanter) || IsDifferentPos(round.bombPlanter.GetNetworkPosition(), bombInteractionPos))
                InteruptPlant();
            else if (shouldBeepSound)
                Effect.server.Run(plantingSound, round.bombPlanter.ServerPosition);
        }
        private void Defusing()
        {
            if (!CanDefuse(round.bombDefuser) || IsDifferentPos(round.bombDefuser.GetNetworkPosition(), bombInteractionPos))
                InteruptDefuse();
        }
        private void InteruptDefuse()
        {
            Puts("Interrupt defuse");
            DestroyTmr();
            if (round.TryGetMatchMember(round.bombDefuser.userID, out MatchMember member))
                member.userInterface.DestroyProgressBar();
            round.bombDefuser = null;
        }
        private void InteruptPlant()
        {
            Puts($"Interrupt plant. Pos [{round.bombPlanter.GetNetworkPosition()}], old pos [{bombInteractionPos}]");
            DestroyTmr();
            if (round.TryGetMatchMember(round.bombPlanter.userID, out MatchMember member))
                member.userInterface.DestroyProgressBar();
            round.bombPlanter = null;
        }
        private void PlantTimeOver()
        {
            LoadConfig();
            DestroyTmr();
            round.timerOnce.Destroy();
            round.timerEvery.Destroy();
            PlantBomb(round.bombPlanter, pluginConfig.BombLifetime);
            PlayerUtility.RemoveActiveItem(round.bombPlanter);

            round.GetOnlineMembers().ForEach(m => m.userInterface.CrateBombIcon());

            string GetPlantName()
            {
                if (IsPlayerInPlantA(round.bombPlanter)) return "A";
                if (IsPlayerInPlantB(round.bombPlanter)) return "B";
                return "?";
            }
            foreach (BasePlayer p in BasePlayer.activePlayerList.ToList())
                PlayerUtility.ShowTip(p, GameTip.Styles.Error, $"Заряд С4 установлен на точке {GetPlantName()}");

            round.bombPlanter = null;
        }
        private void DefuseTimeOver()
        {
            DestroyTmr();
            DefuseBomb();
            round.GetOnlineMembers().ForEach(m => m.userInterface.MakeBombIconGreen());
            CallRoundEnd(ReasonRoundEnd.BombDefused);
            round.bombDefuser = null;
        }

        #endregion

        #region Death


        private void ForceRespawn(BasePlayer player)
        {
            if (player.IsDead())
            {
                player.Respawn();
                PlayerUtility.ClearInventory(player);
            }
        }

        private void OnPlayerDeath(BasePlayer player, HitInfo info)
        {
            ulong userID = player.userID;
            bool connected = PlayerUtility.IsOnline(player);

            if (connected)
            {
                if (!match.isGoing)
                {
                    NextTick(() => { ForceRespawn(player); });
                    return;
                }

                if (!round.IsMember(userID))
                {
                    NextTick(() => { ForceRespawn(player); });
                    return;
                }

                if (round.GetMember(userID).droppedOut) //!!!
                {
                    NextTick(() => { ForceRespawn(player); });
                    return;
                }
            }
            MatchMember matchMember = round.GetMember(userID);
            matchMember.droppedOut = true;

            bool isRaider = matchMember.IsRaider();
            bool isDefender = matchMember.IsDefender();

            DropItems(player);
            matchMember.deaths++;
            foreach (MatchMember member in round.GetOnlineMembers())
            {
                BasePlayer p = member.GetPlayer();
                member.userInterface.SetPlayerScore(matchMember);
                member.userInterface.MakeAvatarDeath(player);
                if (p.SpectatingTarget != null)
                {
                    if (p.SpectatingTarget.userID == matchMember.userID)
                        member.userInterface.UpdateSpectatorPlayerScore(matchMember);
                }
                    
            }
            //matchMember.userInterface.DestroySpectatorMenu();

            RemovePlayerFromTeam(player);

            BaseEntity entInitiator = info.Initiator;
            BasePlayer killer = entInitiator?.ToPlayer();
            if (killer != null)
            {
                if (killer.userID != player.userID)
                {
                    Puts($"Player {player.ToString()} was killed by {killer.ToString()}");
                    if (match.TryGetMatchMember(killer.userID, out MatchMember killerMember))
                    {
                        killerMember.kills++;
                        foreach (MatchMember member in round.GetOnlineMembers())
                            member.userInterface.SetPlayerScore(killerMember);
                    }
                }
            }

            Puts($"Raiders alive {round.CountAliveInTeam(Team.Raiders)}, defenders alive {round.CountAliveInTeam(Team.Defenders)}");

            if (isRaider)
            {
                if (!round.IsTeamAlive(Team.Raiders))
                {
                    Puts($"All raiders is dead");
                    CallRoundEnd(ReasonRoundEnd.TeamRaidersDead);
                    return;
                }
                else
                {
                    timer.Once(pluginConfig.DeathDuration, () =>
                    {
                        if (!round.IsTeamAlive(Team.Raiders))
                            return;
                        List<BasePlayer> raiders = round.GetTeamPlayers(Team.Raiders);
                        foreach (BasePlayer spectator in player.GetSpectators())
                            TrySpectate(spectator, raiders, ListDirection.Next, player);

                        if (connected)
                            TrySpectate(player, raiders, ListDirection.Next, null);
                    });
                }
            }
            else if (isDefender)
            {
                if (!round.IsTeamAlive(Team.Defenders))
                {
                    Puts($"All defenders is dead");
                    CallRoundEnd(ReasonRoundEnd.TeamDefendersDead);
                    return;
                }
                else
                {
                    timer.Once(pluginConfig.DeathDuration, () =>
                    {
                        if (!round.IsTeamAlive(Team.Defenders))
                            return;
                        List<BasePlayer> defenders = round.GetTeamPlayers(Team.Defenders);
                        foreach (BasePlayer spectator in player.GetSpectators())
                            TrySpectate(spectator, defenders, ListDirection.Next, player);

                        if (connected)
                            TrySpectate(player, defenders, ListDirection.Next, null);
                    });
                }
            }
        }

        private void OnPlayerDisconnected(BasePlayer player, string strReason)
        {
            if (match.isGoing)
            {
                if (round.TryGetMatchMember(player.userID, out MatchMember member))
                {
                    member.userInterface.DestroyInterface();
                    member.disconnected = true;
                }
            }
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            NextTick(() =>
            {
                if (match.isGoing)
                {
                    if (round.TryGetMatchMember(player.userID, out MatchMember member))
                    {
                        List<MatchMember> onlineMembers = round.GetOnlineMembers();
                        member.userInterface.CreateInterface(SortMembers(onlineMembers), pluginConfig.MaxTeamSize);
                        if (!round.isBombPlanted)
                            member.userInterface.SetTime(round.countdown);
                        else if (round.isBombPlanted)
                            member.userInterface.CrateBombIcon();
                        else if (round.isBombExploded)
                            member.userInterface.CreateExplosionIcon();

                        if (member.droppedOut)
                            TrySpectate(player, round.GetTeamPlayers(member.team), ListDirection.Next, null);

                        foreach (MatchMember m in onlineMembers)
                        {
                            member.userInterface.SetPlayerScore(m);
                            if (m.droppedOut)
                                member.userInterface.MakeAvatarDeath(m.GetPlayer());
                        }
                        member.GetPlayer().EndSleeping();
                        member.disconnected = false;
                    }
                }
                else
                    ForceRespawn(player);
            });
        }

        private void OnPlayerRespawned(BasePlayer player)
        {
            NextTick(() =>
            {
                if (player.IsSleeping())
                    player.EndSleeping();

                PlayerUtility.ClearInventory(player);

                player.Heal(player.MaxHealth());
            });
        }

        private bool CanBeWounded(BasePlayer player, HitInfo info) => false;

        private bool CanDropActiveItem(BasePlayer player) => false;

        private void DropItems(BasePlayer player)
        {
            //player.UpdateActiveItem(new ItemId());
            PlayerUtility.RemoveActiveItem(player);
            Item c4bomb = player.inventory.FindItemByItemName("explosive.timed");
            Item ak47 = player.inventory.FindItemByItemName("rifle.ak");
            Item syringe = player.inventory.FindItemByItemName("syringe.medical");

            Vector3 vector = new Vector3(UnityEngine.Random.Range(-2f, 2f), 0.2f, UnityEngine.Random.Range(-2f, 2f));
            c4bomb?.Drop(player.GetDropPosition(), player.GetInheritedDropVelocity() + vector.normalized * 3f);
            ak47?.Drop(player.GetDropPosition(), player.GetInheritedDropVelocity() + vector.normalized * 3f);
            syringe?.Drop(player.GetDropPosition(), player.GetInheritedDropVelocity() + vector.normalized * 3f);
        }


        #endregion Death

        #region Commands


        [ChatCommand("start")]
        private void StartMatchCommand(BasePlayer player, string command, string[] args)
        {
            if (player.IPlayer.IsAdmin)
            {
                if (match.isGoing)
                    Puts("Calling startMatch() is cancelled, use /end");
                else
                    StartMatch();
            }
        }

        [ChatCommand("end")]
        private void EndMatchCommand(BasePlayer player, string command, string[] args)
        {
            if (player.IPlayer.IsAdmin)
            {
                if (match.isGoing)
                    EndMatch();
            }
        }

        //[ConsoleCommand("t")]
        //private void test3(BasePlayer player, string command, string[] args)
        //{
        //    LoadConfig();
        //    PlantBomb(player, pluginConfig.BombLifetime);
        //}

        [ConsoleCommand("test")]
        private void cmdTest(ConsoleSystem.Arg arg)
        {
            if (arg.HasArgs(1))
            {
                string userID = arg.Args[0].ToString();
                BasePlayer player = BasePlayer.Find(userID);
                if (player != null)
                    Puts("{0}, {1}", player, PlayerUtility.IsOnline(player));
                else
                    Puts("Can't found player with id [{0}]", userID);
            }
        }

        [ChatCommand("cl")]
        private void ClearMapCommand(BasePlayer player, string command, string[] args)
        {
            if (player.IPlayer.IsAdmin)
                ClearArea();
        }

        [ChatCommand("cr")]
        private void CurrentRoundCommand(BasePlayer player, string command, string[] args)
        {
            if (player.IPlayer.IsAdmin)
                PrintToChat(round.isGoing.ToString() + " " + match.roundCount.ToString());
        }

        [ConsoleCommand("hud.spectating.previous")]
        private void SpectatingPreviousPlr(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            if (player == null)
                return;

            if (!player.IsSpectating())
                return;

            if (!round.IsMember(player.userID))
                return;

            MatchMember matchMember = round.GetMember(player.userID);
            bool isDefender = matchMember.IsDefender();
            bool isRaider = matchMember.IsRaider();

            if (isDefender)
                TrySpectate(player, round.GetTeamPlayers(Team.Defenders), ListDirection.Previous, player.SpectatingTarget);
            else if (isRaider)
                TrySpectate(player, round.GetTeamPlayers(Team.Raiders), ListDirection.Previous, player.SpectatingTarget);
        }

        [ConsoleCommand("hud.spectating.next")]
        private void SpectatingNextPlr(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            if (player == null)
                return;

            if (!player.IsSpectating())
                return;

            if (!round.IsMember(player.userID))
                return;

            MatchMember matchMember = round.GetMember(player.userID);
            bool isDefender = matchMember.IsDefender();
            bool isRaider = matchMember.IsRaider();

            if (isDefender)
                TrySpectate(player, round.GetTeamPlayers(Team.Defenders), ListDirection.Next, player.SpectatingTarget);
            else if (isRaider)
                TrySpectate(player, round.GetTeamPlayers(Team.Raiders), ListDirection.Next, player.SpectatingTarget);
        }
        #endregion

        #region Colosseum
        [ChatCommand("coll")]
        private void ToColesseum(BasePlayer player, string command, string[] args)
        {
            if (match != null)
                if (match.isGoing) return;
            if (round != null)
                if (round.isGoing) return;

            List<Vector3> positions = new List<Vector3>
            {
                new Vector3(-287.651f, -249.985f, -416.629f),
                new Vector3(-281.517f, -249.985f, -413.344f),
                new Vector3(-276.604f, -249.985f, -406.702f),
                new Vector3(-307.769f, -249.985f, -409.070f),
                new Vector3(-308.812f, -249.985f, -393.910f),
                new Vector3(-302.973f, -249.985f, -385.533f),
                new Vector3(-293.651f, -249.985f, -383.234f),
                new Vector3(-281.046f, -249.985f, -388.391f)
            };
            PlayerUtility.Teleport(player, positions[UnityEngine.Random.Range(0, positions.Count)]);
            PlayerUtility.ClearInventory(player);
            PlayerUtility.GiveKit(rust, player, "knight");
        }
        #endregion Colosseum
    }
}