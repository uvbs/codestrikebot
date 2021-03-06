﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CodeStrikeBot
{
    public class ScreenStateFFXV : ScreenState
    {
        public ScreenStateFFXV() : base()
        {
            
        }

        protected override void GetGameArea(SuperBitmap bmp)
        {
            CurrentArea = Area.Unknown;
            //Y-axis 485-551 notification area
            //menu area
            ushort chksum = ScreenState.GetScreenChecksum(bmp, 255, 19, 4, 4);
            ushort chksum2 = 0, chksum3 = 0;
            bool enteredGeneric = false;
            Color c, c2, c3;
            //bmp.Bitmap.Save(String.Format("{0}\\file.bmp", Controller.Instance.GetFullScreenshotDir()), System.Drawing.Imaging.ImageFormat.Bmp);

            switch (chksum)
            {
                case 0x81e8: //menu
                    /*using (Bitmap bmp2 = bmp.SubBitmap(62, 2, 204, 20))
                    {
                        //AutoOcr ocr = new AutoOcr();

                        //OcrResult res = ocr.Read(bmp2);
                        //string text = res.Text;
                        
                        using (TesseractEngine ocr = new TesseractEngine(@"./tessdata", "eng"))
                        {
                            using (Page page = ocr.Process(bmp2))
                            {
                                string text = page.GetText();
                            }
                        }
                    }*/

                    chksum2 = ScreenState.GetScreenChecksum(bmp, 160, 16, 14);
                    switch (chksum2)
                    {
                        case 0x0840:
                            CurrentArea = Area.StateMaps.Main;
                            break;
                        case 0x83ac:
                            CurrentArea = Area.Menus.Alliance;
                            break;
                        case 0x1ab1:
                            CurrentArea = Area.Menus.Gifts;
                            break;
                        case 0xa5ae:
                            CurrentArea = Area.Menus.AllianceHelp;
                            break;
                        case 0xc1a6:
                            CurrentArea = Area.Menus.AllianceWar;
                            break;
                        case 0xbb6b:
                            CurrentArea = Area.Menus.Challenge;
                            break;
                        case 0x474c:
                            CurrentArea = Area.Menus.Deployments;
                            break;
                        case 0x6db9:
                            CurrentArea = Area.Menus.March;
                            break;
                        case 0x67ed:
                            CurrentArea = Area.Menus.Boost;
                            break;
                        case 0x628d:
                            CurrentArea = Area.Menus.Boosts.PeaceShield;
                            break;
                        case 0x9442:
                            CurrentArea = Area.Menus.Boosts.Attack;
                            break;
                        case 0x548b:
                            CurrentArea = Area.Menus.Boosts.Health;
                            break;
                        case 0x0602:
                            CurrentArea = Area.Menus.Boosts.DeploymentSize;
                            break;
                        case 0x7c48:
                            CurrentArea = Area.Menus.Boosts.CommanderXP;
                            break;
                        case 0xe1ff:
                            CurrentArea = Area.Menus.Boosts.UpkeepReduction;
                            break;
                        case 0x464b:
                            CurrentArea = Area.Menus.Boosts.Gathering;
                            break;
                        case 0xad7c:
                            CurrentArea = Area.Menus.Boosts.FakeForces;
                            break;
                        case 0x1586:
                            CurrentArea = Area.Menus.Boosts.AntiScout;
                            break;
                        case 0x19b3:
                            CurrentArea = Area.Menus.Boosts.FoodProduction;
                            break;
                        case 0xc678:
                            CurrentArea = Area.Menus.Boosts.OilProduction;
                            break;
                        case 0x17f4:
                            CurrentArea = Area.Menus.Boosts.StoneProduction;
                            break;
                        case 0xce7f:
                            CurrentArea = Area.Menus.Boosts.IronProduction;
                            break;
                        case 0x2c0e:
                            CurrentArea = Area.Menus.Boosts.CoinProduction;
                            break;
                        case 0x4e99:
                            CurrentArea = Area.Menus.Boosts.TroopQueue;
                            break;
                        case 0x3638:
                            CurrentArea = Area.Menus.Items;
                            break;
                        case 0x9c65:
                            CurrentArea = Area.Menus.Mission;
                            break;
                        case 0xce0f:
                            CurrentArea = Area.Menus.Missions.Base;
                            break;
                        case 0xebbf:
                            CurrentArea = Area.Menus.Missions.Daily;
                            break;
                        case 0xebb1:
                            CurrentArea = Area.Menus.Missions.Alliance;
                            break;
                        case 0xa828:
                            CurrentArea = Area.Menus.Missions.VIP;
                            break;
                        case 0x25d:
                            CurrentArea = Area.Menus.Mail;
                            break;
                        case 0x7687:
                            CurrentArea = Area.Menus.MailCompose;
                            break;
                        case 0x71a7:
                            CurrentArea = Area.Menus.More;
                            break;
                        case 0x232b:
                            CurrentArea = Area.Menus.Account;
                            break;
                        case 0x4425:
                            CurrentArea = Area.Menus.CasinoLobby;
                            break;
                        case 0xa4b5: //blank menu
                            CurrentArea = Area.Menus.Casino;
                            break;
                        case 0xe7d9:
                            CurrentArea = Area.Menus.ResourceHelp;
                            break;
                        case 0xad39:
                            CurrentArea = Area.Menus.Resources;
                            break;
                        case 0x5bbb:
                        case 0x7bdc: //MP TODO: move to new enum
                        case 0xeb6e: //hero boosts TODO: move to new enum
                            CurrentArea = Area.Menus.Commander;
                            break;
                        case 0x375f:
                        case 0x5147: //comments TODO: move to new enum
                            CurrentArea = Area.Menus.Profile;
                            break;
                        case 0x2216: //weapon
                        case 0x29d0: //TODO helmet
                        case 0x8aa6: //armor
                        case 0xbce9: //footwear
                        case 0xd228: //accessory
                            CurrentArea = Area.Menus.Gear;
                            break;
                        case 0x889e:
                        case 0x3181: //vip points
                            CurrentArea = Area.Menus.VIP;
                            break;
                        case 0x6224:
                            CurrentArea = Area.Menus.VIPSubscriptions;
                            break;
                        case 0x71bc:
                        case 0xa9a0: //upgrade building TODO move to new enum
                        case 0xafe0: //speed up building TODO move to new enum
                        case 0x3ce3: //speed up research TODO move to new enum
                            CurrentArea = Area.Menus.BuildingList;
                            break;
                        case 0x474f:
                            CurrentArea = Area.Menus.Buildings.HQ;
                            break;
                        case 0xef15:
                            CurrentArea = Area.Menus.Buildings.Wall;
                            break;
                        case 0x65fc:
                            CurrentArea = Area.Menus.Buildings.Warehouse;
                            break;
                        case 0xc981:
                            CurrentArea = Area.Menus.Buildings.Radar;
                            break;
                        case 0x3816:
                            CurrentArea = Area.Menus.Buildings.TradingPost;
                            break;
                        case 0x35e6:
                            CurrentArea = Area.Menus.Buildings.HallOfHeroes;
                            break;
                        case 0x1e57: //hospital ward
                        case 0x19ff: //hospital
                            CurrentArea = Area.Menus.Buildings.Hospital;
                            break;
                        case 0xf42c: //barracks
                        case 0x8fb2: //training grounds
                            chksum2 = ScreenState.GetScreenChecksum(bmp, 190, 85, 20);
                            if (chksum2 == 0x54bc)
                            {
                                CurrentArea = Area.Menus.ShootingRanges.Lobby;
                            }
                            else
                            {
                                CurrentArea = Area.Menus.Buildings.TrainingGrounds;
                            }
                            break;
                        case 0x5448:
                            CurrentArea = Area.Menus.Buildings.Bank;
                            break;
                        case 0xc39f: //research
                        case 0x5648: //economics
                        case 0xb13a: //combat
                        case 0x5c17: //defense
                        case 0x1803: //crafting
                        case 0xa4dc: //hero
                            CurrentArea = Area.Menus.Buildings.Research;
                            break;
                        case 0x1c5c:
                            CurrentArea = Area.Menus.Buildings.Armory;
                            break;
                        case 0xdadf:
                            CurrentArea = Area.Menus.Buildings.Embassy;
                            break;
                        case 0x69e9:
                            CurrentArea = Area.Menus.Buildings.WarRoom;
                            break;
                        case 0xddb8:
                            CurrentArea = Area.Menus.Buildings.Prison;
                            break;
                        case 0xcac9:
                            CurrentArea = Area.Menus.Buildings.DeathRow;
                            break;
                        case 0x8011:
                            CurrentArea = Area.Menus.Buildings.Farm;
                            break;
                        case 0x9081:
                            CurrentArea = Area.Menus.Buildings.Quarry;
                            break;
                        case 0x9d3e:
                            CurrentArea = Area.Menus.Buildings.IronMine;
                            break;
                        case 0x19b8:
                            CurrentArea = Area.Menus.Buildings.OilWell;
                            break;
                        case 0x2456:
                            CurrentArea = Area.Menus.Buildings.Vault;
                            break;
                        case 0x59a1: //blog TODO: make new enum
                        default:
                            chksum3 = ScreenState.GetScreenChecksum(bmp, 16, 16, 10);
                            if (chksum3 != 0x3ca7)
                            {
                                enteredGeneric = true;
                            }

                            //proving ground check
                            for (int y = 60; y < 90; y++)
                            {
                                c = bmp.GetPixel(20, y);
                                if (c.Equals(173, 4, 24))
                                {
                                    c = bmp.GetPixel(20, y + 10);
                                    if (c.Equals(173, 4, 24))
                                    {
                                        CurrentArea = Area.Menus.ShootingRanges.Main;
                                        break;
                                    }
                                }
                            }
                            break;
                    }
                    break;
                case 0xcfeb: //menu loading
                    chksum2 = ScreenState.GetScreenChecksum(bmp, 160, 16, 14);
                    switch (chksum2)
                    {
                        case 0xb97b:
                            CurrentArea = Area.StateMaps.Main;
                            break;
                        case 0x1c39:
                            CurrentArea = Area.Menus.Alliance;
                            break;
                        case 0x4b7b:
                            CurrentArea = Area.Menus.Mission;
                            break;
                        case 0x8af6: //logging out
                        case 0x3455:
                            CurrentArea = Area.Menus.Account;
                            break;
                        case 0x6caf:
                            CurrentArea = Area.Menus.Deployments;
                            break;
                        case 0x6b06:
                            CurrentArea = Area.Menus.More;
                            break;
                        default:
                            enteredGeneric = true;
                            break;
                    }
                    break;
                case 0xc7bd: //menu modal
                case 0xb3b7: //menu modal (Log out/are you sure)
                //case 0xc4a8: //large modal
                    chksum2 = ScreenState.GetScreenChecksum(bmp, 160, 16, 14);
                    switch (chksum2)
                    {
                        case 0x2689: //login loading
                        case 0x033d: //login modal (failed)
                            CurrentArea = Area.Others.Login;
                            break;
                        case 0x154f:
                            chksum3 = ScreenState.GetScreenChecksum(bmp, 215, 235, 10);
                            if (chksum3 == 0x3702)
                            {
                                CurrentArea = Area.StateMaps.CoordinateError;
                            }
                            else if (chksum3 == 0x8924)
                            {
                                CurrentArea = Area.StateMaps.Coordinate;
                            }
                            else
                            {
                                ushort chksum4 = ScreenState.GetScreenChecksum(bmp, 366, 16, 10);
                                if (chksum4 == 0xe394)
                                {
                                    CurrentArea = Area.StateMaps.Main;
                                }
                                else if (chksum4 == 0x2a78)
                                {
                                    CurrentArea = Area.StateMaps.FullScreen;
                                }
                                else
                                {
                                    enteredGeneric = true;
                                }
                            }
                            break;
                        case 0x2411:
                            //TODO: Convert this to overlay popup, see unknown0cde-6e83
                            CurrentArea = Area.Menus.Missions.ActivateVIP;
                            break;
                        case 0xef0c:
                            CurrentArea = Area.Menus.MailCompose;
                            break;
                        case 0x369e: //log out
                            CurrentArea = Area.Menus.Account;
                            break;
                        case 0x8e61:
                        case 0x2e14: //donation confirmation
                            CurrentArea = Area.Menus.ResourceHelp;
                            break;
                        case 0xec6f: //rss use confirmation
                            CurrentArea = Area.Menus.Resources;
                            break;
                        case 0xddf8: //economics
                        case 0xf417: //economics
                        case 0xd6b2: //combat
                        case 0x0792: //defense
                        case 0x9b6b: //hero
                        case 0x1477: //hero mastered
                            CurrentArea = Area.Menus.Buildings.Research;
                            break;
                        case 0x84e5: //old?
                        case 0xa973: //TODO check, demolish building dialog
                            CurrentArea = Area.Menus.Buildings.Farm;
                            break;
                        case 0x1fe2:
                            CurrentArea = Area.Menus.Buildings.OilWell;
                            break;
                        case 0x1537:
                            CurrentArea = Area.Menus.Buildings.Quarry;
                            break;
                        case 0x8731:
                            CurrentArea = Area.Menus.Buildings.IronMine;
                            break;
                        case 0x54cd:
                            CurrentArea = Area.Menus.Buildings.Bank;
                            break;
                        case 0xdb75:
                        case 0x8f88: //absolute black //TODO this scenario when demo building confirmation screen
                            CurrentArea = Area.Menus.BuildingList;
                            break;
                        case 0x3e5c:
                            CurrentArea = Area.Menus.Deployments;
                            break;
                        case 0xbe95:
                            CurrentArea = Area.Menus.Casino;
                            break;
                        case 0xdccc:
                            CurrentArea = Area.Menus.Profile;
                            break;
                        default:
                            chksum3 = ScreenState.GetScreenChecksum(bmp, 60, 120, 20);
                            if (chksum3 == 0xf2c6) //proving grounds crate
                            {
                                CurrentArea = Area.Menus.ShootingRanges.NormalCrate;
                            }
                            else
                            {
                                enteredGeneric = true;
                            }
                            break;
                    }
                    break;
                case 0xf36b:
                case 0x02f9: //main base loading
                //case 0x080e: //large modal (event tier prize) TODO: Move to popup
                    CurrentArea = Area.MainBases.Main;
                    break;
                case 0xec7f: //realm map monster modal (dark modal)
                    chksum2 = ScreenState.GetScreenChecksum(bmp, 366, 16, 10);
                    switch (chksum2)
                    {
                        case 0x4774:
                            CurrentArea = Area.Menus.ShootingRanges.NormalCrate;
                            break;
                        case 0x0094:
                            CurrentArea = Area.StateMaps.Main;
                            break;
                        default:
                            CurrentArea = Area.StateMaps.FullScreen;
                            break;
                    }
                    break;
                case 0xc930:
                case 0x13b3: //alternate?
                    CurrentArea = Area.Emulators.Loading;
                    break;
                case 0x9d61:
                case 0x4c90: //no search bar on top
                    CurrentArea = Area.Emulators.Android;
                    break;
                case 0xe0fa:
                case 0x11dd: //loading
                case 0x9180: //gray screen
                    CurrentArea = Area.Others.Splash;
                    break;
                case 0x33e2: //login
                case 0xfc07: //failed to connect to server (splash screen modal error, set to login to trick it to closing and retrying)
                    CurrentArea = Area.Others.Login;
                    break;
                /*case 0x9e78: //login modal/loading
                    chksum2 = ScreenState.GetScreenChecksum(bmp, 70, 120, 20);
                    if (chksum2 == 0x54e6) //TODO: collision case: casino jackpot crate collect
                    {
                        CurrentArea = Area.Menus.ShootingRanges.NormalCrate;
                    }
                    //else if (chksum2 == 0xfe5a) //TODO: collision case: deconstruct farm dialog
                    //{
                    //    CurrentArea = Area.Menus.Buildings.Farm;
                    //}
                    else
                    {
                        CurrentArea = Area.Others.Login;
                    }
                    break;*/
                case 0xe8c8:
                case 0xbef1: //modal/uncollectable secret gift
                    chksum2 = ScreenState.GetScreenChecksum(bmp, 190, 115, 20);
                    switch (chksum2)
                    {
                        case 0x8adf: //secret gift
                        case 0xef48: //uncollectable
                            CurrentArea = Area.MainBases.SecretGiftCollect;
                            break;
                        case 0xac19:
                            CurrentArea = Area.MainBases.GlobalGiftCollect;
                            break;
                        default:
                            CurrentArea = Area.MainBases.Main;
                            break;
                    }
                    break;
                case 0xbf9a: //black screen
                    chksum2 = ScreenState.GetScreenChecksum(bmp, 190, 115, 20);
                    if (chksum2 == 0xf1db) //notice
                    {
                        chksum3 = ScreenState.GetScreenChecksum(bmp, 190, 150, 20);
                        if (chksum3 == 0x2f3a) //session timeout
                        {
                            CurrentArea = Area.Others.SessionTimeout;
                        }
                    }
                    else if (chksum2 == 0x88e3)
                    {
                        CurrentArea = Area.Others.LoginPincode;
                    }
                    break;
                case 0x2c55: //guild chat
                case 0x59f8: //realm chat
                case 0xcdf8: //contacts
                    CurrentArea = Area.Others.Chat;
                    break;
                default:
                    break;
            }

            if (CurrentArea == Area.Unknown)
            {
                string text = Utilities.GetTextFromImage(bmp, 64, 300, 200, 30);
                if (text.Replace('i', 'l') == "Closing the Application".Replace('i', 'l'))
                {
                    CurrentArea = Area.Others.Quit;
                }

                text = Utilities.GetTextFromImage(bmp, 30, 325, 340, 30);
                if (text.StartsWith("Unfortunately"))
                {
                    CurrentArea = Area.Emulators.ProcessStopped;
                }
            }

            if (CurrentArea == Area.Unknown)
            {
                chksum2 = ScreenState.GetScreenChecksum(bmp, 72, 302, 20);

                switch (chksum2)
                {
                    case 0x7c8c:
                    case 0x231f: //memu 5.1.1.1
                        string text = Utilities.GetTextFromImage(bmp, 64, 300, 200, 20);

                        CurrentArea = Area.Others.Quit;
                        /*try
                        {
                            bmp.Bitmap.Save(String.Format("{0}\\-save.bmp", Controller.Instance.GetFullScreenshotDir()), System.Drawing.Imaging.ImageFormat.Bmp);
                        }
                        catch (System.Runtime.InteropServices.ExternalException e) { }*/
                        break;
                    default:
                        chksum3 = ScreenState.GetScreenChecksum(bmp, 60, 120, 20);
                        switch (chksum3)
                        {
                            case 0xf2c6: //shooting range crate collect
                            case 0xd0fc: //casino jackpot crate collect
                                CurrentArea = Area.Menus.ShootingRanges.NormalCrate;
                                break;
                            default:
                                chksum3 = ScreenState.GetScreenChecksum(bmp, 190, 476, 20);
                                if (chksum3 == 0xefdf)
                                {
                                    CurrentArea = Area.Menus.BuildingList; //Citadel upgrade complete
                                }
                                else
                                {
                                    chksum3 = ScreenState.GetScreenChecksum(bmp, 194, 642, 20);
                                    switch (chksum3)
                                    {
                                        case 0x5019:
                                        case 0x8a64:
                                            CurrentArea = Area.Others.Ad;
                                            break;
                                    }
                                }
                                break;
                        }
                        break;
                }
            }

            try  //TODO: Move this to outer scope after CONST are set up
            {
                if (CurrentArea == Area.Unknown)
                {
                    //chksum3 = ScreenState.GetScreenChecksum(bmp, 190, 115, 20);
                    if (!System.IO.File.Exists(String.Format("{0}\\unknown\\unknown{1}-{2}.bmp", Controller.Instance.GetFullScreenshotDir(), chksum.ToString("X4"), chksum2.ToString("X4"))))
                    {
                        bmp.Save(String.Format("{0}\\unknown\\unknown{1}-{2}.bmp", Controller.Instance.GetFullScreenshotDir(), chksum.ToString("X4"), chksum2.ToString("X4")));
                    }
                    /*if (CurrentArea == Area.Menu)
                    {
                        bmp.Bitmap.Save(String.Format("{0}\\-save.bmp", Controller.Instance.GetFullScreenshotDir()), System.Drawing.Imaging.ImageFormat.Bmp);
                    }*/
                }
                else if (enteredGeneric && CurrentArea != Area.Others.Quit)
                {
                    if (!System.IO.File.Exists(String.Format("{0}\\unknown\\genericCase-{1}-{2}-{3}.bmp", Controller.Instance.GetFullScreenshotDir(), this.ToString(), chksum.ToString("X4"), chksum2.ToString("X4"))))
                    {
                        bmp.Save(String.Format("{0}\\unknown\\genericCase-{1}-{2}-{3}.bmp", Controller.Instance.GetFullScreenshotDir(), this.ToString(), chksum.ToString("X4"), chksum2.ToString("X4")));
                    }
                    //Controller.Instance.SendNotification(string.Format("ScreenState found but missed above\n{0}-{1}-{2}", this.ToString(), chksum.ToString("X4"), chksum2.ToString("X4")), NotificationType.General);
                }

                /*chksum = ScreenState.GetScreenChecksum(bmp, 67, 16, 7);
                if (!System.IO.File.Exists(String.Format("{0}\\unknown\\unknown{1}.bmp", Controller.Instance.GetFullScreenshotDir(), chksum.ToString("X4"))))
                {
                    bmp.Bitmap.Save(String.Format("{0}\\unknown\\unknown{1}.bmp", Controller.Instance.GetFullScreenshotDir(), chksum.ToString("X4")), System.Drawing.Imaging.ImageFormat.Bmp);
                }*/
            }
            catch (System.Runtime.InteropServices.ExternalException ex) { }
        }

        protected override void GetGameOverlays(SuperBitmap bmp)
        {
            Overlays.Clear();

            ushort chksum;
            Color c;

            //Overlays
            if (CurrentArea == Area.MainBases.Main)
            {
                chksum = ScreenState.GetScreenChecksum(bmp, 67, 16, 6);
                if (chksum == 0x95ca)
                {
                    Overlays.Add(Overlay.Statuses.Loading);
                }

                chksum = ScreenState.GetScreenChecksum(bmp, 352, 545, 20);
                if (chksum == 0xdd80)
                {
                    Overlays.Add(Overlay.Widgets.AllianceHelp);
                }

                chksum = ScreenState.GetScreenChecksum(bmp, 373, 348, 10);
                //if (chksum != 0xe8ae && chksum != 0x6bb1) ///depricated?
                if (chksum == 0x0060)
                {
                    //c = bmp.GetPixel(44, 395);
                    //if (!c.Equals(165, 121, 24))
                    c = bmp.GetPixel(372, 345);
                    if (!c.Equals(198, 150, 33))
                    {
                        Overlays.Add(Overlay.Widgets.DailyLogin);
                    }
                }

                //c = s.bmp.GetPixel(35, 498);
                //c2 = s.bmp.GetPixel(29, 498);
                c = bmp.GetPixel(275, 535);
                //Rectangle(4, 489, 63, 18)
                //if (Math.Abs(c.R - c.G) <= 11 && (c.Within(157, 156, 155, 28) || c2.Within(165, 162, 156, 28)) && 
                if (ScreenState.BlackBoxExists(bmp, new Rectangle(4, 587, 51, 18), Color.FromArgb(74, 77, 74)))
                {
                    Overlays.Add(Overlay.Widgets.GlobalGift);
                }

                if (!ScreenState.BlackBoxExists(bmp, new Rectangle(63, 587, 51, 18), Color.FromArgb(74, 77, 74)))
                {
                    for (int i = 552; i <= 570; i++)
                    {
                        c = bmp.GetPixel(91, i);
                        //int min = 99; //Useful for debugging
                        //min = Math.Min(min, Math.Max(Math.Max(Math.Abs(c.R - 99), Math.Abs(c.G - 97)), Math.Abs(c.B - 99)));
                        if (c.Within(99, 97, 99, 8))
                        {
                            Overlays.Add(Overlay.Widgets.SecretGift);
                            break;
                        }
                    }
                }

                chksum = ScreenState.GetScreenChecksum(bmp, 98, 16, 10);
                if (chksum == 0x0f1e)
                {
                    Overlays.Add(Overlay.Statuses.VIPInactive);
                }

                if (ScreenState.BlackBoxExists(bmp, new Rectangle(16, 562, 63, 18), Color.FromArgb(74, 77, 74)))
                {
                    Overlays.Add(Overlay.Widgets.RewardsCrate);
                }
                
                if (ScreenState.BlackBoxExists(bmp, new Rectangle(248, 589, 51, 16), Color.FromArgb(74, 77, 74))) //DIFF ff
                {
                    Overlays.Add(Overlay.Widgets.AmmoFreeAttack);
                }
            }
            else if (CurrentArea == Area.StateMaps.Main || CurrentArea == Area.StateMaps.FullScreen)
            {
                chksum = ScreenState.GetScreenChecksum(bmp, 352, 545, 20);
                if (chksum == 0xdd80)
                {
                    Overlays.Add(Overlay.Widgets.AllianceHelp);
                }

                chksum = ScreenState.GetScreenChecksum(bmp, 54, 198, 10);
                if (chksum == 0xcadb)
                {
                    Overlays.Add(Overlay.Dialogs.Tiles.Empty);
                }
                else
                {
                    chksum = ScreenState.GetScreenChecksum(bmp, 40, 107, 10);
                    if (chksum == 0x63bd)
                    {
                        c = bmp.GetPixel(195, 240);
                        if (c.Equals(0, 186, 255))
                        {
                            Overlays.Add(Overlay.Dialogs.Tiles.PlayerFriend);
                        }
                        else 
                        {
                            c = bmp.GetPixel(195, 244); //if that player's VIP is active
                            if (c.Equals(0, 186, 255))
                            {
                                Overlays.Add(Overlay.Dialogs.Tiles.PlayerFriend);
                            }
                            else
                            {
                                Overlays.Add(Overlay.Dialogs.Tiles.PlayerEnemy);
                            }
                        }
                    }
                    else
                    {
                        chksum = ScreenState.GetScreenChecksum(bmp, 54, 71, 10);
                        if (chksum == 0x3dec)
                        {
                            Overlays.Add(Overlay.Dialogs.Tiles.RssOpen);
                        }
                        else
                        {
                            chksum = ScreenState.GetScreenChecksum(bmp, 23, 18, 10);
                            if (chksum == 0x10f3)
                            {
                                Overlays.Add(Overlay.Dialogs.Tiles.Rebel);
                            }
                            else
                            {
                                chksum = ScreenState.GetScreenChecksum(bmp, 54, 128, 10);
                                if (chksum == 0x41c0)
                                {
                                    Overlays.Add(Overlay.Dialogs.Tiles.Warzone);
                                }
                                else
                                {
                                    chksum = ScreenState.GetScreenChecksum(bmp, 54, 128, 10);
                                    if (chksum == 0x5345)
                                    {
                                        Overlays.Add(Overlay.Dialogs.Tiles.ControlPoint);
                                    }
                                    else
                                    {
                                        chksum = ScreenState.GetScreenChecksum(bmp, 192, 120, 10);
                                        if (chksum == 0xa992 || chksum == 0x520b) //crystal no occupy allowed <-no longer true, its just a Blocked dialog
                                        {
                                            Overlays.Add(Overlay.Dialogs.Tiles.Blocked);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (CurrentArea == Area.Menus.Alliance || CurrentArea == Area.Menus.Mission || CurrentArea == Area.Menus.Account)
            {
                //bmp.Bitmap.Save(String.Format("{0}\\help{1}.bmp", Controller.Instance.GetFullScreenshotDir(), System.Threading.Thread.CurrentThread.ManagedThreadId.ToString()), System.Drawing.Imaging.ImageFormat.Bmp);
                chksum = ScreenState.GetScreenChecksum(bmp, 67, 16, 6);
                if (chksum == 0x041e)
                {
                    Overlays.Add(Overlay.Statuses.Loading);
                }
            }
            else if (CurrentArea == Area.Menus.AllianceHelp)
            {
                //bmp.Bitmap.Save(String.Format("{0}\\help{1}.bmp", Controller.Instance.GetFullScreenshotDir(), System.Threading.Thread.CurrentThread.ManagedThreadId.ToString()), System.Drawing.Imaging.ImageFormat.Bmp);
                chksum = ScreenState.GetScreenChecksum(bmp, 36, 225, 20);
                if (chksum == 0xfb5e) //f534 loaded2 //fb5e loading
                {
                    Overlays.Add(Overlay.Statuses.Loading);
                }
            }

            chksum = ScreenState.GetScreenChecksum(bmp, 190, 104, 10);
            if (chksum == 0x7cf6 || chksum == 0x124a) //popup
            {
                chksum = ScreenState.GetScreenChecksum(bmp, 190, 115, 20);
                switch (chksum)
                {
                    case 0x0388:
                    case 0x6abd:
                        //TODO: look into possible transparency issue with this dialog
                        // TODO: Remove, debugging
                        chksum = ScreenState.GetScreenChecksum(bmp, 67, 16, 4);
                        ushort chksum2 = ScreenState.GetScreenChecksum(bmp, 160, 16, 14);
                        ushort chksum3 = ScreenState.GetScreenChecksum(bmp, 190, 115, 20);

                        if (!System.IO.File.Exists(String.Format("{0}\\unknown\\rss{1}-{2}-{3}.bmp", Controller.Instance.GetFullScreenshotDir(), chksum.ToString("X4"), chksum2.ToString("X4"), chksum3.ToString("X4"))))
                        {
                            bmp.Save(String.Format("{0}\\unknown\\rss{1}-{2}-{3}.bmp", Controller.Instance.GetFullScreenshotDir(), chksum.ToString("X4"), chksum2.ToString("X4"), chksum3.ToString("X4")));
                        }
                        Overlays.Add(Overlay.Dialogs.Popups.TransferConfirmation);
                        break;
                    case 0xdc05: //max limit
                        Overlays.Add(Overlay.Dialogs.Popups.MaxDeployments);
                        break;
                    case 0x1187:
                        Overlays.Add(Overlay.Dialogs.Popups.DemolishBuilding);
                        break;
                    case 0x4c4a:
                    case 0x6262: //log out
                        Overlays.Add(Overlay.Dialogs.Popups.AreYouSure);
                        break;
                    case 0x152c:
                        Overlays.Add(Overlay.Dialogs.Popups.LoginFailed);
                        break;
                    case 0xba61: //notice
                    case 0x9aed: //memu 5.1.1.1
                        chksum = ScreenState.GetScreenChecksum(bmp, 190, 150, 20);
                        switch (chksum)
                        {
                            case 0x8a67: //march may have not sent due to internet connection
                                Overlays.Add(Overlay.Dialogs.Popups.ConnectionInterrupted);
                                break;
                            case 0x4e5b: //please try to collect the gift later
                                Overlays.Add(Overlay.Dialogs.Popups.Unknown);
                                break;
                            default:
                                Overlays.Add(Overlay.Dialogs.Popups.Unknown);
                                break;
                        }
                             //google play error
                             //quest timer not completed
                             //cannot attack allied base
                             //nsf rss
                        break;
                    case 0xb3aa: //tip!
                        chksum = ScreenState.GetScreenChecksum(bmp, 190, 150, 20);
                        if (chksum == 0x003e) //warning troops attacking outside base are not saved by hospital
                        {
                            Overlays.Add(Overlay.Dialogs.Popups.WarningOutsideAttack);
                        }
                        else
                        {
                            Overlays.Add(Overlay.Dialogs.Popups.Unknown);
                        }
                        break;
                    case 0x49ac: //scout
                        chksum = ScreenState.GetScreenChecksum(bmp, 190, 150, 20);
                        if (chksum == 0x68e1) //scout not unlocked 
                        {
                            Overlays.Add(Overlay.Dialogs.Popups.ScoutNotUnlocked);
                        }
                        else
                        {
                            Overlays.Add(Overlay.Dialogs.Popups.Unknown);
                        }
                        break;
                    case 0x54db:
                        Overlays.Add(Overlay.Dialogs.Popups.ReplaceBoost);
                        break;
                    case 0xe495:
                        Overlays.Add(Overlay.Dialogs.Popups.NewEvent);
                        break;
                    case 0xf58b: //coordinate dialog
                        break;
                    default:
                        Overlays.Add(Overlay.Dialogs.Popups.Unknown);
                        break;
                }
            }

            chksum = ScreenState.GetScreenChecksum(bmp, 200, 660, 10);
            //c = bmp.GetPixel(213, 664); //DIFF MS
            //if (c.Equals(24, 130, 16) || c.Equals(0, 28, 0)) //DIFF MS
            if (chksum == 0x2bd0 || chksum == 0x4dde)
            {
                Overlays.Add(Overlay.Widgets.AllianceGift);
            }

            c = bmp.GetPixel(132, 652);
            if (c.Equals(231, 4, 82))
            {
                Overlays.Add(Overlay.Widgets.MissionsAvailable);
            }

            chksum = ScreenState.GetScreenChecksum(bmp, 360, 494, 8);//DIFF ff
            switch (chksum)
            {
            //    case 0x7d6a:
            //        Overlays.Add(Overlay.Incomings.Rally);
            //        s.SuperBitmap.Bitmap.Save(Controller.Instance.GetFullScreenshotDir() + "rally.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //        break;
                case 0x3d9f:
                    Overlays.Add(Overlay.Incomings.Attack);
                    break;
            //    case 0x1475:
            //        Overlays.Add(Overlay.Incomings.Scout);
            //        break;
            //    case 0xb76e:
            //        Overlays.Add(Overlay.Incomings.Transport);
            //        break;
            //    case 0xe405:
            //        Overlays.Add(Overlay.Incomings.Reinforcement);
            //        break;
            }

            if (Overlays.Count == 0 && CurrentArea != Area.Unknown && CurrentArea != Area.MainBases.Main
                && CurrentArea != Area.StateMaps.Main && CurrentArea != Area.StateMaps.FullScreen
                && CurrentArea != Area.Emulators.Loading && CurrentArea != Area.Emulators.Android
                && CurrentArea != Area.Others.Login && CurrentArea != Area.Others.Splash && CurrentArea != Area.Others.Ad
                && CurrentArea != Area.Others.Chat && CurrentArea != Area.Others.SessionTimeout)
            {
                chksum = ScreenState.GetScreenChecksum(bmp, 67, 16, 4);
                ushort chksum2 = ScreenState.GetScreenChecksum(bmp, 160, 16, 14);
                ushort chksum3 = ScreenState.GetScreenChecksum(bmp, 190, 115, 20);

                if (chksum == 0x0cde || chksum == 0x57ca) //modal or double modal on a menu
                {
                    if (!System.IO.File.Exists(String.Format("{0}\\unknown\\unknown{1}-{2}-{3}.bmp", Controller.Instance.GetFullScreenshotDir(), chksum.ToString("X4"), chksum2.ToString("X4"), chksum3.ToString("X4"))))
                    {
                        bmp.Save(String.Format("{0}\\unknown\\unknown{1}-{2}-{3}.bmp", Controller.Instance.GetFullScreenshotDir(), chksum.ToString("X4"), chksum2.ToString("X4"), chksum3.ToString("X4")));
                    }
                }
            }

            if (Overlays.Contains(Overlay.Dialogs.Popups.Unknown))
            {
                chksum = ScreenState.GetScreenChecksum(bmp, 190, 115, 20);
                ushort chksum2 = ScreenState.GetScreenChecksum(bmp, 190, 150, 20);

                if (!System.IO.File.Exists(String.Format("{0}\\unknown\\popup{1}-{2}.bmp", Controller.Instance.GetFullScreenshotDir(), chksum.ToString("X4"), chksum2.ToString("X4"))))
                {
                    bmp.Save(String.Format("{0}\\unknown\\popup{1}-{2}.bmp", Controller.Instance.GetFullScreenshotDir(), chksum.ToString("X4"), chksum2.ToString("X4")));
                }
            }
        }
    }
}
