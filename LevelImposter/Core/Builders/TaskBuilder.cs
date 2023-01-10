﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine;
using LevelImposter.DB;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LevelImposter.Core
{
    public class TaskBuilder : IElemBuilder
    {
        public static readonly Dictionary<string, TaskType> TASK_LENGTHS = new()
        {
            { "Short", TaskType.Short },
            { "Long", TaskType.Long },
            { "Common", TaskType.Common }
        };
        public static readonly Dictionary<string, int> CONSOLE_ID_PAIRS = new()
        {
            { "task-garbage2", 1 },
            { "task-garbage3", 0 },
            { "task-garbage4", 2 },
            { "task-fans1", 0 },
            { "task-fans2", 1 },
            { "task-records1", 0 }
        };
        public static readonly Dictionary<string, int> CONSOLE_ID_INCREMENTS = new()
        {
            { "task-toilet", 0 },
            { "task-breakers", 0 },
            { "task-towels", 0 },
            { "task-node", 0 },
            { "task-waterwheel1", 0 },
            { "task-fuel2", 0 },
            { "task-align1", 0 },
            { "task-records2", 1 },
            { "task-wires", 0 }
        };

        private GameObject? _taskContainer = null;
        private NormalPlayerTask? _wiresTask = null;
        private int _consoleID = 0;
        private List<string> _builtTypes = new();
        private Dictionary<string, int> _consoleIDIncrements = new(CONSOLE_ID_INCREMENTS);

        private static SystemTypes[] _divertSystems = Array.Empty<SystemTypes>();
        public static SystemTypes[] DivertSystems => _divertSystems;

        private static byte _breakerCount = 0;
        private static byte _toiletCount = 0;
        private static byte _towelCount = 0;
        private static byte _fuelCount = 0;
        private static byte _recordsCount = 0;
        private static byte _alignEngineCount = 0;
        private static byte _waterWheelCount = 0;
        private static byte _wiresCount = 0;

        public static byte BreakerCount => _breakerCount;
        public static byte ToiletCount => _toiletCount;
        public static byte TowelCount => _towelCount;
        public static byte FuelCount => _fuelCount;
        public static byte WaterWheelCount => _waterWheelCount;
        public static byte AlignEngineCount => _alignEngineCount;
        public static byte RecordsCount => _recordsCount;
        public static byte WiresCount => _wiresCount;

        public TaskBuilder()
        {
            _divertSystems = Array.Empty<SystemTypes>();
            _breakerCount = 0;
            _toiletCount = 0;
            _towelCount = 0;
            _fuelCount = 0;
            _waterWheelCount = 0;
            _alignEngineCount = 0;
            _recordsCount = 0;
            _wiresCount = 0;
    }

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("task-"))
                return;
            if (LIShipStatus.Instance?.ShipStatus == null)
                throw new Exception("ShipStatus not found");

            // Create Task Container
            if (_taskContainer == null)
            {
                _taskContainer = new GameObject("Tasks");
                _taskContainer.transform.SetParent(LIShipStatus.Instance.transform);
            }

            // Get DB
            TaskData taskData = AssetDB.Tasks[elem.type];
            ShipStatus shipStatus = LIShipStatus.Instance.ShipStatus;

            // Default Sprite
            obj.layer = (int)Layer.ShortObjects;
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = taskData.SpriteRenderer.sprite;
                if (elem.properties.color != null)
                    spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);
            }
            spriteRenderer.material = taskData.SpriteRenderer.material;

            // Parent
            SystemTypes systemType = RoomBuilder.GetParentOrDefault(elem);

            // Console
            Console console;
            Console origConsole = taskData.GameObj.GetComponent<Console>();
            if (elem.type == "task-pistols1" || elem.type == "task-rifles1")
            {
                console = obj.AddComponent<StoreArmsTaskConsole>();
                StoreArmsTaskConsole specialConsole = console.Cast<StoreArmsTaskConsole>();
                StoreArmsTaskConsole origSpecialConsole = origConsole.Cast<StoreArmsTaskConsole>();

                specialConsole.timesUsed = origSpecialConsole.timesUsed;
                specialConsole.Images = origSpecialConsole.Images;
                specialConsole.useSound = origSpecialConsole.useSound;
                specialConsole.usesPerStep = origSpecialConsole.usesPerStep;
            }
            else if (elem.type.StartsWith("task-towels") && elem.type != "task-towels1")
            {
                console = obj.AddComponent<TowelTaskConsole>();
                TowelTaskConsole specialConsole = console.Cast<TowelTaskConsole>();
                TowelTaskConsole origSpecialConsole = origConsole.Cast<TowelTaskConsole>();

                specialConsole.useSound = origSpecialConsole.useSound;
            }
            else
            {
                console = obj.AddComponent<Console>();
            }
            console.ConsoleId = _consoleID;
            console.Image = spriteRenderer;
            console.onlyFromBelow = elem.properties.onlyFromBelow == true;
            console.usableDistance = elem.properties.range == null ? 1.0f : (float)elem.properties.range;
            console.Room = systemType;
            console.TaskTypes = origConsole.TaskTypes;
            console.ValidTasks = origConsole.ValidTasks;
            console.AllowImpostor = false;

            if (CONSOLE_ID_PAIRS.ContainsKey(elem.type))
            {
                console.ConsoleId = CONSOLE_ID_PAIRS[elem.type];
            }
            else if (elem.type == "task-waterjug2")
            {
                TaskSet taskSet = new()
                {
                    taskType = TaskTypes.ReplaceWaterJug,
                    taskStep = new IntRange(1, 1)
                };
                console.ValidTasks = new Il2CppReferenceArray<TaskSet>(new TaskSet[] {
                    taskSet
                });
            }
            else if (elem.type.StartsWith("task-towels"))
            {
                if (elem.type == "task-towels1")
                    console.ConsoleId = 255;
                else
                {
                    console.ConsoleId = _consoleIDIncrements["task-towels"];
                    _consoleIDIncrements["task-towels"]++;
                }
            }
            else if (elem.type == "task-fuel1")
            {
                console.ValidTasks = new Il2CppReferenceArray<TaskSet>(byte.MaxValue / 2);
                for (byte i = 0; i < byte.MaxValue - 1; i+=2)
                {
                    TaskSet taskSet = new()
                    {
                        taskType = TaskTypes.FuelEngines,
                        taskStep = new(i, i)
                    };
                    console.ValidTasks[i / 2] = taskSet;
                }
            }
            else if (elem.type == "task-fuel2")
            {
                console.ConsoleId = _consoleIDIncrements[elem.type];
                TaskSet taskSet = new()
                {
                    taskType = TaskTypes.FuelEngines,
                    taskStep = new(console.ConsoleId * 2 + 1, console.ConsoleId * 2 + 1)
                };
                console.ValidTasks = new Il2CppReferenceArray<TaskSet>(new TaskSet[] {
                    taskSet
                });
                _consoleIDIncrements[elem.type]++;
            }
            else if (_consoleIDIncrements.ContainsKey(elem.type))
            {
                console.ConsoleId = _consoleIDIncrements[elem.type];
                _consoleIDIncrements[elem.type]++;
            }
            else
            {
                console.ConsoleId = _consoleID;
                _consoleID++;
            }

            // Button
            PolygonCollider2D collider = obj.AddComponent<PolygonCollider2D>();
            collider.isTrigger = true;
            PassiveButton origBtn = taskData.GameObj.GetComponent<PassiveButton>();
            if (origBtn != null)
            {
                PassiveButton btn = obj.AddComponent<PassiveButton>();
                btn.ClickMask = collider;
                btn.OnMouseOver = new UnityEvent();
                btn.OnMouseOut = new UnityEvent();
                Action action = console.Use;
                btn.OnClick.AddListener(action);
            }

            // Task
            bool isBuilt = _builtTypes.Contains(elem.type);
            _builtTypes.Add(elem.type);

            if (!isBuilt)
                LILogger.Info("Adding task for " + elem.name + "...");

            if (elem.type == "task-divert1" && !isBuilt)
            {
                List<LIElement> divertTargets = new();
                if (LIShipStatus.Instance.CurrentMap == null)
                    throw new Exception("Current map is unavailable");
                foreach (LIElement mapElem in LIShipStatus.Instance.CurrentMap.elements)
                    if (mapElem.type == "task-divert2")
                        divertTargets.Add(mapElem);

                _divertSystems = new SystemTypes[divertTargets.Count];
                NormalPlayerTask origTask = taskData.Behavior;
                for (int i = 0; i < divertTargets.Count; i++)
                {
                    LIElement divertTarget = divertTargets[i];

                    SystemTypes divertSystem = RoomBuilder.GetParentOrDefault(divertTarget);
                    _divertSystems[i] = divertSystem;

                    GameObject taskHolder = new(elem.name);
                    taskHolder.transform.SetParent(_taskContainer.transform);

                    DivertPowerTask task = taskHolder.AddComponent<DivertPowerTask>();
                    task.StartAt = systemType;
                    task.taskStep = origTask.taskStep;
                    task.MaxStep = origTask.MaxStep;
                    task.arrowSuspended = origTask.arrowSuspended;
                    task.ShowTaskTimer = origTask.ShowTaskTimer;
                    task.ShowTaskStep = origTask.ShowTaskStep;
                    task.TaskTimer = origTask.TaskTimer;
                    task.TimerStarted = origTask.TimerStarted;
                    task.TaskType = origTask.TaskType;
                    task.MinigamePrefab = origTask.MinigamePrefab;
                    task.TargetSystem = divertSystem;

                    GameObject arrow = UnityEngine.Object.Instantiate(origTask.Arrow.gameObject);
                    arrow.transform.SetParent(task.transform);
                    arrow.SetActive(false);
                    task.Arrow = arrow.GetComponent<ArrowBehaviour>();

                    shipStatus.LongTasks = MapUtils.AddToArr(shipStatus.LongTasks, task.Cast<NormalPlayerTask>());
                }
            }
            else if (!string.IsNullOrEmpty(taskData.BehaviorName) && !isBuilt)
            {
                if (!string.IsNullOrEmpty(elem.properties.description))
                    MapUtils.Rename(taskData.Behavior.TaskType, elem.properties.description);

                GameObject taskHolder = new(elem.name);
                taskHolder.transform.SetParent(_taskContainer.transform);

                NormalPlayerTask origTask = taskData.Behavior;
                NormalPlayerTask task = taskHolder.AddComponent(taskData.Behavior.GetIl2CppType()).Cast<NormalPlayerTask>();
                task.StartAt = systemType;
                task.taskStep = origTask.taskStep;
                task.MaxStep = origTask.MaxStep;
                task.arrowSuspended = origTask.arrowSuspended;
                task.ShowTaskTimer = origTask.ShowTaskTimer;
                task.ShowTaskStep = origTask.ShowTaskStep;
                task.TaskTimer = origTask.TaskTimer;
                task.TimerStarted = origTask.TimerStarted;
                task.TaskType = origTask.TaskType;
                task.MinigamePrefab = origTask.MinigamePrefab;

                if (origTask.Arrow != null)
                {
                    GameObject arrow = UnityEngine.Object.Instantiate(origTask.Arrow.gameObject);
                    arrow.transform.SetParent(task.transform);
                    arrow.SetActive(false);

                    task.Arrow = arrow.GetComponent<ArrowBehaviour>();
                }

                if (elem.type == "task-node")
                {
                    WeatherNodeTask nodeTask = task.Cast<WeatherNodeTask>();
                    nodeTask.NodeId = console.ConsoleId;
                    nodeTask.Stage2Prefab = origTask.Cast<WeatherNodeTask>().Stage2Prefab;
                }

                if (elem.type == "task-wires")
                {
                    _wiresTask = task;
                }

                string? taskLengthProp = elem.properties.taskLength;
                TaskType taskLength = taskLengthProp != null ? TASK_LENGTHS[taskLengthProp] : taskData.TaskType;
                if (taskLength == TaskType.Common)
                    shipStatus.CommonTasks = MapUtils.AddToArr(shipStatus.CommonTasks, task);
                if (taskLength == TaskType.Short)
                    shipStatus.NormalTasks = MapUtils.AddToArr(shipStatus.NormalTasks, task);
                if (taskLength == TaskType.Long)
                    shipStatus.LongTasks = MapUtils.AddToArr(shipStatus.LongTasks, task);
            }

            // Medscan
            if (elem.type == "task-medscan")
            {
                if (shipStatus.MedScanner != null)
                    LILogger.Warn("Only 1 med scanner can be used per map");
                MedScannerBehaviour medscan = obj.AddComponent<MedScannerBehaviour>();
                shipStatus.MedScanner = medscan;
            }
        }

        public void PostBuild() {
            string[] keys = new string[_consoleIDIncrements.Keys.Count];
            _consoleIDIncrements.Keys.CopyTo(keys, 0);

            foreach (var key in keys)
            {
                byte count = (byte)_consoleIDIncrements[key];
                if (key == "task-breakers")
                    _breakerCount = count;
                if (key == "task-toilet")
                    _toiletCount = count;
                if (key == "task-towels")
                    _towelCount = count;
                if (key == "task-fuel2")
                    _fuelCount = count;
                if (key == "task-waterwheel1")
                    _waterWheelCount = count;
                if (key == "task-align1")
                    _alignEngineCount = count;
                if (key == "task-records2")
                    _recordsCount = count;
                if (key == "task-wires")
                    _wiresCount = count;
                _consoleIDIncrements[key] = 0;
            }

            // Wires Length
            if (_wiresTask != null)
            {
                _wiresTask.MaxStep = Math.Min(WiresCount, (byte)3);
            }
            _wiresTask = null;
        }
    }
}
