﻿using DSMFramework.Modding;
using UnityEngine;
using BepInEx;

namespace ExampleMod2
{

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            //Register our event into the game, a new instance is created every time a new dungeon (derelict) loads.
            ModGameEventManager.Manager.RegisterEvent(typeof(ExampleEvent));
        }
    }

    public class ExampleEvent : BaseGameEvent
    {
        //Use a random seed for when the event is created.
        public ExampleEvent() : base(Random.Range(int.MinValue, int.MaxValue))
        {
        }

        public override void Initalize()
        {
            //value between 0.0 and 1.0, 1 will always be 100% 
            Probability = 1f;

            // Frequency the event will be called to check if it can run based on the probability it has
            CheckFrequency = 1f;

            // Event cooldown for events that can happen more than one time.
            Cooldown = 5000f;

            //If true the event will be only run ExecuteEvent one time.
            //After running it the event will be removed from the current dungeon (derelict)
            //OneTimeEvent = true;
        }

        //Method called when a random float between 0 and 1 is grater or equals as your event probability.
        public override void ExecuteEvent()
        {
            SystemMessageManager.ShowSystemMessage("Some special example event happened!", ConsoleMessageType.Warning);
            //Commented dangerous part, we don't need to destroy someone's mission just because they installed
            //an example mod to see how it works
            //foreach (var room in DungeonManager.Instance.rooms)
            //{
            //    room.Radiate("Special mod event",1f);
            //}
        }
    }
}