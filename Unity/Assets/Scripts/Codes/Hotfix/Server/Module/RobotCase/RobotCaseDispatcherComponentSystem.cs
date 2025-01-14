using System;
using System.Collections.Generic;

namespace ET.Server
{
    [FriendOf(typeof(RobotCaseDispatcherComponent))]
    [FriendOf(typeof(RobotCase))]
    public static class RobotCaseDispatcherComponentSystem
    {
        [ObjectSystem]
        public class RobotCaseDispatcherComponentAwakeSystem: AwakeSystem<RobotCaseDispatcherComponent>
        {
            protected override void Awake(RobotCaseDispatcherComponent self)
            {
                RobotCaseDispatcherComponent.Instance = self;
                self.Load();
            }
        }

        [ObjectSystem]
        public class RobotCaseDispatcherComponentLoadSystem: LoadSystem<RobotCaseDispatcherComponent>
        {
            protected override void Load(RobotCaseDispatcherComponent self)
            {
                self.Load();
            }
        }
        
        public static void Load(this RobotCaseDispatcherComponent self)
        {
            self.Dictionary.Clear();

            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof(RobotCaseAttribute));

            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(RobotCaseAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }
                
                RobotCaseAttribute attr = attrs[0] as RobotCaseAttribute;
                if (attr == null)
                {
                    continue;
                }
                
                IRobotCase robotCase = Activator.CreateInstance(type) as IRobotCase;
                if (robotCase == null)
                {
                    Log.Error($"RobotCase handle {type.Name} 需要继承 IRobotCase");
                    continue;
                }
                
                self.Dictionary.Add(attr.CaseType, robotCase);
            }
        }
        
        public static async ETTask Run(this RobotCaseDispatcherComponent self, int caseType, string line)
        {
            if (!self.Dictionary.TryGetValue(caseType, out IRobotCase iRobotCase))
            {
                return;
            }

            try
            {
                using (RobotCase robotCase = await RobotCaseComponent.Instance.New())
                {
                    robotCase.CommandLine = line;
                    await iRobotCase.Run(robotCase);
                }
            }
            catch (Exception e)
            {
                Log.Error($"{self.DomainZone()} {e}");
                RobotLog.Console($"RobotCase Error {caseType}:\n\t{e}");
            }
        }
    }
}