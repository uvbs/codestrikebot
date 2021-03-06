﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace CodeStrikeBot.Messages
{
    public class SyncedDataMessage : JsonMessage
    {
        public List<Objects.Watchtower> Watchtowers { get; private set; }
        public bool empireinappsale { get; private set; }

        public SyncedDataMessage(JsonMessage message)
            : base(message)
        {
            this.Type = MessageType.SyncedData;

            this.Watchtowers = new List<Objects.Watchtower>();
            this.empireinappsale = false;

            try
            {
                if (this.Json is JObject)
                {
                    foreach (KeyValuePair<string, JToken> kvp in (JObject)this.Json)
                    {
                        switch (kvp.Key.Replace("\"", ""))
                        {
                            case "Watchtower": //watchtower
                                foreach (KeyValuePair<string, JToken> m in (JObject)kvp.Value)
                                {
                                    string marchId = m.Key;
                                    Objects.Watchtower watch;

                                    if (m.Value is JObject)
                                    {
                                        watch = new Objects.Watchtower((JObject)m.Value);
                                    }
                                    else
                                    {
                                        watch = new Objects.Watchtower(m.Key);
                                    }

                                    this.Error |= watch.Error;

                                    this.Watchtowers.Add(watch);
                                }
                                break;
                            case "empireinappsale":
                                this.empireinappsale = true;
                                break;
                            default:
                                //other synceddata types
                                //save types to list for future additions
                                break;
                        }
                    }
                }
            }
            catch (FormatException ex)
            {
                this.Error = true;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                this.Error = true;
            }
        }

        public override string ToString()
        {
            if (this.Watchtowers.Count > 0)
            {
                return (this.Error ? "*ERROR* " : "") + String.Format("WATCH{0}: {1}", this.Watchtowers[0].march_id, this.Watchtowers[0].ActualTotalUnits);
            }
            else
            {
                return "";
            }
        }
    }
}
